
namespace FrostyPipeServer.ServerFiles
{
    class ThreadTransfer
    {
        private static readonly List<Action> Tosendthread = new List<Action>();
        private static readonly List<Action> SendThreadsCopy = new List<Action>();
        private static bool sendactionsready = false;

        private static readonly List<Action> Tosystemthread = new List<Action>();
        private static readonly List<Action> SystemThreadsCopy = new List<Action>();
        private static bool systemactionsready = false;



        public static void GiveToSendThread(Action _action)
        {
            if (_action == null)
            {
                Console.WriteLine("No action to execute on main thread!");
                return;
            }

            lock (Tosendthread)
            {
                Tosendthread.Add(_action);
                sendactionsready = true;
            }
        }
        public static void RunSend()
        {
            if (sendactionsready)
            {
                SendThreadsCopy.Clear();
                lock (Tosendthread)
                {
                    SendThreadsCopy.AddRange(Tosendthread);
                    Tosendthread.Clear();
                    sendactionsready = false;
                }

                for (int i = 0; i < SendThreadsCopy.Count; i++)
                {
                    SendThreadsCopy[i]();
                }
            }
        }

        public static void GiveToSystemThread(Action _action)
        {
            if (_action == null)
            {
                Console.WriteLine("No action to execute on main thread!");
                return;
            }

            lock (Tosystemthread)
            {
                Tosystemthread.Add(_action);
                systemactionsready = true;
            }
        }
        public static void RunSystem()
        {
            if (systemactionsready)
            {
                SystemThreadsCopy.Clear();
                lock (Tosystemthread)
                {
                    SystemThreadsCopy.AddRange(Tosystemthread);
                    Tosystemthread.Clear();
                    systemactionsready = false;
                }

                for (int i = 0; i < SystemThreadsCopy.Count; i++)
                {
                    SystemThreadsCopy[i]();
                }
            }
        }


    }
}
