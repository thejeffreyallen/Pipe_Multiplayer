
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
using System.Diagnostics;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Main Loop
    /// </summary>
    static class Server
    {
		#region Servers data

		
		public static float VERSIONNUMBER { get;} = 2.13f;


		public static List<BanProfile> BanProfiles = new List<BanProfile>();
		

		/// <summary>
		/// Timers linked to connection ID, every message received resets the timer related to the sender of the message, 60second timeout will close the connection
		/// </summary>
		public static Dictionary<uint, Stopwatch> TimeoutWatches = new Dictionary<uint, Stopwatch>();


		


		/// <summary>
		/// Switch For main thread loop
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
				{ (int)ClientPackets.ReceiveBootPlayer,ServersHandles.AdminBootPlayer},
				{ (int)ClientPackets.AdminLogin,ServersHandles.ReceiveAdminlogin},
				{ (int)ClientPackets.SpawnNewObjectreceive,ServersHandles.SpawnNewObject},
				{ (int)ClientPackets.DestroyAnObject,ServersHandles.DestroyObject},
				{ (int)ClientPackets.MoveAnObject,ServersHandles.MoveObject},
				{ (int)ClientPackets.Turnmeon,ServersHandles.TurnPlayerOn},
				{ (int)ClientPackets.Turnmeoff,ServersHandles.TurnPlayerOff},
				{ (int)ClientPackets.VoteToRemoveObject,ServersHandles.VoteToRemoveObject},
				{ (int)ClientPackets.KeepAlive,ServersHandles.KeepAlive},
				{ (int)ClientPackets.AdminRemoveObject,ServersHandles.AdminRemoveObject},


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
			int sendBufferSize = 209715200;

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
						
						Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
						ServerSend.DisconnectTellAll(info.connection);
						DisconnectPlayerAndCleanUp(info.connection);

						break;
						
						


				}
			};

			utils.SetStatusCallback(status);

			Address address = new Address();
			
			address.SetAddress("::0", (ushort)port);
			


			

			uint listenSocket = server.CreateListenSocket(ref address);

			Console.WriteLine("Ready and listening for connections..");
			

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



			const int maxMessages = 256;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];


			// Server Loop
			while (isRunning)
			{

			


				if (server != null) GC.KeepAlive(server);

				
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

							foreach(uint watch in TimeoutWatches.Keys)
                            {
                                if (TimeoutWatches[watch].Elapsed.TotalSeconds > 40)
                                {
									Console.WriteLine("Watch Error, Timeout watch reached 15 seconds");
									TimeoutWatches[watch].Reset();



								}

								if(watch == from)
                                {
							       TimeoutWatches[from].Reset();
								   TimeoutWatches[from].Start();

                                }
                            }

							}



							netMessage.Destroy();
						}

				}

				TimeoutCheck();
				BanReleaseCheck();

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
				foreach(BanProfile ban in BanProfiles)
                {
					if (ban.IP == info.connectionInfo.address.GetIP())
                    {
						server.CloseConnection(info.connection);
						
						Console.WriteLine($"refused a connection due to Ban: {info.connectionInfo.address.GetIP()}");
						return;
                    }
					if (ban.ConnId == info.connection)
					{
						server.CloseConnection(info.connection);

						Console.WriteLine($"refused a connection due to Ban: {info.connection}");
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


		public static void CheckForTextureFiles(List<PlayerTextureInfo> listoffilenames, uint _fromclient)
        {
			DirectoryInfo info = new DirectoryInfo(TexturesDir);
			FileInfo[] files = info.GetFiles();
			List<string> Unfound = new List<string>();

			foreach (PlayerTextureInfo s in listoffilenames)
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
		

		public static void TimeoutCheck()
        {
			foreach(Player _player in Players.Values.ToList())
            {
				bool foundwatch = false;
				foreach(uint key in TimeoutWatches.Keys.ToList())
                {
					if(_player.RiderID == key)
                    {
						foundwatch = true;
                        if (TimeoutWatches[key].Elapsed.TotalSeconds > 30f)
                        {
                            try
                            {
								
								ServerSend.DisconnectPlayer("Timeout", key);
								Console.WriteLine("Player timeout");
								TimeoutWatches[key].Reset();
								TimeoutWatches[key].Stop();

							}
                            catch (Exception x)
                            {
								Console.WriteLine($"Error closing connection from timeout : {x}");
                            }
                        }
                    }
                }
                if (!foundwatch)
                {
					Console.WriteLine("Player with no Watch");
                }
            }
        }


		public static void DisconnectPlayerAndCleanUp(uint ClientThatDisconnected)
        {
			server.CloseConnection(ClientThatDisconnected);
			server.FlushMessagesOnConnection(ClientThatDisconnected);


			foreach (Player p in Server.Players.Values.ToList())
			{
				if (p.RiderID == ClientThatDisconnected)
				{
					Server.Players.Remove(ClientThatDisconnected);

				}
			}


			foreach (uint watch in Server.TimeoutWatches.Keys.ToList())
			{
				if (watch == ClientThatDisconnected)
				{
					Server.TimeoutWatches[ClientThatDisconnected].Stop();
					Server.TimeoutWatches.Remove(ClientThatDisconnected);

				}
			}
		}

        
		public static void BanReleaseCheck()
        {
			foreach(BanProfile ban in BanProfiles.ToList())
            {
				if(DateTime.Now >= ban.Timeofbanrelease)
                {
					BanProfiles.Remove(ban);
					Console.WriteLine($"Ban Removed for {ban.Username}");
                }
            }
        }


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
