﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Sockets;
using System.Threading;
using System.Net;
using FrostyP_Game_Manager;
using System.Diagnostics;
//using UnityEngine;



namespace PIPE_Valve_Console_Client
{
    public class GameNetworking
    {

		public float VERSIONNUMBER { get; } = 2.1f;


		// this class accessable anywhere
		public static GameNetworking instance;
		public bool ServerLoopIsRunning = false;

		// Encrypted key to share
		public string Key = "";

		/// <summary>
		/// Server is looping in NetworkThreadLoop
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
		public NetworkingSockets client;

		const int maxMessages = 256;

		NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

		/// <summary>
		/// connection number of server, only ever one connection for client, client.sendmessage(connection,any byte[], reference a flag)
		/// </summary>
		public uint connection = 0;
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


			


			//Debug.Log("Initialising GameNetworking..");

			// list of functions linking to incoming int codes
			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ServerPacket.Welcome,ClientHandle.Welcome },
				{ (int)ServerPacket.ReceiveTransformUpdate,ClientHandle.PlayerPositionReceive },
				{ (int)ServerPacket.SetupAPlayer,ClientHandle.SetupPlayerReceive},
				{ (int)ServerPacket.RequestTexNames,ClientHandle.RequestforDaryienTexNamesReceive},
				{ (int)ServerPacket.requestTextures,ClientHandle.RequestForTextures},
				{ (int)ServerPacket.ReceiveTextureforPlayer,ClientHandle.ReceiveTexture},
				{ (int)ServerPacket.DisconnectedPlayer,ClientHandle.PlayerDisconnected},
				{ (int)ServerPacket.ReceiveAudioForPlayer,ClientHandle.ReceiveAudioForaPlayer},
				{ (int)ServerPacket.IncomingTextMessage, ClientHandle.IncomingTextMessage},
				{ (int)ServerPacket.RequestForAllParts, ClientHandle.RequestForAllParts},
				{ (int)ServerPacket.BikeQuickUpdate, ClientHandle.BikeQuickupdate},
				{ (int)ServerPacket.RiderQuickUpdate, ClientHandle.RiderQuickupdate},
				{ (int)ServerPacket.ReceiveSetupAllOnlinePlayers, ClientHandle.SetupAllOnlinePlayers},
				{ (int)ServerPacket.ReceiveMapName, ClientHandle.ReceiveMapname},
				{ (int)ServerPacket.disconnectme, ClientHandle.Disconnectme},


			};





			
			status = (ref StatusInfo info) => {
				switch (info.connectionInfo.state)
				{
					case ConnectionState.None:
						break;

					case ConnectionState.Connected:
						SendToUnityThread.instance.ExecuteOnMainThread(() =>
						{
							InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Connected To Server", (int)MessageColour.System, 1));
						});
						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						client.CloseConnection(connection);
						SendToUnityThread.instance.ExecuteOnMainThread(() =>
						{
							InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Disconnected from Server", (int)MessageColour.System, 1));
						});
						break;
				}
			};

			//Debug.Log("GameNetworking Startup Complete");

		}


		


		/// <summary>
		/// Receiving System loop for network
		/// </summary>
		public void Run()
        {
			
				//client.RunCallbacks(); //=====================================================================    this will provide callbacks about connection, disconnection, data about current
				//GC.KeepAlive(status);
                

#if VALVESOCKETS_SPAN
	MessageCallback message = (in NetworkingMessage netMessage) => {
		Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else

#endif


#if VALVESOCKETS_SPAN
		client.ReceiveMessagesOnConnection(connection, message, 20);
#else
			netMessages = new NetworkingMessage[maxMessages];

			// Do incoming on Server thread, send to unity
			int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);
			
				// if theres messages, send Back to Unity Thread for processing, neccessary for anything that uses Untiy API even debug.log for some reason, maybe because its outside assemblyC
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
							
								packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
						 }

					
				       });


						//Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

						netMessage.Destroy();
					}
				

				

				}



#endif

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
			client = new NetworkingSockets();
			
				utils.SetStatusCallback(status);

				string _ip = ip.Replace(" ", "");
				

				Address address = new Address();
			address.SetAddress(_ip,(ushort)port);
			connection = client.Connect(ref address);
				int sendRateMin = 400000;
				int sendRateMax = 1048576;
				int sendBufferSize = 10485760;


				unsafe
			{
				utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
				utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
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



		public void ConnectFrosty()
		{
			try
			{
				Library.Initialize();
				utils = new NetworkingUtils();
				client = new NetworkingSockets();

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
				connection = client.Connect(ref address);
				int sendRateMin = 400000;
				int sendRateMax = 1048576;
				int sendBufferSize = 10485760;


				unsafe
				{
					utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
					utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
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
			client.CloseConnection(connection);
			client.FlushMessagesOnConnection(connection);
			utils = null;
			client = null;
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

		private Stopwatch watch;

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

			watch = new Stopwatch();
			watch.Start();
			// while running, update at tick rate
			// while running, update at tick rate
			while (ServerLoopIsRunning)
			{
				watch.Reset(); watch.Start();


				// this is the fixedupdate of server thread
				try
				{
					ServerUpdate.Update();
				}
				catch (System.Exception x)
				{
					SendToUnityThread.instance.ExecuteOnMainThread(() =>
					{
						UnityEngine.Debug.Log("Server update issue : " + x);
						InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server conflict", 1, 0));
					});

				}
				if (Constants.MSPerTick - (int)watch.ElapsedMilliseconds < 100 && Constants.MSPerTick - (int)watch.ElapsedMilliseconds > 0)
				{
					Thread.Sleep(Constants.MSPerTick - (int)watch.ElapsedMilliseconds);
				}
				else
				{
					watch.Reset();
					Thread.Sleep(Constants.MSPerTick);

				}





			}





			watch.Reset();
			SendToUnityThread.instance.ExecuteOnMainThread(() =>
			{
				UnityEngine.Debug.Log("Thread Ended");
				InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server Thread ending", 1, 0));
			});
		}


		public void ConnectionStatus()
        {
			ConnectionStatus constat = new ConnectionStatus();
			client.GetQuickConnectionStatus(connection, ref constat);

			// if theres no connection, trigger full end with cleanup
			if(constat.state == Valve.Sockets.ConnectionState.None | constat.state == Valve.Sockets.ConnectionState.ProblemDetectedLocally | constat.state == Valve.Sockets.ConnectionState.None)
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
						InGameUI.instance.Waittoend();
					});
				}
            }

			if(InGameUI.instance.Connected && constat.state == Valve.Sockets.ConnectionState.Connected)
            {
				InGameUI.instance.Ping = constat.ping;
				//InGameUI.instance.Outbytespersec = constat.outBytesPerSecond;
				//InGameUI.instance.InBytespersec = constat.inBytesPerSecond;
				
				//InGameUI.instance.SendBytesPersec = constat.sendRateBytesPerSecond;
            }


		}




	}
}
