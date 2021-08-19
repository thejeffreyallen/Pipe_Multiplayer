
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

		[JsonProperty]
		public static float VERSIONNUMBER { get;} = 2.16f;
		public static string SERVERNAME = "PIPE Server";
		public static List<BanProfile> BanProfiles = new List<BanProfile>();
		/// <summary>
		/// Timers linked to connection ID, every message received resets the timer related to the sender of the message, 60second timeout will close the connection
		/// </summary>
		public static Dictionary<uint, Stopwatch> TimeoutWatches = new Dictionary<uint, Stopwatch>();
		/// <summary>
		/// Switch For main thread loop
		/// </summary>
		static bool isRunning = true;
		[JsonProperty]
		public static int MaxPlayers;
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

		
		public static int Port = 7777;
       

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



			

		}

		public static void PostRequest()
		{
			var url = "https://pipe-multiplayerservers.herokuapp.com/api/server/1";

			var httpRequest = (HttpWebRequest)WebRequest.Create(url);

			httpRequest.Method = "POST";
			httpRequest.AllowWriteStreamBuffering = true;
			httpRequest.KeepAlive = false;

			httpRequest.Accept = "application/json";
			httpRequest.ContentType = "application/json";

			   var data = @"{""Id"": 78912,""Server"": ""1"",""IP"": 00.00.00}";
			;
           
			using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
			{
				streamWriter.Write(data);
			}

			var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();
			}

			Console.WriteLine(httpResponse.StatusCode);
		}

		/// <summary>
		/// Servers Primary thread loop
		/// </summary>
		public static void Run(int port, int _MaxPlayers)
		{
			Console.Write("Booting...");
			MaxPlayers = _MaxPlayers;
			Port = port;
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
