
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Windows.Forms;
using System.Diagnostics;



namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Console
    /// </summary>
    class Program
    {

        private static bool isrunning = false;


        static void Main(string[] args)
        {
            int Maxplayers;
            int port;

            Console.WriteLine($"PIPE ONLINE SERVER V{Server.VERSIONNUMBER}");
            Console.WriteLine("Powered by Valve's GamenetworkingSockets");
            Console.WriteLine("Checking Directories..");
           

            ServerData.LoadData();
            Console.WriteLine("Directories good");
            if (args.Length != 10)
            {
                Console.WriteLine("Please enter Max player count (1 - ~");
                Maxplayers = int.Parse(Console.ReadLine());

                Console.WriteLine("Please enter Port to listen on");
                port = int.Parse(Console.ReadLine());

                Console.WriteLine("Set Ticks per second, 30-60 recommended");

                Constants.TicksPerSec = int.Parse(Console.ReadLine());

                Console.WriteLine("Please enter an admin login password.. (in game, Use F12 key when in online mode to toggle Admin controls -- password is case sensitive)");
                ServerData.AdminPassword = Console.ReadLine();

                Console.WriteLine("Please enter a name for this server..");
                Server.SERVERNAME = Console.ReadLine();
                Console.WriteLine("Public Server? y/n");
                Server.Public_server = Console.ReadLine() == "y" ? true : false;
                Console.WriteLine("Specify address to API? y/n");
                Server.SpecifyAPI = Console.ReadLine() == "y"? true: false;
                if (Server.SpecifyAPI)
                {
                    Console.WriteLine("Enter post address:");
                    Server.posturl = Console.ReadLine();
                    Console.WriteLine("Enter Put address:");
                    Server.puturl = Console.ReadLine();
                    Console.WriteLine("Enter API Key (the key used by API):");
                    Server.APIKEY = Console.ReadLine();
                }
                Console.WriteLine($"Boot with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");
                Console.ReadLine();
            }
            else 
            {
                Maxplayers = int.Parse(args[0]);
                port = int.Parse(args[1]);
                Constants.TicksPerSec = int.Parse(args[2]);
                ServerData.AdminPassword = args[3];
                Server.SERVERNAME = args[4];        
                Server.Public_server = args[5] == "y"? true:false;
                Server.SpecifyAPI = args[6] == "y" ? true : false;
                if (Server.SpecifyAPI)
                {
                    if(args[6].Length>5 && args[7].Length > 5)
                    {
                      Server.posturl = args[7];
                      Server.puturl = args[8];
                      Server.APIKEY = args[9];

                    }

                }

                Console.WriteLine($"Auto Booted with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");


            }
            Thread _ProcessThread = new Thread(new ThreadStart(ProcessThread))
            {
                IsBackground = true
            };


            isrunning = true;
            _ProcessThread.Start();





            Console.OutputEncoding = Encoding.Unicode;
            Server.Run(port, Maxplayers);




        }







        /// <summary>
        /// Server's Secondary thread loop
        /// </summary>
        private static void ProcessThread()
        {
            Console.WriteLine($"Main Thread running at {Constants.TicksPerSec} ticks per second");
            DateTime _nextloop = DateTime.Now;


            while (isrunning)
            {
                while (_nextloop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextloop = _nextloop.AddMilliseconds(Constants.MSPerTick);

                    if (_nextloop > DateTime.Now)
                    {
                        Thread.Sleep(_nextloop - DateTime.Now);
                    }
                }
            }
        }
    




      







    }
}
