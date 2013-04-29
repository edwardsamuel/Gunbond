using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Gunbond_Client.Model
{
    public class Peer
    {
        private IPAddress ip;
        public IPAddress IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public int PeerId
        {
            get;
            set;
        }

        public Peer(int id, IPAddress ip)
        {
            this.PeerId = id;
            this.ip = ip;
        }

        public Peer(byte[] id_and_ip)
        {
            byte[] temp = new byte[4];
            Buffer.BlockCopy(id_and_ip, 0, temp, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(temp);

            this.PeerId =  BitConverter.ToInt32(temp, 0);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; ++i)
            {
                sb.Append(id_and_ip[4 + i] + ".");
            }
            sb.Remove(sb.Length - 1, 1);

            IPAddress.TryParse(sb.ToString(), out this.ip);
        }
    }
}
