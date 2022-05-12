
namespace FrostyPipeServer.ServerFiles
{
    public static class ThreadManager
    {
        static bool isrunning;
        static Thread? Sendingthread;
        static Thread? receivethread;
        static Thread? SystemThread;


        static int MSPerTickMax;
        static int MSPerTickMin;
        static int MSPerTickTarget = MSPerTickMax;

        public static void BootUp()
        {
            if (Servermanager.config == null)
            {
                Console.WriteLine("No Server config found, aborting....\n");
                return;
            }


            MSPerTickMax = Servermanager.config.TickrateMax > 10 ? 1000 / Servermanager.config.TickrateMax : 10;
            MSPerTickMin = Servermanager.config.TickrateMin > 0 ? 1000 / Servermanager.config.TickrateMin : 1;

            isrunning = true;
            StartSendthread();
            StartReceiveThread();
            StartServerSystemThread();
        }
        public static async Task ShutdownThreads()
        {
            isrunning = false;
            while (ThreadsActive())
            {
                await Task.Delay(500);
            }
        }
        public static void UpdateTickRate()
        {
            if (Servermanager.Players.Count < 1)
            {
                if (MSPerTickTarget != 1000 / Servermanager.config.StandbyTick)
                {
                    Console.WriteLine("Tick rate dropping to standby");
                    MSPerTickTarget = 1000 / Servermanager.config.StandbyTick;
                }
            }
            else
            {
                if (!Servermanager.config.DynamicTick)
                {
                    MSPerTickTarget = MSPerTickMax;
                    return;
                }

                // if changing from min, players arrived
                if (MSPerTickTarget == MSPerTickMin && Servermanager.Players.Count > 1)
                {
                 Console.WriteLine("Tick rate leaving standby");
                }

                // dynamic tick rate option
                int proposedrate = 0;

                List<int> playersFPS = new List<int>();
                List<int> PlayerRtts = new List<int>();
                foreach(Player player in Servermanager.Players.Values)
                {
                    playersFPS.Add(player.FPS);
                    if(player.connection != null)
                    {
                     PlayerRtts.Add((int)player.connection.RTT);
                    }
                }
                if (playersFPS.Count < 2 | PlayerRtts.Count < 2)
                {
                    MSPerTickTarget = MSPerTickMin;
                    return;
                }
                {
                    MSPerTickTarget = MSPerTickMax;
                    return;
                }
                int fps = (int)playersFPS.Average();
                int rtts = (int)PlayerRtts.Average();
                if(fps == 0 | rtts == 0)
                {
                    MSPerTickTarget = MSPerTickMax;
                    return;
                }

                int averageMS = 1000 / (int)playersFPS.Average();
                int averagerttMS = (int)PlayerRtts.Average();
                if(averageMS >= MSPerTickMax)
                {
                 MSPerTickTarget = MSPerTickMax;
                    return;
                }

                proposedrate = averageMS - (averageMS / 100 * Math.Clamp((int)Servermanager.config.DynamicTickMultiplier,1,100)) - (averagerttMS / 100 * Math.Clamp((int)Servermanager.config.DynamicTickMultiplier, 1, 100));

                MSPerTickTarget = Math.Clamp(proposedrate, MSPerTickMin, MSPerTickMax);
            }
        }
        public static int CurrentTickrate()
        {
            return (int)(1000 / MSPerTickTarget);
        }

        static bool ThreadsActive()
        {
            if(Sendingthread != null)
            {
                if (Sendingthread.IsAlive)
                {
                    return true;
                }
            }
            if(receivethread != null)
            {
                if (receivethread.IsAlive)
                {
                    return true;
                }
            }
            if (SystemThread != null)
            {
                if (SystemThread.IsAlive)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Server's Sending thread loop
        /// </summary>
        static void StartSendthread()
        {
            Sendingthread = new Thread(new ThreadStart(SendThread));
            Sendingthread.Start();

        }
        static void SendThread()
        {
            Console.WriteLine("Send thread booting");
            DateTime _nextloop = DateTime.Now;
            double timeout;

            while (isrunning)
            {
                    
                    Servermanager.RunOutgoingThread();
                    _nextloop = _nextloop.AddMilliseconds(MSPerTickTarget);
                    timeout = (_nextloop - DateTime.Now).TotalMilliseconds;
                    Servermanager.lastloopsendthread = (float)(MSPerTickTarget - timeout);
                    if (timeout > 0)
                    {
                        Servermanager.lastloopsendthreadtimeout = (float)timeout;
                        Thread.Sleep((int)timeout);
                    }
                
            }
        }

        /// <summary>
        /// Server's receving thread loop
        /// </summary>
        static void StartReceiveThread()
        {
            Console.WriteLine("Receive thread booting");
            receivethread = new Thread(new ThreadStart(ReceiveThread));
            receivethread.Start();
        }
        static void ReceiveThread()
        {
            DateTime nextloop = DateTime.Now;
            double timeout = 0;

            while (isrunning)
            {
                Servermanager.RunIncomingThread();
                nextloop = nextloop.AddMilliseconds(MSPerTickTarget);
                timeout = (nextloop - DateTime.Now).TotalMilliseconds;
                Servermanager.lastloopreceivethread = (float)(MSPerTickTarget - timeout);

                if (timeout >= 0)
                {
                    Servermanager.lastloopreceivethreadtimeout = (float)timeout;
                    Thread.Sleep((int)timeout);
                }

                   
            }
        }


        static void StartServerSystemThread()
        {
            SystemThread = new Thread(new ThreadStart(ServersystemThread));
            SystemThread.Start();
        }  
        static void ServersystemThread()
        {
            Console.WriteLine("System thread booting");
            DateTime nextloop = DateTime.Now;
            double timeout = 0;

            if (Servermanager.config.Publicise)
            {
                Servermanager.Publicise();
            }

            while (isrunning)
            {
                Servermanager.RunSystemThread();
                nextloop = nextloop.AddMilliseconds(1000 / Servermanager.config.SystemTickrate);
                timeout = (nextloop - DateTime.Now).TotalMilliseconds;

                if (timeout > 0)
                {
                    //Servermanager.lastloopreceivethreadtimeout = timeout;
                    Thread.Sleep((int)timeout);
                }
            }
        }

    }
}
