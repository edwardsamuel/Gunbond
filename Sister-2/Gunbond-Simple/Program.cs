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
            GunConsole gunConsole = new GunConsole(args[0]);
            Logger.Active = true;

            Logger.WriteLine();
            Logger.WriteLine("Gunbond Client");
            Logger.WriteLine("----------------------------");
            Logger.WriteLine("Current Settings:");
            gunConsole.Configuration.Print();
            Logger.WriteLine("Client has started successfully...");
            Logger.WriteLine();

            gunConsole.ConnectTracker();

            if (args.Length >= 2 && args[1] == "c")
            {
                gunConsole.CreateRoom("liluu", 4);
            }
            else
            {
                Console.ReadLine();
                var list = gunConsole.ListRooms();
                if (list != null)
                {
                    gunConsole.JoinRoom(list[0].roomId);
                }
            }

            Console.ReadLine();
            Logger.WriteLine("Send START 1");
            gunConsole.SEND_START(">>>" + gunConsole.PeerId + "<<<");
            gunConsole.SEND_START("???" + gunConsole.PeerId + "???");
            gunConsole.SEND_START("///" + gunConsole.PeerId + "\\\\\\");
            Logger.WriteLine("END Send START 1");
            Console.ReadLine();
            Logger.WriteLine("Send START 2");
            gunConsole.SEND_START(">>>" + gunConsole.PeerId + "<<<");
            gunConsole.SEND_START("???" + gunConsole.PeerId + "???");
            gunConsole.SEND_START("///" + gunConsole.PeerId + "\\\\\\");
            Logger.WriteLine("END Send START 2");
            Console.ReadLine();
            gunConsole.Quit();
            Console.ReadLine();
        }
    }
}
