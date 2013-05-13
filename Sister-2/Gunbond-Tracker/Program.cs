using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gunbond_Tracker.Util;

namespace Gunbond_Tracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Gunbond Tracker";
            Console.SetWindowSize(120, 40);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();

            Tracker tracker = new Tracker("config.xml");

            Logger.WriteLine();
            Logger.WriteLine("GunBond Tracker");
            Logger.WriteLine("----------------------------");
            Logger.WriteLine("Current Settings:");
            tracker.Configuration.Print();
            Logger.WriteLine("Tracker has started successfully...");
            Logger.WriteLine();

            bool check = true;
            while (check)
            {
                String input = Console.ReadLine();
                String[] parsed = input.Split(' ');
                if (parsed[0].ToLower().Equals("max_peer"))
                {
                    int max_peer;
                    if ((parsed.Length == 2) && (Int32.TryParse(parsed[1], out max_peer)))
                    {
                        tracker.Configuration.MaxPeer = max_peer;
                        Console.WriteLine("Max peer is set to " + max_peer + ".");
                        tracker.Configuration.Print();
                    }
                    else
                    {
                        Console.WriteLine("Parameter of max_peer is wrong.");
                    }
                }
                else if (parsed[0].ToLower().Equals("max_room"))
                {
                    int max_room;
                    if ((parsed.Length == 2) && (Int32.TryParse(parsed[1], out max_room)))
                    {
                        tracker.Configuration.MaxRoom = max_room;
                        Console.WriteLine("Max room is set to " + max_room + ".");
                        tracker.Configuration.Print();
                    }
                    else
                    {
                        Console.WriteLine("Parameter of max_room is wrong.");
                    }
                }
                else if (parsed[0].ToLower().Equals("log"))
                {
                    if ((parsed.Length == 2) && ("on".Equals(parsed[1].ToLower())))
                    {
                        tracker.Configuration.Log = true;
                        Logger.Active = tracker.Configuration.Log;
                        Console.WriteLine("Log is on.");
                        tracker.Configuration.Print();
                    }
                    else if ((parsed.Length == 2) && ("off".Equals(parsed[1].ToLower())))
                    {
                        tracker.Configuration.Log = false;
                        Logger.Active = tracker.Configuration.Log;
                        Console.WriteLine("Log is off.");
                        tracker.Configuration.Print();
                    }
                    else
                    {
                        Console.WriteLine("Parameter of log is wrong.");
                    }
                }
                else if (parsed[0].ToLower().Equals("shutdown"))
                {
                    check = false;
                }
                else
                {
                    Console.WriteLine("Command unrecognized.");
                }
            }

            tracker.Configuration.SaveData("config.xml");
            Console.WriteLine("Gunbond Tracker is going to turn off.");
        }
    }
}
