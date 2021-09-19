
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
using System.Threading;
using System.Threading.Tasks;
using System.Net;

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
		public static Dictionary<uint, Player> Players = new Dictionary<uint, Player>();

		#endregion

		#region Publicised Data
		public static string rawlist = "";

		public static bool Publicise;
		static Stopwatch PubliciseWatch = new Stopwatch();

		[JsonProperty]
		static int ServerID = 1;
		static bool Idassigned;
		public static float VERSIONNUMBER { get;} = 2.15f;
		[JsonProperty("name")]
		public static string SERVERNAME = "PIPE Server";
		[JsonProperty]
		public static int MaxPlayers;
		
		[JsonProperty]
		public static string Port = "7777";
		[JsonProperty]
		static string IP = "127.0.0.1";

		#endregion

		static int HubisEmpty;
		static string url = "https://pipe-multiplayerservers.herokuapp.com/api/servers";
		static string ServerURL = "https://pipe-multiplayerservers.herokuapp.com/api/server/" + ServerID;

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

		public static void GetServerData()
		{
			GetAndUpdate(url, ServerURL);
			
		}

		async static void GetAndUpdate(string url, string posturl)
        {
			using(HttpClient client = new HttpClient())
            {
				using(HttpResponseMessage response = await client.GetAsync(url))
                {

					using(HttpContent content = response.Content)
                    {
						string mycontent = await content.ReadAsStringAsync();
						//Console.WriteLine("Got Server list");
						rawlist = mycontent;

						// Hub data stored, figure out what my ID should be
						List<PublicServer> liveservers = new List<PublicServer>();

						//Console.WriteLine("Full Data: " + rawlist);

                        if (rawlist.Length > 4)
                        {

						HubisEmpty = 0;
						int start = rawlist.IndexOf("{");
						int end = rawlist.LastIndexOf('}');
						if(end != -1)
                        {
							rawlist = rawlist.Remove(end);
                        }

						// Build liveservers list
						string[] servers = rawlist.Split('}');
						if (servers != null)
						{
							for (int i = 0; i < servers.Length; i++)
							{
								int idofserver = -1;
								if (servers[i].Replace(" ", "").Length > 0)
								{
									servers[i] = servers[i].Replace("{", "").Replace("}", "");
									int ind = servers[i].IndexOf("name");
									if (ind != -1)
									{
										servers[i] = servers[i].Remove(ind, 6);
									}
									// Debug.Log("Servers full string: " + servers[i]);
									string[] data = servers[i].Split(',');
									List<string> values = new List<string>();
                                    for (int T = 0; T < data.Length; T++)
                                    {
											data[T] = data[T].Replace(" ", "");
											if(data[T].Length > 0)
                                            {
												values.Add(data[T]);
										       //Console.WriteLine("Value split: " + data[T]);
                                            }
                                    }
									int startid = values[0].IndexOf('"');
									//Console.WriteLine(startid + " : start");
									int endid = values[0].IndexOf('"', startid + 1);
										//Console.WriteLine(endid+ " : end");
										string id = values[0].Substring(startid, endid-startid);
										idofserver = int.Parse(id.Replace('"', ' ').Replace(" ", ""));

										values[0] = values[0].Remove(0, endid);
										string[] built = new string[3];
									int count = 0;
									
										for (int _i = 0; _i < values.Count; _i++)
										{
											values[_i] = values[_i].ToLower().Replace('"', ' ').Replace(" ", "").Replace(":", "");
											if(_i != 0)
                                            {
												values[_i] = values[_i].Replace("port", "").Replace("ip", "");

											}
											if(values[_i]!= " " && values[_i]!= "," && values[_i]!= "")
                                            {
                                                if (count < 3)
                                                {
												built[count] = values[_i];
												count++;
                                                }
                                            }
											// Debug.Log($"Server {i}: value{_i}: {data[_i]}");
										}

										liveservers.Add(new PublicServer(built[0], built[1],built[2],idofserver));
									


								}

							}

                            for (int i = 0; i < liveservers.Count; i++)
                            {
								Console.WriteLine($"Server:{liveservers[i].Name} found, HubID: {liveservers[i].Id} ");
                            }

						}



							// check if were in the list
							bool foundus = false;
							List<int> Ids = new List<int>();
						    foreach(PublicServer ps in liveservers)
                            {
								Ids.Add(ps.Id);
								if(ps.IP == IP && ps.Name == SERVERNAME)
                                {
									foundus = true;
                                    if (Idassigned)
                                    {
									// PUT Request
                                    }
                                }
                            }

                            if (!foundus && !Idassigned)
                            {
								ServerID = GiveServerID(Ids);
								ServerURL = "https://pipe-multiplayerservers.herokuapp.com/api/server/" + ServerID;
								Idassigned = true;
								PostRequest(ServerURL);
							}
							else if(!foundus && Idassigned)
                            {
								PostRequest(ServerURL);
                            }

						

                        }
                        else
                        {
							Console.WriteLine("Empty list");
							HubisEmpty++;
							// buffer because the page can sometimes come back blank
                            if (HubisEmpty >= 5)
                            {
                                if (!Idassigned)
                                {
									ServerID = GiveServerID(new List<int>());
									ServerURL = "https://pipe-multiplayerservers.herokuapp.com/api/server/" + ServerID;
									Idassigned = true;
									PostRequest(ServerURL);
								}
                                else
                                {
									PostRequest(ServerURL);
								}

                            }

                        }

					}

                }


            }
        }

		async static void PostRequest(string url)
		{
            
			
			Dictionary<string, string> dict = new Dictionary<string, string>()
			{
				{"name",SERVERNAME },
				{"ip", IP },
				{"port", Port }


			};

                try
                {
			       using (HttpContent post = new FormUrlEncodedContent(dict))
			       {
				
			              //post.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
			             using (HttpClient client = new HttpClient())
			             {
				             using (HttpResponseMessage response = await client.PostAsync(url,post))
				             {
					             // Console.WriteLine(post.Headers);
					             using (HttpContent content = response.Content)
					             {
						            string mycontent = await content.ReadAsStringAsync();
						           //Console.WriteLine(mycontent);
						           Console.WriteLine("Hub Status " + response.StatusCode);
						           //Console.WriteLine(response.RequestMessage);

					             }

				             }

			             }


			       }

                }
                catch (Exception)
                {

                   
                }


            
          
				
			
		
	    }

		async static void DeleteRequest(string url)
		{
			Console.WriteLine("Deleting from Hub...");
			
			
				using (HttpClient client = new HttpClient())
				{
					using (HttpResponseMessage response = await client.DeleteAsync(url))
					{
						
						using (HttpContent content = response.Content)
						{
							string mycontent = await content.ReadAsStringAsync();
							Console.WriteLine(mycontent);
							Console.WriteLine(response.StatusCode);
							Console.WriteLine(response.RequestMessage);

						}

					}

				}


			



		}

		async static void DeleteAndUpdate(string url)
		{
			
			using (HttpClient client = new HttpClient())
			{
				using (HttpResponseMessage response = await client.DeleteAsync(url))
				{

					using (HttpContent content = response.Content)
					{
						string mycontent = await content.ReadAsStringAsync();
						Console.WriteLine("Hub Status: " + response.StatusCode);
						PostRequest(ServerURL);
					}

				}

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

		static int GiveServerID(List<int> ids)
        {
			bool gotuniquenum = false;
			bool check = false;
			int num = -1;
            while (!gotuniquenum)
            {
			Random random = new Random();
			 int no = random.Next(1, 999);
                for (int i = 0; i < ids.Count; i++)
                {
					if(no == ids[i])
                    {
					  check = true;
                    }
                }

                if (!check)
                {
					num = no;
					gotuniquenum = true;
                }


            }


			return num;
        }



		/// <summary>
		/// Servers Primary thread loop
		/// </summary>
		public static void Run(int port, int _MaxPlayers)
		{
			Console.Write("Booting...");
			MaxPlayers = _MaxPlayers;
			Port = port.ToString();
			// boots up Valve dependencies (GameNetworkingSockets etc)
			Library.Initialize();

			// call Setup Server function to arrange packethandlers
			Initialise();

			utils = new NetworkingUtils();

			Connection = new NetworkingSockets();

			ConnectedRiders = Connection.CreatePollGroup();

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
						// test inital connection info for bans etc
						ConnectionRequest(info);
						break;

					case ConnectionState.Connected:
						Console.WriteLine($"New Rider from {info.connectionInfo.address.GetIP()} given ValveID: {info.connection} ");

						// inital message
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

			utils.SetStatusCallback(status);

			Address address = new Address();
			
			address.SetAddress("::0", (ushort)port);
			


			

			uint listenSocket = Connection.CreateListenSocket(ref address);

			Console.WriteLine("Ready and listening for connections..");
			

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



			const int maxMessages = 256;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
            if (Publicise)
            {
			PubliciseWatch.Start();

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



							netMessage.Destroy();
						}

				}

				TimeoutCheck();
				BanReleaseCheck();

                if (Publicise)
                {
				// do publicising
				if(PubliciseWatch.Elapsed.TotalSeconds > 30)
                {
					GetServerData();
					

					PubliciseWatch.Reset();
					PubliciseWatch.Start();
                }

                }
				Thread.Sleep(Constants.MSPerTick);
				
			}


			// shutdown
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
