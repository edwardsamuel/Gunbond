using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunbond_Tracker.Model
{
    public class Room
    {
        #region Properties
        public Peer Creator
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public int MaxPlayers
        {
            get;
            set;
        }

        public Dictionary<int, Peer> Members
        {
            get;
            set;
        }
        #endregion

        public Room(string Id, Peer Creator, int MaxPlayers)
        {
            this.Members = new Dictionary<int, Peer>();
            this.Id = Id;
            this.Creator = Creator;
            this.MaxPlayers = MaxPlayers;

            this.Members.Add(Creator.Id, Creator);
        }

        public void AddPeer(Peer peer)
        {
            peer.InRoom = true;
            peer.RoomId = Id;
            Members.Add(peer.Id, peer);
        }

        public bool RemovePeer(int peerId)
        {
            Peer peer;
            if (Members.TryGetValue(peerId, out peer))
            {
                peer.RoomId = String.Empty;
                peer.InRoom = false;
            }
            return Members.Remove(peerId);
        }

        public override string ToString()
        {
            return "{Id = " + Id + ", Creator = " + Creator + ", MaxPlayers = " + MaxPlayers + "}";
        }
    }
}
