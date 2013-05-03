using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GunBond_Client.Model
{
    public class Peer
    {
        private IPAddress ip;
        private byte[] id = new byte[4];

        public Peer(byte[] id_and_ip)
        {
            Buffer.BlockCopy(id_and_ip, 0, this.id, 0, 4);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; ++i)
            {
                sb.Append(id_and_ip[4 + i] + ".");
            }
            sb.Remove(sb.Length - 1, 1);

            IPAddress.TryParse(sb.ToString(), out this.ip);
        }

        public Peer(byte[] id, IPAddress ip)
        {
            this.id = id;
            this.ip = ip;
        }

        public IPAddress IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public byte[] ID
        {
            get { return id; }
            set { id = value; }
        }

        public override string ToString()
        {
            String s = (int)id[0] + "." + (int)id[1] + "." + (int)id[2] + "." + (int)id[3];
            return s;
        }

        public List<byte> to_byte_message()
        {
            List<byte> ret = new List<byte>();

            for (int i = 0; i < id.Length; ++i)
            {
                ret.Add(id[i]);
            }

            String[] temps = ip.ToString().Split('.');

            for (int i = 0; i < temps.Length; ++i)
            {
                int temp;
                Int32.TryParse(temps[i], out temp);
                ret.Add((byte)temp);
            }

            return ret;
        }
    }
}
