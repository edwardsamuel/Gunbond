using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gunbond;
using Gunbond_Client.Util;
using Gunbond_Client;
using GunBond_Client;

namespace Gunbond_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Game1.main_console = new GunConsole(args[0]);
            Logger.Active = true;

            Logger.WriteLine();
            Logger.WriteLine("Gunbond Client");
            Logger.WriteLine("----------------------------");
            Logger.WriteLine("Current Settings:");
            Game1.main_console.Configuration.Print();
            Logger.WriteLine("Client has started successfully...");
            Logger.WriteLine();
            
            Game1 game = new Game1();
            game.Run();
        }
    }
}
