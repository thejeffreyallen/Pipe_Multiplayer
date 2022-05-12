using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Threading;
using System.Diagnostics;

namespace FrostyP_Game_Manager
{
    public class RiptideManager 
    {
        public static RiptideManager instance;
        string IP = "127.0.0.1";
        string PORT = "7777";
        public Client client;
        public float VERSIONNUMBER { get; } = 2.17f;
        Thread NetThread;
        Thread systemThread;
        bool isRunning;
        public int ServerMaxTick = 0;
        
        public RiptideManager()
        {
            Init();
        }

        #region System Functions
        void Init()
        {
            instance = this;
            UnityEngine.Debug.Log("Starting Riptide..");
            try
            {
                RiptideLogger.Initialize(GiveToUnityLog, GiveToUnityLog, GiveToUnityLog, GiveToUnityLog, true);
                MultiplayerConfig config = MultiplayerManager.GetConfig();

            }
            catch (System.TypeLoadException x)
            {
                UnityEngine.Debug.Log("Riptide:  " + x);
            }
        }
        public void RunRiptideReceive()
        {
            if (client == null) return;
            client.Tick();
        }
        public void RunRiptideSend()
        {
            SendToServerThread.UpdateMain();
        }
        public void ConnectMaster(string ip, string port)
        {
            IP = ip.Trim();
            PORT = port.Trim();
            isRunning = true;
            NetThread = new Thread(new ThreadStart(NetworkThread));
            systemThread = new Thread(new ThreadStart(SystemThread));
            NetThread.Start();
            systemThread.Start();
        }
        public void DisconnectMaster()
        {
            isRunning = false;
            if (client != null)
            {
                client.Disconnect();
            }

        }
        void NetworkThread()
        {
            
            // Tell main thread ive started
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Networking thread started");
            });

            try
            {
                Message.MaxPayloadSize = 100000;
                client = new Client();
                client.Connected += OnConnectionHappened;
                client.Disconnected += OnLostServerConnection;
                client.ConnectionFailed += OnConnectionFail;
                client.MessageReceived += AnyMessageReceived;
                client.Connect($"{IP}:{PORT}");
            }
            catch (Exception x)
            {
                SendToUnityThread.instance.ExecuteOnMainThread(() =>
                {
                    UnityEngine.Debug.Log("Server thread Client connect error : " + x);
                });
                return;
            }




            DateTime nextloop = DateTime.Now;
            while (isRunning)
            {
                // this is the fixedupdate of server thread
                try
                {
                    RunRiptideReceive();
                    RunRiptideSend();

                }
                catch (System.Exception x)
                {
                    SendToUnityThread.instance.ExecuteOnMainThread(() =>
                    {
                        UnityEngine.Debug.Log("Network update issue : " + x);
                        //InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server conflict", 1, 0));
                    });

                }



                // sleep
                int servermilliseconds = ServerMaxTick > 0 ? (1000 / ServerMaxTick) : Constants.MSPerTick;
                nextloop = nextloop.AddMilliseconds(servermilliseconds);
                double timeout = (nextloop - DateTime.Now).TotalMilliseconds;
                if (timeout > 0)
                {
                    Thread.Sleep((int)timeout);
                }
               
            }

           
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Networking thread ended");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Networking thread ending", FrostyUIColor.System, 0));
            });
        }
        void SystemThread()
        {
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Network system thread started");
            });
            while (isRunning)
            {
                SystemUpdate();
                Thread.Sleep(1000);
            }
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Network system thread ended");
            });

        }
        /// <summary>
        /// lets riptidelog send to unity thread
        /// </summary>
        /// <param name="log"></param>
        void GiveToUnityLog(string log)
        {
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log(log);
            });
        }
        void SystemUpdate()
        {

            if(client != null && client.IsConnected)
            {
                SendToUnityThread.instance.ExecuteOnMainThread(() =>
                {
                   // UnityEngine.Debug.Log("Updating Network data");
                    MultiplayerManager.KBoutpersec = client.KBOutPerSec();
                    MultiplayerManager.KBinpersec = client.KBInPerSec();
                    MultiplayerManager.Ping = client.RTT > 0 ? client.RTT / 2:0;
                    MultiplayerManager.PendingReliable = client.PendingReliable();
                    MultiplayerManager.ActiveFragments = client.FragmentCount();
                    MultiplayerManager.TransformDump = 0;
                    MultiplayerManager.AudioDump = 0;
                });
            }
        }
        #endregion

        public int PendingReliable()
        {
            if (client == null) return 0;
            return client.PendingReliable();
        }
        public bool isConnected()
        {
            if(client == null) return false;
            if (!NetThread.IsAlive) return false;
            return client.IsConnected;
        }
        public bool isConnecting()
        {
            if (client == null) return false;
            if (!NetThread.IsAlive) return false;
            return client.IsConnecting;
        }

        #region ConnectionState Callbacks
        static void OnConnectionHappened(object sender, EventArgs e)
        {
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Connected to server, waiting for server config..");
            });
            
        }
        static void OnLostServerConnection(object sender, EventArgs e)
        {
            RiptideLogger.Log(LogType.info, "server timeout");
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("server not repsonding");
                MultiplayerManager.DisconnectMaster();
            });
        }
        static void OnConnectionFail(object sender, EventArgs e)
        {
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                UnityEngine.Debug.Log("Connection failed..");
                MultiplayerManager.DisconnectMaster();
            });
        }
        static void AnyMessageReceived(object sender,EventArgs e)
        {

        }

        #endregion

    }
}