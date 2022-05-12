using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using System.Xml.Serialization;
using FrostyPipeServer.RipTide;
using RiptideNetworking;
using RiptideNetworking.Utils;
using RiptideNetworking.Transports;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System;


namespace FrostyPipeServer.ServerFiles
{
    public static class Servermanager
    {
        static Process proc = Process.GetCurrentProcess();
        public static string ServerconfigString = "";
        public static float lastloopreceivethread;
        public static float lastloopreceivethreadtimeout;
        public static float lastloopsendthread;
        public static float lastloopsendthreadtimeout;

        
        

        /// <summary>
        /// Live players on server
        /// </summary>
        public static Dictionary<ushort, Player> Players = new Dictionary<ushort, Player>(5);

        public static int CurrentRAMuse;
        public static Server server;
        public static Config.ServerConfig? config;
        public static string IP;


        /// <summary>
        /// Timers linked to connection ID, every message received resets the timer related to the sender of the message, 60second timeout will close the connection
        /// </summary>
        public static Dictionary<ushort, Stopwatch> TimeoutWatches = new Dictionary<ushort, Stopwatch>(5);


        [JsonProperty]
        public static float VERSIONNUMBER { get; } = 2.17f;

        #region Publicise Data

        [JsonProperty]
        public static string APIKEY = "PIPE_BMX_Multiplayer_FROSTYP";
        [JsonProperty]
        static string MostPopMap;
        [JsonProperty]
        static string AveragePing;
        static bool PostOK;
        public static bool SpecifyAPI;
        static DateTime lastposttohub;

        #endregion

        #region On Connection change

        static void OnClientConnect(object? sender, ServerClientConnectedEventArgs e)
        {
            Console.WriteLine($"Client connection, doing checks..");
            if (AllowNewConnection(e.Client))
            {
                OutgoingMessage.Welcome(e.Client.Id);
                Console.WriteLine($"Started talks with new client");
            }
            
        }
        static void OnClientDisconnect(object? sender,ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Client disconnection triggered");
            try
            {
             PlayerDisconnected(e.Id);
            }
            catch (Exception x)
            {
                Console.WriteLine("Player cleanup error severmanager.OnClientDisconnect : " + x);
                throw;
            }
        }
        static void AnyMessageReceived(object? sender, ServerMessageReceivedEventArgs e)
        {
            if (TimeoutWatches.ContainsKey(e.FromClientId))
            {
                if(TimeoutWatches.TryGetValue(e.FromClientId, out Stopwatch watch))
                {
                    watch.Reset();
                    watch.Start();
                }
            }
        }
        static void PlayerDisconnected(ushort ClientThatDisconnected)
        {
            OutgoingMessage.DisconnectTellAll(ClientThatDisconnected);
            lock (Players)
            {
                // find and remove PlayerData
                foreach (Player p in Players.Values.ToList())
                {
                    if (p.Uniqueid == ClientThatDisconnected)
                    {
                        Players.Remove(ClientThatDisconnected);

                    }
                    else
                    {
                        if (p.SendDataOverrides.ContainsKey(ClientThatDisconnected))
                        {
                            p.SendDataOverrides.Remove(ClientThatDisconnected);
                        }
                    }
                }

            }

            lock (TimeoutWatches)
            {
                // find and remove Timeout watch for this connection
                foreach (ushort watch in TimeoutWatches.Keys.ToList())
                {
                    if (watch == ClientThatDisconnected)
                    {
                        TimeoutWatches[ClientThatDisconnected].Stop();
                        TimeoutWatches.Remove(ClientThatDisconnected);

                    }
                }

            }


            lock (ServerData.OutgoingIndexes)
            {
                // Find and remove and Send indexes for this connection
                List<SendReceiveIndex> segments = new List<SendReceiveIndex>();
                foreach (SendReceiveIndex s in ServerData.OutgoingIndexes.ToList())
                {
                    if (s.PlayerTosendTo == ClientThatDisconnected)
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

            }

            lock (ServerData.IncomingIndexes)
            {
                // Remove connection from any Incoming Indexes
                foreach (SendReceiveIndex InIndex in ServerData.IncomingIndexes.ToList())
                {
                    foreach (ushort ui in InIndex.PlayersRequestedFrom.ToList())
                    {
                        if (ui == ClientThatDisconnected)
                        {
                            InIndex.PlayersRequestedFrom.Remove(ui);
                        }
                    }
                }
            }

            if (Players.Count < 1)
            {
                Players.Clear();
                Players.TrimExcess();
                TimeoutWatches.Clear();
                TimeoutWatches.TrimExcess();
            }

        }
        
        #endregion

        #region Server Loops
        public static void RunOutgoingThread()
        {
           // Console.Write("Out thread..");
            try
            {
                // run any outgoing commands left by receiving thread
                ThreadTransfer.RunSend();

                try
                {
                    // FileSync
                    if (ServerData.OutgoingIndexes.Count > 0)
                    {
                        bool AddedAPacket = false;
                        foreach (SendReceiveIndex Index in ServerData.OutgoingIndexes.ToArray())
                        {
                            if (Index.isSending && PendingReliable(Index.PlayerTosendTo) < 500 && !AddedAPacket)
                            {

                                byte[] Packet = null;
                                FileStream _stream = Index.Fileinfo.OpenRead();



                                // determine packet number to send
                                int PacketnoToSend = 0;
                                bool foundapacketno = false;

                                for (int i = 1; i <= Index.TotalPacketsinFile; i++)
                                {
                                    if (!foundapacketno)
                                    {
                                        bool gotit = false;
                                        foreach (int t in Index.PacketNumbersStored)
                                        {
                                            if (t == i)
                                            {
                                                gotit = true;
                                            }
                                        }

                                        if (!gotit)
                                        {
                                            PacketnoToSend = i;
                                            foundapacketno = true;

                                        }

                                    }


                                }
                                if (!foundapacketno)
                                {
                                    Index.isSending = false;
                                    return;
                                }







                                // position to read from
                                int PostoStartAt = 0;
                                PostoStartAt = (PacketnoToSend - 1) * 400000;




                                // make byte array 400,000 unless its the last packet
                                if (PacketnoToSend == Index.TotalPacketsinFile && Index.TotalPacketsinFile == 1)
                                {
                                    Packet = new byte[_stream.Length];


                                }
                                if (PacketnoToSend < Index.TotalPacketsinFile && Index.TotalPacketsinFile > 1)
                                {
                                    Packet = new byte[400000];


                                }
                                if (PacketnoToSend == Index.TotalPacketsinFile && Index.TotalPacketsinFile > 1)
                                {
                                    Packet = new byte[_stream.Length - 400000 * (PacketnoToSend - 1)];


                                }

                                if (Packet == null)
                                {

                                    return;
                                }

                                // Read bytes
                                _stream.Seek(PostoStartAt, SeekOrigin.Begin);
                                _stream.Read(Packet, 0, Packet.Length);

                                Console.WriteLine(Index.Fileinfo.FullName);
                                // add to send rack
                                FileSegment Segment = new FileSegment(Packet, Index.NameOfFile, Index.TotalPacketsinFile, PacketnoToSend, Index.PlayerTosendTo, Index.Filetype, Index.ByteLength, Index.Fileinfo.DirectoryName);
                                RipTide.OutgoingMessage.SendFileSegment(Segment);
                                AddedAPacket = true;
                                Index.PacketNumbersStored.Add(PacketnoToSend);

                                _stream.Close();


                            }
                        }


                    }

                }
                catch (Exception x)
                {
                    Console.WriteLine("Filesync error : " + x);
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Send thread error");
            }
        }
        public static void RunIncomingThread()
        {
           // Console.Write("In thread..");
            server.Tick();
        }
        public static void RunSystemThread()
        {
            ThreadTransfer.RunSystem();

            if (config.DynamicTick)
            {
             ThreadManager.UpdateTickRate();
            }

            if (config.Publicise)
            {
                if ((DateTime.Now - lastposttohub).TotalMinutes >= config.HubPostTimeout)
                {
                    Publicise();
                }
            }

            TimeoutCheck();
            BanReleaseCheck();
            UpdatePlayerData();
            MemoryCheck();
        }
        #endregion

        #region Publicising
        public static void Publicise()
        {
            Console.WriteLine("Publicising..");
            // update string data either way
            AveragePing = GetAveragePing().ToString();
            MostPopMap = GetMostPopularMap();

            
                if (PostOK)
                {
                    UpdateServerOnHub(config.PUT);
                }
                else
                {
                    PostServerToHub(config.POST);
                }
                lastposttohub = DateTime.Now;
            


        }

        async static void PostServerToHub(string url)
        {

            // setup data, the key values here must match the parameter names of the PostServer method of the API that will receive this
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"server",config.Servername },
                {"ip", IP },
                {"port", config.Port.ToString() },
                {"average_ping", AveragePing },
                {"player_count", Players.Count.ToString() },
                {"max_players", config.Maxplayers.ToString() },
                {"most_popular_map", MostPopMap },
                {"version", VERSIONNUMBER.ToString() },
                {"tick_rate", config.TickrateMax.ToString() },




            };

            try
            {
                using (HttpContent post = new FormUrlEncodedContent(dict))
                {


                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("apikey", APIKEY);
                        using (HttpResponseMessage response = await client.PostAsync(url, post))
                        {
                            using (HttpContent content = response.Content)
                            {
                                string mycontent = await content.ReadAsStringAsync();
                                Console.WriteLine("Hub Status " + response.StatusCode);
                                Console.WriteLine(mycontent);
                                if (response.StatusCode == HttpStatusCode.OK)
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
                {"server",config.Servername },
                {"average_ping", AveragePing },
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
                                if (response.StatusCode == HttpStatusCode.BadRequest | mycontent.ToLower().Contains("couldn't find server"))
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
        #endregion

        public static async void Startup()
        {
            Console.WriteLine("Checking Directories..");
            ServerData.LoadData();
            Console.WriteLine("Directories good\n");

            await GetIPAddress();
            Console.WriteLine("public Ip address: " + IP);
            RiptideLogger.Initialize(Console.WriteLine, Console.WriteLine, Console.WriteLine, Console.Error.WriteLine, true);
            Message.MaxPayloadSize = config.MessageMaxPayload;
            Message.MTU = config.MessageMTU;
            server = new Server();
            server.SetMaxPlayers(config.Maxplayers);
            server.ClientConnected += OnClientConnect;
            server.ClientDisconnected += OnClientDisconnect;
            server.MessageReceived += AnyMessageReceived;
            server.Start(config.Port, config.Maxplayers);
            ThreadManager.BootUp();

        }
        public static async void ShutDown()
        {
            Console.WriteLine("Shutting down Server..");
           
            Console.WriteLine("Waiting on threads..");
            await ThreadManager.ShutdownThreads();
        }
        

        #region Stats and System
        static async Task GetIPAddress()
        {
            bool badgateway = true;
            string address = "";
            while (badgateway)
            {
                if (badgateway)
                {
                    Console.WriteLine("Finding Remote IP");
                    try
                    {
                        HttpClient client = new HttpClient();
                        var request = await client.GetAsync(config.IPCheck);
                        var response = await request.Content.ReadAsStringAsync();
                        badgateway = request.StatusCode == HttpStatusCode.BadGateway ? true : false;
                        address = response;

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error with IP request");
                    }

                }
                Thread.Sleep(5000);
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first).Trim();

            IP = address;
        }
        public static string GetPlayerIP(ushort clientid)
        {
            if(Players.ContainsKey(clientid)) return Players[clientid].IP;
            return "";
        }
        static int PendingReliable()
        {
            int total = 0;
            foreach(IConnectionInfo c in server.Clients)
            {
              total += c.PendingReliable();
            }
            return total;
        }
        static int PendingReliable(ushort client)
        {
            foreach (IConnectionInfo c in server.Clients.ToArray())
            {
                if(c.Id == client)
                {
                    return c.PendingReliable();
                }
            }
            return 0;
        }
        static int GetAveragePing()
        {
            int ping = 0;
            foreach (Player pl in Players.Values)
            {
                ping += pl.connection.RTT;
            }

            return ping;
        }
        static string GetMostPopularMap()
        {
            // gather list of mapnames
            List<string> maps = new List<string>();
            foreach(Player p in Players.Values.ToList())
            {
                maps.Add(p.MapName);
            }



            int highest = 0;
            string mostpop = "None";
            // determine most pop
            foreach(string map in maps)
            {
                // get count of that name
                int count = 0;
                foreach (string match in maps)
                {
                    if(map == match)
                    {
                        count++;
                    }

                }

                if(count > highest)
                {
                    mostpop = map;
                }
            }


            return mostpop;
        }
        static void UpdatePlayerData() 
        {
            foreach(Player p in Players.Values)
            {
                lock (p)
                {
                 p.UpdateFPS();
                }
            }
        }
        #endregion

        #region Security and Checks
        static bool AllowNewConnection(IConnectionInfo id)
        {
            List<Config.PlayerBan>? livebans = LoadBans();

            foreach(Config.PlayerBan ban in livebans)
            {
                if(ban.IP == id.Ip)
                {
                    OutgoingMessage.DisconnectPlayer($"Banned until {ban.Timeofbanrelease.ToLongTimeString()}", id.Id);
                    return false;
                }
            }
            return true;
        }
        public static void TimeoutCheck()
        {
            foreach (Player _player in Players.Values.ToList())
            {
                bool foundwatch = false;

                if (TimeoutWatches.ContainsKey(_player.Uniqueid))
                {
                    foundwatch = true;
                    if(TimeoutWatches.TryGetValue(_player.Uniqueid,out Stopwatch? watch))
                    {

                        if (watch.Elapsed.TotalSeconds > 120f)
                        {
                            try
                            {
                                OutgoingMessage.DisconnectPlayer("Timeout", _player.Uniqueid);
                                Console.WriteLine("Player timeout");
                                watch.Reset();
                                watch.Stop();
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
        public static void BanReleaseCheck()
        {
            List<Config.PlayerBan>? livebans = LoadBans();
            foreach (Config.PlayerBan ban in livebans.ToList())
            {
                if (DateTime.Now >= ban.Timeofbanrelease)
                {
                    livebans.Remove(ban);
                    Console.WriteLine($"Ban Removed for {ban.Username}");
                }
            }
            SaveBans(livebans);

        }
        public static void BanPlayer(string _username, string IP, ushort connid, int mins)
        {

            DateTime Time_of_release = DateTime.Now.AddMinutes(mins);

            Config.PlayerBan Ban = new Config.PlayerBan(IP, _username, connid, Time_of_release);

            List<Config.PlayerBan>? Bans = LoadBans();

            Bans.Add(Ban);

            SaveBans(Bans);
            string utctime = Time_of_release.ToUniversalTime().ToString();
            OutgoingMessage.DisconnectPlayer("Banned until " + utctime + " Utc", connid);

        }
        static void SaveBans(List<Config.PlayerBan> Bans)
        {
            string json = JsonConvert.SerializeObject(Bans);
            File.WriteAllText("Config/PlayerBans.json", json);
        }
        static List<Config.PlayerBan> LoadBans()
        {
            List<Config.PlayerBan>? Bans = File.Exists("Config/PlayerBans.json") ? JsonConvert.DeserializeObject<List<Config.PlayerBan>>(File.ReadAllText("Config/PlayerBans.json")) : new List<Config.PlayerBan>();
            if (Bans == null) Bans = new List<Config.PlayerBan>();
            return Bans;
        }

        static void MemoryCheck()
        {
           

        }

        #endregion

        #region WEBPORTAL Controls
        public static bool ReloadConfig()
        {
            Console.WriteLine("Reloading config");
            ServerData.LoadData();
            if(server != null)
            {
                server.SetMaxPlayers(config.Maxplayers);
            }
            return true;
        }

        public static string GiveConfigasJSONString()
        {
            return ServerconfigString;
        }
        public static string GiveStatsasJSONString()
        {
            if (server == null) return "";

            

            Dictionary<string,string> stats = new Dictionary<string,string>();
            stats.Add("ramused", GetRamUsedMBytes().ToString());
            stats.Add("players", Players.Count.ToString());
            stats.Add("cpuused", GetCpuUsage());
            stats.Add("trnow", ThreadManager.CurrentTickrate().ToString());
            stats.Add("lastlooprec", lastloopreceivethread.ToString());
            stats.Add("lastloopsen", lastloopsendthread.ToString());
            stats.Add("lastloopsentimeout", lastloopsendthreadtimeout.ToString());
            stats.Add("lastlooprectimeout", lastloopreceivethreadtimeout.ToString());
            stats.Add("avping", GetAveragePing().ToString());
            Dictionary<string, string> riptide = GetRiptideStats();
            foreach(string key in riptide.Keys)
            {
                stats.Add(key, riptide[key]);
            }
            return JsonConvert.SerializeObject(stats);
        }
        public static Dictionary<string,string> GetRiptideStats()
        {
            Dictionary<string,string> stats = new Dictionary<string, string>();
           
            stats.Add("Kbytesout", server.KBOUT().ToString());
            stats.Add("Kbytesin", server.KBIN().ToString());
            stats.Add("pendingrel", PendingReliable().ToString());
            stats.Add("Fragments", server.FragmentCount().ToString());
            return stats;
        }
        public static bool OverwriteConfigFile(string jsonconfig)
        {
            Console.WriteLine("Overwriting config..");
            
            try
            {
                Dictionary<string, string> incomingconfig = JsonConvert.DeserializeObject<Dictionary<string,string>>(jsonconfig);

                if (incomingconfig == null) return false;


                Config.ServerConfig newconfig = config;

               

                foreach(string key in incomingconfig.Keys)
                {
                    bool didoverwrite = false;
                    foreach (FieldInfo m in config.GetType().GetFields())
                    {
                        if (m.Name.ToLower() == key.ToLower())
                        {
                           // if the name matches the member and the value converts to the right type, overwrite
                            if(m.FieldType == typeof(string))
                            {
                                m.SetValue(newconfig, incomingconfig[key]);
                                didoverwrite = true;
                            }
                            else if(m.FieldType == typeof(ushort))
                            {
                                if(ushort.TryParse(incomingconfig[key], out ushort value))
                                {
                                    m.SetValue(newconfig, value);
                                    didoverwrite = true;
                                }
                            }
                            else if (m.FieldType == typeof(bool))
                            {
                                if (bool.TryParse(incomingconfig[key], out bool value))
                                {
                                    m.SetValue(newconfig, value);
                                    didoverwrite = true;
                                }
                            }
                            else if (m.FieldType == typeof(int))
                            {
                                if (int.TryParse(incomingconfig[key], out int value))
                                {
                                    m.SetValue(newconfig, value);
                                    didoverwrite = true;
                                }
                            }
                        }
                    }

                    if (didoverwrite)
                    {
                        Console.WriteLine($"Overwrote {key} to {incomingconfig[key]}");
                    }

                }

               

                if (incomingconfig != null)
                {
                    string newconfigjson = JsonConvert.SerializeObject(newconfig);
                    File.WriteAllText("Config/ServerConfig.json", newconfigjson);
                    ServerconfigString = newconfigjson;
                    Console.WriteLine("Done");
                    return true;
                }
                else
                {
                    Console.WriteLine("invalid config");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("invalid config");
                return false;
            }
        }


        public static long GetRamUsedMBytes()
        {
            /*
            Console.WriteLine($"  Physical memory usage     : {proc.WorkingSet64}");
            Console.WriteLine($"  Base priority             : {proc.BasePriority}");
            Console.WriteLine($"  Priority class            : {proc.PriorityClass}");
            Console.WriteLine($"  User processor time       : {proc.UserProcessorTime}");
            Console.WriteLine($"  Privileged processor time : {proc.PrivilegedProcessorTime}");
            Console.WriteLine($"  Total processor time      : {proc.TotalProcessorTime}");
            Console.WriteLine($"  Paged system memory size  : {proc.PagedSystemMemorySize64}");
            Console.WriteLine($"  Paged memory size         : {proc.PagedMemorySize64}");
            Console.WriteLine($"  Private memory size  64   : {proc.PrivateMemorySize64}");
            Console.WriteLine($"  Private memory size       : {proc.PrivateMemorySize}");
            */
           
            return Process.GetCurrentProcess().PrivateMemorySize64/1024/1024;
        }
        public static void GetRamAvailable()
        {
           
        }
       
        public static string GetCpuUsage()
        {
            proc = Process.GetCurrentProcess();
            return proc.TotalProcessorTime.Milliseconds.ToString();
        }

        #endregion

        #region Garage
        public static GarageSaveList GarageDeserialize(string presetname, ushort playerid)
        {
            try
            {
                
                if (Players.ContainsKey(playerid))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(GarageSaveList));
                    TextReader reader = new StreamReader(ServerData.GarageSavespath + presetname + "_ID_" + Players[playerid].Username.ToLower() + ".preset");
                    object? obj = deserializer.Deserialize(reader);
                    reader.Close();
                    return (GarageSaveList)obj;
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + " : " + e);
                return null;
            }
        }
        public static bool SaveGaragePreset(string rawxml, ushort playerid, string presetname)
        {
            if (rawxml.Trim() == "") return false;
            if (Players.ContainsKey(playerid))
            { 
                File.WriteAllText(ServerData.GarageSavespath + presetname + "_ID_" + Players[playerid].Username.ToLower()+".preset",rawxml);
                return true;

            }
            else
            {
                return false;
            }

        }
        #endregion

    }
}
