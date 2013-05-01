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

        public Peer Creator
        {
            get;
            set;
        }

        private List<Peer> members;
        public List<Peer> Members
        {
            get { return members; }
            set { members = value; }
        }

        private int maxPlayer;
        public int MaxPlayer
        {
            get { return maxPlayer; }
            set { maxPlayer = value; }
        }

        public Room(string roomId, Peer creator, int maxPlayers)
        {
            this.maxPlayer = maxPlayers;
            this.roomId = roomId;
            this.Creator = creator;
            this.members = new List<Peer>();
        }
    }
}