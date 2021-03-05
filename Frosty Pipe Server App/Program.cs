using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;



namespace Frosty_Pipe_Server
{
    class Program
    {
        private static bool isrunning = false;

        static void Main(string[] args)
        {
            
            Console.Title = "PIPE Server";
            Console.WriteLine("Enter Listening Port");
            int port = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter Max Players");
            int maxPlayers = int.Parse(Console.ReadLine());

            Console.WriteLine($"Port: {port} with max players: {maxPlayers}");
            Console.WriteLine("Any Key to Start");
            Console.Read();

            
            Server.Start(50, port);
            isrunning = true;

            Thread MainThread = new Thread(new ThreadStart(Mainthread));
            MainThread.Start();




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
