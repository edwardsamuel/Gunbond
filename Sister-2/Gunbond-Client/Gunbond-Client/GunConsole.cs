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
        #region Attribute and Getter Setter
        private Config conf = new Config();
        private string conf_filename = "peer_conf.xml";
        private Socket track_socket;
        private Socket listener;
        private Socket connector;
        private byte[] peer_id;
        private List<Room> rooms;
        private List<Peer> peers;

        private Thread keep_alive;
        private Thread keep_alive_room;
        private Thread listen_to_room;

        private Room current_room;
        private bool in_room;
        private bool is_creator;
        private bool is_connected;

        public List<Room> ROOMS
        {
            get { return rooms; }
            set { rooms = value; }
        }

        public List<Peer> PEERS
        {
            get { return peers; }
            set { peers = value; }
        }

        public byte[] PEERS_ID
        {
            get { return peer_id; }
            set { peer_id = value; }
        }
        #endregion

        #region Configuration Data
        private void LoadData(String xml)
        {
            if (File.Exists(xml))
            {
                XmlTextReader reader = new XmlTextReader(xml);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                switch (reader.Name)
                                {
                                    case "tracker_address":
                                        {
                                            conf.trackerAddress = reader.ReadString();
                                            break;
                                        }
                                    case "max_timeout":
                                        {
                                            conf.maxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "port":
                                        {
                                            conf.port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "listen_port":
                                        {
                                            conf.listenPort = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
                reader.Close();
            }
            else
            {
                // default configuration
                conf.trackerAddress = "127.0.0.1";
                conf.maxTimeout = 30000;
                conf.port = 9351;
                conf.listenPort = 9757;
                this.SaveData(xml);
            }
        }
        private void SaveData(String xml)
        {
            XmlWriter writer = XmlWriter.Create(xml);
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");
            writer.WriteElementString("tracker_address", conf.trackerAddress);
            writer.WriteElementString("max_timeout", conf.maxTimeout.ToString());
            writer.WriteElementString("port", conf.port.ToString());
            writer.WriteElementString("listen_port", conf.listenPort.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }
        #endregion
    }
}
