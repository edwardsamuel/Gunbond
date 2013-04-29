using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.IO;

namespace Gunbond_Client
{
    public class Config
    {
        private string tracker_address;
        public string trackerAddress
        {
            get { return tracker_address; }
            set { tracker_address = value; }
        }

        private int _port;
        public int port
        {
            get { return _port; }
            set { _port = value; }
        }
        private int listen_port;
        public int listenPort
        {
            get { return listen_port; }
            set { listen_port = value; }
        }
        private int max_timeout;
        public int maxTimeout
        {
            get { return max_timeout; }
            set { max_timeout = value; }
        }
    };

    class GunConsole
    {

    }
}
