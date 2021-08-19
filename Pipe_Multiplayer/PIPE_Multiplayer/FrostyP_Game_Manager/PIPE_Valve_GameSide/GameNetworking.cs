using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Sockets;
using System.Threading;
using System.Net;
using FrostyP_Game_Manager;
using System.Diagnostics;



namespace PIPE_Valve_Console_Client
{
    public class GameNetworking
    {

		public float VERSIONNUMBER { get; } = 2.16f;
		public static GameNetworking instance;
		public bool ServerLoopIsRunning = false;

		// Encrypted key to share
		public string Key = "";

		/// <summary>
		/// All Outgoing and incoming networking is done on this thread, all data is processed by Unity thread before/after
		/// </summary>
	   public Thread ServerThread;


		// ip to connect to, default local
		public string ip = "127.0.0.1";
		public int port;
		public string FrostyIP = "";
		public int frostyport = 4130;


		

		/// <summary>
		/// Contains callbacks that happen on state change and other stuff
		/// </summary>
		public NetworkingUtils utils;
		/// <summary>
		/// The Socket itself used to send and receive through ValveSockets
		/// </summary>
		public NetworkingSockets Socket;
		/// <summary>
		/// connection number of server, only ever one connection for client, client.sendmessage(connection,any byte[], reference a flag)
		/// </summary>
		public uint ServerConnection = 0;

		const int maxMessages = 256;

		NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

		/// <summary>
		/// callbacks from connection
		/// </summary>
		StatusCallback status;

		/// <summary>
		/// Any function that takes a packet as an argument (ClientHandles)
		/// </summary>
		/// <param name="_packet"></param>
		public delegate void PacketHandler(Packet _packet);
		/// <summary>
		/// On receive message, fire packethandler corresponding to the int read from start of message
		/// </summary>
		public static Dictionary<int, PacketHandler> packetHandlers;


		
		


		public void Start()
        {
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				//Debug.Log("Instance already exists, destroying object!");
				
			}

			// list of functions linking to incoming int codes
			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ServerPacket.Welcome,ClientHandle.Welcome },
				{ (int)ServerPacket.ReceiveTransformUpdate,ClientHandle.PlayerPositionReceive },
				{ (int)ServerPacket.SetupAPlayer,ClientHandle.SetupPlayerReceive},
				{ (int)ServerPacket.requestTextures,ClientHandle.RequestForFile},
				{ (int)ServerPacket.SendTexturetoplayer,ClientHandle.ReceiveFileSegment},
				{ (int)ServerPacket.DisconnectedPlayer,ClientHandle.PlayerDisconnected},
				{ (int)ServerPacket.ReceiveAudioForPlayer,ClientHandle.ReceiveAudioForaPlayer},
				{ (int)ServerPacket.IncomingTextMessage, ClientHandle.IncomingTextMessage},
				{ (int)ServerPacket.RequestForAllParts, ClientHandle.RequestForAllParts},
				{ (int)ServerPacket.GearUpdate, ClientHandle.GearUpdate},
				{ (int)ServerPacket.ReceiveSetupAllOnlinePlayers, ClientHandle.SetupAllOnlinePlayers},
				{ (int)ServerPacket.ReceiveMapName, ClientHandle.ReceiveMapname},
				{ (int)ServerPacket.disconnectme, ClientHandle.Disconnectme},
				{ (int)ServerPacket.Logingood, ClientHandle.LoginGood},
				{ (int)ServerPacket.SpawnAnObjectReceive, ClientHandle.SpawnAnObjectReceive},
				{ (int)ServerPacket.DestroyAnObject, ClientHandle.DestroyAnObject},
				{ (int)ServerPacket.MoveAnObject, ClientHandle.MoveAnObject},
				{ (int)ServerPacket.FileStatus, ClientHandle.FileStatus},
				{ (int)ServerPacket.Update, ClientHandle.Update},
				{ (int)ServerPacket.AdminStream, ClientHandle.AdminStream},
				{ (int)ServerPacket.InviteToSpawn,ClientHandle.InviteToSpawn},


			};

			

		}

		/// <summary>
		/// Receiving System loop for network
		/// </summary>
		public void Run()
        {
			
                

			// create new message array each loop
			netMessages = new NetworkingMessage[maxMessages];





			// Do incoming receive of messages on Server thread, send each to Unity thread
			int netMessagesCount = Socket.ReceiveMessagesOnConnection(ServerConnection, netMessages, maxMessages);
			
				
				if (netMessagesCount > 0)
				{

					
				    for (int i = 0; i < netMessagesCount; i++)
					{
						ref NetworkingMessage netMessage = ref netMessages[i];


						byte[] bytes = new byte[netMessage.length];
						netMessage.CopyTo(bytes);
					
	 				
			           SendToUnityThread.instance.ExecuteOnMainThread(() =>
				       {

						 using (Packet _packet = new Packet(bytes))
						 {
							   int _packetId = _packet.ReadInt();


						    packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet, see Packet class on both server and game side to find codes, see Start() for setup of Gameside Codes

							  
						 }

					
				       });


						

						netMessage.Destroy();
					}
				

				

				}




			// do connection status on tickrate to log to IngameGUI info about ping, connection state etc, alternative to client.runcallbacks()
			 ConnectionStatus();
			

			
			
		
		}


		/// <summary>
		/// Master connect, takes care of everything except GUI's setup, GUI's connect does setup then calls this
		/// </summary>
		 public void ConnectMaster()
        {
            try
            {
			Library.Initialize();
			utils = new NetworkingUtils();
			Socket = new NetworkingSockets();
			
				utils.SetStatusCallback(status);

				string _ip = ip.Replace(" ", "");
				

				Address address = new Address();
			address.SetAddress(_ip,(ushort)port);
			ServerConnection = Socket.Connect(ref address);
				int sendRateMin = 400000;
				int sendRateMax = 12048576;
				int sendBufferSize = 40485760;
				//int MTUDatasize = 600000;
				//int MTUPacketsize = 600000;

				unsafe
			{
				utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(ServerConnection), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
				utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(ServerConnection), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
				utils.SetConfigurationValue(ConfigurationValue.SendBufferSize, ConfigurationScope.ListenSocket, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&sendBufferSize));
				//utils.SetConfigurationValue(ConfigurationValue.MTUDataSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUDatasize));
				//utils.SetConfigurationValue(ConfigurationValue.MTUPacketSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUPacketsize));
			}




				if (ServerThread == null && !ServerLoopIsRunning)
				{
					ServerLoopIsRunning = true;
					ServerThread = new Thread(NetWorkThreadLoop)
					{
						IsBackground = true
					};
					ServerThread.Start();
				}
				if (ServerThread != null && !ServerLoopIsRunning)
				{
					ServerLoopIsRunning = true;
					ServerThread.Start();
				}


			}
			catch(Exception x)
            {
				UnityEngine.Debug.Log("Thread start error   :" + x);	
            }
			
		}


		/// <summary>
		/// Uses DNS
		/// </summary>
		public void ConnectFrosty()
		{
			try
			{
				Library.Initialize();
				utils = new NetworkingUtils();
				Socket = new NetworkingSockets();

				utils.SetStatusCallback(status);

				

				
				
					try
					{
						IPHostEntry hostInfo = Dns.GetHostEntry("b6828e9.online-server.cloud");
						foreach (IPAddress _address in hostInfo.AddressList)
						{
							if (_address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
							{
							FrostyIP = _address.ToString();
							}
						}
					}
					catch (Exception) { }
				


				Address address = new Address();
				address.SetAddress(FrostyIP, (ushort)frostyport);
				ServerConnection = Socket.Connect(ref address);
				int sendRateMin = 400000;
				int sendRateMax = 12048576;
				int sendBufferSize = 40485760;
				//int MTUDatasize = 600000;
				//int MTUPacketsize = 600000;

				unsafe
				{
					utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(ServerConnection), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
					utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(ServerConnection), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
					utils.SetConfigurationValue(ConfigurationValue.SendBufferSize, ConfigurationScope.ListenSocket, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&sendBufferSize));
					//utils.SetConfigurationValue(ConfigurationValue.MTUDataSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUDatasize));
					//utils.SetConfigurationValue(ConfigurationValue.MTUPacketSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUPacketsize));
				}


				if (ServerThread == null && !ServerLoopIsRunning)
				{
					ServerLoopIsRunning = true;
					ServerThread = new Thread(NetWorkThreadLoop)
					{
						IsBackground = true
					};
					ServerThread.Start();
				}
                if(ServerThread != null && !ServerLoopIsRunning)
                {
					ServerLoopIsRunning = true;
					ServerThread.Start();
                }
				


			}
			catch (Exception x)
			{
				UnityEngine.Debug.Log("Thread start error   :" + x);
			}

		}








		/// <summary>
		/// Master disconnect, closes down all networking
		/// </summary>
		public void DisconnectMaster()
        {
            try
            {
			ServerLoopIsRunning = false;
			Socket.CloseConnection(ServerConnection);
			Socket.FlushMessagesOnConnection(ServerConnection);
			utils = null;
			Socket = null;
			status = null;

				ServerThread.Join();

				
			   ServerThread = null;

			Library.Deinitialize();

            }
            catch (System.Exception x)
            {
				UnityEngine.Debug.Log("Thread end error  : " + x);
            }
		}



		private Stopwatch NetThreadWatch;
		/// <summary>
		/// This function is running on the ProcessThread, This is unable to use any Unity API, therfore to transfer commands use processthreadmanager, Fixedupdate will come along and run them. 
		/// </summary>
		public void NetWorkThreadLoop()
		{
			// Tell main thread ive started
			SendToUnityThread.instance.ExecuteOnMainThread(() =>
			{
				UnityEngine.Debug.Log("Thread Started");
				InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server thread Started up", 1, 0));
			});




			NetThreadWatch = new Stopwatch();
			NetThreadWatch.Start();
			
		
			while (ServerLoopIsRunning)
			{
			 


				// this is the fixedupdate of server thread
				try
				{
					
					NetworkThread.Update();
				}
				catch (System.Exception x)
				{
					SendToUnityThread.instance.ExecuteOnMainThread(() =>
					{
						UnityEngine.Debug.Log("Network update issue : " + x);
						//InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server conflict", 1, 0));
					});

				}
				if (Constants.MSPerTick - (int)NetThreadWatch.ElapsedMilliseconds < 100 && Constants.MSPerTick - (int)NetThreadWatch.ElapsedMilliseconds > 0)
				{
					

					Thread.Sleep(Constants.MSPerTick - (int)NetThreadWatch.ElapsedMilliseconds);
					NetThreadWatch.Reset();
				}
				else
				{
					NetThreadWatch.Reset();
					Thread.Sleep(Constants.MSPerTick);

				}





			}





			NetThreadWatch.Reset();
			NetThreadWatch.Stop();
			SendToUnityThread.instance.ExecuteOnMainThread(() =>
			{
				UnityEngine.Debug.Log("Thread Ended");
				InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server Thread ending", 1, 0));
			});
		}


		// Data about the connection to Server, called on tick
		public void ConnectionStatus()
        {
			ConnectionStatus constat = new ConnectionStatus();
			Socket.GetQuickConnectionStatus(ServerConnection, ref constat);

			// if theres no connection, trigger full end with cleanup
			if(constat.state == Valve.Sockets.ConnectionState.None | constat.state == Valve.Sockets.ConnectionState.ProblemDetectedLocally | constat.state == Valve.Sockets.ConnectionState.ClosedByPeer)
            {
                if (InGameUI.instance.Connected)
                {
					InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Connection lost!", 4, 0));
					SendToUnityThread.instance.ExecuteOnMainThread(() =>
					{
					InGameUI.instance.Minigui = false;
					InGameUI.instance.OnlineMenu = true;
						InGameUI.instance.OfflineMenu = false;
						FrostyPGamemanager.instance.OpenMenu = true;
					    InGameUI.instance.Connected = false;
						InGameUI.instance.Disconnect();
						InGameUI.instance.ShutdownAfterMessageFromServer();
					});
				}
            }



			// if there is a connection and were in online mode, extract data from the Valve quick connectionstatus
			if(InGameUI.instance.Connected && constat.state == Valve.Sockets.ConnectionState.Connected)
            {
				SendToUnityThread.instance.ExecuteOnMainThread(() =>
				{
					InGameUI.instance.LastPing = InGameUI.instance.Ping;
					InGameUI.instance.Ping = constat.ping;
					InGameUI.instance.Pendingreliable = constat.pendingReliable;
					InGameUI.instance.Pendingunreliable = constat.pendingUnreliable;
					InGameUI.instance.Outbytespersec = constat.outBytesPerSecond;
					InGameUI.instance.InBytespersec = constat.inBytesPerSecond;
					InGameUI.instance.connectionstate = constat.state;
					InGameUI.instance.connectionqualitylocal = constat.connectionQualityLocal;
					InGameUI.instance.connectionqualityremote = constat.connectionQualityRemote;
					InGameUI.instance.SendRate = constat.sendRateBytesPerSecond;
					
					
				});
				
				
            }


		}

		public int GetPendingReliable()
        {
			ConnectionStatus constat = new ConnectionStatus();
			Socket.GetQuickConnectionStatus(ServerConnection, ref constat);


			return constat.pendingReliable;
		}


	}
}
