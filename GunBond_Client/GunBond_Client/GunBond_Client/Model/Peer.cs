using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public Vector2 Position;

        public bool IsAlive
        {
            get;
            set;
        }

        public Color Color
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public float Power
        {
            get;
            set;
        }

        public float Health
        {
            get;
            set;
        }

        public Texture2D CarriageTexture
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

            this.IsAlive = true;
            this.Angle = MathHelper.ToRadians(90);
            this.Power = 100;
            this.Health = 500;
            this.Position = new Vector2();
        }
    }
}
