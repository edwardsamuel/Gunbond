using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Gunbond.Model
{
    public class Message
    {
        public byte[] data;

        public enum MessageType
        {
            Unknown = 0,
            HandshakePeer = 135,
            HandshakeTracker = 136,
            KeepAlive = 182,
            CreateRoom = 255,
            ListRoom = 254,
            CreatorInfo = 111,
            Room = 200,
            Success = 127,
            Failed = 128,
            Join = 253,
            Start = 252,
            Quit = 235
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MessageRoomBody
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string roomId;
            public int maxPlayers;
            public int currentPlayer;
        }

        public Message()
        {
            // do nothing
        }

        public Message(byte[] data)
        {
            this.data = data;
        }

        private static byte[] ConvertIntToByte(int intValue)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(intValue >> 24);
            bytes[1] = (byte)(intValue >> 16);
            bytes[2] = (byte)(intValue >> 8);
            bytes[3] = (byte)intValue;

            return bytes;
        }

        private static byte[] ConvertStringToByte(string str)
        {
            char[] charArray = str.ToCharArray();
            byte[] byteArray = new byte[charArray.Length];

            for (int i = 0; i < charArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(charArray[i]);
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
            byte[] data = new byte[20];
            FillHeader(data);
            data[19] = 135;

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageHandshakeTracker(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 136;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageKeepAlive(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 182;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageCreate(int peerId, int maxPlayer, String roomId)
        {
            byte[] data = new byte[78];
            FillHeader(data);
            data[19] = 255;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            byte[] maxP = ConvertIntToByte(maxPlayer);
            data[24] = id[0];
            data[25] = id[1];
            data[26] = id[2];
            data[27] = id[3];

            byte[] rID = ConvertStringToByte(roomId);
            int i;
            for (i = 28; i < 28 + rID.Length; i++)
            {
                data[i] = rID[i - 28];
            }

            for (; i < 50; i++)
            {
                data[i] = 0;
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageList(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 254;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            Message m = new Message();
            m.data = data;
            return m;
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

            byte[] id = ConvertIntToByte(rooms.Count);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            int j = 24;
            for (int i = 0; i < rooms.Count; i++)
            {
                byte[] temp = Gunbond.Helper.StructureToByteArray(rooms[i]);
                Buffer.BlockCopy(temp, 0, data, j, mbSize);
                j += mbSize;
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageCreatorInfo(string host, int port)
        {
            byte[] hostBytes = ConvertStringToByte(host);
            byte[] data = new byte[20 + 4 + 4 + hostBytes.Length];

            FillHeader(data);
            data[19] = 111;

            byte[] portBytes = ConvertIntToByte(port);
            data[20] = portBytes[0];
            data[21] = portBytes[1];
            data[22] = portBytes[2];
            data[23] = portBytes[3];

            byte[] hostLengthBytes = ConvertIntToByte(hostBytes.Length);
            data[24] = hostLengthBytes[0];
            data[25] = hostLengthBytes[1];
            data[26] = hostLengthBytes[2];
            data[27] = hostLengthBytes[3];

            for (int i = 28, len = 28 + hostBytes.Length; i < len; i++)
            {
                data[i] = hostBytes[i - 28];
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageJoin(int peerId, String roomId)
        {
            // Create buffer data
            byte[] data = new byte[74];

            // Fill with static header data
            FillHeader(data);

            // Join Code
            data[19] = 253;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            byte[] rID = ConvertStringToByte(roomId);
            int i;
            for (i = 24; i < 24 + rID.Length; i++)
            {
                data[i] = rID[i - 24];
            }
            for (; i < 50; i++)
            {
                data[i] = 0;
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageSuccess()
        {
            byte[] data = new byte[20];
            FillHeader(data);
            data[19] = 127;

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageFailed()
        {
            byte[] data = new byte[20];
            FillHeader(data);
            data[19] = 128;

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageStart(int peerID, String roomID)
        {
            byte[] data = new byte[74];
            FillHeader(data);
            data[19] = 252;

            byte[] id = ConvertIntToByte(peerID);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            byte[] rID = ConvertStringToByte(roomID);
            int i;
            for (i = 24; i < rID.Length + 24; i++)
            {
                data[i] = rID[i - 24];
            }
            for (; i < 50 + 24; i++)
            {
                data[i] = 0;
            }

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageQuit(int peerId)
        {
            byte[] data = new byte[24];
            FillHeader(data);
            data[19] = 235;

            byte[] id = ConvertIntToByte(peerId);
            data[20] = id[0];
            data[21] = id[1];
            data[22] = id[2];
            data[23] = id[3];

            Message m = new Message();
            m.data = data;
            return m;
        }

        public static Message CreateMessageGame(int x, int y, int power)
        {
            byte[] data = new byte[24];
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
            byte[] temp = ConvertIntToByte(x);
            data[11] = temp[0];
            data[12] = temp[1];
            data[13] = temp[2];
            data[14] = temp[3];
            //data[15..18] for y position
            byte[] temp2 = ConvertIntToByte(y);
            data[15] = temp2[0];
            data[16] = temp2[1];
            data[17] = temp2[2];
            data[18] = temp2[3];
            //data[19..22] for power
            byte[] temp3 = ConvertIntToByte(power);
            data[19] = temp3[0];
            data[20] = temp3[1];
            data[21] = temp3[2];
            data[22] = temp3[3];

            Message m = new Message();
            m.data = data;
            return m;
        }

        public void GetHandshakeTracker(out int peerId)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerId = ConvertBytesToInt(d);
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

        public void GetCreate(out int peerID, out int maxPlayer, out String roomID)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerID = ConvertBytesToInt(d);

            byte[] max = new byte[4];
            max[0] = data[24];
            max[1] = data[25];
            max[2] = data[26];
            max[3] = data[27];
            maxPlayer = ConvertBytesToInt(max);

            byte[] room = new byte[50];
            int j = 28;
            for (int i = 0; i < 50; i++)
            {
                room[i] = data[j++];
            }
            roomID = ConvertBytesToString(room);
        }

        public void GetList(out int peerID)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerID = ConvertBytesToInt(d);
        }

        public void GetRoom(out List<string> room)
        {
            room = new List<string>();

            byte[] id = new byte[4];
            id[0] = data[20];
            id[1] = data[21];
            id[2] = data[22];
            id[3] = data[23];

            int CountRoom = ConvertBytesToInt(id);

            byte[] r = new byte[50];
            int offset = 24;
            for (int i = 0; i < CountRoom; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    r[j] = data[offset++];
                }
                room.Add(ConvertBytesToString(r));
            }
        }

        public void GetCreatorInfo(out string host, out int port)
        {
            byte[] portBytes = new byte[4];
            portBytes[0] = data[20];
            portBytes[1] = data[21];
            portBytes[2] = data[22];
            portBytes[3] = data[23];
            port = ConvertBytesToInt(portBytes);

            byte[] hostLengthBytes = new byte[4];
            hostLengthBytes[0] = data[24];
            hostLengthBytes[1] = data[25];
            hostLengthBytes[2] = data[26];
            hostLengthBytes[3] = data[27];
            int hostLength = ConvertBytesToInt(hostLengthBytes);

            byte[] hostBytes = new byte[hostLength];
            for (int i = 0; i < hostLength; i++)
            {
                hostBytes[i] = data[i + 28];
            }

            host = ConvertBytesToString(hostBytes);
        }

        public void GetJoin(out int peerId, out String roomId)
        {
            byte[] d = new byte[4];
            d[0] = data[20];
            d[1] = data[21];
            d[2] = data[22];
            d[3] = data[23];
            peerId = ConvertBytesToInt(d);

            byte[] room = new byte[50];
            int j = 24;
            for (int i = 0; i < 50; i++)
            {
                room[i] = data[j++];
            }
            roomId = ConvertBytesToString(room);
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

        public void GetMessageGame(out int x, out int y, out int power)
        {
            byte[] d = new byte[4];
            //d[11..14] for x position
            d[0] = data[11];
            d[1] = data[12];
            d[2] = data[13];
            d[3] = data[14];
            x = ConvertBytesToInt(d);
            //d[15..18] for y position
            d[0] = data[15];
            d[1] = data[16];
            d[2] = data[17];
            d[3] = data[18];
            y = ConvertBytesToInt(d);   
            //d[19..22] for power
            d[0] = data[19];
            d[1] = data[20];
            d[2] = data[21];
            d[3] = data[22];
            power = ConvertBytesToInt(d);
        }

        public MessageType GetMessageType()
        {
            if (data.Length >= 20)
            {
                switch (data[19])
                {
                    case 135: return MessageType.HandshakePeer;
                    case 136: return MessageType.HandshakeTracker;
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
