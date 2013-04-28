using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using Gunbond_Tracker.Util;

namespace Gunbond_Tracker
{
    public class Tracker
    {
        #region Properties
        public TrackerConfig Config
        {
            get;
            set;
        }
        #endregion

        public Socket listener;

        public Tracker()
        {
            Config = new TrackerConfig("tracker.xml");
            Logger.Active = Config.Log;

            // Listening on socket
            IPAddress ipAddr = IPAddress.Parse(Config.IpAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Config.Port);
            SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", Config.Port);
            permission.Demand();

            try
            {
                listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(ipEndPoint);

                Console.WriteLine("Listening at IP " + ipEndPoint.Address + " and port " + ipEndPoint.Port + ".");

                listener.Listen(Config.Backlog);

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }
            catch (SocketException exc)
            {
                Logger.WriteLine(exc);
            }
        }

        public void AcceptCallback(IAsyncResult target)
        {

        }
    }
}
