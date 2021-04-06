
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
using System.Security.Cryptography;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Main Loop
    /// </summary>
    static class Server
    {
		#region Variables_Held_Here

		
		public static float VERSIONNUMBER { get;} = 2.1f;


		/// <summary>
		/// to be used for encryption/decryption mostly for authentication
		/// </summary>
        private static Aes aes;
		/// <summary>
		/// Switch For 
		/// </summary>
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

        #endregion









        // creates any data needed at startup
        public static void Initialise()
        {
			
			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ClientPackets.WelcomeReceived, ServersHandles.WelcomeReceived },
				{ (int)ClientPackets.ReceiveAllParts, ServersHandles.ReceiveAllParts },
				{ (int)ClientPackets.TransformUpdate, ServersHandles.TransformReceive},
				{ (int)ClientPackets.SendTextureNames, ServersHandles.TexturenamesReceive},
				{ (int)ClientPackets.ReceiveTexturenames, ServersHandles.TexturenamesReceive},
				{ (int)ClientPackets.SendTexture, ServersHandles.TextureReceive },
				{ (int)ClientPackets.SendAudioUpdate,ServersHandles.ReceiveAudioUpdate},
				{ (int)ClientPackets.SendTextMessage,ServersHandles.RelayPlayerMessage},
				{ (int)ClientPackets.RequestforTex,ServersHandles.RequestforTex },
				{ (int)ClientPackets.ReceiveQuickBikeUpdate,ServersHandles.BikeDataQuickUpdate },
				{ (int)ClientPackets.ReceiveQuickRiderUpdate,ServersHandles.RiderQuickUpdate },
				{ (int)ClientPackets.ReceiveMapname,ServersHandles.ReceiveMapname},

			};

			

		}






		/// <summary>
		/// Servers Primary thread loop
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

			int sendRateMin = 600000;
			int sendRateMax = 65400000;
			int sendBufferSize = 509715200;

			unsafe
			{
				utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(pollGroup), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
				utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(pollGroup), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
				utils.SetConfigurationValue(ConfigurationValue.SendBufferSize, ConfigurationScope.ListenSocket, IntPtr.Zero, ConfigurationDataType.Int32, new IntPtr(&sendBufferSize));
				
			}

			// what happens on connection state change
			StatusCallback status = (ref StatusInfo info) =>
			{

				switch (info.connectionInfo.state)
				{
					
					case ConnectionState.None:
						break;

					case ConnectionState.Connecting:
						// test inital connection info for bans etc
						ConnectionRequest(info);
						break;

					case ConnectionState.Connected:
						Console.WriteLine("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
						
						// inital message
						ServerSend.Welcome(info.connection);

						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						server.FlushMessagesOnConnection(info.connection);
						server.CloseConnection(info.connection);
						Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

						
						
						ServerSend.DisconnectTellAll(info.connection);
						break;


				}
			};

			utils.SetStatusCallback(status);

			Address address = new Address();
			
			address.SetAddress("::0", (ushort)port);
			


			

			uint listenSocket = server.CreateListenSocket(ref address);

			Console.WriteLine("Ready and listening for connections..");
			

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#if VALVESOCKETS_SPAN
	 MessageCallback message = (in NetworkingMessage netMessage) => {
		Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else
			const int maxMessages = 256;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

			// Server Loop
			while (isRunning)
			{

			

#if VALVESOCKETS_SPAN
		//server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else
				if (server != null) GC.KeepAlive(server);

				// process Incoming Data by reading int from byte[] and sending to function that corresponds to the int

				server.RunCallbacks();
				int netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, maxMessages);
					
				// if theres messages, add them to secondary thread for processing at tick rate
				if (netMessagesCount > 0)
				{
					
						for (int i = 0; i < netMessagesCount; i++)
						{
							ref NetworkingMessage netMessage = ref netMessages[i];


							byte[] bytes = new byte[netMessage.length];
							netMessage.CopyTo(bytes);

								// build packet using byte[] then send to function[code], code is read and movepos of _packet
								// is moved forward, so the function that receives this doesnt have to re-read the code from start
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
					
					
					
#endif
				Thread.Sleep(Constants.MSPerTick);
				
			}


			// shutdown
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
				foreach(string ip in ServerData.BannedIps.Values)
                {
					if (ip == info.connectionInfo.address.GetIP())
                    {
						server.CloseConnection(info.connection);
						
						Console.WriteLine($"refused a connection due to Ban: {info.connectionInfo.address.GetIP()}");
						return;
                    }
                }

			server.AcceptConnection(info.connection);
			server.SetConnectionPollGroup(pollGroup, info.connection);

            }
            else
            {
				server.CloseConnection(info.connection);
				Console.WriteLine("Player refused connection due to MaxPlayer cap");
			}
		}


		public static void CheckForTextureFiles(List<TextureInfo> listoffilenames, uint _fromclient)
        {
			DirectoryInfo info = new DirectoryInfo(TexturesDir);
			FileInfo[] files = info.GetFiles();
			List<string> Unfound = new List<string>();

			foreach (TextureInfo s in listoffilenames)
			{
				if(s.Nameoftexture != "e" && s.Nameoftexture != "" && s.Nameoftexture != " ")
                {
				bool found = false;
				foreach (FileInfo file in files)
				{
					if (file.Name == s.Nameoftexture)
					{
						found = true;
						Console.WriteLine($"Matched {s.Nameoftexture} to {file.Name}");
					}
				}
				if (!found)
				{
					// server doesnt have it, request it
					Unfound.Add(s.Nameoftexture);
					Console.WriteLine(s.Nameoftexture + " Added to unfound list");
				}

                }
			}



			// if any unfound, send list of required textures back to the client
			if (Unfound.Count > 0)
			{
				//ServerSend.RequestTextures(_fromclient, Unfound);
				Console.WriteLine($"Send Texture request for {Unfound.Count} items");
			}
			else
			{
				Console.WriteLine("Got All Textures");
			}

		}



		public static List<TextureBytes> GiveTexturesFromDirectory(List<string> listoffilenames)
        {
			List<TextureBytes> bytes = new List<TextureBytes>();

			DirectoryInfo info = new DirectoryInfo(TexturesDir);
			FileInfo[] files = info.GetFiles();
			List<string> Found = new List<string>();
			

			// goes through Texture directory, add found to list, if not it may be incoming now or incompatible though its name got through
			foreach (string s in listoffilenames)
			{
				if (s != "")
				{
					bool found = false;
					foreach (FileInfo file in files)
					{
						if (file.Name.Contains(s))
						{
						Found.Add(file.FullName);
							
							found = true;
							Console.WriteLine($"Matched {s} to {file.Name}, sending to player");
						}
					}
					if (!found)
					{
						// server doesnt have it, request it
						Console.WriteLine(s + " Unfound to give to player");
					}

				}
			}

            if (Found.Count > 0)
            {
				foreach(string n in Found)
                {
					byte[] b = File.ReadAllBytes(n);
					bytes.Add(new TextureBytes(b,n));
                }

			return bytes;
            }
            else
            {
				return null;
            }

        }
		


        #region Functions_Fired_By_Servers_GUI
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

		private static void RemoveBan(string username, string Ipaddress)
        {

        }

		private static void ChangeUsername(string old, string _new)
        {

        }



        #endregion
    }

	/// <summary>
	/// Keep track of a texture in bytes and its file name
	/// </summary>
	class TextureBytes
    {
		public byte[] bytes;
		public string Texname;

		public TextureBytes(byte[] _bytes, string _texname)
        {
			Texname = _texname;
			bytes = _bytes;
        }

    }



}
