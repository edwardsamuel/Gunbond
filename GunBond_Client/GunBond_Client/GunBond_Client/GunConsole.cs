using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Net;
using Gunbond_Client.Util;
using Gunbond_Client.Model;
using Gunbond;

namespace Gunbond_Client
{
    public class ClientConfig
    {
        private string tracker_address;
        public string TrackerAddress
        {
            get { return tracker_address; }
            set { tracker_address = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private int listen_port;
        public int ListenPort
        {
            get { return listen_port; }
            set { listen_port = value; }
        }
        private int max_timeout;
        public int MaxTimeout
        {
            get { return max_timeout; }
            set { max_timeout = value; }
        }

        public void LoadData(String filename)
        {
            if (File.Exists(filename))
            {
                XmlTextReader reader = new XmlTextReader(filename);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                switch (reader.Name)
                                {
                                    case "TrackerAddress":
                                        {
                                            this.TrackerAddress = reader.ReadString();
                                            break;
                                        }
                                    case "MaxTimeout":
                                        {
                                            this.MaxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "Port":
                                        {
                                            this.Port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "ListenPort":
                                        {
                                            this.ListenPort = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
                reader.Close();
            }
            else
            {
                // default this
                this.TrackerAddress = "127.0.0.1";
                this.MaxTimeout = 30000;
                this.Port = 9351;
                this.ListenPort = 9757;
                this.SaveData(filename);
            }
        }

        public void SaveData(String filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");
            writer.WriteElementString("TrackerAddress", this.TrackerAddress);
            writer.WriteElementString("MaxTimeout", this.MaxTimeout.ToString());
            writer.WriteElementString("Port", this.Port.ToString());
            writer.WriteElementString("ListenPort", this.ListenPort.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        public void Print()
        {
            Logger.WriteLine("ListenPort     : " + this.ListenPort.ToString());
            Logger.WriteLine("TrackerAddress : " + this.TrackerAddress);
            Logger.WriteLine("Port           : " + this.Port.ToString());
            Logger.WriteLine("MaxTimeout     : " + this.MaxTimeout.ToString());
            Logger.WriteLine();
        }
    };

    public class GunConsole
    {
        #region Attributes and Properties
        private Socket trackerSocket;
        private Socket listenerSocket;
        private Socket nextPeerSocket;

        private readonly object trackerPaddle = new object();
        private readonly object nextPeerPaddle = new object();
        private readonly object statusPaddle = new object();

        private Thread keepAlive;
        private Thread waitPeer;
        private Thread keepAliveRoom;

        private ClientConfig _config;
        public ClientConfig Configuration
        {
            get
            {
                return _config;
            }
        }

        public bool IsInRoom
        {
            get;
            set;
        }

        public bool IsCreator
        {
            get;
            set;
        }

        public bool IsConnected
        {
            get;
            set;
        }


        public Room Room
        {
            get;
            set;
        }

        public int PeerId
        {
            get;
            set;
        }

        #endregion

        public delegate void GameHandler(Message m);
        public event GameHandler GameEvent;
        public event GameHandler StartEvent;


        public GunConsole(string fileName)
        {
            _config = new ClientConfig();
            _config.LoadData(fileName);

            trackerSocket = null;
            listenerSocket = null;
            nextPeerSocket = null;

            keepAlive = null;
            waitPeer = null;
            keepAliveRoom = null;

            Room = null;

            IsConnected = false;
            IsInRoom = false;
            IsCreator = false;
        }

        public bool ConnectTracker()
        {
            IPEndPoint ipEndPointListener = null;
            try
            {
                if (!IsConnected)
                {
                    SocketPermission permission = new SocketPermission(
                        NetworkAccess.Connect,
                        TransportType.Tcp,
                        "",
                        SocketPermission.AllPorts
                        );

                    permission.Demand();

                    IPAddress trackerAddr;
                    if (IPAddress.TryParse(Configuration.TrackerAddress, out trackerAddr))
                    {
                        byte[] buffer = new byte[1024];
                        lock (trackerPaddle)
                        {
                            IPEndPoint ipEndPoint = new IPEndPoint(trackerAddr, Configuration.Port);
                            trackerSocket = new Socket(
                                trackerAddr.AddressFamily,
                                SocketType.Stream,
                                ProtocolType.Tcp
                               );

                            trackerSocket.NoDelay = false;
                            trackerSocket.Connect(ipEndPoint);
                            Message request = Message.CreateMessageHandshakePeer();

                            trackerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                            trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                        }

                        Message response = new Message(buffer);
                        if (response.GetMessageType() == Message.MessageType.HandshakeTracker)
                        {
                            int peerId;
                            response.GetHandshakeTracker(out peerId);
                            PeerId = peerId;
                            IsConnected = true;
                            Logger.WriteLine("Connection to tracker is successfully established. PeerID: " + PeerId);

                            IPAddress ipAddr = (trackerSocket.LocalEndPoint as IPEndPoint).Address;
                            ipEndPointListener = new IPEndPoint(ipAddr, Configuration.ListenPort);
                            SocketPermission permissionListener = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", Configuration.ListenPort);

                            listenerSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


                            Logger.WriteLine("---- " + ipEndPointListener);
                            listenerSocket.Bind(ipEndPointListener);

                            permission.Demand();

                            waitPeer = new Thread(new ParameterizedThreadStart(WaitPeer));
                            waitPeer.Start(4);


                            keepAlive = new Thread(new ThreadStart(SendAliveTracker));
                            keepAlive.Start();
                            return true;
                        }
                        else
                        {
                            Logger.WriteLine("Failed to connect to tracker.");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.WriteLine("Failed to connect, tracker not found.");
                        return false;
                    }
                }
                else
                {
                    Logger.WriteLine("Already connected to tracker.");
                    return false;
                }
            }
            catch (SocketException exc)
            {
                Logger.WriteLine(exc);
                return false;
            }
        }

        public bool CreateRoom(string roomId, int maxPlayers)
        {
            try
            {
                if (IsConnected && !IsInRoom)
                {
                    Logger.WriteLine("Requesting to create room");

                    byte[] buffer = new byte[1024];
                    Message request = Message.CreateMessageCreate(PeerId, maxPlayers, roomId, Configuration.ListenPort);

                    lock (trackerPaddle)
                    {
                        trackerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                        trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    }

                    Message response = new Message(buffer);
                    Message.MessageType responseType = response.GetMessageType();
                    if (responseType == Message.MessageType.Success)
                    {
                        Logger.WriteLine("Request create room success: " + roomId);

                        Room = new Room(roomId, new Peer(PeerId, (trackerSocket.LocalEndPoint as IPEndPoint).Address, Configuration.ListenPort), maxPlayers);
                        Room.Members.Add(Room.Creator);

                        IsInRoom = true;
                        IsCreator = true;

                        return true;
                    }
                    else if (responseType == Message.MessageType.Failed)
                    {
                        Logger.WriteLine("Request create room failed.");
                        return false;
                    }
                    else
                    {
                        Logger.WriteLine("Response unrecognized.");
                        return false;
                    }
                }
                else if (!IsConnected)
                {
                    Logger.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (IsInRoom)
                {
                    Logger.WriteLine("Currently in room, quit room first.");
                    return false;
                }
                else
                {
                    Logger.WriteLine("Unknown error.");
                    return false;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
                return false;
            }
        }

        public List<Message.MessageRoomBody> ListRooms()
        {
            try
            {
                Logger.WriteLine("Requesting to list room");

                Message messageOut = Message.CreateMessageList(PeerId);
                byte[] buffer = new byte[1024];

                trackerSocket.Send(messageOut.data, 0, messageOut.data.Length, SocketFlags.None);
                trackerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                Message messageIn = new Message(buffer);
                if (messageIn.GetMessageType() == Message.MessageType.Room)
                {
                    List<Message.MessageRoomBody> s;
                    messageIn.GetRoom(out s);
                    foreach (var a in s)
                    {
                        Logger.WriteLine("ROOM: " + a.roomId);
                    }
                    return s;
                }
                else
                {
                    Logger.WriteLine("Failed to list rooms");
                    return null;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
                return null;
            }
        }

        public bool JoinRoom(string roomId)
        {
            try
            {
                if ((IsConnected) && (!IsInRoom))
                {
                    Logger.WriteLine("Requesting to join room " + roomId);
                    Message request = Message.CreateMessageJoin(PeerId, roomId);
                    byte[] buffer = new byte[1024];

                    lock (trackerPaddle)
                    {
                        trackerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                        trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    }

                    Message response = new Message(buffer);
                    Message.MessageType responseType = response.GetMessageType();
                    if (responseType == Message.MessageType.CreatorInfo)
                    {
                        IPAddress ip;
                        int port;
                        response.GetCreatorInfo(out ip, out port);

                        Logger.WriteLine("GunbondPeer (Peer - Tracker): Creator Info");
                        Logger.WriteLine("Hostname : " + ip);
                        Logger.WriteLine("Port     : " + port);

                        SocketPermission permission = new SocketPermission(
                             NetworkAccess.Connect,
                             TransportType.Tcp,
                             "",
                             SocketPermission.AllPorts
                             );
                        permission.Demand();

                        byte[] bufferFromCreator = new byte[1024];

                        lock (nextPeerPaddle)
                        {
                            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                            nextPeerSocket = new Socket(
                                ip.AddressFamily,
                                SocketType.Stream,
                                ProtocolType.Tcp
                                );

                            nextPeerSocket.NoDelay = false;
                            nextPeerSocket.Connect(ipEndPoint);
                            Message messageConnectToCreator = Message.CreateMessageHandshakePeerCreator(PeerId, Configuration.ListenPort);

                            nextPeerSocket.Send(messageConnectToCreator.data, 0, messageConnectToCreator.data.Length, SocketFlags.None);
                            nextPeerSocket.Receive(bufferFromCreator, bufferFromCreator.Length, SocketFlags.None);
                        }

                        keepAliveRoom = new Thread(new ThreadStart(SendAliveNextPeer));
                        keepAliveRoom.Start();

                        Message messageFromCreator = new Message(bufferFromCreator);
                        Message.MessageType fromCreatorMessageType = messageFromCreator.GetMessageType();
                        if (fromCreatorMessageType == Message.MessageType.Success)
                        {
                            IsInRoom = true;
                            IsCreator = false;

                            Logger.WriteLine("Successfully joined room.");
                            return true;
                        }
                        else
                        {
                            Logger.WriteLine("Request join room failed 1.");
                            return false;
                        }

                    }
                    else if (responseType == Message.MessageType.Failed)
                    {
                        Console.WriteLine("Request join room failed 2.");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Response unrecognized.");
                        return false;
                    }
                }
                else if (!IsConnected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (IsInRoom)
                {
                    Console.WriteLine("Currently in room, quit room first.");
                    return false;
                }
                else
                {
                    Console.WriteLine("Unknown error.");
                    return false;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                return false;
            }
        }

        public bool Quit()
        {
            try
            {
                if ((IsConnected) && (IsInRoom))
                {
                    Logger.WriteLine("Requesting to quit room");

                    Message request = Message.CreateMessageQuit(PeerId);
                    byte[] buffer = new byte[1024];

                    lock (nextPeerPaddle)
                    {
                        nextPeerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                        nextPeerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    }

                    Message response = new Message(buffer);
                    Message.MessageType responseType = response.GetMessageType();
                    if (responseType == Message.MessageType.Success)
                    {
                        nextPeerSocket.Close();
                        nextPeerSocket = null;
                        listenerSocket.Disconnect(true);
                        return true;
                    }
                    else
                    {
                        Logger.WriteLine("Quit is prohibited");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                return false;
            }
        }

        #region Keep Alive Thread
        private void SendAliveTracker()
        {
            //try
            //{
            //    while (true)
            //    {
            //        Logger.WriteLine("SendAliveTracker");
            //        Message mes = Message.CreateMessageKeepAlive(PeerId);
            //        byte[] buffer = new byte[1024];

            //        lock (trackerPaddle)
            //        {
            //            trackerSocket.Send(mes.data, 0, mes.data.Length, SocketFlags.None);
            //            trackerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            //        }

            //        Message response = new Message(buffer);
            //        if (response.GetMessageType() == Message.MessageType.KeepAlive)
            //        {
            //            Logger.WriteLine("KeepAlive success.");
            //            Thread.Sleep(Configuration.MaxTimeout / 10);
            //        }
            //        else
            //        {
            //            Logger.WriteLine("Message KeepAlive has not been received.");
            //        }
            //        Logger.WriteLine();
            //    }
            //}
            //catch (Exception)
            //{
            //    lock (trackerPaddle)
            //    {
            //        if (trackerSocket == null)
            //        {
            //            Logger.WriteLine("Connection terminated");
            //            IsConnected = false;
            //        }
            //    }
            //}
        }

        private void SendAliveNextPeer()
        {
            try
            {
                while (true)
                {
                    Logger.WriteLine("SendAliveNextPeer");
                    byte[] buffer = new byte[1024];

                    lock (nextPeerPaddle)
                    {
                        if (nextPeerSocket == null)
                        {
                            Thread.Sleep(Configuration.MaxTimeout / 2);
                            continue;
                        }

                        Message mes = Message.CreateMessageKeepAlive(PeerId);
                        nextPeerSocket.Send(mes.data, 0, mes.data.Length, SocketFlags.None);
                        nextPeerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    }

                    Message response = new Message(buffer);
                    if (response.GetMessageType() == Message.MessageType.KeepAlive)
                    {
                        Logger.WriteLine("KeepAlive to next peer success.");
                        Thread.Sleep(Configuration.MaxTimeout / 2);
                    }
                    else
                    {
                        Logger.WriteLine("Message KeepAlive reply from next peer has not been received.");
                        lock (nextPeerPaddle)
                        {
                            Logger.WriteLine("Closing connection " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);
                            nextPeerSocket.Close();
                        }
                        break;
                    }
                    Logger.WriteLine();
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message);
            }
        }
        #endregion

        #region Listening Socket
        private void WaitPeer(Object obj)
        {
            try
            {
                Logger.WriteLine("Listening thread has just been created.");
                listenerSocket.Listen((int)obj);

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listenerSocket.BeginAccept(aCallback, listenerSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult target)
        {
            Logger.WriteLine("--- AcceptCallback");

            Socket slistener = null;
            Socket handler = null;
            try
            {
                byte[] buffer = new byte[1024];
                slistener = (Socket)target.AsyncState;
                handler = slistener.EndAccept(target);
                handler.NoDelay = false;

                object[] obj = new object[2];
                obj[0] = buffer;
                obj[1] = handler;

                Logger.WriteLine("AcceptCallback from " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);

                IAsyncResult nextAsyncResult = handler.BeginReceive(
                    buffer,
                    0,
                    buffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveCallback),
                    obj
                    );

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                slistener.BeginAccept(aCallback, slistener);

                if (!nextAsyncResult.AsyncWaitHandle.WaitOne(Configuration.MaxTimeout))
                {
                    Logger.WriteLine("Connection timeout for peer with IP Address " + (handler.LocalEndPoint as IPEndPoint).Address + ":" + (handler.LocalEndPoint as IPEndPoint).Port + " -> " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                    handler.EndReceive(target);
                    Logger.WriteLine("Closing connection " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                    handler.Close();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
            }
            Logger.WriteLine();
        }

        private void ReceiveCallback(IAsyncResult target)
        {
            Logger.WriteLine("--- ReceiveCallback");
            Message.MessageType requestType = Message.MessageType.Unknown;
            Socket handler = null;
            try
            {
                object[] obj = new object[2];
                obj = (object[])target.AsyncState;

                byte[] buffer = (byte[]) obj[0];
                handler = (Socket) obj[1];

                Logger.WriteLine("ReceiveCallback from " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);

                bool quit = false;

                int bytesRead = handler.EndReceive(target);
                if (bytesRead > 0)
                {
                    Message request = new Message(buffer);
                    Message response;

                    requestType = request.GetMessageType();
                    if (requestType == Message.MessageType.HandshakePeerCreator) // Peer client mau join ke room ini
                    {
                        #region Handshake
                        Logger.WriteLine("HandshakePeerCreator");
                        if (Room.Members.Count < Room.MaxPlayer)
                        {
                            int newPeerId, newPeerListenPort;
                            IPAddress newPeerIp = (handler.RemoteEndPoint as IPEndPoint).Address;
                            request.GetHandshakePeerCreator(out newPeerId, out newPeerListenPort);
                            Room.Members.Add(new Peer(newPeerId, newPeerIp, newPeerListenPort));

                            Logger.WriteLine("Peer ID : " + newPeerId);
                            Logger.WriteLine("Peer IP : " + newPeerIp + ":" + newPeerListenPort);

                            // Send SUCCESS message
                            response = Message.CreateMessageSuccess();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);

                            if (nextPeerSocket != null)
                            {
                                Logger.WriteLine("Sending NEW MEMBER to nextPeerSocket: " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);
                                // Send NewMember message to next peer
                                response = Message.CreateMessageNewMember(newPeerId, (handler.RemoteEndPoint as IPEndPoint).Address, newPeerListenPort);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                            else
                            {
                                IPEndPoint ipEndPoint = new IPEndPoint(newPeerIp, newPeerListenPort);
                                nextPeerSocket = new Socket(
                                    newPeerIp.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp
                                   );
                                nextPeerSocket.NoDelay = false;
                                nextPeerSocket.Connect(ipEndPoint);

                                Logger.WriteLine("Setting up nextPeerSocket to " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);

                                response = Message.CreateMessageRoomModel(Room);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                                Logger.WriteLine("Send room info");

                                keepAliveRoom = new Thread(new ThreadStart(SendAliveNextPeer));
                                keepAliveRoom.Start();
                            }

                            response = Message.CreateMessageAdd(newPeerId, Room.RoomId);
                            lock (trackerPaddle)
                            {
                                trackerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                                byte[] bufferRecv = new byte[1024];
                                trackerSocket.Receive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            Logger.WriteLine("Send FAILED: Room penuh");
                            // Send FAILED message akibat ruang penuh
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            handler.Close();
                            quit = true;
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.KeepAlive)
                    {
                        #region Keep Alive
                        int peerId;
                        request.GetKeepAlive(out peerId);
                        Peer peer = Room.Members.Find(fpeer => fpeer.PeerId == peerId);
                        if (peer != null)
                        {
                            //Send KeepAlive Message back
                            response = request;
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        else
                        {
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.NewMember)
                    {
                        #region NewMember
                        Logger.WriteLine("NewMember");
                        if (IsCreator)
                        {
                            Logger.WriteLine("NewMember has been received by peer creator");
                            Logger.WriteLine("Closing connection from " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                            handler.Close();
                            quit = true;
                        }
                        else
                        {
                            bool isLast = Room.Members.Last().PeerId == PeerId;

                            int newPeerId, newPeerListenPort;
                            IPAddress newPeerIp;
                            request.GetNewMember(out newPeerId, out newPeerIp, out newPeerListenPort);
                            Room.Members.Add(new Peer(newPeerId, newPeerIp, newPeerListenPort));

                            Logger.WriteLine("Sending NEW MEMBER to nextPeerSocket: " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);
                            response = request;
                            nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);

                            if (isLast)
                            {
                                Logger.WriteLine("Closing connection " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);
                                nextPeerSocket.Close();

                                IPEndPoint ipEndPoint = new IPEndPoint(newPeerIp, newPeerListenPort);
                                nextPeerSocket = new Socket(
                                    newPeerIp.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp
                                   );

                                nextPeerSocket.NoDelay = false;
                                nextPeerSocket.Connect(ipEndPoint);

                                Logger.WriteLine("Setting up nextPeerSocket to " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address + ":" + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Port);

                                response = Message.CreateMessageRoomModel(Room);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                                Logger.WriteLine("Send room info");
                            }
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.RoomModel)
                    {
                        #region Room
                        Logger.WriteLine("RoomModel");
                        Room room;
                        request.GetRoomModel(out room);
                        Room = room;
                        #endregion
                    }

                    else if (requestType == Message.MessageType.Start)
                    {
                        #region Start
                        int peerId;
                        string roomId;
                        request.GetStart(out peerId, out roomId);

                        if (PeerId != peerId)
                        {
                            if (StartEvent != null)
                            {
                                StartEvent(request);
                            }

                            response = request;
                            nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.Quit)
                    {
                        int peerId;
                        request.GetQuit(out peerId);
                        Peer peer = Room.Members.Find(fpeer => fpeer.PeerId == peerId);
                        byte[] bufferRecv;

                        if (Room.Members.Count != 2)
                        {
                            if (PeerId == peerId)
                            {
                                Logger.WriteLine("Quit done");
                            }
                            else
                            {
                                int i = 0;
                                for (; i < Room.Members.Count; i++)
                                {// i adalah indeks peer yang ingin quit
                                    if (Room.Members[i].PeerId == peerId)
                                    {
                                        break;
                                    }
                                }

                                int j = 0;
                                for (; j < Room.Members.Count; j++)
                                {// j adalah indeks current peer
                                    if (Room.Members[j].PeerId == PeerId)
                                    {
                                        break;
                                    }
                                }

                                if (j == i + 1 || (j == 0 && i == Room.Members.Count - 1)) // yg mau quit adalah peer sebelum current peer
                                {
                                    quit = true;
                                }

                                response = request;

                                lock (nextPeerPaddle)
                                {
                                    nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                                    Logger.WriteLine("Forward Quit to " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address);
                                    bufferRecv = new byte[1024];
                                    nextPeerSocket.Receive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None);
                                }

                                if (i == j + 1 || (i == 0 && j == Room.Members.Count - 1))// yg mau quit adalah peer setelah current peer
                                {
                                    nextPeerSocket.Close();
                                    IPAddress ip = Room.Members[(i + 1) % (Room.Members.Count - 1)].IPAddress;
                                    IPEndPoint ipEndPoint = new IPEndPoint(ip, Room.Members[(i + 1) % (Room.Members.Count - 1)].ListeningPort);
                                    nextPeerSocket = new Socket(
                                        ip.AddressFamily,
                                        SocketType.Stream,
                                        ProtocolType.Tcp
                                        );

                                    nextPeerSocket.NoDelay = false;
                                    nextPeerSocket.Connect(ipEndPoint);

                                    nextPeerSocket = null;
                                }

                                if (this.PeerId == Room.Creator.PeerId || (peerId == Room.Creator.PeerId && Room.Members[1].PeerId == this.PeerId))
                                {
                                    lock (trackerPaddle)
                                    {
                                        response = Message.CreateMessageRemove(peerId, PeerId, Configuration.ListenPort, Room.RoomId);
                                        trackerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);

                                        bufferRecv = new byte[1024];
                                        trackerSocket.Receive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None);
                                    }
                                }

                                if (peerId == Room.Creator.PeerId)
                                {
                                    Room.Creator = Room.Members[1];
                                }

                                response = Message.CreateMessageSuccess();
                                handler.Send(response.data, 0, response.data.Length, SocketFlags.None);

                                bufferRecv = new byte[1024];
                                nextPeerSocket.Receive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            lock (trackerPaddle)
                            {
                                response = Message.CreateMessageRemove(peerId, PeerId, Configuration.ListenPort, Room.RoomId);
                                trackerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);

                                bufferRecv = new byte[1024];
                                trackerSocket.Receive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None);
                            }
                        }
                        Room.Members.Remove(peer);
                    }
                    else
                    {
                        if (GameEvent != null)
                        {
                            GameEvent(request);
                        }
                    }
                }

                if (!quit)
                {
                    IAsyncResult nextAsyncResult = handler.BeginReceive(
                        buffer,
                        0,
                        buffer.Length,
                        SocketFlags.None,
                        new AsyncCallback(ReceiveCallback),
                        obj
                    );

                    if (!nextAsyncResult.AsyncWaitHandle.WaitOne(Configuration.MaxTimeout))
                    {
                        Logger.WriteLine("Connection timeout for peer with IP Address " + (handler.LocalEndPoint as IPEndPoint).Address + ":" + (handler.LocalEndPoint as IPEndPoint).Port + " -> " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                        Logger.WriteLine("Closing connection " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                        handler.Close();
                    }
                }
            }
            catch (SocketException exc)
            {
                Logger.WriteLine(exc);
                if (exc.SocketErrorCode == SocketError.HostDown || exc.SocketErrorCode == SocketError.ConnectionReset)
                {
                    if (Room.Members.Count > 2)
                    {
                        int index = -1;
                        for (int i = 0; i < Room.Members.Count; i++)
                        {
                            Peer p = Room.Members[i];
                            if (p.IPAddress.Equals((handler.LocalEndPoint as IPEndPoint).Address))
                            {
                                index = i;
                            }
                        }

                        if (index == 0)
                        {
                            index = Room.Members.Count - 2;
                        }
                        else
                        {
                            index--;
                        }

                        Peer backPeer = Room.Members[index];
                        byte[] buffer = new byte[1024];
                        Message request = Message.CreateMessageQuit(backPeer.PeerId);
                        lock (nextPeerPaddle)
                        {
                            nextPeerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                            nextPeerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        }
                    }
                    else
                    {
                        // Remove last element
                        Room.Members.RemoveAt(1);
                        lock (nextPeerPaddle)
                        {
                            nextPeerSocket = null;
                        }
                    }
                }
            }

            Logger.WriteLine();
        }
        #endregion

        public void SEND_START(string str)
        {
            Message m = Message.CreateMessageStart(PeerId, str);
            lock (nextPeerPaddle)
            {
                nextPeerSocket.Send(m.data, 0, m.data.Length, SocketFlags.None);
            }
        }

        public void StartGame()
        {
            Message m = Message.CreateMessageStart(PeerId, Room.RoomId);
            lock (nextPeerPaddle)
            {
                nextPeerSocket.Send(m.data, 0, m.data.Length, SocketFlags.None);
            }
        }
    }
}
