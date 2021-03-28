

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;



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

            Console.WriteLine("PIPE ONLINE SERVER V1.0.0");
            Console.WriteLine("Powered by Valve's GamenetworkingSockets");
            Console.WriteLine("Checking Directories..");
            // Check directories info on startup, if not create
            DirectoryInfo texinfo = new DirectoryInfo(Server.TexturesDir);
            DirectoryInfo info = new DirectoryInfo(Server.Rootdir);
            if (!info.Exists)
            {
                info.Create();
                texinfo.Create();
            }
            Console.WriteLine("Directories good");
            if (args.Length != 3)
            {
                Console.WriteLine("Please enter Max player count (1 - ~");
                Maxplayers = int.Parse(Console.ReadLine());

                Console.WriteLine("Please enter Port to listen on");
                port = int.Parse(Console.ReadLine());

                Console.WriteLine("Set Ticks per second, 30-60 recommended");

                Constants.TicksPerSec = int.Parse(Console.ReadLine());



                Console.WriteLine($"Boot with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");
                Console.ReadLine();
            }
            else {
                Maxplayers = int.Parse(args[0]);
                port = int.Parse(args[1]);
                Constants.TicksPerSec = int.Parse(args[2]);
                Console.WriteLine($"Boot with maxplayers: {Maxplayers}, port: {port}, tick rate: {Constants.TicksPerSec}");


            }
            Thread _ProcessThread = new Thread(new ThreadStart(ProcessThread))
            {
                IsBackground = true
            };

            
            isrunning = true;
        _ProcessThread.Start();

			Server.Run(port,Maxplayers);

          

			
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
