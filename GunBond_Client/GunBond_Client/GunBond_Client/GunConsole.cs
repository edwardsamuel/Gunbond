using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

using GunBond_Client.Model;

namespace GunBond_Client
{
    public struct Config
    {
        public string tracker_address;
        public int port;
        public int listen_port;
        public int max_timeout;
    };
    public class GunConsole
    {
        #region Attribute and Getter Setter
            private Config conf;
            private string conf_filename = "peer_conf.xml";
            private Socket track_socket;
            private Socket listener;
            private Socket connector;
            private byte[] peer_id;
            private List<Room> rooms;
            private List<Peer> peers;

            private Thread keep_alive;
            private Thread keep_alive_room;
            private Thread listen_to_room;

            private Room current_room;
            private bool in_room;
            private bool is_creator;
            private bool is_connected;

            public List<Room> ROOMS
            {
                get { return rooms; }
                set { rooms = value; }
            }

            public List<Peer> PEERS
            {
                get { return peers; }
                set { peers = value; }
            }

            public byte[] PEERS_ID
            {
                get { return peer_id; }
                set { peer_id = value; }
            }
        #endregion

        public GunConsole()
        {
            track_socket = null;
            listener = null;
            connector = null;

            current_room = null;
            peer_id = new byte[4];
            is_connected = false;
            in_room = false;
            is_creator = false;

            rooms = new List<Room>();
            peers = new List<Peer>();

            keep_alive = null;
            keep_alive_room = null;
            listen_to_room = null;

            LoadData(conf_filename);
        }

        public bool connect()
        {
            try
            {
                if (!is_connected)
                {
                    SocketPermission permission = new SocketPermission(
                        NetworkAccess.Connect,
                        TransportType.Tcp,
                        "",
                        SocketPermission.AllPorts
                        );

                    permission.Demand();

                    IPAddress server_addr;
                    if (IPAddress.TryParse(conf.tracker_address, out server_addr))
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(server_addr, conf.port);
                        track_socket = new Socket(
                            server_addr.AddressFamily,
                            SocketType.Stream,
                            ProtocolType.Tcp
                           );

                        track_socket.NoDelay = false;
                        track_socket.Connect(ipEndPoint);

                        byte[] buffer = new byte[1024];

                        track_socket.Send(Constant.msg_handshake, 0, Constant.msg_handshake.Length, SocketFlags.None);
                        track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                        int result = Constant.check_message(buffer);
                        if (result == Constant.MSG_HANDSHAKE)
                        {
                            // handshake result approved
                            Buffer.BlockCopy(buffer, 20, peer_id, 0, 4);
                            String s = (int)peer_id[0] + "." + (int)peer_id[1] + "." + (int)peer_id[2] + "." + (int)peer_id[3];
                            Console.WriteLine("Connection with tracker is successfully established, get peer id " + s + ".");

                            is_connected = true;

                            keep_alive = new Thread(new ThreadStart(send_alive));
                            keep_alive.Start();
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Failed to connect to tracker.");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect, tracker not found.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Already connected to tracker.");
                    return false;
                }
            }
            catch (SocketException exc)
            {
                if (exc.ErrorCode.Equals(SocketError.AddressNotAvailable))
                {
                    Console.WriteLine("Try Connecting to local");
                    IPAddress server_addr = IPAddress.Parse("127.0.0.1");
                    IPEndPoint ipEndPoint = new IPEndPoint(server_addr, conf.port);
                    track_socket = new Socket(
                        server_addr.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp
                       );

                    track_socket.NoDelay = false;
                    track_socket.Connect(ipEndPoint);

                    byte[] buffer = new byte[1024];

                    track_socket.Send(Constant.msg_handshake, 0, Constant.msg_handshake.Length, SocketFlags.None);
                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_HANDSHAKE)
                    {
                        // hash handshake
                        Buffer.BlockCopy(buffer, 20, peer_id, 0, 4);
                        String s = (int)peer_id[0] + "." + (int)peer_id[1] + "." + (int)peer_id[2] + "." + (int)peer_id[3];
                        Console.WriteLine("Connection with tracker is successfully established, get peer id " + s + ".");

                        is_connected = true;

                        keep_alive = new Thread(new ThreadStart(send_alive));
                        keep_alive.Start();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to tracker.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine(exc.ToString());
                    return false;
                }
            }
        }

        public void create()
        {
            try
            {
                if ((is_connected) && (!in_room))
                {
                    Console.WriteLine("Requesting to create room");

                    byte[] msg = new byte[75];
                    Constant.msg_create.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    bool checkinput = false;
                    byte max_player_num = 0;
                    while (!checkinput) //getting max player in the room
                    {
                        Console.Write("Berapa jumlah player dalam room (2/4/6/8) :");
                        string input = Console.ReadLine();
                        int x;
                        if (Int32.TryParse(input, out x))
                        {
                            if ((x == 2) || (x == 4) || (x == 6) || (x == 8))
                            {
                                max_player_num = (byte)x;
                                checkinput = true;
                            }
                            else
                            {
                                Console.WriteLine("Masukan anda salah.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Masukan anda salah.");
                        }
                    }
                    msg[24] = max_player_num;

                    byte[] room_id = new byte[50];
                    int i = 0;
                    Console.Write("Nama dari room anda (maks 50 karakter) :");
                    string input2 = Console.ReadLine();
                    while (input2.Length >= 50)
                    {
                        Console.WriteLine("Nama room maksimal terdiri dari 50 karakter.");
                        Console.Write("Nama dari room anda (maks 50 karakter) :");
                        input2 = Console.ReadLine();
                    }

                    while (i < 50 && i < input2.Length)
                    {
                        room_id[i] = (byte)input2[i];
                        i++;
                    }
                    room_id.CopyTo(msg, 25);
                    byte[] buffer = new byte[1024];

                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_SUCCESS)
                    {
                        Console.WriteLine("Request create room success");
                        byte[] room = new byte[52];
                        room_id.CopyTo(room, 0);
                        room[50] = 0;
                        room[51] = max_player_num;

                        current_room = new Room(room);

                        in_room = true;
                        is_creator = true;

                        IPAddress ipAddr = (track_socket.LocalEndPoint as IPEndPoint).Address;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, conf.listen_port);
                        SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", conf.listen_port);

                        listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(ipEndPoint);

                        listen_to_room = new Thread(new ParameterizedThreadStart(listen_room));
                        listen_to_room.Start(max_player_num);
                        listen_to_room.Name = "Listen to room";

                    }
                    else if (result == Constant.MSG_FAILED)
                    {
                        Console.WriteLine("Request create room failed.");
                    }
                    else
                    {
                        Console.WriteLine("Response unrecognized.");
                    }
                }
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                }
                else if (in_room)
                {
                    Console.WriteLine("Currently in room, quit room first.");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public bool create(string room_name, int max_player)
        {
            try
            {
                if ((is_connected) && (!in_room))
                {
                    Console.WriteLine("Requesting to create room");

                    byte[] msg = new byte[75];
                    Constant.msg_create.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    msg[24] = (byte)max_player;

                    byte[] room_id = new byte[50];
                    int i = 0;

                    while (i < 50 && i < room_name.Length)
                    {
                        room_id[i] = (byte)room_name[i];
                        i++;
                    }
                    room_id.CopyTo(msg, 25);

                    byte[] buffer = new byte[1024];
                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);
                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_SUCCESS)
                    {
                        Console.WriteLine("Request create room success");
                        byte[] room = new byte[52];
                        room_id.CopyTo(room, 0);
                        room[50] = 0;
                        room[51] = (byte)max_player;

                        current_room = new Room(room);

                        in_room = true;
                        is_creator = true;

                        IPAddress ipAddr = (track_socket.LocalEndPoint as IPEndPoint).Address;
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, conf.listen_port);
                        SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", conf.listen_port);

                        listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(ipEndPoint);

                        listen_to_room = new Thread(new ParameterizedThreadStart(listen_room));
                        listen_to_room.Start((byte)max_player);

                        return true;
                    }
                    else if (result == Constant.MSG_FAILED)
                    {
                        Console.WriteLine("Request create room failed.");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Response unrecognized.");
                        return false;
                    }
                }
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (in_room)
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

        public void list()
        {
            try
            {
                if (is_connected)
                {
                    if (in_room)
                    {
                        if (is_creator)
                        {
                            Console.WriteLine("Peer list:");
                            for (int i = 0; i < peers.Count; ++i)
                            {
                                Console.WriteLine("- Peer " + (i + 1) + " " + peers[i].ToString());
                            }
                        }
                        else
                        {
                            byte[] msg = new byte[24];
                            Constant.msg_list.CopyTo(msg, 0);
                            peer_id.CopyTo(msg, 20);

                            byte[] buffer = new byte[1024];

                            connector.Send(msg, msg.Length, SocketFlags.None);
                            connector.Receive(buffer, buffer.Length, SocketFlags.None);

                            int result = Constant.check_message(buffer);
                            if (result == Constant.MSG_ROOM)
                            {
                                int pos = 20;
                                byte total_peer = buffer[pos];
                                pos++;

                                peers.Clear();
                                int n = 0;
                                while (n < total_peer)
                                {
                                    byte[] temp = new byte[8];
                                    Buffer.BlockCopy(buffer, pos, temp, 0, 8);
                                    peers.Add(new Peer(temp));
                                    pos += 8;
                                    n++;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Request list pair failed.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Requesting to list room");

                        byte[] msg = new byte[24];
                        Constant.msg_list.CopyTo(msg, 0);
                        peer_id.CopyTo(msg, 20);

                        byte[] buffer = new byte[1024];

                        track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                        track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                        int result = Constant.check_message(buffer);
                        if (result == Constant.MSG_ROOM)
                        {
                            // hash room
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
                        else if (result == Constant.MSG_FAILED)
                        {
                            Console.WriteLine("Request list room failed.");
                        }
                        else
                        {
                            Console.WriteLine("Response unrecognized.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public void join()
        {
            try
            {
                if ((is_connected) && (!in_room))
                {
                    Console.WriteLine("Requesting to join room");

                    byte[] msg = new byte[74];
                    Constant.msg_join.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    bool checkinput = false;
                    int n = 0;
                    while (!checkinput)
                    {
                        Console.WriteLine("Pilihan room :");
                        n = 0;
                        StringBuilder sb = new StringBuilder();
                        while (n < rooms.Count)//menuliskan string dari setiap room
                        {
                            Console.WriteLine((n + 1) + ". " + rooms[n].ToLanguageString());
                            n++;
                        }
                        n--;
                        Console.Write("Room to join (nomor room) : ");

                        string input = Console.ReadLine();
                        int chosen;
                        Int32.TryParse(input, out chosen);
                        chosen--;
                        if (chosen >= 0 && chosen < rooms.Count)
                        {
                            rooms[chosen].ID.CopyTo(msg, 24);
                            checkinput = true;
                        }
                        else
                        {
                            Console.WriteLine("Room ada tidak ada ");
                        }
                    }

                    byte[] buffer = new byte[1024];

                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_JOIN)
                    {
                        Console.WriteLine("IP Address of room creator accepted.");
                        byte[] peer_ip = new byte[4];
                        Buffer.BlockCopy(buffer, 20, peer_ip, 0, 4);

                        StringBuilder Peer_Ip = new StringBuilder();
                        for (int i = 0; i < peer_ip.Length; ++i)
                        {
                            Peer_Ip.Append(peer_ip[i] + ".");
                        }
                        Peer_Ip.Remove(Peer_Ip.Length - 1, 1);

                        SocketPermission permission = new SocketPermission(
                            NetworkAccess.Connect,
                            TransportType.Tcp,
                            "",
                            SocketPermission.AllPorts
                            );

                        permission.Demand();

                        IPAddress server_addr;
                        if (IPAddress.TryParse(Peer_Ip.ToString(), out server_addr))
                        {
                            IPEndPoint ipEndPoint = new IPEndPoint(server_addr, conf.listen_port);
                            connector = new Socket(
                                server_addr.AddressFamily,
                                SocketType.Stream,
                                ProtocolType.Tcp
                               );

                            connector.NoDelay = false;
                            connector.Connect(ipEndPoint);

                            Array.Clear(buffer, 0, buffer.Length);

                            byte[] msg_add = new byte[24];
                            Constant.msg_handshake.CopyTo(msg_add, 0);
                            peer_id.CopyTo(msg_add, 20);

                            connector.Send(msg_add, 0, msg_add.Length, SocketFlags.None);
                            connector.Receive(buffer, buffer.Length, SocketFlags.None);

                            result = Constant.check_message(buffer);
                            if (result == Constant.MSG_SUCCESS)
                            {
                                // hash handshake
                                in_room = true;

                                keep_alive_room = new Thread(new ThreadStart(send_alive_room));
                                keep_alive_room.Start();

                                Console.WriteLine("Successfully joined room.");
                            }
                            else
                            {
                                Console.WriteLine("Request join room failed.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cannot connect to room creator");
                        }

                        // connecting to peer_ip
                        in_room = true;

                        current_room = rooms[n];
                        is_creator = false;
                    }
                    else if (result == Constant.MSG_FAILED)
                    {
                        Console.WriteLine("Request join room failed.");
                    }
                    else
                    {
                        Console.WriteLine("Response unrecognized.");
                    }
                }
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                }
                else if (in_room)
                {
                    Console.WriteLine("Currently in room, quit room first.");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public bool join(int room)
        {
            try
            {
                if ((is_connected) && (!in_room))
                {
                    Console.WriteLine("Requesting to join room");

                    byte[] msg = new byte[74];
                    Constant.msg_join.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    int n = 0;
                    int chosen = room;
                    rooms[chosen].ID.CopyTo(msg, 24);

                    byte[] buffer = new byte[1024];

                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_JOIN)
                    {
                        Console.WriteLine("IP Address of room creator accepted.");
                        byte[] peer_ip = new byte[4];
                        Buffer.BlockCopy(buffer, 20, peer_ip, 0, 4);

                        StringBuilder Peer_Ip = new StringBuilder();
                        for (int i = 0; i < peer_ip.Length; ++i)
                        {
                            Peer_Ip.Append(peer_ip[i] + ".");
                        }
                        Peer_Ip.Remove(Peer_Ip.Length - 1, 1);

                        SocketPermission permission = new SocketPermission(
                            NetworkAccess.Connect,
                            TransportType.Tcp,
                            "",
                            SocketPermission.AllPorts
                            );

                        permission.Demand();

                        IPAddress server_addr;
                        if (IPAddress.TryParse(Peer_Ip.ToString(), out server_addr))
                        {
                            IPEndPoint ipEndPoint = new IPEndPoint(server_addr, conf.listen_port);
                            connector = new Socket(
                                server_addr.AddressFamily,
                                SocketType.Stream,
                                ProtocolType.Tcp
                               );

                            connector.NoDelay = false;
                            connector.Connect(ipEndPoint);

                            Array.Clear(buffer, 0, buffer.Length);

                            byte[] msg_add = new byte[24];
                            Constant.msg_handshake.CopyTo(msg_add, 0);
                            peer_id.CopyTo(msg_add, 20);

                            connector.Send(msg_add, 0, msg_add.Length, SocketFlags.None);
                            connector.Receive(buffer, buffer.Length, SocketFlags.None);

                            result = Constant.check_message(buffer);
                            if (result == Constant.MSG_SUCCESS)
                            {
                                // hash handshake
                                in_room = true;
                                current_room = rooms[n];
                                is_creator = false;

                                keep_alive_room = new Thread(new ThreadStart(send_alive_room));
                                keep_alive_room.Start();

                                Console.WriteLine("Successfully joined room.");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("Request join room failed 1.");
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cannot connect to room creator");
                            return false;
                        }
                    }
                    else if (result == Constant.MSG_FAILED)
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
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (in_room)
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

        public bool start()
        {
            try
            {
                if ((is_connected) && (is_creator))
                {
                    Console.WriteLine("Requesting to start game");

                    byte[] msg = new byte[74];
                    Constant.msg_start.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);
                    current_room.ID.CopyTo(msg, 24);

                    byte[] buffer = new byte[1024];

                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);
                    track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    int result = Constant.check_message(buffer);
                    if (result == Constant.MSG_SUCCESS)
                    {
                        in_room = false;
                        Console.WriteLine("Successfully started the game.");
                        return true;
                    }
                    else if (result == Constant.MSG_FAILED)
                    {
                        Console.WriteLine("Failed to start the game.");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Response unrecognized.");
                        return false;
                    }
                }
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (!is_creator)
                {
                    Console.WriteLine("You are not authorized to start.");
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

        public bool quit()
        {
            try
            {
                if ((is_connected) && (in_room))
                {
                    Console.WriteLine("Requesting to quit game");

                    byte[] msg = new byte[24];
                    Constant.msg_quit.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    byte[] buffer = new byte[1024];

                    int result;

                    if (!is_creator)
                    {
                        connector.Send(msg, 0, msg.Length, SocketFlags.None);
                        connector.Receive(buffer, buffer.Length, SocketFlags.None);
                        result = Constant.check_message(buffer);
                        if (result == Constant.MSG_SUCCESS)
                        {
                            in_room = false;
                            if (keep_alive_room != null)
                            {
                                keep_alive_room.Abort();
                                keep_alive_room = null;
                                if (connector != null)
                                    connector.Close();
                            }
                            Console.WriteLine("Successfully quit the room.");
                            return true;
                        }
                        else if (result == Constant.MSG_FAILED)
                        {
                            Console.WriteLine("Failed to quit the room.");
                            return false;
                        }
                        else
                        {
                            Console.WriteLine("Response unrecognized.");
                            return false;
                        }
                    }
                    else
                    {
                        track_socket.Send(msg, 0, msg.Length, SocketFlags.None);
                        track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                        result = Constant.check_message(buffer);
                        if (result == Constant.MSG_SUCCESS)
                        {
                            Console.WriteLine("Success quiting room.");
                            in_room = false;
                            if (listen_to_room != null)
                            {
                                listen_to_room.Abort();
                                listen_to_room = null;
                                if (listener != null)
                                    listener.Close();
                            }
                            return true;
                        }
                        else if (result == Constant.MSG_FAILED)
                        {
                            Console.WriteLine("Failed to quit the room.");
                            return false;
                        }
                        else
                        {
                            Console.WriteLine("Response unrecognized.");
                            return false;
                        }
                    }
                }
                else if (!is_connected)
                {
                    Console.WriteLine("Not connected to tracker, connect to tracker first.");
                    return false;
                }
                else if (!in_room)
                {
                    Console.WriteLine("You are not in room.");
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

        public void close()
        {
            if (keep_alive != null)
            {
                keep_alive.Abort();
                keep_alive = null;
            }
            if (keep_alive_room != null)
            {
                keep_alive_room.Abort();
                keep_alive_room = null;
            }
            if (listen_to_room != null)
            {
                listen_to_room.Abort();
                listen_to_room = null;
            }
        }

        public void listen_room(Object obj)
        {
            try
            {
                Console.WriteLine("tes");
                byte max_player_num = (byte)obj;
                listener.Listen(max_player_num * 2);

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult target)
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

                IAsyncResult handle_timeout = handler.BeginReceive(
                    buffer,
                    0,
                    buffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveCallback),
                    obj
                    );

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                slistener.BeginAccept(aCallback, slistener);

                if (!handle_timeout.AsyncWaitHandle.WaitOne(conf.max_timeout))
                {
                    Console.WriteLine("Connection timeout for peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address);
                    handler.EndReceive(target);
                    handler.Close();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public void ReceiveCallback(IAsyncResult target)
        {
            try
            {
                object[] obj = new object[2];
                obj = (object[])target.AsyncState;

                byte[] buffer = (byte[])obj[0];

                Socket handler = (Socket)obj[1];

                string content = string.Empty;

                bool quit = false;

                int bytesRead = handler.EndReceive(target);

                if (bytesRead > 0)
                {
                    content += Encoding.Unicode.GetString(buffer, 0, bytesRead);

                    int result = Constant.check_message(buffer);
                    Console.WriteLine(result);
                    if (result == Constant.MSG_HANDSHAKE)
                    {
                        if (bytesRead == 24)
                        {
                            // hash handshake
                            if (peers.Count < current_room.MAX_PLAYER)
                            {
                                // check new unique ID
                                byte[] ID = new byte[4];
                                Buffer.BlockCopy(buffer, 20, ID, 0, 4);

                                Peer p = new Peer(ID, (handler.RemoteEndPoint as IPEndPoint).Address);

                                Console.WriteLine("Peer " + p.ToString() + " enter your room.");

                                byte[] msg = new byte[78];
                                Constant.msg_add.CopyTo(msg, 0);
                                peer_id.CopyTo(msg, 20);
                                current_room.ID.CopyTo(msg, 24);
                                ID.CopyTo(msg, 74);

                                track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                                Array.Clear(buffer, 0, buffer.Length);
                                track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                                result = Constant.check_message(buffer);
                                if (result == Constant.MSG_SUCCESS)
                                {
                                    current_room.CURRENT_PLAYER += 1;
                                    peers.Add(p);
                                    handler.Send(Constant.msg_success, 0, Constant.msg_success.Length, SocketFlags.None);
                                }
                                else
                                {
                                    handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                                }
                            }
                            else
                            {
                                handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                            }
                        }
                    }
                    else if (result == Constant.MSG_KEEP_ALIVE)
                    {
                        // hash keep alive
                        // actually keep alive has no use, 
                        // just ensure the socket is still connecting
                        if (bytesRead == 24)
                        {
                            bool check = true;
                            int i = 0;
                            while ((check) && (i < peers.Count))
                            {
                                if (Constant.compare_bytes_special(buffer, peers[i].ID, 20, 4))
                                {
                                    check = false;
                                }
                                i++;
                            }
                            if (!check)
                            {
                                i--; // i = position of current peer in list
                            }
                            else
                            {
                                // throw message failed
                                Console.WriteLine("keep alive failed 1");
                                handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            // throw message failed
                            Console.WriteLine("keep alive failed 2");
                            handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                        }
                    }
                    else if (result == Constant.MSG_LIST)
                    {
                        if (bytesRead == 24)
                        {
                            Console.WriteLine("Receive list peer message.");
                            bool check = true;
                            int i = 0;
                            while ((check) && (i < peers.Count))
                            {
                                if (Constant.compare_bytes_special(buffer, peers[i].ID, 20, 4))
                                {
                                    check = false;
                                }
                                i++;
                            }
                            if (!check)
                            {
                                i--; // i = position of current peer in list
                                byte[] returnmsg = new byte[1024];
                                Constant.msg_room.CopyTo(returnmsg, 0);
                                returnmsg[20] = ((byte)(peers.Count));

                                int pos = 21;
                                for (int j = 0; j < peers.Count; ++j)
                                {
                                    if (j != i)
                                    {
                                        List<byte> peer = peers[j].to_byte_message();
                                        Buffer.BlockCopy(peer.ToArray(), 0, returnmsg, pos, peer.Count());
                                        pos += peer.Count;
                                    }
                                }

                                List<byte> ret = new List<byte>();

                                for (int j = 0; j < peer_id.Length; ++j)
                                {
                                    ret.Add(peer_id[j]);
                                }

                                String[] temps = (handler.LocalEndPoint as IPEndPoint).Address.ToString().Split('.');
                                Console.WriteLine("test " + (handler.LocalEndPoint as IPEndPoint).Address.ToString());

                                for (int j = 0; j < temps.Length; ++j)
                                {
                                    int temp;
                                    Int32.TryParse(temps[j], out temp);
                                    ret.Add((byte)temp);
                                }

                                Buffer.BlockCopy(ret.ToArray(), 0, returnmsg, pos, ret.Count());

                                handler.Send(returnmsg, 0, returnmsg.Length, SocketFlags.None);
                            }
                            else
                            {
                                // throw message failed
                                handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            // throw message failed
                            handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                        }
                    }
                    else if (result == Constant.MSG_QUIT)
                    {
                        // hash keep alive
                        // actually keep alive has no use, 
                        // just ensure the socket is still connecting
                        if (bytesRead == 24)
                        {
                            bool check = true;
                            int i = 0;
                            while ((check) && (i < peers.Count))
                            {
                                if (Constant.compare_bytes_special(buffer, peers[i].ID, 20, 4))
                                {
                                    check = false;
                                }
                                i++;
                            }
                            if (!check)
                            {
                                i--; // i = position of current peer in list

                                Console.WriteLine("Quit request from peer with ID " + peers[i].ToString() + " for room " + current_room.ToLanguageString());

                                // check new unique ID
                                byte[] ID = new byte[4];
                                Buffer.BlockCopy(buffer, 20, ID, 0, 4);

                                byte[] msg = new byte[78];
                                Constant.msg_remove.CopyTo(msg, 0);
                                peer_id.CopyTo(msg, 20);
                                current_room.ID.CopyTo(msg, 24);
                                ID.CopyTo(msg, 74);

                                track_socket.Send(msg, 0, msg.Length, SocketFlags.None);

                                Array.Clear(buffer, 0, buffer.Length);
                                track_socket.Receive(buffer, buffer.Length, SocketFlags.None);

                                result = Constant.check_message(buffer);
                                if (result == Constant.MSG_SUCCESS)
                                {
                                    Console.WriteLine("Peer " + peers[i].ToString() + " leave your room.");
                                    current_room.CURRENT_PLAYER -= 1;
                                    peers.RemoveAt(i);
                                    handler.Send(Constant.msg_success, 0, Constant.msg_success.Length, SocketFlags.None);
                                    quit = true;
                                    handler.Close();
                                }
                                else
                                {
                                    Console.WriteLine("Peer " + peers[i].ToString() + " failed leave your room.");
                                    handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                                }
                            }
                            else
                            {
                                // throw message failed
                                handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                            }
                        }
                        else
                        {
                            // throw message failed
                            handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                        }
                    }
                    else
                    {
                        handler.Send(Constant.msg_failed, 0, Constant.msg_failed.Length, SocketFlags.None);
                    }
                }

                if (!quit)
                {
                    IAsyncResult handle_timeout = handler.BeginReceive(
                        buffer,
                        0,
                        buffer.Length,
                        SocketFlags.None,
                        new AsyncCallback(ReceiveCallback),
                        obj
                    );

                    if (!handle_timeout.AsyncWaitHandle.WaitOne(conf.max_timeout))
                    {
                        Console.WriteLine("Connection timeout for peer with IP Address " + (handler.RemoteEndPoint as IPEndPoint).Address);

                        // find peer id
                        bool check = true;
                        int i = 0;
                        while ((check) && (i < peers.Count))
                        {
                            if (peers[i].IP.Equals((handler.RemoteEndPoint as IPEndPoint).Address))
                            {
                                peers.RemoveAt(i);
                                check = false;
                            }
                            i++;
                        }

                        handler.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        private void send_alive()
        {
            try
            {
                while (true)
                {
                    byte[] msg = new byte[24];
                    Constant.msg_keep_alive.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    track_socket.Send(msg, 0, msg.Length, SocketFlags.None);
                    Thread.Sleep(conf.max_timeout / 4);
                }
            }
            catch (Exception)
            {
                if (track_socket == null)
                {
                    Console.WriteLine("Connection terminated");
                    is_connected = false;
                }
            }
        }

        private void send_alive_room()
        {
            try
            {
                while (true)
                {
                    byte[] msg = new byte[24];
                    Constant.msg_keep_alive.CopyTo(msg, 0);
                    peer_id.CopyTo(msg, 20);

                    connector.Send(msg, 0, msg.Length, SocketFlags.None);
                    Thread.Sleep(conf.max_timeout / 4);
                }
            }
            catch (Exception)
            {
                if (connector == null)
                {
                    Console.WriteLine("Force quit from room");
                    in_room = false;
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
                                            conf.tracker_address = reader.ReadString();
                                            break;
                                        }
                                    case "max_timeout":
                                        {
                                            conf.max_timeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "port":
                                        {
                                            conf.port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "listen_port":
                                        {
                                            conf.listen_port = Int32.Parse(reader.ReadString());
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
                conf.tracker_address = "127.0.0.1";
                conf.max_timeout = 30000;
                conf.port = 9351;
                conf.listen_port = 9757;

                XmlWriter writer = XmlWriter.Create(xml);
                writer.WriteStartDocument();
                writer.WriteStartElement("Config");
                writer.WriteElementString("tracker_address", conf.tracker_address);
                writer.WriteElementString("max_timeout", conf.max_timeout.ToString());
                writer.WriteElementString("port", conf.port.ToString());
                writer.WriteElementString("listen_port", conf.listen_port.ToString());

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
                writer.Close();
            }
        }
        #endregion
    }
}
