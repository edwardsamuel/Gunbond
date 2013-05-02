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
                                    case "tracker_address":
                                        {
                                            this.TrackerAddress = reader.ReadString();
                                            break;
                                        }
                                    case "max_timeout":
                                        {
                                            this.MaxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "port":
                                        {
                                            this.Port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "listen_port":
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
            writer.WriteElementString("tracker_address", this.TrackerAddress);
            writer.WriteElementString("max_timeout", this.MaxTimeout.ToString());
            writer.WriteElementString("port", this.Port.ToString());
            writer.WriteElementString("listen_port", this.ListenPort.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }
    };

    public class GunConsole
    {

        #region Attributes and Properties
        private Socket trackerSocket;
        private Socket listenerSocket;
        private Socket nextPeerSocket;

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
                        IPEndPoint ipEndPoint = new IPEndPoint(trackerAddr, Configuration.Port);
                        trackerSocket = new Socket(
                            trackerAddr.AddressFamily,
                            SocketType.Stream,
                            ProtocolType.Tcp
                           );

                        trackerSocket.NoDelay = false;
                        trackerSocket.Connect(ipEndPoint);
                        Message messageOut = Message.CreateMessageHandshakePeer();

                        byte[] buffer = new byte[1024];

                        trackerSocket.Send(messageOut.data, 0, messageOut.data.Length, SocketFlags.None);
                        trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);

                        Message messageIn = new Message(buffer);
                        if (messageIn.GetMessageType() == Message.MessageType.HandshakeTracker)
                        {
                            int peerId;
                            messageIn.GetHandshakeTracker(out peerId);
                            PeerId = peerId;
                            IsConnected = true;
                            Logger.WriteLine("Connection to tracker is successfully established. PeerID: " + PeerId);

                            IPAddress ipAddr = (trackerSocket.LocalEndPoint as IPEndPoint).Address;
                            IPEndPoint ipEndPointListener = new IPEndPoint(ipAddr, Configuration.ListenPort);
                            SocketPermission permissionListener = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", Configuration.ListenPort);

                            listenerSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
                    trackerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                    trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);

                    Message response = new Message(buffer);
                    Message.MessageType responseType = response.GetMessageType();
                    if (responseType == Message.MessageType.Success)
                    {
                        Logger.WriteLine("Request create room success");

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
                    Logger.WriteLine("Requesting to join room");
                    Message request = Message.CreateMessageJoin(PeerId, roomId, Configuration.ListenPort);
                    byte[] buffer = new byte[1024];
                    trackerSocket.Send(request.data, 0, request.data.Length, SocketFlags.None);
                    trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);

                    Message response = new Message(buffer);
                    Message.MessageType responseType = response.GetMessageType();
                    if (responseType == Message.MessageType.CreatorInfo)
                    {
                        IPAddress ip;
                        int port;
                        response.GetCreatorInfo(out ip, out port);

                        Logger.WriteLine("GunbondPeer (Peer - Tracker): Creator Info");
                        Logger.WriteLine("Hostname: " + ip);
                        Logger.WriteLine("Port    : " + port);

                        SocketPermission permission = new SocketPermission(
                             NetworkAccess.Connect,
                             TransportType.Tcp,
                             "",
                             SocketPermission.AllPorts
                             );
                        permission.Demand();

                        IPEndPoint ipEndPoint = new IPEndPoint(ip, Configuration.ListenPort);
                        nextPeerSocket = new Socket(
                            ip.AddressFamily,
                            SocketType.Stream,
                            ProtocolType.Tcp
                            );

                        nextPeerSocket.NoDelay = false;
                        nextPeerSocket.Connect(ipEndPoint);
                        Message messageConnectToCreator = Message.CreateMessageHandshakeTracker(PeerId);

                        byte[] bufferFromCreator = new byte[1024];

                        nextPeerSocket.Send(messageConnectToCreator.data, 0, messageConnectToCreator.data.Length, SocketFlags.None);
                        nextPeerSocket.Receive(bufferFromCreator, bufferFromCreator.Length, SocketFlags.None);

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

        #region Keep Alive Thread
        private void SendAliveTracker()
        {
            //try
            //{
            //    while (true)
            //    {
            //        Message mes = Message.CreateMessageKeepAlive(PeerId);
            //        byte[] buffer = new byte[1024];

            //        trackerSocket.Send(mes.data, 0, mes.data.Length, SocketFlags.None);
            //        trackerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            //        Message response = new Message(buffer);
            //        if (response.GetMessageType() == Message.MessageType.KeepAlive)
            //        {
            //            Logger.WriteLine("KeepAlive success.");
            //            Thread.Sleep(Configuration.MaxTimeout / 2);
            //        }
            //        else
            //        {
            //            Logger.WriteLine("Message KeepAlive has not been received.");
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    if (trackerSocket == null)
            //    {
            //        Logger.WriteLine("Connection terminated");
            //        IsConnected = false;
            //    }
            //}
        }

        private void SendAliveNextPeer()
        {
            //try
            //{
            //    while (true)
            //    {
            //        if (nextPeerSocket == null || !nextPeerSocket.Connected)
            //        {
            //            Thread.Sleep(Configuration.MaxTimeout / 10);
            //            continue;
            //        }

            //        Message mes = Message.CreateMessageKeepAlive(PeerId);
            //        byte[] buffer = new byte[1024];

            //        nextPeerSocket.Send(mes.data, 0, mes.data.Length, SocketFlags.None);
            //        nextPeerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            //        Message response = new Message(buffer);
            //        if (response.GetMessageType() == Message.MessageType.KeepAlive)
            //        {
            //            Logger.WriteLine("KeepAlive to next peer success.");
            //            Thread.Sleep(Configuration.MaxTimeout / 10);
            //        }
            //        else
            //        {
            //            Logger.WriteLine("Message KeepAlive from next peer has not been received.");
            //            nextPeerSocket.Close();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Logger.WriteLine(e.Message);
            //}
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
            Socket slistener = null;

            // new socket
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

                Logger.WriteLine("Connection accepted from IPAddress " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);

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
                    Logger.WriteLine("Connection timeout for peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address);
                    handler.EndReceive(target);
                    handler.Close();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
            }
        }

        private void ReceiveCallback(IAsyncResult target)
        {
            try
            {
                object[] obj = new object[2];
                obj = (object[])target.AsyncState;
                byte[] buffer = (byte[])obj[0];
                Socket handler = (Socket)obj[1];

                bool quit = false;

                int bytesRead = handler.EndReceive(target);
                if (bytesRead > 0)
                {
                    Message request = new Message(buffer);
                    Message response;

                    Message.MessageType requestType = request.GetMessageType();
                    if (requestType == Message.MessageType.HandshakePeerCreator) // Peer client mau join ke room ini
                    {
                        #region Joining Room (Handshake)
                        if (Room.Members.Count < Room.MaxPlayer)
                        {
                            int newPeerId, newPeerListenPort;
                            IPAddress newPeerIp = (handler.RemoteEndPoint as IPEndPoint).Address;
                            request.GetHandshakePeerCreator(out newPeerId, out newPeerListenPort);
                            Room.Members.Add(new Peer(newPeerId, newPeerIp, newPeerListenPort));

                            // Send SUCCESS message
                            response = Message.CreateMessageSuccess();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);

                            if (nextPeerSocket != null)
                            {
                                // Send NewMember message to next peer
                                response = Message.CreateMessageNewMember(newPeerId, (handler.RemoteEndPoint as IPEndPoint).Address, newPeerListenPort);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                            else
                            {
                                IPEndPoint ipEndPoint = new IPEndPoint(newPeerIp, Configuration.ListenPort);
                                nextPeerSocket = new Socket(
                                    newPeerIp.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp
                                   );

                                nextPeerSocket.NoDelay = false;
                                nextPeerSocket.Connect(ipEndPoint);

                                Logger.WriteLine("Your next has been changed to " + newPeerIp);

                                response = Message.CreateMessageRoomModel(Room);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);

                                Logger.WriteLine("Send room info....");
                            }
                        }
                        else
                        {
                            // Send FAILED message akibat ruang penuh
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.KeepAlive)
                    {
                        #region Keep Alive
                        //int peerId;
                        //request.GetKeepAlive(out peerId);
                        //if (backPeer != null && backPeer.PeerId == peerId)
                        //{
                        //    //Send KeepAlive Message back
                        //    response = request;
                        //    handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        //}
                        #endregion
                    }
                    else if (requestType == Message.MessageType.NewMember)
                    {
                        #region NewMember
                        if (IsCreator)
                        {
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

                            Logger.WriteLine("Tell new candidate to next peer");
                            response = request;
                            nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);

                            if (isLast)
                            {
                                IPEndPoint ipEndPoint = new IPEndPoint(newPeerIp, Configuration.ListenPort);
                                nextPeerSocket.Close();

                                nextPeerSocket = new Socket(
                                    newPeerIp.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp
                                   );

                                nextPeerSocket.NoDelay = false;
                                nextPeerSocket.Connect(ipEndPoint);

                                Logger.WriteLine("Your next has been changed to " + newPeerIp);

                                response = Message.CreateMessageRoomModel(Room);
                                nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.RoomModel)
                    {
                        #region Room
                        Room room;
                        request.GetRoomModel(out room);
                        Room = room;
                        #endregion
                    }
                    else if (requestType == Message.MessageType.Start)
                    {
                        int peerId;
                        string roomId;
                        request.GetStart(out peerId, out roomId);

                        Logger.WriteLine("Received START message with PeerId : " + peerId + " RoomId : " + roomId);

                        if (peerId == PeerId)
                        {
                            // do nothing
                            Logger.WriteLine("FINISHED");
                        }
                        else
                        {
                            // Send NewMember message to next peer
                            response = request;
                            nextPeerSocket.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            Logger.WriteLine("Forward START to " + (nextPeerSocket.RemoteEndPoint as IPEndPoint).Address);
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
                        Logger.WriteLine("Connection timeout for peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address);
                        handler.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
            }
        }
        #endregion

        public void SEND_START(string str)
        {
            Message m = Message.CreateMessageStart(PeerId, str);
            nextPeerSocket.Send(m.data, 0, m.data.Length, SocketFlags.None);
        }
    }
}
