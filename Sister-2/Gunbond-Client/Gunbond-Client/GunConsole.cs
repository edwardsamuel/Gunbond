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
    public class Config
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
    };

    public class GunConsole
    {

        #region Attribute and Getter Setter
        private Config conf = new Config();
        private Socket trackerSocket;
        private Socket listener;
        private Socket connector;
        private int peerId;

        private List<Room> rooms;
        public List<Room> Rooms
        {
            get { return rooms; }
            set { rooms = value; }
        }

        private List<Peer> peers;
        public List<Peer> Peers
        {
            get { return peers; }
            set { peers = value; }
        }
        private Thread keepAlive;
        private Thread listenToRoom;
        private Thread keepAliveRoom;

        private Room current_room;
        private bool inRoom;
        private bool isCreator;

        private bool isConnected;




        #endregion

        public GunConsole(string fileName)
        {
            trackerSocket = null;
            listener = null;

            current_room = null;
            isConnected = false;
            inRoom = false;
            isCreator = false;

            rooms = new List<Room>();

            keepAlive = null;
            listenToRoom = null;
            keepAliveRoom = null;



            LoadData(fileName);
        }

        public bool connect()
        {
            try
            {
                if (!isConnected)
                {
                    SocketPermission permission = new SocketPermission(
                        NetworkAccess.Connect,
                        TransportType.Tcp,
                        "",
                        SocketPermission.AllPorts
                        );

                    permission.Demand();

                    IPAddress trackerAddr;
                    if (IPAddress.TryParse(conf.TrackerAddress, out trackerAddr))
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(trackerAddr, conf.Port);
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

                            messageIn.GetHandshakeTracker(out peerId);
                            Logger.WriteLine("Connection to tracker is successfully established. PeerID: " + peerId);
                            isConnected = true;

                            keepAlive = new Thread(new ThreadStart(send_alive));
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

        public bool create(string roomId, int maxPlayers)
        {
            try
            {
                if (isConnected && !inRoom)
                {
                    byte[] buffer = new byte[1024];
                    Logger.WriteLine("Requesting to create room");
                    Message messageOut = Message.CreateMessageCreate(peerId, maxPlayers, roomId);
                    trackerSocket.Send(messageOut.data, 0, messageOut.data.Length, SocketFlags.None);
                    trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    Message messageIn = new Message(buffer);
                    if (messageIn.GetMessageType() == Message.MessageType.Success)
                    {
                        Logger.WriteLine("Request create room success");

                        current_room = new Room(roomId, maxPlayers);
                        inRoom = true;
                        isCreator = true;

                        IPAddress ipAddr = (trackerSocket.LocalEndPoint as IPEndPoint).Address;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, conf.ListenPort);
                        SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", conf.ListenPort);

                        listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(ipEndPoint);

                        listenToRoom = new Thread(new ParameterizedThreadStart(ListenRoom));
                        listenToRoom.Start(maxPlayers);

                        return true;
                    }
                    else if (messageIn.GetMessageType() == Message.MessageType.Failed)
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
                else if (!isConnected)
                {
                    Logger.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (inRoom)
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

        public void list()
        {
            Logger.WriteLine("Requesting to list room");
            
            Message messageOut = Message.CreateMessageList(peerId);
            byte[] buffer = new byte[1024];

            trackerSocket.Send(messageOut.data, 0, messageOut.data.Length, SocketFlags.None);
            trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
            Message messageIn = new Message(buffer);
            if (messageIn.GetMessageType() == Message.MessageType.Room)
            {
                List<Message.MessageRoomBody> s;
                messageIn.GetRoom(out s);
                foreach (var a in s)
                {
                    Logger.WriteLine("ROOM: " + a);
                }
            }
             /*try
             {
                 if (isConnected)
                 {
                     if (inRoom)
                     {
                         if (isCreator)
                         {
                             Logger.WriteLine("Peer list: ");
                             for (int i = 0; i < peers.Count; ++i)
                             {
                                Logger.WriteLine("- Peer " + (i + 1) + " " + peers[i].ToString());
                             }
                         }
                         else
                         {
                             Message messageListOut = Message.CreateMessageList(peerId);
                             byte[] bufferList = new byte[1024];

                             trackerSocket.Send(messageListOut.data, 0, messageListOut.data.Length, SocketFlags.None);
                             trackerSocket.Receive(bufferList, bufferList.Length, SocketFlags.None);

                             Message messageListIn = new Message(bufferList);
                             if (messageIn.GetMessageType() == Message.MessageType.Room)
                             {
                                 messageIn.getr
                             }
                         }
                        
                     }
                     else
                     {
                         Logger.WriteLine("Requesting to list room");
                         Message messageOut = Message.CreateMessageList(peerId);
                         byte[] buffer = new byte[1024];

                         trackerSocket.Send(messageOut.data, 0, messageOut.data.Length, SocketFlags.None);
                         trackerSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                         Message messageIn = new Message(buffer);
                         if (messageIn.GetMessageType() == Message.MessageType.Room)
                         {
                              hash room
                             int pos = 20;
                             byte total_room_digit = buffer[pos];
                             pos++;

                             long room_count = 0;
                             for (int i = 0; i < total_room_digit; ++i)
                             {
                                 room_count = room_count << 256;
                                 room_count += buffer[pos];
                                 pos++;
                             }

                             rooms.Clear();
                             int n = 0;
                             while (n < room_count)
                             {
                                 while ((n < room_count) && (pos + 52 < 1024))
                                 {
                                     byte[] temp = new byte[52];
                                     Buffer.BlockCopy(buffer, pos, temp, 0, 52);
                                     rooms.Add(new Room(temp));
                                     pos += 52;
                                     n++;
                                 }
                                 if (n < room_count)
                                 {
                                     Array.Clear(buffer, 0, buffer.Length);
                                     track_socket.Receive(buffer, buffer.Length, SocketFlags.None);
                                 }
                             }

                             Console.WriteLine("List of room: ");

                             StringBuilder sb = new StringBuilder();
                             n = 0;
                             while (n < room_count)//menuliskan string dari setiap room
                             {
                                 sb.AppendLine("Room ke-" + (n + 1) + ": ");

                                 sb.AppendLine(rooms[n].ToLanguageString());
                                 sb.AppendLine("");
                                 n++;
                             }
                             Console.WriteLine(sb.ToString());
                         }
                     }
                 }*/
             }
        

        private void ListenRoom(Object obj)
        {
            try
            {
                Logger.WriteLine("ListenToRoom thread has just been created.");
                listener.Listen((int)obj);

                //  AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                //listener.BeginAccept(aCallback, listener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void send_alive()
        {
            try
            {
                while (true)
                {
                    Message mes = Message.CreateMessageKeepAlive(peerId);
                    trackerSocket.Send(mes.data, 0, mes.data.Length, SocketFlags.None);
                    Thread.Sleep(conf.MaxTimeout / 10);
                }
            }
            catch (Exception)
            {
                if (trackerSocket == null)
                {
                    Logger.WriteLine("Connection terminated");
                    isConnected = false;
                }
            }
        }

        #region Configuration Data
        private void LoadData(String xml)
        {
            if (File.Exists(xml))
            {
                XmlTextReader reader = new XmlTextReader(xml);
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
                                            conf.TrackerAddress = reader.ReadString();
                                            break;
                                        }
                                    case "max_timeout":
                                        {
                                            conf.MaxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "port":
                                        {
                                            conf.Port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "listen_port":
                                        {
                                            conf.ListenPort = Int32.Parse(reader.ReadString());
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
                conf.TrackerAddress = "127.0.0.1";
                conf.MaxTimeout = 30000;
                conf.Port = 9351;
                conf.ListenPort = 9757;
                this.SaveData(xml);
            }
        }
        private void SaveData(String xml)
        {
            XmlWriter writer = XmlWriter.Create(xml);
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");
            writer.WriteElementString("tracker_address", conf.TrackerAddress);
            writer.WriteElementString("max_timeout", conf.MaxTimeout.ToString());
            writer.WriteElementString("port", conf.Port.ToString());
            writer.WriteElementString("listen_port", conf.ListenPort.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }
        #endregion

    }
}
