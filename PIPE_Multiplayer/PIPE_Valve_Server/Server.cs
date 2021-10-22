
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
using Newtonsoft.Json;
using System.Net.Http;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Servers Main Loop
    /// </summary>
    static class Server
    {
		#region Servers data

		public static List<BanProfile> BanProfiles = new List<BanProfile>();
		/// <summary>
		/// Timers linked to connection ID, every message received resets the timer related to the sender of the message, 60second timeout will close the connection
		/// </summary>
		public static Dictionary<uint, Stopwatch> TimeoutWatches = new Dictionary<uint, Stopwatch>();
		/// <summary>
		/// Switch For main thread loop
		/// </summary>
		static bool isRunning = true;
		/// <summary>
		/// Internal Callbacks etc with info about connection
		/// </summary>
		public static NetworkingUtils utils;
		/// <summary>
		/// Actual Socket that sends and receives
		/// </summary>
		public static NetworkingSockets Connection;
        // connected clients
		public static uint ConnectedRiders;
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
		[JsonProperty]
		public static Dictionary<uint, Player> Players = new Dictionary<uint, Player>();

		#endregion

		#region Publicise Data
		public static string rawlist = "";

		public static bool Public_server;
		static Stopwatch PubliciseWatch = new Stopwatch();

		[JsonProperty]
		public static string APIKEY = "PIPE_BMX_Multiplayer_FROSTYP";
		[JsonProperty]
		public static float VERSIONNUMBER { get;} = 2.16f;
		[JsonProperty]
		public static string SERVERNAME = "PIPE Server";
		[JsonProperty]
		public static int MaxPlayers;
		[JsonProperty]
		public static string Port = "7777";
		[JsonProperty]
		static string IP = "127.0.0.1";
		[JsonProperty]
		static string MostPopMap;
		[JsonProperty]
		static string averageping;
		public static string posturl = "https://pipe-bmx-api.herokuapp.com/post";
		public static string puturl = "https://pipe-bmx-api.herokuapp.com/update";
		static bool PostOK;
		public static bool SpecifyAPI;

		#endregion



		// creates any data needed at startup
		public static void Initialise()
        {
			
			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ClientPackets.WelcomeReceived, ServersHandles.WelcomeReceived },
				{ (int)ClientPackets.ReceiveAllParts, ServersHandles.ReceiveAllParts },
				{ (int)ClientPackets.TransformUpdate, ServersHandles.TransformReceive},
				{ (int)ClientPackets.ReceiveFileSegment, ServersHandles.FileSegmentReceive },
				{ (int)ClientPackets.PlayerRequestedFile,ServersHandles.PlayerRequestedFile },
				{ (int)ClientPackets.SendAudioUpdate,ServersHandles.ReceiveAudioUpdate},
				{ (int)ClientPackets.SendTextMessage,ServersHandles.RelayPlayerMessage},
				{ (int)ClientPackets.GearUpdate,ServersHandles.GearUpdate },
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
				{ (int)ClientPackets.FileStatus,ServersHandles.FileStatus},
				{ (int)ClientPackets.LogOut,ServersHandles.AdminLogOut},
				{ (int)ClientPackets.InviteToSpawn, ServersHandles.InviteToSpawn },
				{ (int)ClientPackets.AlterBanWords, ServersHandles.AdminAlterBanwords },
				{ (int)ClientPackets.OverrideMapMatch, ServersHandles.OverrideMapMatch },

			};


			IP = GetIPAddress();
			Console.WriteLine("Ip address: " + IP);


		}

		public static void Publicise()
		{
			// update string data either way
			averageping = GetAveragePing().ToString();
			MostPopMap = GetMostPopularMap();

			if (PostOK)
            {
				UpdateServerOnHub(puturl);
            }
            else
            {
				PostServerToHub(posturl);
            }
			
		}

		async static void PostServerToHub(string url)
		{
			
			// setup data, the key values here must match the parameter names of the PostServer method of the API that will receive this
			Dictionary<string, string> dict = new Dictionary<string, string>()
			{
				{"server",SERVERNAME },
				{"ip", IP },
				{"port", Port },
				{"average_ping", averageping },
				{"player_count", Players.Count.ToString() },
				{"max_players", MaxPlayers.ToString() },
				{"most_popular_map", MostPopMap },
				{"version", VERSIONNUMBER.ToString() },
				{"tick_rate", Constants.TicksPerSec.ToString() },
				



			};

                try
                {
			       using (HttpContent post = new FormUrlEncodedContent(dict))
			       {
					
				         
			             using (HttpClient client = new HttpClient())
			             {
						     client.DefaultRequestHeaders.Add("apikey", APIKEY);
				             using (HttpResponseMessage response = await client.PostAsync(url,post))
				             {
					             using (HttpContent content = response.Content)
					             {
						            string mycontent = await content.ReadAsStringAsync();
						           Console.WriteLine("Hub Status " + response.StatusCode);
								   Console.WriteLine(mycontent);
								   if(response.StatusCode == HttpStatusCode.OK)
                                   {
									PostOK = true; 
                                   }
					             }

				             }

			             }


			       }

                }
                catch (Exception x)
                {
				Console.Write($"Post Request error: {x}");
                }

	    }

		async static void UpdateServerOnHub(string url)
		{

			// setup data, the key values here must match the parameter names of the PostServer method of the API that will receive this
			Dictionary<string, string> dict = new Dictionary<string, string>()
			{
				{"server",SERVERNAME },
				{"average_ping", averageping },
				{"player_count", Players.Count.ToString() },
				{"most_popular_map", MostPopMap },
				

			};

			try
			{
				using (HttpContent post = new FormUrlEncodedContent(dict))
				{

					using (HttpClient client = new HttpClient())
					{
						client.DefaultRequestHeaders.Add("apikey", APIKEY);
						using (HttpResponseMessage response = await client.PutAsync(url, post))
						{
							using (HttpContent content = response.Content)
							{
								string mycontent = await content.ReadAsStringAsync();
								Console.WriteLine("Hub Status " + response.StatusCode);
								Console.WriteLine(mycontent);
								if(response.StatusCode == HttpStatusCode.BadRequest | mycontent.ToLower().Contains("couldn't find server"))
                                {
									PostOK = false;
                                }
							}

						}

					}


				}

			}
			catch (Exception x)
			{
				Console.Write($"Put Request error: {x}");
			}







		}

		
		/// <summary>
		/// Give my public IP address
		/// </summary>
		/// <returns></returns>
		static string GetIPAddress()
		{
			String address = "";
			WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
			using (WebResponse response = request.GetResponse())
			using (StreamReader stream = new StreamReader(response.GetResponseStream()))
			{
				address = stream.ReadToEnd();
			}

			int first = address.IndexOf("Address: ") + 9;
			int last = address.LastIndexOf("</body>");
			address = address.Substring(first, last - first);

			return address;
		}

		static int GetAveragePing()
        {
			List<int> pings = new List<int>();
			foreach(Player p in Players.Values)
            {
				pings.Add(p.ping);
            }

			int average = 0;
            for (int i = 0; i < pings.Count; i++)
            {
				average = average + pings[i];
            }

			return pings.Count>0 ? average / pings.Count : 0;
        }

		static string GetMostPopularMap()
        {
            try
            {

			List<string> names = new List<string>();
			foreach(Player p in Players.Values)
            {
				names.Add(p.MapName);
            }

			int topmap = 0;
			string topmapname = "none";

            if (Players.Count > 0)
            {
			// for each mapname
            for (int i = 0; i < names.Count; i++)
            {
				int countofthismap = 0;
				// count every mapname matching this one
                for (int _i = 0; _i < names.Count; _i++)
                {
					if(names[_i] == names[i])
                    {
						// add one to count for this map
						countofthismap++;
                    }

					// map has more players, make new topmap
					if(countofthismap >= topmap)
                    {
						topmap = countofthismap;
						topmapname = names[i];
                    }

                }
				
            }

            }
			// map name retrieved
			return topmapname;

            }
            catch (Exception x)
            {
				return "none";
            }



        }
		
		/// <summary>
		/// Servers Primary thread loop, stays in while loop until app closes
		/// </summary>
		public static void Run(int port, int _MaxPlayers)
		{
			Console.Write("Booting...");
			MaxPlayers = _MaxPlayers;
			Port = port.ToString();

			// Master call to load GameNetworkingSockets.dll and begin Valve's system
			Library.Initialize();
			utils = new NetworkingUtils();

			// the servers socket being created
			Connection = new NetworkingSockets();
			// the bundle to add each client connection to
			ConnectedRiders = Connection.CreatePollGroup();

			// call Setup Server function to arrange packethandlers
			Initialise();

			



			int sendRateMin = 60000;
			int sendRateMax = 95400000;
			int sendBufferSize = 95400000;

			unsafe
			{
				utils.SetConfigurationValue(ConfigurationValue.SendRateMin, ConfigurationScope.ListenSocket, new IntPtr(ConnectedRiders), ConfigurationDataType.Int32, new IntPtr(&sendRateMin));
				utils.SetConfigurationValue(ConfigurationValue.SendRateMax, ConfigurationScope.ListenSocket, new IntPtr(ConnectedRiders), ConfigurationDataType.Int32, new IntPtr(&sendRateMax));
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
						// test inital connection info for bans, maxplayer count
						ConnectionRequest(info);
						break;

					case ConnectionState.Connected:
						Console.WriteLine($"New Rider from {info.connectionInfo.address.GetIP()} given ValveID: {info.connection} ");
						// inital message to ask for data
						ServerSend.Welcome(info.connection);

						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						
						Console.WriteLine("Rider called it a Day:  " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
						ServerSend.DisconnectTellAll(info.connection);
						DisconnectPlayerAndCleanUp(info.connection);

						break;
						
						


				}
			};

			// set address to any and start to listen
			utils.SetStatusCallback(status);
			Address address = new Address();
			address.SetAddress("::0", (ushort)port);
			uint listenSocket = Connection.CreateListenSocket(ref address);


			Console.WriteLine("Ready and listening for connections..");
			

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


			// create incoming message bundle
			const int maxMessages = 256;
			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

			// if public selected, start timer
            if (Public_server)
            {
			PubliciseWatch.Start();
			Publicise();
            }

			// Server Loop
			while (isRunning)
			{

			


				if (Connection != null) GC.KeepAlive(Connection);

				
				Connection.RunCallbacks();
				int netMessagesCount = Connection.ReceiveMessagesOnPollGroup(ConnectedRiders, netMessages, maxMessages);
					
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

                               if (packetHandlers.ContainsKey(code))
                               {
								packetHandlers[code](from, _packet);
                               }
                               else
                               {
								Console.WriteLine($"Received unsupported packet number {code}");
                                if (Players.ContainsKey(from))
                                {
									ServerSend.SendTextFromServerToOne(from, "Server Doesnt support this feature");
                                }
  
                               }


							foreach(uint watch in TimeoutWatches.Keys)
                            {
                                if (TimeoutWatches[watch].Elapsed.TotalSeconds > 130)
                                {
									Console.WriteLine("Watch Error, Timeout watch went over 130 seconds");
									TimeoutWatches[watch].Reset();



								}

								if(watch == from)
                                {
							       TimeoutWatches[from].Reset();
								   TimeoutWatches[from].Start();

                                }
                            }

							}


						    SetPlayersLastPing(netMessage);
							netMessage.Destroy();
						}

				}

				TimeoutCheck();
				BanReleaseCheck();

                if (Public_server)
                {
				// do publicising
				if(PubliciseWatch.Elapsed.TotalSeconds >= 120)
                {
					Publicise();
					PubliciseWatch.Reset();
					PubliciseWatch.Start();
                }

                }
				Thread.Sleep(Constants.MSPerTick);
				
			}


			// shutdown server by flipping is running false
			Connection.DestroyPollGroup(ConnectedRiders);
			Library.Deinitialize();

		}

		// Data about the connection to Server, called on tick
		public static int PendingReliableForConnection(uint _player)
		{
			ConnectionStatus constat = new ConnectionStatus();
			Connection.GetQuickConnectionStatus(_player, ref constat);

			return constat.pendingReliable;

		}

		public static void GiveAdminStream(uint admin)
        {
			ConnectionStatus constat = new ConnectionStatus();
			Connection.GetQuickConnectionStatus(ConnectedRiders, ref constat);

			ServerSend.StreamAdminInfo(admin, new AdminDataStream(constat.pendingReliable, constat.pendingUnreliable, Players.Count, constat.outBytesPerSecond, constat.inBytesPerSecond, ServerData.IncomingIndexes.Count, ServerData.OutgoingIndexes.Count));

		}

		static void SetPlayersLastPing(NetworkingMessage mess)
        {
            if (Players.ContainsKey(mess.connection))
			{
				Players[mess.connection].ping = GetConnectionStatus(mess.connection);
			}

        }

		static int GetConnectionStatus(uint conn)
        {
			ConnectionStatus _status = new ConnectionStatus();
			Connection.GetQuickConnectionStatus(conn, ref _status);
			return _status.ping;
        }

        /// <summary>
        /// Called when connectionstate is changed to connecting, area for checks before accepting
        /// </summary>
        /// <param name="info"></param>
        private static void ConnectionRequest(StatusInfo info)
        {
			
			if(Players.Values.Count >= MaxPlayers)
            {
				Connection.CloseConnection(info.connection);
				Console.WriteLine("Player refused connection due to MaxPlayer cap");
				return;
			}
				foreach(BanProfile ban in BanProfiles)
                {
					if (ban.IP == info.connectionInfo.address.GetIP())
                    {
						Connection.CloseConnection(info.connection);
						
						Console.WriteLine($"{ban.Username} refused a connection due to Ban: {info.connectionInfo.address.GetIP()}");
						return;
                    }
					if (ban.ConnId == info.connection)
					{
						Connection.CloseConnection(info.connection);

						Console.WriteLine($"{ban.Username} refused a connection due to Ban: {info.connection}");
						return;
					}


				}
				



			Connection.AcceptConnection(info.connection);
			Connection.SetConnectionPollGroup(ConnectedRiders, info.connection);

           
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
                        if (TimeoutWatches[key].Elapsed.TotalSeconds > 120f)
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
			// cut connection
			Connection.CloseConnection(ClientThatDisconnected);
			Connection.FlushMessagesOnConnection(ClientThatDisconnected);

			// find and remove PlayerData
			foreach (Player p in Server.Players.Values.ToList())
			{
				if (p.RiderID == ClientThatDisconnected)
				{
					Server.Players.Remove(ClientThatDisconnected);

				}
                else
                {
                    if (p.SendDataOverrides.ContainsKey(ClientThatDisconnected))
                    {
						p.SendDataOverrides.Remove(ClientThatDisconnected);
                    }
                }
			}

			// find and remove Timeout watch for this connection
			foreach (uint watch in Server.TimeoutWatches.Keys.ToList())
			{
				if (watch == ClientThatDisconnected)
				{
					Server.TimeoutWatches[ClientThatDisconnected].Stop();
					Server.TimeoutWatches.Remove(ClientThatDisconnected);

				}
			}


			// Find and remove and Send indexes for this connection
			List<SendReceiveIndex> segments = new List<SendReceiveIndex>();
			foreach(SendReceiveIndex s in ServerData.OutgoingIndexes)
            {
				if(s.PlayerTosendTo == ClientThatDisconnected)
                {	
				segments.Add(s);
                }
            }

            if (segments.Count > 0)
            {
                for (int i = 0; i < segments.Count; i++)
                {
			      ServerData.OutgoingIndexes.Remove(segments[i]);

                }

            }

			// Remove connection from any Incoming Indexes
			foreach(SendReceiveIndex InIndex in ServerData.IncomingIndexes)
            {
				foreach(uint ui in InIndex.PlayersRequestedFrom.ToList())
                {
					if(ui == ClientThatDisconnected)
                    {
						InIndex.PlayersRequestedFrom.Remove(ui);
                    }
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
					ServerData.SaveServerData();
                }
            }
        }


    }

	public class AdminDataStream
    {
		public int PendingRel;
		public int PendingUnrel;
		public int Playercount;
		public float Bytesoutpersec;
		public float bytesinpersec;
		public int inindexes;
		public int outindexes;

		public AdminDataStream(int prel, int punrel, int playcount,float bytesoutpsec,float bytesinpsec,int inindex, int outindex)
        {
			PendingRel = prel;
			PendingUnrel = punrel;
			Playercount = playcount;
			Bytesoutpersec = bytesoutpsec;
			bytesinpersec = bytesinpsec;
			inindexes = inindex;
			outindexes = outindex;
        }

    }



}
