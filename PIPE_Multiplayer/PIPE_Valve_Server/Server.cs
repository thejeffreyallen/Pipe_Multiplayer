
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Reflection;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Main Loop
    /// </summary>
    class Server
    {
		static bool isRunning = true;
		public static int MaxPlayers;
		/// <summary>
		/// Internal Callbacks etc with info about connection
		/// </summary>
		public static NetworkingUtils utils;
		/// <summary>
		/// Actual Socket that sends and receives
		/// </summary>
		public static NetworkingSockets server;
        // connected clients
		private static uint pollGroup;

		// Directories for Servers Stored Data
		public static string Rootdir = Assembly.GetExecutingAssembly().Location.Replace(".exe", "") + "/Game Data/";
		public static string TexturesDir = Rootdir + "Textures/";


		/// <summary>
		/// A PacketHandler is any fuction that takes in a uint and a Packet
		/// </summary>
		/// <param name="_fromClient"></param>
		/// <param name="_packet"></param>
		public delegate void PacketHandler(uint _fromClient, Packet _packet);
		/// <summary>
		/// PacketHandlers linked with an Int key, incoming messages fire PacketHandlers[packetcode] which is a function that takes in uint(from connection) and Packet(received bytes)
		/// </summary>
		public static Dictionary<int, PacketHandler> packetHandlers;

		/// <summary>
		/// Live players on server
		/// </summary>
		public static Dictionary<uint, Player> Players = new Dictionary<uint, Player>();

		/// <summary>
		/// dictionary of currently being ridden Maps and amount of players in each map
		/// </summary>
		public static Dictionary<string,int> MapsBeingRidden = new Dictionary<string,int>();



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
				{ (int)ClientPackets.ReceiveAudioUpdate,ServersHandles.ReceiveAudioUpdate},

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
			
			address.SetAddress("::0", 7777);

			uint listenSocket = server.CreateListenSocket(ref address);

			Console.WriteLine("Ready and listening");

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#if VALVESOCKETS_SPAN
	 MessageCallback message = (in NetworkingMessage netMessage) => {
		Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else
			const int maxMessages = 200;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

			while (isRunning)
			{

#if VALVESOCKETS_SPAN
		//server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else


				// process Incoming Data by reading int from byte[] and sending to function that corresponds to the int
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

								// build packet using byte[] then send to function[code], code is read and movepos of _packet is moved forward, so the function that receives this doesnt have to re-read the code from start
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
			Library.Deinitialize();

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


		public static void CheckForTextureFiles(List<string> listoffilenames, uint _fromclient)
        {
			DirectoryInfo info = new DirectoryInfo(TexturesDir);
			FileInfo[] files = info.GetFiles();
			List<string> Unfound = new List<string>();

			foreach (string s in listoffilenames)
			{
				bool found = false;
				foreach (FileInfo file in files)
				{
					if (file.Name == s)
					{
						found = true;
						Console.WriteLine($"Matched {s} to {file.Name}");
					}
				}
				if (!found)
				{
					// server doesnt have it, request it
					Unfound.Add(s);
					Console.WriteLine(s + "Added to unfound list");
				}

			}
			// if any unfound, send list of required textures back to the client
			if (Unfound.Count > 0)
			{
				ServerSend.RequestTextures(_fromclient, Unfound);

			}
			else
			{
				Console.WriteLine("Got All Textures");
			}

		}


		private static void BanAPlayer(uint Playersconnection, string username)
        {
			foreach(Player p in Players.Values)
            {
				if(p.clientID == Playersconnection)
                {
					// send message to disconnect client
					// grab ip,
					


					server.CloseConnection(Playersconnection);
                }
            }
        }

	}
}
