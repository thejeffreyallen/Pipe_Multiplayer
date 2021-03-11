
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.Sockets;
using System.Threading;
using System.Net;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Main Loop
    /// </summary>
    class Server
    {
		static bool isRunning = true;
		public static int MaxPlayers;
		public static NetworkingUtils utils;
		public static NetworkingSockets server;
		private static uint pollGroup;


		// links functions in ServerHandles to Ints in ClientPackets
		public delegate void PacketHandler(uint _fromClient, Packet _packet);
		public static Dictionary<int, PacketHandler> packetHandlers;


		public static Dictionary<uint, Player> Players = new Dictionary<uint, Player>();




		// creates any data needed at startup
		public static void Initialise()
        {

			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ClientPackets.WelcomeReceived, ServersHandles.WelcomeReceived },
				{ (int)ClientPackets.ClientsRiderInfo, ServersHandles.RiderInfoReceive },
				{ (int)ClientPackets.ReceiveTexture, ServersHandles.TextureReceive },
				{ (int)ClientPackets.ReceiveTexturenames, ServersHandles.TexturenamesReceive},
				{ (int)ClientPackets.TransformUpdate, ServersHandles.TransformReceive},

			};


		}






		/// <summary>
		/// Loops Server
		/// </summary>
		public static void Run(int port, int _MaxPlayers)
		{
			Console.Write("Booting...");
			MaxPlayers = _MaxPlayers;

			// boots up Valve dependencies (GameNetworkingSockets etc)
			Library.Initialize();

			// call Setup Server function to arrange packethandlers
			Initialise();

			utils = new NetworkingUtils();

			server = new NetworkingSockets();

			pollGroup = server.CreatePollGroup();
			
			StatusCallback status = (ref StatusInfo info) =>
			{


				switch (info.connectionInfo.state)
				{
					
					case ConnectionState.None:
						break;

					case ConnectionState.Connecting:
						ConnectionRequest(info);
						break;

					case ConnectionState.Connected:
						Console.WriteLine("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
						ServerSend.Welcome(info.connection);

						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						server.CloseConnection(info.connection);
						Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
						ServerSend.DisconnectTellAll(info.connection);
						Players.Remove(info.connection);
						break;


				}
			};

			utils.SetStatusCallback(status);

			Address address = new Address();
			//string Ip = IPAddress.Any.ToString();
			address.SetAddress("::0", 7777);

			uint listenSocket = server.CreateListenSocket(ref address);

			Console.WriteLine("Ready and listening");

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#if VALVESOCKETS_SPAN
	 MessageCallback message = (in NetworkingMessage netMessage) => {
		Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else
			const int maxMessages = 20;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

			while (isRunning)
			{

#if VALVESOCKETS_SPAN
		//server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else


				// process Incoming Data on seperate thread, updated at tickrate
					ThreadManager.ExecuteOnMainThread(() =>
					{
				server.RunCallbacks();
				int netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, maxMessages);

				if (netMessagesCount > 0)
				{
						for (int i = 0; i < netMessagesCount; i++)
						{
							ref NetworkingMessage netMessage = ref netMessages[i];


							byte[] bytes = new byte[netMessage.length];
							netMessage.CopyTo(bytes);

								// build packet using byte[] then send to function[code]
							using (Packet _packet = new Packet(bytes))
							{
								int code = _packet.ReadInt();
								uint from = netMessage.connection;

								packetHandlers[code](from, _packet);
							}


							//Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);



							netMessage.Destroy();
						}
				}
					});
					
#endif
				Thread.Sleep(Constants.MSPerTick);
				
			}

			server.DestroyPollGroup(pollGroup);

		}









		/// <summary>
		/// Called when connectionstate is changed to connecting, area for checks before accepting
		/// </summary>
		/// <param name="info"></param>
		private static void ConnectionRequest(StatusInfo info)
        {
			if(Players.Values.Count < MaxPlayers)
            {
				foreach(string ip in Server_SavedData.BannedIps.Values)
                {
					if (ip == info.connectionInfo.address.GetIP())
                    {
						return;
                    }
                }
			server.AcceptConnection(info.connection);
			server.SetConnectionPollGroup(pollGroup, info.connection);

            }
		}



    }
}
