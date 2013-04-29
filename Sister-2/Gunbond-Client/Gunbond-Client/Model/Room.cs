using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunbond_Client.Model
{
    public class Room
    {
        private string roomId;
        public string RoomId
        {
            get { return roomId; }
            set { roomId = value; }
        }

        private int currentPlayer;
        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }
        
        private int maxPlayer;
        public int MaxPlayer
        {
            get { return maxPlayer; }
            set { maxPlayer = value; }
        }

        public Room(string roomId, int maxPlayers)
        {
            this.maxPlayer = maxPlayers;
            this.roomId = roomId;
        }
        /*public Room(byte[] id)
        {
            byte[] temp = new byte[4];
            Buffer.BlockCopy(id, 0, temp, 0, 50);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(temp);

            this.roomId = new System.Text.ASCIIEncoding().GetString(temp);
            this.currentPlayer = id[50];
            this.maxPlayer = id[51];
        }
        */
        

        
    }
}