using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Gunbond_Tracker.Model
{
    public class Peer
    {
        #region Properties
        public IPAddress IpAddress
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string RoomId
        {
            get;
            set;
        }

        public bool InRoom
        {
            get;
            set;
        }
        #endregion

        public Peer(int Id, IPAddress IpAddress)
        {
            this.Id = Id;
            this.IpAddress = IpAddress;
            this.InRoom = false;
        }

        public override string ToString()
        {
            return "{Id = " + Id + ", IpAddress = " + IpAddress + ", InRoom = " + InRoom + "}";
        }
    }
}
