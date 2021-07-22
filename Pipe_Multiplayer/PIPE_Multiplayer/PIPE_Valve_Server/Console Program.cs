
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using PIPE_Server_GUI;
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
            if (args.Length != 4)
            {
                Console.WriteLine("Please enter Max player count (1 - ~");
                Maxplayers = int.Parse(Console.ReadLine());

                Console.WriteLine("Please enter Port to listen on");
                port = int.Parse(Console.ReadLine());

                Console.WriteLine("Set Ticks per second, 30-60 recommended");

                Constants.TicksPerSec = int.Parse(Console.ReadLine());

                Console.WriteLine("Please enter an admin login password.. (in game, Use A key when in online mode to toggle Admin controls -- password is case sensitive)");
                ServerData.AdminPassword = Console.ReadLine();


                Console.WriteLine($"Boot with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");
                Console.ReadLine();
            }
            else {
                Maxplayers = int.Parse(args[0]);
                port = int.Parse(args[1]);
                Constants.TicksPerSec = int.Parse(args[2]);
                ServerData.AdminPassword = args[3];
                Console.WriteLine($"Auto Booted with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");


            }
            Thread _ProcessThread = new Thread(new ThreadStart(ProcessThread))
            {
                IsBackground = true
            };


            isrunning = true;
            _ProcessThread.Start();






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
