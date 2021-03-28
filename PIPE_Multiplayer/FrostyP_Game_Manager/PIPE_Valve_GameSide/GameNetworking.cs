using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Sockets;
using System.Threading;
using System.Net;
using UnityEngine;



namespace PIPE_Valve_Console_Client
{
    public class GameNetworking
    {
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
				Debug.Log("Instance already exists, destroying object!");
				
			}


			


			Debug.Log("Initialising GameNetworking..");

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
				{ (int)ServerPacket.RequestformyBike, ClientHandle.RequestforMyBike},
				{ (int)ServerPacket.BikeQuickUpdate, ClientHandle.BikeQuickupdate},
				{ (int)ServerPacket.RiderQuickUpdate, ClientHandle.RiderQuickupdate},


			};


			
			Debug.Log("GameNetworking Startup Complete");

        }


		


		/// <summary>
		/// Receiving System loop for network
		/// </summary>
		public void Run()
        {
			client.RunCallbacks();

			status = (ref StatusInfo info) => {
				switch (info.connectionInfo.state)
				{
					case ConnectionState.None:
						break;

					case ConnectionState.Connected:
						SendToUnityThread.instance.ExecuteOnMainThread(() =>
						{
							Debug.Log("connected to server - ID: " + connection);
						});
						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						client.CloseConnection(connection);
						SendToUnityThread.instance.ExecuteOnMainThread(() =>
						{
							Debug.Log("Disconnected from Server - ID: " + connection);
						});
						break;
				}
			};


			

#if VALVESOCKETS_SPAN
	MessageCallback message = (in NetworkingMessage netMessage) => {
		Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else

#endif


#if VALVESOCKETS_SPAN
		client.ReceiveMessagesOnConnection(connection, message, 20);
#else


			SendToUnityThread.instance.ExecuteOnMainThread(() =>
				{
			    // Do incoming on Unity thread
				int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);


			    // if theres messages, send Back to Unity Thread for processing, neccessary for anything that uses Untiy API even debug.log for some reason, maybe because its outside assemblyC
				if (netMessagesCount > 0)
				{

					
				for (int i = 0; i < netMessagesCount; i++)
					{
						ref NetworkingMessage netMessage = ref netMessages[i];


						byte[] bytes = new byte[netMessage.length];
						netMessage.CopyTo(bytes);

					
						using (Packet _packet = new Packet(bytes))
						{
							int _packetId = _packet.ReadInt();
							
								packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
						}

					


						//Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

						netMessage.Destroy();
					}
				

				

				}
				});

			

#endif
			
			

			
			
		
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
			netMessages = new NetworkingMessage[maxMessages];

			utils.SetStatusCallback(status);

			Address address = new Address();
			address.SetAddress(ip,(ushort)port);
			connection = client.Connect(ref address);
			int sendRateMin = 600000;
			int sendRateMax = 25400000;
			int sendBufferSize = 409715200;
			

			unsafe
			{
				utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
				utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(connection), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
				//utils.SetConfigurationValue(ConfigurationValue.SendBufferSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&sendBufferSize));
				//utils.SetConfigurationValue(ConfigurationValue.MTUDataSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUDatasize));
				//utils.SetConfigurationValue(ConfigurationValue.MTUPacketSize, ConfigurationScope.Global, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&MTUPacketsize));
			}

			if(ServerThread == null)
            {
			ServerLoopIsRunning = true;
			ServerThread = new Thread(NetWorkThreadLoop)
			{
				IsBackground = true
			};
			ServerThread.Start();
            }
            else
            {
                if (ServerThread.IsAlive)
                {
					ServerLoopIsRunning = false;
                }
				ServerThread.Abort();
				ServerThread = null;
				ServerThread = new Thread(NetWorkThreadLoop)
				{
					IsBackground = true
				};
				ServerLoopIsRunning = true;
				ServerThread.Start();

			}
			

            }
			catch(Exception x)
            {
				Debug.Log("Error on Connect click : " + x);
            }
			
		}

		/// <summary>
		/// Master disconnect, closes down all networking
		/// </summary>
		public void DisconnectMaster()
        {
			ServerLoopIsRunning = false;
			client.CloseConnection(connection);
			//client.FlushMessagesOnConnection(connection);
			utils = null;
			client = null;
			status = null;

			
			ServerThread.Abort();
			ServerThread = null;

			Library.Deinitialize();
		}


		/// <summary>
		/// This function is running on the ProcessThread, This is unable to use any Unity API, therfore to transfer commands use processthreadmanager, Fixedupdate will come along and run them. 
		/// </summary>
		public void NetWorkThreadLoop()
		{
			// Tell main thread ive started
			SendToUnityThread.instance.ExecuteOnMainThread(() =>
			{
				InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server thread Started up", 1, 0));
			});

			DateTime _nextloop = DateTime.Now;

			// while running, update at tick rate
			while (ServerLoopIsRunning)
			{



				while (_nextloop < DateTime.Now)
				{
					// this is the fixedupdate of server thread
					ServerUpdate.Update();

					_nextloop = _nextloop.AddMilliseconds(Constants.MSPerTick);

					if (_nextloop > DateTime.Now)
					{
						Thread.Sleep(_nextloop - DateTime.Now);
					}
				}




			}






			SendToUnityThread.instance.ExecuteOnMainThread(() =>
			{
				InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Server Thread ending", 1, 0));
			});
		}


	}
}
