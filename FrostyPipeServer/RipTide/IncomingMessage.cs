using RiptideNetworking;
using RiptideNetworking.Utils;
using RiptideNetworking.Transports;
using FrostyPipeServer.ServerFiles;
using UnityEngine;

namespace FrostyPipeServer.RipTide
{
    public class IncomingMessage
    {

        // server comms

        [MessageHandler((ushort)ClientPackets.WelcomeReceived)]
        public static void WelcomeReceived(ushort _from, Message packet)
        {
            try
            {
                Console.WriteLine("connection responded to data request, processing..");
                // --------------------------------- receive ---------------------------------------------------
                string name = packet.GetString();
                string Ridermodel = packet.GetString();
                string RidermodelBundlename = packet.GetString();
                string CurrentLevel = "Unknown";
                float VersionNo = 0.00f;
                bool GarageEnabled = false;
                string Garageversion = "null";
                try
                {
                    CurrentLevel = packet.GetString();

                }
                catch (Exception x)
                {
                    Console.WriteLine($"couldnt get map name, using {CurrentLevel} in their package: {x}");
                    Servermanager.server.DisconnectClient(_from);
                    return;

                }
                try
                {
                    VersionNo = packet.GetFloat();

                }
                catch (Exception x)
                {
                    Console.WriteLine($"no Version number found from {name}, cut off");
                    Servermanager.server.DisconnectClient(_from);
                    return;
                }
                try
                {
                    GarageEnabled = packet.GetBool();
                    Garageversion = packet.GetString();
                }
                catch (Exception x)
                {
                    Console.WriteLine($"error reading garage data in welcome message from {name}, cut off");
                    Servermanager.server.DisconnectClient(_from);
                    return;
                }


                // -------------------------- Process data ---------------------------------------------------

                // -------------- checks --------------------

                // cut off ridermodels with prefab in the name, most likely cant identify what model it is
                if (Ridermodel.ToLower().Contains("prefab"))
                {
                    OutgoingMessage.DisconnectPlayer($"player model:{Ridermodel}: isn't supportable", _from);
                    Console.WriteLine($"Player refused due to ridermodel name: {Ridermodel}");
                    return;
                }

                // cut off if version is wrong, send Update offer if version knows what to do
                if (VersionNo != Servermanager.VERSIONNUMBER)
                {
                    if (VersionNo > 2.13f && VersionNo < Servermanager.VERSIONNUMBER) { OutgoingMessage.Update(_from, ServerData.GiveUpdateFileNames()); Console.WriteLine($"Player {name} offered update. Their version:{VersionNo}"); }
                    else { OutgoingMessage.DisconnectPlayer($"Mod Version Doesnt Match, Install Version {Servermanager.VERSIONNUMBER} to connect to this Server", _from); Console.WriteLine($"Player {name} refused. Their version:{VersionNo}"); }

                    return;
                }

                // check Garage version match
                if (Garageversion != Servermanager.config.GarageVersion)
                {
                    GarageEnabled = false;
                    OutgoingMessage.SendTextFromServerToOne(_from, "Garage data disabled on server");
                }

                /// ------------- player accepted ------------------------



                // restrict usernames
                foreach (string banword in ServerData.BannedWords)
                {
                    if (name.ToLower().Contains(banword))
                    {
                        name = name.ToLower().Replace(banword, "XOXO");
                    }
                }


                Console.WriteLine($"{name} started riding as {Ridermodel} at {CurrentLevel}");
                Console.WriteLine("Setting up their server data..");
                Player p = new Player(_from, Ridermodel, name, RidermodelBundlename, CurrentLevel, VersionNo, GarageEnabled, Garageversion);
                Servermanager.Players.Add(_from, p);
                IConnectionInfo[] conns = Servermanager.server.Clients;
                foreach(IConnectionInfo conn in conns)
                {
                    if(conn.Id == _from)
                    {
                      p.connection = conn;
                        p.IP = conn.Ip;
                        Console.WriteLine($"Ip: {p.IP}");
                    }
                }
                // player is created, either they will respond to the request for all parts and continue or timeout

                OutgoingMessage.RequestAllParts(_from);

                // add timeout watch for player and start, reset by receive of any message with their id attached
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                Servermanager.TimeoutWatches.Add(_from, watch);


                // if there rider model and map isnt stock, check we have them
                Console.WriteLine("Checking Server has RiderModel and Map");
                if (Ridermodel != "Daryien")
                {
                    ServerData.FileCheckAndRequest(Ridermodel, _from, "pipe_data/Custom Players/");
                }
                if (CurrentLevel != "Unknown" && CurrentLevel != "" && CurrentLevel != " " && !CurrentLevel.ToLower().Contains("pipe") && !CurrentLevel.ToLower().Contains("chuck") && !CurrentLevel.ToLower().Contains("buildplayer") && !CurrentLevel.ToLower().Contains("tutorialsetup"))
                {
                    ServerData.FileCheckAndRequest(CurrentLevel, _from, "pipe_data/CustomMaps/");
                }

                if (CurrentLevel.ToLower().Contains("samplescene") | CurrentLevel.ToLower().Contains("buildplayer"))
                {
                    OutgoingMessage.SendTextFromServerToOne(_from, "You appear to be using M to change map, using L for Patcha Map Changer syncs your map names properly so riders can be matched to your level");
                }
                Console.WriteLine($"Welcome Complete for {name}\n");




            }
            catch (Exception x)
            {
                Console.WriteLine("Unknown Welcome error: " + x);
                Servermanager.server.DisconnectClient(_from);
            }

        }

        [MessageHandler((ushort)ClientPackets.ReceiveAllParts)]
        public static void ReceiveAllParts(ushort _from, Message _packet)
        {
            Console.WriteLine("Receiving Player Data");


            try
            {
                if (Servermanager.Players.TryGetValue(_from, out Player _player))
                {
                    // rider  ----------------------------------------------------------

                    Vector3 pos = _packet.GetVector3();
                    Vector3 rot = _packet.GetVector3();
                    bool capforward = true;


                    List<TextureInfo> RiderTexnames = new List<TextureInfo>();

                    if (_player.Ridermodel == "Daryien")
                    {
                        int Ridertexcount = _packet.GetInt();
                        if (Ridertexcount > 0)
                        {

                            for (int i = 0; i < Ridertexcount; i++)
                            {
                                string Texname = _packet.GetString();
                                string ParentG_O = _packet.GetString();
                                int matnum = _packet.GetInt();
                                string dir = _packet.GetString();
                                RiderTexnames.Add(new TextureInfo(Texname, ParentG_O, false, matnum, dir));
                            }


                        }

                        capforward = _packet.GetBool();
                    }

                    // bike ( garage )   ----------------------------------------------------------
                    GarageSaveList? glist = null;
                    _player.GarageEnabled = _packet.GetBool();
                    if (_player.GarageEnabled)
                    {
                        string xmlgaragestring = _packet.GetString();
                        string presetname = _packet.GetString();
                        if(Servermanager.SaveGaragePreset(xmlgaragestring, _from, presetname))
                        {
                            glist = Servermanager.GarageDeserialize(presetname, _from);
                            _player.Gear.Garagesave = glist;
                            _player.Gear.garagexml = xmlgaragestring;
                            _player.Gear.presetname = presetname;

                        }
                        else
                        {
                            if (glist == null)
                            {
                                Console.WriteLine("Garage load failed");
                                OutgoingMessage.DisconnectPlayer("Garage failure", _from);
                                return;
                            }

                        }

                    }
                    else
                    {
                        _player.Gear.Garagesave = null;
                        _player.Gear.garagexml = "null";
                        _player.Gear.presetname = "null";
                    }



                    // Parkbuilder ----------------------------------------------------------
                    try
                    {


                        int Objectcount = _packet.GetInt();

                        if (Objectcount > 0)
                        {
                            Console.WriteLine($"Found objects to sync..");
                            for (int i = 0; i < Objectcount; i++)
                            {
                                string NameofGO = _packet.GetString();
                                string NameofFile = _packet.GetString();
                                string NameofBundle = _packet.GetString();

                                Vector3 Position = _packet.GetVector3();
                                Vector3 Rotation = _packet.GetVector3();
                                Vector3 Scale = _packet.GetVector3();
                                int ObjectID = _packet.GetInt();
                                string dir = _packet.GetString();

                                NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, dir);

                                try
                                {
                                    Servermanager.Players[_from].PlayerObjects.Add(OBJ);

                                    Console.WriteLine($"{Servermanager.Players[_from].Username} spawned a {NameofGO}, objectID: {ObjectID}");

                                }
                                catch (Exception x)
                                {
                                    Console.WriteLine($"Spawn object error adding to player list : NameofGO: {NameofGO}, nameoffile: {NameofFile}, nameofbundle: {NameofBundle}, Player: {Servermanager.Players[_from].Username}: error: {x}");
                                }

                            }

                        }

                    }
                    catch (Exception x)
                    {
                        Console.WriteLine("Error with Park Data : " + x);
                        OutgoingMessage.DisconnectPlayer("Error with ParkBuilder Data", _from);
                        return;
                    }


                    ///// Add all data to the players Loadout, created by the new Player assigned to this connection in Welcome


                    if (_player.Ridermodel == "Daryien")
                    {
                        _player.Gear.RiderTextures = RiderTexnames;
                    }
                    _player.Gear.Capforward = capforward;
                    _player.RiderRootPosition = pos;
                    _player.RiderRootRotation = rot;





                    Console.WriteLine($"Received Player Data and stored in loadout..");
                    Console.WriteLine("Matching Loadout data to Server data..");

                    for (int i = 0; i < RiderTexnames.Count; i++)
                    {
                        ServerData.FileCheckAndRequest(RiderTexnames[i].Nameoftexture, _from, RiderTexnames[i].directory);
                    }
                    for (int i = 0; i < _player.PlayerObjects.Count; i++)
                    {
                        ServerData.FileCheckAndRequest(_player.PlayerObjects[i].NameOfFile, _from, _player.PlayerObjects[i].Directory);
                    }

                    if (glist != null && glist.partMeshes != null)
                    {

                        foreach (PartMesh mesh in glist.partMeshes)
                        {
                            if (mesh.isCustom)
                            {
                                int indexer = mesh.fileName.LastIndexOf("/");
                                string shortname = mesh.fileName.Remove(0, indexer + 1);
                                string dir = ServerData.Garagepath + mesh.fileName;
                                dir = dir.Replace(shortname, "");

                                ServerData.FileCheckAndRequest(shortname, _from, dir);
                            }
                        }
                    }

                    if (glist != null && glist.partTextures != null)
                    {
                        foreach (PartTexture tex in glist.partTextures)
                        {
                            if (tex.url.ToLower().Contains("frostypgamemanager"))
                            {
                                int lastslash = tex.url.LastIndexOf("/");
                                string name = tex.url.Remove(0, lastslash + 1);
                                int f = tex.url.LastIndexOf("FrostyPGameManager");
                                string dir = "PIPE_Data/" + tex.url.Remove(0, f + 1);


                                ServerData.FileCheckAndRequest(name, _from, dir);
                            }
                        }
                    }



                    Console.WriteLine("Done");
                    OutgoingMessage.SendTextFromServerToOne(_from, "Rider sync starting..");


                    // if theres players, run setup all, collects playerinfo in groups of five max and sends, then send all player objects
                    if (Servermanager.Players.Count > 1)
                    {
                        // other riders already active
                        foreach (Player c in Servermanager.Players.Values.ToList())
                        {
                            if (c.Uniqueid != _from)
                            {
                                OutgoingMessage.NewPlayerArrived(c.Uniqueid, Servermanager.Players[_from]);

                            }
                        }

                        // this new rider
                        Console.WriteLine($"Telling {_player.Username} about {Servermanager.Players.Count - 1} other riders");
                        OutgoingMessage.PlayersAlreadyOnline(_from, Servermanager.Players.Values.ToList());


                    }

                    _player.ReadytoRoll = true;
                    OutgoingMessage.SendTextFromServerToOne(_from, "Ready to Roll!");

                }


            }
            catch (Exception x)
            {
                if (Servermanager.Players.TryGetValue(_from, out Player _player))
                {
                    Console.WriteLine($"Receive all parts error: player {_player.Username} : error {x}");
                }

                Console.WriteLine($"Receive all parts error: {x}");
                OutgoingMessage.DisconnectPlayer("Error with your Rider/Garage Data", _from);
            }



        }

        [MessageHandler((ushort)ClientPackets.PlayerRequestedFile)]
        public static void PlayerRequestedFile(ushort _from, Message _packet)
        {
            // read data
            string name = _packet.GetString();
            string dir = _packet.GetString();
            int Listcount = _packet.GetInt();
            List<int> Packetsowned = new List<int>();
            if (Listcount > 0)
            {
                for (int i = 0; i < Listcount; i++)
                {
                    int e = _packet.GetInt();
                    Packetsowned.Add(e);
                }

            }


            // if index exists, isSending and has our id number, were already receiving this file
            foreach (SendReceiveIndex Ind in ServerData.OutgoingIndexes)
            {
                if (Ind.NameOfFile == name && Ind.isSending && Ind.PlayerTosendTo == _from)
                {
                    OutgoingMessage.SendTextFromServerToOne(_from, "File already incoming");
                    return;

                }

                // continue send
                if (Ind.NameOfFile == name && !Ind.isSending && Ind.PlayerTosendTo == _from)
                {
                    Ind.isSending = true;
                    OutgoingMessage.SendTextFromServerToOne(_from, "Resumed");
                    return;

                }

            }

            // if not, try to find and send
            ServerData.FileCheckAndSend(name, Packetsowned, _from, dir);

        }

        /// <summary>
        /// Receive a Texture
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_packet"></param>
        [MessageHandler((ushort)ClientPackets.ReceiveFileSegment)]
        public static void FileSegmentReceive(ushort _from, Message _packet)
        {
            string name = _packet.GetString();
            int segmentscount = _packet.GetInt();
            int segmentno = _packet.GetInt();
            int bytecount = _packet.GetInt();
            byte[] bytes = _packet.GetBytes(bytecount);
            long Totalbytes = _packet.GetLong();
            string path = _packet.GetString();


            ServerData.FileSaver(bytes, name, segmentscount, segmentno, _from, Totalbytes, path);
            
        }

        [MessageHandler((ushort)ClientPackets.FileStatus)]
        public static void FileStatus(ushort _from, Message _packet)
        {
            string name = _packet.GetString();
            int Status = _packet.GetInt();

            ThreadTransfer.GiveToSendThread(() =>
            {
                foreach (SendReceiveIndex s in ServerData.OutgoingIndexes.ToList())
                {
                    if (s.NameOfFile.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() && s.PlayerTosendTo == _from)
                    {
                        ServerData.OutgoingIndexes.Remove(s);
                    }
                }

            });

        }

        [MessageHandler((ushort)ClientPackets.ReceiveMapname)]
        public static void ReceiveMapname(ushort _from, Message _packet)
        {
            string name = _packet.GetString();

            try
            {
                if (Servermanager.Players.TryGetValue(_from, out Player _player))
                {

                    _player.MapName = name;
                    OutgoingMessage.SendMapName(_from, name);
                    Console.WriteLine($"{_player.Username} went to {name}");
                    ServerData.FileCheckAndRequest(name, _from, "pipe_data/CustomMaps/");
                    if (_player.MapName.ToLower().Contains("samplescene") | _player.MapName.ToLower().Contains("buildplayer"))
                    {
                        OutgoingMessage.SendTextFromServerToOne(_from, "You appear to be using M to change map, using L for Patcha Map Changer syncs your map names properly so riders can be matched to your level");
                    }

                }

            }
            catch (Exception x)
            {
                Console.WriteLine($"Map name sync issue, player: {_from}:    " + x);
            }


        }

        [MessageHandler((ushort)ClientPackets.Turnmeon)]
        public static void TurnPlayerOn(ushort _from, Message _packet)
        {
            try
            {
                foreach (Player p in Servermanager.Players.Values)
                {
                    if (p.Uniqueid == _from)
                    {
                        p.ReadytoRoll = true;
                    }
                }

            }
            catch (Exception x)
            {
                Console.WriteLine("TurnPlayerON Error : " + x);

            }
        }

        [MessageHandler((ushort)ClientPackets.Turnmeoff)]
        public static void TurnPlayerOff(ushort _from, Message _packet)
        {
            try
            {
                foreach (Player p in Servermanager.Players.Values)
                {
                    if (p.Uniqueid == _from)
                    {
                        p.ReadytoRoll = false;
                    }
                }

            }
            catch (Exception x)
            {
                Console.WriteLine("TurnPlayerOff Error : " + x);

            }
        }

        [MessageHandler((ushort)ClientPackets.OverrideMapMatch)]
        public static void OverrideMapMatch(ushort _from, Message _packet)
        {
            ushort Playertooverride = _packet.GetUShort();
            bool Value = _packet.GetBool();

            if (Servermanager.Players.TryGetValue(_from, out Player player))
            {
                if (player.SendDataOverrides.ContainsKey(Playertooverride))
                {
                    player.SendDataOverrides[Playertooverride] = Value;
                }
                else
                {
                    player.SendDataOverrides.Add(Playertooverride, Value);
                }

                OutgoingMessage.SendTextFromServerToOne(_from, $"Override {Value} confirmed");
            }



        }

        [MessageHandler((ushort)ClientPackets.KeepAlive)]
        public static void KeepAlive(ushort _from, Message _packet)
        {
            // job done
            // Console.WriteLine($"Keep Alive : {(DateTime.Now - then).TotalMilliseconds}");
            //  then = DateTime.Now;


        }



        // user input
        [MessageHandler((ushort)ClientPackets.GearUpdate)]
        public static void GearUpdate(ushort _from, Message _packet)
        {


            if (!Servermanager.Players.TryGetValue(_from, out Player player))
            {
                return;
            }

            bool IsRiderUpdate = _packet.GetBool();


            if (IsRiderUpdate)
            {

                // Rider update

                List<TextureInfo> RidersTextures = new List<TextureInfo>();
                int amount = _packet.GetInt();

                for (int i = 0; i < amount; i++)
                {

                    string nameoftex = _packet.GetString();
                    string nameofgo = _packet.GetString();
                    int matnum = _packet.GetInt();
                    string dir = _packet.GetString();

                    RidersTextures.Add(new TextureInfo(nameoftex, nameofgo, false, matnum, dir));
                }

                bool capforward = _packet.GetBool();

                for (int i = 0; i < RidersTextures.Count; i++)
                {
                    ServerData.FileCheckAndRequest(RidersTextures[i].Nameoftexture, _from, RidersTextures[i].directory);
                }


                player.Gear.Capforward = capforward;
                player.Gear.RiderTextures = RidersTextures;

            }
            else
            {
                // bike ( garage )

                string xmlgaragestring = _packet.GetString();
                string presetname = _packet.GetString();
                //Console.WriteLine($"Garage Save string: " + xmlgaragestring);
                bool success = Servermanager.SaveGaragePreset(xmlgaragestring, _from, presetname);
                GarageSaveList glist = Servermanager.GarageDeserialize(presetname, _from);
                player.Gear.Garagesave = glist;
                player.Gear.presetname = presetname;
                player.Gear.garagexml = xmlgaragestring;


                if (player.Gear.Garagesave != null && player.Gear.Garagesave.partMeshes != null)
                {

                    foreach (PartMesh mesh in player.Gear.Garagesave.partMeshes)
                    {
                        if (mesh.isCustom)
                        {
                            int indexer = mesh.fileName.LastIndexOf("/");
                            string shortname = mesh.fileName.Remove(0, indexer + 1);
                            string dir = ServerData.Rootdir + "GarageContent/" + mesh.fileName;
                            dir = dir.Replace(shortname, "");

                            ServerData.FileCheckAndRequest(shortname, _from, dir);
                        }
                    }
                }

                if (player.Gear.Garagesave != null && player.Gear.Garagesave.partTextures != null)
                {
                    foreach (PartTexture tex in player.Gear.Garagesave.partTextures)
                    {
                        if (tex.url.ToLower().Contains("frostypgamemanager"))
                        {
                            int lastslash = tex.url.LastIndexOf("/");
                            string name = tex.url.Remove(0, lastslash + 1);
                            int f = tex.url.LastIndexOf("frostypmanager");
                            string dir = ServerData.Rootdir + tex.url.Remove(0, f + 1);

                            ServerData.FileCheckAndRequest(name, _from, dir);
                        }
                    }
                }




            }

            Message ServersPacket = Message.Create(MessageSendMode.reliable, ServerPacket.GearUpdate);
            // send on to others
            
                // who from and which type of gear update
                ServersPacket.AddUShort(_from);
                ServersPacket.AddBool(IsRiderUpdate);

                if (IsRiderUpdate)
                {
                    // pack on rider info

                    ServersPacket.AddInt(player.Gear.RiderTextures.Count);
                    if (player.Gear.RiderTextures.Count > 0)
                    {
                        for (int i = 0; i < player.Gear.RiderTextures.Count; i++)
                        {
                            ServersPacket.AddString(player.Gear.RiderTextures[i].Nameoftexture);
                            ServersPacket.AddString(player.Gear.RiderTextures[i].NameofparentGameObject);
                            ServersPacket.AddInt(player.Gear.RiderTextures[i].Matnum);
                            ServersPacket.AddString(player.Gear.RiderTextures[i].directory);
                        }

                    }

                    ServersPacket.AddBool(player.Gear.Capforward);

                }
                else
                {
                    //pack on bike
                    ServersPacket.AddString(player.Gear.garagexml);
                    ServersPacket.AddString(player.Gear.presetname);
                }

                OutgoingMessage.SendGearUpdate(_from, ServersPacket);

            
            Console.WriteLine("Gear Update stored and relayed, player:" + player.Username);
        }

        [MessageHandler((ushort)ClientPackets.TransformUpdate)]
        public static void TransformReceive(ushort _from, Message _packet)
        {
            if (!Servermanager.Players.TryGetValue(_from, out Player player)) return; 

                long Serverutc = DateTime.Now.ToFileTimeUtc();
                int _ping = 1;

            try
            {
                // if there's players, send on
                if (Servermanager.Players.Count > 1)
                {
                    OutgoingMessage.SendATransformUpdate(_from, _packet, Serverutc, _ping);
                }


                // store latest root for initialization
                double msFromLast = _packet.GetDouble();
                Vector3 pos = _packet.GetVector3();
                Vector3 rot = _packet.GetVector3();

                
                ThreadTransfer.GiveToSystemThread(() =>
                {
                    player.RiderRootPosition = pos;
                    player.RiderRootRotation = rot;
                    player.AddtoUpdateRate(msFromLast);
                });
                
            }
            catch (Exception)
            {
                Console.WriteLine($"Transform relay error : clientid {_from} ");
            }

        }

        [MessageHandler((ushort)ClientPackets.SendAudioUpdate)]
        public static void ReceiveAudioUpdate(ushort _from, Message _packet)
        {


            try
            {
                int RiserOrOneshot = _packet.GetInt();


                if (RiserOrOneshot == 1)
                {

                    string nameofriser = _packet.GetString();
                    int playstate = _packet.GetInt();
                    float vol = _packet.GetFloat();
                    float pitch = _packet.GetFloat();
                    float vel = _packet.GetFloat();
                    MessageSendMode flagcode = (MessageSendMode)_packet.GetByte();


                    if (Servermanager.Players.Count > 1)
                    {
                        Message outpacket = Message.Create(flagcode, ServerPacket.SendAudioUpdate);
                        
                            outpacket.AddUShort(_from);
                            outpacket.AddInt(RiserOrOneshot);
                            outpacket.AddString(nameofriser);
                            outpacket.AddInt(playstate);
                            outpacket.AddFloat(vol);
                            outpacket.AddFloat(pitch);
                            outpacket.AddFloat(vel);

                            OutgoingMessage.SendAudioUpdate(_from, outpacket);
                    }


                }
                if (RiserOrOneshot == 2)
                {
                    string nameofoneshot = _packet.GetString();
                    float volofoneshot = _packet.GetFloat();

                    if (Servermanager.Players.Count > 1)
                    {
                        Message outpacket = Message.Create(MessageSendMode.reliable, ServerPacket.SendAudioUpdate);
                       
                            outpacket.AddUShort(_from);
                            outpacket.AddInt(RiserOrOneshot);
                            outpacket.AddString(nameofoneshot);
                            outpacket.AddFloat(volofoneshot);

                            OutgoingMessage.SendAudioUpdate(_from, outpacket);
                        


                    }


                }


            }
            catch (Exception x)
            {
                Console.WriteLine("Failed audio relay, Player left? :   " + x);
            }






        }

        [MessageHandler((ushort)ClientPackets.SendTextMessage)]
        public static void RelayPlayerMessage(ushort _from, Message _packet)
        {
            string _mess = _packet.GetString();

            foreach (string banword in ServerData.BannedWords)
            {
                if (_mess.ToLower().Contains(banword))
                {
                    System.Random Randomgenerator = new System.Random();
                    int num = Randomgenerator.Next(0, ServerData.BanMessageAlternates.Count - 1);
                    _mess = ServerData.BanMessageAlternates[num];
                }
            }

            OutgoingMessage.SendTextMessageFromPlayerToAll(_from, _mess);

        }

        [MessageHandler((ushort)ClientPackets.VoteToRemoveObject)]
        public static void VoteToRemoveObject(ushort _from, Message _packet)
        {
            int ObjectId = _packet.GetInt();
            ushort OwnerID = _packet.GetUShort();


            foreach (Player p in Servermanager.Players.Values)
            {
                foreach (NetGameObject n in p.PlayerObjects.ToList())
                {
                    if (n.ObjectID == ObjectId)
                    {
                        foreach (ushort voter in n.Votestoremove)
                        {
                            if (voter == _from)
                            {
                                OutgoingMessage.SendTextFromServerToOne(_from, "Already voted for this object");
                                return;
                            }
                        }

                        n.Votestoremove.Add(_from);
                        OutgoingMessage.SendTextFromServerToOne(_from, $"Ban vote at {n.Votestoremove.Count} of {Servermanager.Players.Count / 2}");
                        if (n.Votestoremove.Count >= Servermanager.Players.Count / 2 && n.Votestoremove.Count > 1)
                        {
                            OutgoingMessage.DestroyAnObjectToAllButOwner(OwnerID, ObjectId);
                            p.PlayerObjects.Remove(n);
                            p.AmountofObjectBoots++;
                            OutgoingMessage.SendTextFromServerToOne(OwnerID, $"Your {n.NameofObject} object was booted from the server, boot count {p.AmountofObjectBoots} of 5");
                            if (p.AmountofObjectBoots >= 5)
                            {
                                Servermanager.BanPlayer(p.Username,Servermanager.GetPlayerIP(OwnerID), OwnerID, 10);

                                OutgoingMessage.DisconnectPlayer($"You were booted for {10} minutes", p.Uniqueid);

                            }

                        }
                    }
                }
            }

        }

        [MessageHandler((ushort)ClientPackets.InviteToSpawn)]
        public static void InviteToSpawn(ushort _from, Message _packet)
        {
            ushort goingto = _packet.GetUShort();
            Vector3 pos = _packet.GetVector3();
            Vector3 rot = _packet.GetVector3();

            OutgoingMessage.InviteToSpawn(_from, goingto, pos, rot);
        }




        // parkbuilder
        [MessageHandler((ushort)ClientPackets.SpawnNewObjectreceive)]
        public static void SpawnNewObject(ushort _ownerID, Message _packet)
        {
            
            try
            {

                if (Servermanager.Players.TryGetValue(_ownerID, out Player player))
                {
                    try
                    {

                        string NameofGO = _packet.GetString();
                        string NameofFile = _packet.GetString();
                        string NameofBundle = _packet.GetString();

                        Vector3 Position = _packet.GetVector3();
                        Vector3 Rotation = _packet.GetVector3();
                        Vector3 Scale = _packet.GetVector3();
                        int ObjectID = _packet.GetInt();
                        string directory = _packet.GetString();

                        NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, directory);









                        player.PlayerObjects.Add(OBJ);
                        foreach (Player p in Servermanager.Players.Values.ToList())
                        {
                            if (p.Uniqueid != _ownerID)
                            {
                                OutgoingMessage.SpawnAnObject(p.Uniqueid, _ownerID, OBJ);
                            }
                        }
                        Console.WriteLine($"{player.Username} spawned a {NameofGO}, objectID: {ObjectID}");
                        ServerData.FileCheckAndRequest(NameofFile, _ownerID, directory);

                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"Spawn Object Error : Player {player} : V{player.VersionNo} : object count {player.PlayerObjects.Count} : error :  " + x);
                    }






                }


            }
            catch (Exception x)
            {
                Console.WriteLine($"Spawn object error, Player not found: {x}");

            }


        }

        [MessageHandler((ushort)ClientPackets.DestroyAnObject)]
        public static void DestroyObject(ushort _ownerID, Message _packet)
        {

            try
            {
                int ObjectID = _packet.GetInt();

                // remove from players object list
                NetGameObject objtodel = null;

                foreach (NetGameObject n in Servermanager.Players[_ownerID].PlayerObjects)
                {
                    if (n.ObjectID == ObjectID)
                    {
                        objtodel = n;
                    }
                }
                if (objtodel != null)
                {
                    Servermanager.Players[_ownerID].PlayerObjects.Remove(objtodel);
                    // Tell all players

                    OutgoingMessage.DestroyAnObjectToAllButOwner(_ownerID, ObjectID);

                    Console.WriteLine($"{Servermanager.Players[_ownerID].Username} destroyed an object, objectID: {ObjectID}");

                }
                //

            }
            catch (Exception X)
            {
                Console.WriteLine($"Destroy Object Error: player {Servermanager.Players[_ownerID].Username}:  error:   {X}");
            }


        }

        [MessageHandler((ushort)ClientPackets.MoveAnObject)]
        public static void MoveObject(ushort _ownerID, Message _packet)
        {
            try
            {
                int objectID = _packet.GetInt();
                Vector3 _newpos = _packet.GetVector3();
                Vector3 _newrot = _packet.GetVector3();
                Vector3 _newscale = _packet.GetVector3();

                foreach (NetGameObject n in Servermanager.Players[_ownerID].PlayerObjects)
                {
                    if (n.ObjectID == objectID)
                    {
                        n.Position = _newpos;
                        n.Rotation = _newrot;
                        n.Scale = _newscale;

                        OutgoingMessage.MoveAnObject(_ownerID, n);
                    }
                }

            }
            catch (Exception x)
            {
                Console.WriteLine($"Move Object error : {x}");
            }


        }





        // admin
        [MessageHandler((ushort)ClientPackets.AdminLogin)]
        public static void ReceiveAdminlogin(ushort _admin, Message _packet)
        {
            string _password = _packet.GetString();
            if (Servermanager.config.Adminpassword == _password)
            {
                Servermanager.Players[_admin].AdminLoggedIn = true;
                OutgoingMessage.LoginGood(_admin);
                Servermanager.Players[_admin].AdminStreamWatch.Start();
            }
            else
            {
                OutgoingMessage.SendTextFromServerToOne(_admin, "Incorrect Password");
            }
        }

        [MessageHandler((ushort)ClientPackets.LogOut)]
        public static void AdminLogOut(ushort _admin, Message _packet)
        {
            Servermanager.Players[_admin].AdminLoggedIn = false;
            Servermanager.Players[_admin].AdminStream = false;
            Servermanager.Players[_admin].AdminStreamWatch.Stop();
            Servermanager.Players[_admin].AdminStreamWatch.Reset();
        }

        [MessageHandler((ushort)ClientPackets.ReceiveBootPlayer)]
        public static void AdminBootPlayer(ushort _admin, Message _packet)
        {
            string _usertoboot = _packet.GetString();
            int mins = _packet.GetInt();



            foreach (Player p in Servermanager.Players.Values)
            {
                if (p.Username == _usertoboot)
                {

                    Servermanager.BanPlayer(p.Username, Servermanager.GetPlayerIP(p.Uniqueid), _admin, mins);

                    OutgoingMessage.DisconnectPlayer($"You were booted for {mins} minutes", p.Uniqueid);
                    OutgoingMessage.SendTextFromServerToOne(_admin, $"{p.Username} was booted for {mins} mins");

                }
            }



        }

        [MessageHandler((ushort)ClientPackets.AdminRemoveObject)]
        public static void AdminRemoveObject(ushort _admin, Message _packet)
        {
            ushort OwnerID = _packet.GetUShort();
            int ObjectId = _packet.GetInt();


            foreach (Player p in Servermanager.Players.Values)
            {
                foreach (NetGameObject n in p.PlayerObjects.ToList())
                {
                    if (n.ObjectID == ObjectId)
                    {

                        OutgoingMessage.DestroyAnObjectToAllButOwner(OwnerID, ObjectId);
                        p.PlayerObjects.Remove(n);

                        OutgoingMessage.SendTextFromServerToOne(OwnerID, $"Your {n.NameofObject} object was removed by admin");



                    }
                }
            }


        }

        [MessageHandler((ushort)ClientPackets.AlterBanWords)]
        public static void AdminAlterBanwords(ushort _admin, Message _packet)
        {
            string word = _packet.GetString();
            bool addtolist = _packet.GetBool();

            ServerData.AlterBanWords(_admin, addtolist, word);
        }



    }
}
