using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using Gunbond;
using Gunbond_Tracker.Model;
using Gunbond_Tracker.Util;

namespace Gunbond_Tracker
{
    public class TrackerConfig
    {
        #region Properties
        public int MaxRoom
        {
            get;
            set;
        }

        public int MaxPeer
        {
            get;
            set;
        }

        public bool Log
        {
            get;
            set;
        }

        public int Backlog
        {
            get;
            set;
        }

        public int MaxTimeout
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string IpAddress
        {
            get;
            set;
        }
        #endregion

        public TrackerConfig()
        {
            // do nothing
        }

        public TrackerConfig(string filename)
        {
            LoadData(filename);
        }

        public void LoadData(string filename)
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
                                    case "MaxPeer":
                                        {
                                            MaxPeer = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "MaxRoom":
                                        {
                                            MaxRoom = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "Log":
                                        {
                                            string temp = reader.ReadString();
                                            Log = ("on".Equals(temp)) ? true : false;
                                            break;
                                        }
                                    case "Backlog":
                                        {
                                            Backlog = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "MaxTimeout":
                                        {
                                            MaxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "Port":
                                        {
                                            Port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "IpAddress":
                                        {
                                            IpAddress = reader.ReadString();
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
                // default configuration
                MaxPeer = 1000;
                MaxRoom = 100;
                Log = true;
                Backlog = 10000;
                MaxTimeout = 30000;
                Port = 9351;
                IpAddress = "127.0.0.1";
            }
        }

        public void SaveData(string filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("IpAddress", IpAddress.ToString());
            writer.WriteElementString("MaxPeer", MaxPeer.ToString());
            writer.WriteElementString("MaxRoom", MaxRoom.ToString());
            writer.WriteElementString("Log", (Log) ? "on" : "off");
            writer.WriteElementString("Backlog", Backlog.ToString());
            writer.WriteElementString("MaxTimeout", MaxTimeout.ToString());
            writer.WriteElementString("Port", Port.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        public void Print()
        {
            Logger.WriteLine("Current Settings:");
            Logger.WriteLine("Max Peer\t\t: " + MaxPeer);
            Logger.WriteLine("Max Room\t\t: " + MaxRoom);
            string log_state = (Log) ? "on" : "off";
            Logger.WriteLine("Log\t\t\t: " + log_state);
            Logger.WriteLine();
        }
    }

    public class Tracker
    {
        #region Properties
        public TrackerConfig Configuration
        {
            get;
            set;
        }
        #endregion

        private Random random = new Random();
        public Socket listener;
        public Dictionary<int, Peer> peers;
        public Dictionary<String, Room> rooms;

        public Tracker(string configfile)
        {
            Configuration = new TrackerConfig(configfile);
            Logger.Active = Configuration.Log;

            peers = new Dictionary<int, Peer>();
            rooms = new Dictionary<string, Room>();

            // Listening on socket
            IPAddress ipAddr = IPAddress.Parse(Configuration.IpAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Configuration.Port);
            SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", Configuration.Port);
            permission.Demand();

            try
            {
                listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(ipEndPoint);

                Console.WriteLine("Listening at IP " + ipEndPoint.Address + " and port " + ipEndPoint.Port + ".");

                listener.Listen(Configuration.Backlog);

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }
            catch (SocketException exc)
            {
                Logger.WriteLine(exc);
            }
        }

        public void AcceptCallback(IAsyncResult result)
        {
            try
            {
                Socket slistener = null;

                // new socket
                Socket handler = null;

                byte[] buffer = new byte[1024];
                slistener = (Socket)result.AsyncState;
                handler = slistener.EndAccept(result);
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
                    Logger.WriteLine("Connection timeout for peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address + ":" + (handler.RemoteEndPoint as IPEndPoint).Port);
                    handler.EndReceive(result);
                    handler.Close();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteLine(exc);
            }
        }

        public void ReceiveCallback(IAsyncResult target)
        {
            try
            {
                object[] obj = new object[2];
                obj = (object[]) target.AsyncState;

                byte[] data = (byte[]) obj[0];
                Socket handler = (Socket) obj[1];
                IPAddress tempIP = (handler.RemoteEndPoint as IPEndPoint).Address;

                string content = string.Empty;
                int bytesRead = handler.EndReceive(target);

                if (bytesRead > 0)
                {
                    Message request = new Message(data);
                    Message response;

                    Message.MessageType requestType = request.GetMessageType();
                    if (requestType == Message.MessageType.HandshakePeer)
                    {
                        #region Handshake
                        Logger.WriteLine("New peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address.ToString() + " request handshake.");

                        if (peers.Count < Configuration.MaxPeer)
                        {
                            int newPeerId = GenerateNewPeerId();
                            Peer peer = new Peer(newPeerId, (handler.RemoteEndPoint as IPEndPoint).Address);
                            peers.Add(newPeerId, peer);

                            Logger.WriteLine("Peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address.ToString() + " registered with ID " + newPeerId + ".");
                            Logger.WriteLine(peer);
                            Logger.WriteLine();

                            // send handshake response with ID
                            response = Message.CreateMessageHandshakeTracker(newPeerId);
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        else
                        {
                            // send handshake response with ID
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.KeepAlive)
                    {
                        #region Keep Alive

                        int peerId;
                        Peer peer;
                        request.GetKeepAlive(out peerId);
                        if (peers.TryGetValue(peerId, out peer))
                        {
                            Logger.WriteLine("Keep alive is sent by Peer: " + peer);
                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.CreateRoom)
                    {
                        #region Create Room
                        int peerId, maxPlayers;
                        string roomId;
                        request.GetCreate(out peerId, out maxPlayers, out roomId);

                        Peer peer;
                        if (peers.TryGetValue(peerId, out peer))
                        {
                            Room room;
                            if (!rooms.TryGetValue(roomId, out room))
                            {
                                room = new Room(roomId, peer, maxPlayers);
                                rooms.Add(roomId, room);
                                Logger.WriteLine("Create room request is sent by peer " + peer + " for room " + room);

                                // send reply message SUCCESS
                                response = Message.CreateMessageSuccess();
                                handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                            else
                            {
                                // send message FAIL that room doesn't exist
                                response = Message.CreateMessageFailed();
                                handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.ListRoom)
                    {
                        #region List Room
                        int peerId;
                        request.GetList(out peerId);

                        Peer peer;
                        if (peers.TryGetValue(peerId, out peer) && !peer.InRoom)
                        {
                            Logger.WriteLine("List room is sent to Peer " + peer);
                            List<Message.MessageRoomBody> listRooms = new List<Message.MessageRoomBody>();
                            foreach (var r in rooms.Values)
                            {
                                Message.MessageRoomBody mb;
                                mb.roomId = r.Id;
                                mb.maxPlayers = r.MaxPlayers;
                                mb.currentPlayer = r.Members.Count;
                                listRooms.Add(mb);
                            }

                            // send ROOM message
                            response = Message.CreateMessageRoom(listRooms);
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.Join)
                    {
                        #region Join Room
                        int peerId;
                        string roomId;
                        request.GetJoin(out peerId, out roomId);
                        Peer peer;
                        if (peers.TryGetValue(peerId, out peer) && !peer.InRoom)
                        {
                            Room room;
                            if (rooms.TryGetValue(roomId, out room) && room.Members.Count + 1 < room.MaxPlayers)
                            {
                                Logger.WriteLine("Join room request is sent by Peer " + peer + " for Room " + room);

                                // send IP address back to peer
                                response = Message.CreateMessageCreatorInfo(room.Creator.IpAddress.ToString(), 0);
                                handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                            else
                            {
                                // send message FAIL that peer is not registered
                                response = Message.CreateMessageFailed();
                                handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.Start)
                    {
                        #region Start
                        int peerId;
                        request.GetList(out peerId);
                        Peer peer;
                        if (peers.TryGetValue(peerId, out peer) && !peer.InRoom)
                        {

                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion
                    }
                    else if (requestType == Message.MessageType.Quit)
                    {
                        #region Quit
                        int peerId;
                        request.GetList(out peerId);
                        Peer peer;
                        if (peers.TryGetValue(peerId, out peer) && !peer.InRoom)
                        {

                        }
                        else
                        {
                            // send message FAIL that peer is not registered
                            response = Message.CreateMessageFailed();
                            handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        }
                        #endregion

                        //}
                        //else if (requestType == Message.MessageType.Add)
                        //{
                        //    #region Add
                        //    int peerId;
                        //    request.GetList(out peerId);
                        //    Peer peer;
                        //    if (peers.TryGetValue(peerId, out peer) && !peer.InRoom)
                        //    {
                        //        // ...
                        //    }
                        //    else
                        //    {
                        //        // send message FAIL that peer is not registered
                        //        response = Message.CreateMessageFailed();
                        //        handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                        //    }
                        //    #endregion
                    }
                    else
                    {
                        Logger.WriteLine("Unknown message received from " + (handler.RemoteEndPoint as IPEndPoint));
                        // throw message failed
                        response = Message.CreateMessageFailed();
                        handler.Send(response.data, 0, response.data.Length, SocketFlags.None);
                    }

                    #region Receive for Next Message
                    byte[] buffer = new byte[1024];
                    obj = new object[2];
                    obj[0] = buffer;
                    obj[1] = handler;
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

                        // find peer id
                        int peerId = 0;
                        foreach (KeyValuePair<int, Peer> entry in peers)
                        {
                            if (entry.Value.IpAddress.Equals((handler.RemoteEndPoint as IPEndPoint).Address))
                            {
                                peerId = entry.Key;
                                break;
                            }
                        }
                        peers.Remove(peerId);

                        handler.Close();
                    }
                    #endregion
                }

            }
            catch (SocketException exc)
            {
                Logger.WriteLine(exc);
            }
        }

        private int GenerateNewPeerId()
        {
            int newId = random.Next(0, int.MaxValue);
            Peer peer;
            while (peers.TryGetValue(newId, out peer))
            {
                newId = random.Next(0, int.MaxValue);
            }
            return newId;
        }
    }
}
