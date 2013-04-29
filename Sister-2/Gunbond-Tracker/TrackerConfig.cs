using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Gunbond_Tracker.Util;

namespace Gunbond_Tracker
{
    public class TrackerConfig
    {
        #region Properties
        public int MaxRoom
        {
            get;
            set;
        }

        public int MaxPeer
        {
            get;
            set;
        }

        public bool Log
        {
            get;
            set;
        }

        public int Backlog
        {
            get;
            set;
        }

        public int MaxTimeout
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string IpAddress
        {
            get;
            set;
        }
        #endregion

        public TrackerConfig()
        {
            // do nothing
        }

        public TrackerConfig(string filename)
        {
            LoadData(filename);
        }

        public void LoadData(string filename)
        {
            if (File.Exists(filename))
            {
                XmlTextReader reader = new XmlTextReader(filename);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                switch (reader.Name)
                                {
                                    case "MaxPeer":
                                        {
                                            MaxPeer = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "MaxRoom":
                                        {
                                            MaxRoom = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "Log":
                                        {
                                            string temp = reader.ReadString();
                                            Log = ("on".Equals(temp)) ? true : false;
                                            break;
                                        }
                                    case "Backlog":
                                        {
                                            Backlog = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "MaxTimeout":
                                        {
                                            MaxTimeout = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "Port":
                                        {
                                            Port = Int32.Parse(reader.ReadString());
                                            break;
                                        }
                                    case "IpAddress":
                                        {
                                            IpAddress = reader.ReadString();
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
                MaxPeer = 1000;
                MaxRoom = 100;
                Log = true;
                Backlog = 10000;
                MaxTimeout = 30000;
                Port = 9351;
                IpAddress = "127.0.0.1";
            }
        }

        public void SaveData(string filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("IpAddress", IpAddress.ToString());
            writer.WriteElementString("MaxPeer", MaxPeer.ToString());
            writer.WriteElementString("MaxRoom", MaxRoom.ToString());
            writer.WriteElementString("Log", (Log) ? "on" : "off");
            writer.WriteElementString("Backlog", Backlog.ToString());
            writer.WriteElementString("MaxTimeout", MaxTimeout.ToString());
            writer.WriteElementString("Port", Port.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        public void Print()
        {
            Logger.WriteLine("Current Settings:");
            Logger.WriteLine("Max Peer\t\t: " + MaxPeer);
            Logger.WriteLine("Max Room\t\t: " + MaxRoom);
            string log_state = (Log) ? "on" : "off";
            Logger.WriteLine("Log\t\t\t: " + log_state);
            Logger.WriteLine();
        }
    }
}
