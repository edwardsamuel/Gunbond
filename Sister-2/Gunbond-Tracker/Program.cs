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

                if (parsed[0].ToLower().Equals("shutdown"))
                {
                    check = false;
                }
            }

            tracker.Configuration.SaveData("config.xml");
            Console.WriteLine("GunBond Tracker is going to turn off.");
        }
    }
}
