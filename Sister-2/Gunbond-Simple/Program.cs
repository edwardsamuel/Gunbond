using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gunbond;
using Gunbond_Client.Util;

namespace Gunbond_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            GunConsole gunConsole = new GunConsole("peerConf.xml");
            Logger.Active = true;
            gunConsole.ConnectTracker();
            // gunConsole.CreateRoom("liluu", 4);
            var list = gunConsole.ListRooms();
            Logger.WriteLine(list);
            if (list != null)
            {
                gunConsole.JoinRoom(list[0].roomId);
            }
            Console.ReadLine();
            gunConsole.SEND_START(">>>" + gunConsole.PeerId + "<<<");
            Console.ReadLine();
            gunConsole.SEND_START("???" + gunConsole.PeerId + "???");
            Console.ReadLine();
            gunConsole.SEND_START("///" + gunConsole.PeerId + "\\\\\\");
            Console.ReadLine();
        }
    }
}
