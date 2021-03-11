

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



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

			Console.WriteLine("PIPE ONLINE SERVER V1.0.0");
			Console.WriteLine("Please enter Max player count (1 - ~");
			int Maxplayers = int.Parse(Console.ReadLine());

			Console.WriteLine("Please enter Port to listen on");
			int port = int.Parse(Console.ReadLine());

			Console.WriteLine($"Boot with maxplayers: {Maxplayers} and port: {port}");
			Console.ReadLine();

            Thread MainThread = new Thread(new ThreadStart(Mainthread));
        MainThread.Start();
            isrunning = true;

			Server.Run(port,Maxplayers);



			
		}






        

    private static void Mainthread()
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
