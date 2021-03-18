

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

            // Check directories exist on startup, if not create
            DirectoryInfo texinfo = new DirectoryInfo(Server.TexturesDir);
            DirectoryInfo info = new DirectoryInfo(Server.Rootdir);
            if (!info.Exists)
            {
                info.Create();
                texinfo.Create();
            }


            Console.WriteLine("PIPE ONLINE SERVER V1.0.0");
			Console.WriteLine("Please enter Max player count (1 - ~");
			int Maxplayers = int.Parse(Console.ReadLine());

			Console.WriteLine("Please enter Port to listen on");
			int port = int.Parse(Console.ReadLine());

			Console.WriteLine($"Boot with maxplayers: {Maxplayers} and port: {port}");
			Console.ReadLine();

            Thread _ProcessThread = new Thread(new ThreadStart(ProcessThread));
        _ProcessThread.Start();
            isrunning = true;

			Server.Run(port,Maxplayers);



			
		}






        

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
