using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;

namespace Gunbond
{
    public class Message
    {
        #region Message Room Body
        [StructLayout(LayoutKind.Sequential)]
        public struct MessageRoomBody
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string roomId;
            public int maxPlayers;
            public int currentPlayer;
        }

        private static byte[] GetBytes(MessageRoomBody mrb)
        {
            int size = Marshal.SizeOf(mrb);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(mrb, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        private static MessageRoomBody FromBytes(byte[] arr)
        {
            MessageRoomBody str = new MessageRoomBody();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (MessageRoomBody) Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
        #endregion

        public enum MessageType
        {
            Unknown = 0,
            HandshakePeer = 135,
            HandshakeTracker = 136,
            HandshakePeerCreator = 137,
            KeepAlive = 182,
            CreateRoom = 255,
            ListRoom = 254,
            CreatorInfo = 111,
            Room = 200,
            Success = 127,
            Failed = 128,
            Join = 253,
            Start = 252,
            Quit = 235,

            InGame = 123,
            NewMember = 30,
            RoomModel = 31
        };
   
        public byte[] data;

        public Message()
        {
            // do nothing
        }

        public Message(byte[] data)
        {
            this.data = data;
        }

        private static byte[] ConvertIntToBytes(int intValue)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(intValue >> 24);
            bytes[1] = (byte)(intValue >> 16);
            bytes[2] = (byte)(intValue >> 8);
            bytes[3] = (byte)intValue;

            return bytes;
        }

        private static byte[] ConvertStringToBytes(string str, int length)
        {
            char[] charArray = str.ToCharArray();
            byte[] byteArray = new byte[length];

            for (int i = 0; i < charArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(charArray[i]);
            }
            for (int i = charArray.Length; i < length; i++)
            {
                byteArray[i] = 0;
            }

            return byteArray;
        }

        private static int ConvertBytesToInt(byte[] bytes)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        private static String ConvertBytesToString(byte[] bytes)
        {
            return new System.Text.ASCIIEncoding().GetString(bytes);
        }

        public static void FillHeader(byte[] data)
        {
            data[0] = (byte)'G';
            data[1] = (byte)'U';
            data[2] = (byte)'N';
            data[3] = (byte)'B';
            data[4] = (byte)'O';
            data[5] = (byte)'N';
            data[6] = (byte)'D';
            data[7] = (byte)'G';
            data[8] = (byte)'A';
            data[9] = (byte)'M';
            data[10] = (byte)'E';
            data[11] = 0;
            data[12] = 0;
            data[13] = 0;
            data[14] = 0;
            data[15] = 0;
            data[16] = 0;
            data[17] = 0;
            data[18] = 0;
        }

        public static Message CreateMessageHandshakePeer()
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 135;

            return new Message(data);
        }

        public static Message CreateMessageHandshakeTracker(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 136;

            byte[] id = ConvertIntToBytes(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageHandshakePeerCreator(int peerId, int listeningPort)
        {
            byte[] data = new byte[28];
            FillHeader(data);
            data[19] = 137;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            byte[] bListeningPort = ConvertIntToBytes(listeningPort);
            Buffer.BlockCopy(bListeningPort, 0, data, 24, 4);

            return new Message(data);
        }

        public static Message CreateMessageKeepAlive(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 182;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            return new Message(data);
        }

        public static Message CreateMessageCreate(int peerId, int maxPlayers, String roomId, int listeningPort)
        {
            byte[] data = new byte[82];
            FillHeader(data);
            data[19] = 255;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            byte[] bMaxPlayers = ConvertIntToBytes(maxPlayers);
            Buffer.BlockCopy(bMaxPlayers, 0, data, 24, 4);

            byte[] bRoomId = ConvertStringToBytes(roomId, 50);
            Buffer.BlockCopy(bRoomId, 0, data, 28, 50);

            byte[] bListeningPort = ConvertIntToBytes(listeningPort);
            Buffer.BlockCopy(bListeningPort, 0, data, 78, 4);

            return new Message(data);
        }

        public static Message CreateMessageList(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 254;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            return new Message(data);
        }

        public static Message CreateMessageRoom(List<MessageRoomBody> rooms)
        {
            int mbSize = 0;
            if (rooms.Count > 0)
            {
                mbSize = Marshal.SizeOf(rooms[0]);
            }

            byte[] data = new byte[20 + 4 + rooms.Count * mbSize];
            FillHeader(data);
            data[19] = 200;

            byte[] id = ConvertIntToBytes(rooms.Count);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            int j = 24;
            for (int i = 0; i < rooms.Count; i++)
            {
                byte[] temp = GetBytes(rooms[i]);
                Buffer.BlockCopy(temp, 0, data, j, mbSize);
                j += mbSize;
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageCreatorInfo(IPAddress host, int port)
        {
            byte[] hostBytes = host.GetAddressBytes();
            byte[] data = new byte[28];

            FillHeader(data);
            data[19] = 111;

            byte[] bHost = host.GetAddressBytes();
            Buffer.BlockCopy(bHost, 0, data, 20, 4);

            byte[] bPort = ConvertIntToBytes(port);
            Buffer.BlockCopy(bPort, 0, data, 24, 4);
            
            return new Message(data);
        }

        public static Message CreateMessageJoin(int peerId, String roomId)
        {
            // Create buffer data
            byte[] data = new byte[74];

            // Fill with static header data
            FillHeader(data);

            // Join Code
            data[19] = 253;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            byte[] bRoomId = ConvertStringToBytes(roomId, 50);
            Buffer.BlockCopy(bRoomId, 0, data, 24, 50);
            
            return new Message(data);
        }

        public static Message CreateMessageSuccess()
        {
            byte[] data = new byte[20];
            FillHeader(data);
            data[19] = 127;

            return new Message(data);
        }

        public static Message CreateMessageFailed()
        {
            byte[] data = new byte[20];
            FillHeader(data);
            data[19] = 128;

            return new Message(data);
        }

        public static Message CreateMessageStart(int peerID, String roomID)
        {
            byte[] data = new byte[74];
            FillHeader(data);
            data[19] = 252;

            byte[] bPeerId = ConvertIntToBytes(peerID);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            byte[] bRoomId = ConvertStringToBytes(roomID, 50);
            Buffer.BlockCopy(bRoomId, 0, data, 24, 50);

            return new Message(data);
        }

        public static Message CreateMessageQuit(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 235;

            byte[] bPeerID = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerID, 0, data, 20, 4);

            return new Message(data);
        }

        #region MessageGame
        public static Message CreateMessageGame(int x, int y, int angle, int power, int damage)
        {
            byte[] data = new byte[32];
            FillHeader(data);
            //data[0] = (byte)'G';
            //data[1] = (byte)'U';
            //data[2] = (byte)'N';
            //data[3] = (byte)'B';
            //data[4] = (byte)'O';
            //data[5] = (byte)'N';
            //data[6] = (byte)'D';
            //data[7] = (byte)'G';
            //data[8] = (byte)'A';
            //data[9] = (byte)'M';
            //data[10] = (byte)'E';
            //data[11..14] for x position
            byte[] temp = ConvertIntToBytes(x);
            data[11] = temp[0];
            data[12] = temp[1];
            data[13] = temp[2];
            data[14] = temp[3];
            //data[15..18] for y position
            byte[] temp2 = ConvertIntToBytes(y);
            data[15] = temp2[0];
            data[16] = temp2[1];
            data[17] = temp2[2];
            data[18] = temp2[3];
            //data[19] for Message Type
            data[19] = 123;
            //data[20..23] for angle
            byte[] temp3 = ConvertIntToBytes(angle);
            data[20] = temp3[0];
            data[21] = temp3[1];
            data[22] = temp3[2];
            data[23] = temp3[3];
            //data[24..27] for power
            byte[] temp4 = ConvertIntToBytes(power);
            data[24] = temp4[0];
            data[25] = temp4[1];
            data[26] = temp4[2];
            data[27] = temp4[3];
            //data[28..31] for damage
            byte[] temp5 = ConvertIntToBytes(damage);
            data[28] = temp5[0];
            data[29] = temp5[1];
            data[30] = temp5[2];
            data[31] = temp5[3];
            Message m = new Message();
            m.data = data;
            return m;
        }
        #endregion

        public static Message CreateMessageNewMember(int peerId, IPAddress IP, int listeningPort)
        {
            byte[] data = new byte[32];
            FillHeader(data);
            data[19] = 30;

            byte[] bPeerId = ConvertIntToBytes(peerId);
            Buffer.BlockCopy(bPeerId, 0, data, 20, 4);

            byte[] bIP = IP.GetAddressBytes();
            Buffer.BlockCopy(bIP, 0, data, 24, 4);

            byte[] bListeningPort = ConvertIntToBytes(listeningPort);
            Buffer.BlockCopy(bListeningPort, 0, data, 28, 4);

            return new Message(data);
        }

        public static Message CreateMessageRoomModel(Gunbond_Client.Model.Room room)
        {
            int len = 20 + 50 + 4 + 4 + room.Members.Count * (4 + 4 + 4);
            if (len > 1024) throw new Exception("Panjang data > 1024");

            byte[] data = new byte[len];
            FillHeader(data);
            data[19] = 31;

            byte[] bMembersCount = ConvertIntToBytes(room.Members.Count);
            byte[] bMaxPlayer = ConvertIntToBytes(room.MaxPlayer);
            byte[] bRoomId = ConvertStringToBytes(room.RoomId, 50);

            Buffer.BlockCopy(bMembersCount, 0, data, 20, 4);
            Buffer.BlockCopy(bMaxPlayer, 0, data, 24, 4);
            Buffer.BlockCopy(bRoomId, 0, data, 28, 50);

            for (int i = 0; i < room.Members.Count; i++)
            {
                byte[] bPeerId = ConvertIntToBytes(room.Members[i].PeerId);
                byte[] bPeerIp = room.Members[i].IPAddress.GetAddressBytes();
                byte[] bPeerPort = ConvertIntToBytes(room.Members[i].ListeningPort);

                Buffer.BlockCopy(bPeerId, 0, data, 78 + (i * 8), 4);
                Buffer.BlockCopy(bPeerIp, 0, data, 78 + (i * 8) + 4, 4);
                Buffer.BlockCopy(bPeerPort, 0, data, 78 + (i * 8) + 8, 4);
            }

            return new Message(data);
        }

        public void GetHandshakeTracker(out int peerId)
        {
            byte[] bPeerId = new byte[4];
            bPeerId[0] = data[20];
            bPeerId[1] = data[21];
            bPeerId[2] = data[22];
            bPeerId[3] = data[23];
            peerId = ConvertBytesToInt(bPeerId);
        }

        public void GetHandshakePeerCreator(out int peerId, out int listeningPort)
        {
            byte[] bPeerId = new byte[4];
            Buffer.BlockCopy(data, 20, bPeerId, 0, 4);
            peerId = ConvertBytesToInt(bPeerId);

            byte[] bListeningPort = new byte[4];
            Buffer.BlockCopy(data, 24, bListeningPort, 0, 4);
            listeningPort = ConvertBytesToInt(bListeningPort);
        }

        public void GetKeepAlive(out int peerID)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerID = ConvertBytesToInt(d);
        }

        public void GetCreate(out int peerID, out int maxPlayer, out String roomID, out int listeningPort)
        {
            byte[] d = new byte[4];
            Buffer.BlockCopy(data, 20, d, 0, 4);
            peerID = ConvertBytesToInt(d);

            byte[] bMaxPlayers = new byte[4];
            Buffer.BlockCopy(data, 24, bMaxPlayers, 0, 4);
            maxPlayer = ConvertBytesToInt(bMaxPlayers);

            byte[] bRoomId = new byte[50];
            Buffer.BlockCopy(data, 28, bRoomId, 0, 50);
            roomID = ConvertBytesToString(bRoomId);

            byte[] bListeningPort = new byte[4];
            Buffer.BlockCopy(data, 78, bListeningPort, 0, 4);
            listeningPort = ConvertBytesToInt(bListeningPort);
        }

        public void GetList(out int peerID)
        {
            byte[] d = new byte[4];
            Buffer.BlockCopy(data, 20, d, 0, 4);
            peerID = ConvertBytesToInt(d);
        }

        public void GetRoom(out List<MessageRoomBody> rooms)
        {
            rooms = new List<MessageRoomBody>();

            MessageRoomBody mb;
            mb.currentPlayer = 0;
            mb.maxPlayers = 2;
            mb.roomId = "";
            int mbSize = Marshal.SizeOf(mb);

            byte[] id = new byte[4];
            id[0] = data[20];
            id[1] = data[21];
            id[2] = data[22];
            id[3] = data[23];

            int CountRoom = ConvertBytesToInt(id);

            byte[] r = new byte[mbSize];
            int offset = 24;
            for (int i = 0; i < CountRoom; i++)
            {
                Buffer.BlockCopy(data, offset, r, 0, mbSize);
                offset += mbSize;
                rooms.Add(FromBytes(r));
            }
        }

        public void GetCreatorInfo(out IPAddress host, out int port)
        {
            byte[] bHost = new byte[4];
            Buffer.BlockCopy(data, 20, bHost, 0, 4);
            host = new IPAddress(bHost);

            byte[] bPort = new byte[4];
            Buffer.BlockCopy(data, 24, bPort, 0, 4);
            port = ConvertBytesToInt(bPort);
        }

        public void GetJoin(out int peerId, out String roomId)
        {
            byte[] bPeerId = new byte[4];
            Buffer.BlockCopy(data, 20, bPeerId, 0, 4);
            peerId = ConvertBytesToInt(bPeerId);

            byte[] bRoomId = new byte[50];
            Buffer.BlockCopy(data, 24, bRoomId, 0, 50);
            roomId = ConvertBytesToString(bRoomId);
        }

        public void GetStart(out int peerID, out String roomID)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerID = ConvertBytesToInt(d);

            byte[] room = new byte[50];
            int j = 24;
            for (int i = 0; i < 50; i++)
            {
                room[i] = data[j++];
            }
            roomID = ConvertBytesToString(room);
        }

        public void GetQuit(out int peerID)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerID = ConvertBytesToInt(d);
        }

        public void GetNewMember(out int peerID, out IPAddress IP, out int listeningPort)
        {
            byte[] bPeerId = new byte[4];
            Buffer.BlockCopy(data, 20, bPeerId, 0, 4);
            peerID = ConvertBytesToInt(bPeerId);

            byte[] bIP = new byte[4];
            Buffer.BlockCopy(data, 24, bIP, 0, 4);
            IP = new IPAddress(bIP);

            byte[]  bListeningPort = new byte[4];
            Buffer.BlockCopy(data, 28, bListeningPort, 0, 4);
            listeningPort = ConvertBytesToInt(bListeningPort);
        }


        public void GetRoomModel(out Gunbond_Client.Model.Room room)
        {
            byte[] bMembersCount = new byte[4];
            byte[] bMaxPlayer = new byte[4];
            byte[] bRoomId = new byte[50];

            Buffer.BlockCopy(data, 20, bMembersCount, 0, 4);
            Buffer.BlockCopy(data, 24, bMaxPlayer, 0, 4);
            Buffer.BlockCopy(data, 28, bRoomId, 0, 50);

            string roomId = ConvertBytesToString(bRoomId);
            int maxPlayer = ConvertBytesToInt(bMaxPlayer);
            int memberCount = ConvertBytesToInt(bMembersCount);

            List<Gunbond_Client.Model.Peer> members = new List<Gunbond_Client.Model.Peer>();
            for (int i = 0; i < memberCount; i++)
            {
                byte[] bPeerId = new byte[4];
                byte[] bPeerIp = new byte[4];
                byte[] bPeerPort = new byte[4];

                Buffer.BlockCopy(data, 78 + (i * 8), bPeerId, 0, 4);
                Buffer.BlockCopy(data, 78 + (i * 8) + 4, bPeerIp, 0, 4);
                Buffer.BlockCopy(data, 78 + (i * 8) + 8, bPeerPort, 0, 4);

                int peerId = ConvertBytesToInt(bPeerId);
                IPAddress peerIp = new IPAddress(bPeerIp);
                int listeningPort = ConvertBytesToInt(bPeerPort);

                members.Add(new Gunbond_Client.Model.Peer(peerId, peerIp, listeningPort));
            }

            room = new Gunbond_Client.Model.Room(roomId, members.First(), maxPlayer);
            room.Members = members;
        }

        #region GetMessageGame
        public void GetMessageGame(out int x, out int y, out int angle, out int power, out int damage)
        {
            byte[] d = new byte[4];
            //data[11..14] for x position
            d[0] = data[11];
            d[1] = data[12];
            d[2] = data[13];
            d[3] = data[14];
            x = ConvertBytesToInt(d);
            //data[15..18] for y position
            d[0] = data[15];
            d[1] = data[16];
            d[2] = data[17];
            d[3] = data[18];
            y = ConvertBytesToInt(d);
            //data[20..23] for angle
            d[0] = data[19];
            d[1] = data[20];
            d[2] = data[21];
            d[3] = data[22];
            angle = ConvertBytesToInt(d);
            //data[24..27] for power
            d[0] = data[19];
            d[1] = data[20];
            d[2] = data[21];
            d[3] = data[22];
            power = ConvertBytesToInt(d);
            //data[28..31] for damage
            d[0] = data[28];
            d[1] = data[29];
            d[2] = data[30];
            d[3] = data[31];
            damage = ConvertBytesToInt(d);
        }
        #endregion

        public MessageType GetMessageType()
        {
            if (data.Length >= 20)
            {
                switch (data[19])
                {
                    case 135: return MessageType.HandshakePeer;
                    case 136: return MessageType.HandshakeTracker;
                    case 137: return MessageType.HandshakePeerCreator;
                    case 182: return MessageType.KeepAlive;
                    case 255: return MessageType.CreateRoom;
                    case 254: return MessageType.ListRoom;
                    case 200: return MessageType.Room;
                    case 111: return MessageType.CreatorInfo;
                    case 127: return MessageType.Success;
                    case 128: return MessageType.Failed;
                    case 253: return MessageType.Join;
                    case 252: return MessageType.Start;
                    case 235: return MessageType.Quit;

                    case 30: return MessageType.NewMember;
                    case 31: return MessageType.RoomModel;

                    case 123: return MessageType.InGame;

                    default: return MessageType.Unknown;
                }
            }
            else
            {
                return MessageType.Unknown;
            }
        }
    }
}
