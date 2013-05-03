using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunBond_Client.Model
{
    public class Room
    {
        private byte[] id = new byte[50];
        private int current_player;
        private int max_player;

        public Room(byte[] id)
        {
            Buffer.BlockCopy(id, 0, this.id, 0, 50);
            this.current_player = id[50];
            this.max_player = id[51];
        }

        public byte[] ID
        {
            get { return id; }
            set { id = value; }
        }

        public int CURRENT_PLAYER
        {
            get { return current_player; }
            set { current_player = value; }
        }

        public int MAX_PLAYER
        {
            get { return max_player; }
            set { max_player = value; }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < 50; ++i)
            {
                s.Append((int)id[i] + ".");
            }
            s.Remove(s.Length - 1, 1);
            return s.ToString();
        }

        public string ToLanguageString()
        {
            int x = 0;
            StringBuilder s = new StringBuilder();
            while (x < 50)
            {
                if ((Char.IsLetterOrDigit((char)id[x]) || (Char.IsSeparator((char)id[x]))))
                {
                    s.Append((char)id[x]);
                }
                x++;
            }
            return s.ToString();
        }
    }
}
