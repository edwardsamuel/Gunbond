using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Gunbond_Client.Model
{
    public class Peer
    {
        #region Properties
        public IPAddress IPAddress
        {
            get;
            set;
        }

        public int PeerId
        {
            get;
            set;
        }

        public int ListeningPort
        {
            get;
            set;
        }
        #endregion

        public Peer(int peerId, IPAddress IPAddress, int listeningPort)
        {
            this.PeerId = peerId;
            this.IPAddress = IPAddress;
            this.ListeningPort = listeningPort;
        }
    }
}
