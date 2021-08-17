using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Valve.Sockets;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Functions attached to ConnectionCallBacks, Fired by receiving matching packetCode
    /// </summary>
    class ServersHandles
    {

       


        // server comms


        /// <summary>
        /// Connection happened, i replied, now they've confirmed they're connected
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_pack"></param>
        public static void WelcomeReceived(uint _from,Packet _pack)
        {
            try
            {
            // receive
            string name = _pack.ReadString();
            string Ridermodel = _pack.ReadString();
            string RidermodelBundlename = _pack.ReadString();
            string CurrentLevel = "Unknown";
            float VersionNo = 0.00f;
            try
            {
            CurrentLevel = _pack.ReadString();
            }
            catch(Exception x)
            { 
            Console.WriteLine($"couldnt get map name, using {CurrentLevel} in their package: {x}");
                    Server.Connection.CloseConnection(_from);
                    return;

            }
            try
            {
                VersionNo = _pack.ReadFloat();
               
            }
            catch (Exception x)
            {
                Console.WriteLine($"no Version number found from {name}, cut off");
                Server.Connection.CloseConnection(_from);
                return;
            }

            if (Ridermodel.ToLower().Contains("prefab"))
            {
                ServerSend.DisconnectPlayer($"player model:{Ridermodel}: isn't supportable", _from);
                Console.WriteLine($"Player refused due to ridermodel name: {Ridermodel}");
                    return;
            }

            if(VersionNo != Server.VERSIONNUMBER)
            {
                    if (VersionNo > 2.13f) { ServerSend.Update(_from, ServerData.GiveUpdateFileNames()); Console.WriteLine($"Player {name} offered update. Their version:{VersionNo}"); }
                    else { ServerSend.DisconnectPlayer($"Mod Version Doesnt Match, Install Version {Server.VERSIONNUMBER} to connect to this Server", _from); Console.WriteLine($"Player {name} refused. Their version:{VersionNo}"); }
                   
                    return;
            }
            else
            {
                foreach(string banword in ServerData.BannedWords)
                {
                    if (name.ToLower().Contains(banword))
                    {
                       name = name.ToLower().Replace(banword, "XOXO");
                    }
                }

                /// Version is correct, continue setup...
                
            Console.WriteLine($"{name} starting riding as {Ridermodel} at {CurrentLevel}");
            Console.WriteLine("Setting up their server data..");
            Player p = new Player(_from, Ridermodel, name, RidermodelBundlename,CurrentLevel);
            Server.Players.Add(_from, p);


                    // add timeout watch for player and start
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    Server.TimeoutWatches.Add(_from, watch);


                    // the receiving of all parts now triggers setup of players
                    ServerSend.RequestAllParts(_from);
                    Console.WriteLine("Server Data setup, Request sent for all RiderData");

                    Console.WriteLine("Checking Server has RiderModel and Map");
                    if(Ridermodel != "Daryien")
                    {
                     ServerData.FileCheckAndRequest(Ridermodel, _from,"pipe_data/Custom Players/");
                    }
                    if(CurrentLevel != "Unknown" && CurrentLevel != "" && CurrentLevel != " " && !CurrentLevel.ToLower().Contains("pipe") && !CurrentLevel.ToLower().Contains("chuck"))
                    {
                        ServerData.FileCheckAndRequest(CurrentLevel,_from, "pipe_data/CustomMaps/");
                    }

                    
            }



            }
            catch (Exception x)
            {
                Console.WriteLine("Welcome error: " + x);
                Server.Connection.CloseConnection(_from);
            }




        }

        public static void ReceiveAllParts(uint _from, Packet _packet)
        {
            Console.WriteLine("Receiving Player Data");


            try
            {
                if(Server.Players.TryGetValue(_from, out Player _player))
                {
                    // rider
                    
                    Vector3 pos = _packet.ReadVector3();
                    Vector3 rot = _packet.ReadVector3();
                    bool capforward = true;


                   List<TextureInfo> RiderTexnames = new List<TextureInfo>();

                   if(_player.Ridermodel == "Daryien")
                   {
                     int Ridertexcount = _packet.ReadInt();
                     if (Ridertexcount > 0)
                     {

                         for (int i = 0; i < Ridertexcount; i++)
                         {
                           string Texname = _packet.ReadString();
                           string ParentG_O = _packet.ReadString();
                           int matnum = _packet.ReadInt();
                           string dir = _packet.ReadString();
                           RiderTexnames.Add(new TextureInfo(Texname, ParentG_O,false,matnum,dir));
                         }

                    
                     }

                        capforward = _packet.ReadBool();
                   }

                     // bike ( garage )

                     int bytecount = _packet.ReadInt();
                    if(bytecount == 0)
                    {
                        ServerSend.DisconnectPlayer("Server encountered error reading your Garage save data, check that your preset works as intended and that your logging on messages show the correct preset being sent", _from);
                        return;
                    }

                     byte[] bytes = _packet.ReadBytes(bytecount);
                     Console.WriteLine($"Garage Save bytes: {bytes.Length}");
                     SaveList glist = new SaveList();
                     glist = ServerData.DeserialiseGarage(bytes);

                     if(glist == null)
                     {
                       Console.WriteLine("Fail to cast garage save: Receive all parts");
                     }




                // Parkbuilder

                int Objectcount = _packet.ReadInt();

            if (Objectcount > 0)
            {
                Console.WriteLine($"Found objects to sync..");
                for (int i = 0; i < Objectcount; i++)
                {
                    string NameofGO = _packet.ReadString();
                    string NameofFile = _packet.ReadString();
                    string NameofBundle = _packet.ReadString();

                    Vector3 Position = _packet.ReadVector3();
                    Vector3 Rotation = _packet.ReadVector3();
                    Vector3 Scale = _packet.ReadVector3();
                    int ObjectID = _packet.ReadInt();
                    string dir = _packet.ReadString();

                    NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID,dir);

                    try
                    {
                        Server.Players[_from].PlayerObjects.Add(OBJ);
                       
                        Console.WriteLine($"{Server.Players[_from].Username} spawned a {NameofGO}, objectID: {ObjectID}");

                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"Spawn object error adding to player list : NameofGO: {NameofGO}, nameoffile: {NameofFile}, nameofbundle: {NameofBundle}, Player: {Server.Players[_from].Username}: error: {x}");
                    }

                }

            }



            ///// Add all data to the players Loadout, created by the new Player assigned to this connection in Welcome

           
                    if(_player.Ridermodel == "Daryien")
                    {
                    _player.Gear.RiderTextures = RiderTexnames;
                    }
                    _player.Gear.Capforward = capforward;
                    _player.Gear.Garagesave = bytes;
                    _player.RiderRootPosition = pos;
                    _player.RiderRootRotation = rot;

                
           


            Console.WriteLine($"Received Player Data and stored in loadout..");
                Console.WriteLine("Matching Loadout data to Server data..");
               
                for (int i = 0; i < RiderTexnames.Count; i++)
                {
                    ServerData.FileCheckAndRequest(RiderTexnames[i].Nameoftexture, _from,RiderTexnames[i].directory);
                }
                for (int i = 0; i < _player.PlayerObjects.Count; i++)
                {
                    ServerData.FileCheckAndRequest(_player.PlayerObjects[i].NameOfFile, _from, _player.PlayerObjects[i].Directory);
                }

                if(glist != null && glist.partMeshes != null)
                {

                foreach (PartMesh mesh in glist.partMeshes)
                {
                    if (mesh.isCustom)
                    {
                            int indexer = mesh.fileName.LastIndexOf("/");
                            string shortname = mesh.fileName.Remove(0, indexer + 1);
                                string dir = "PIPE_Data/GarageContent/" + mesh.fileName;
                                dir = dir.Replace(shortname, "");

                        ServerData.FileCheckAndRequest(shortname, _from,dir);
                    }
                }
                }

                if(glist != null && glist.partTextures != null)
                {
                    foreach(PartTexture tex in glist.partTextures)
                    {
                        if (tex.url.ToLower().Contains("frostypgamemanager"))
                        {
                            int lastslash = tex.url.LastIndexOf("/");
                            string name = tex.url.Remove(0, lastslash + 1);
                                int f = tex.url.LastIndexOf("FrostyPGameManager");
                                string dir = "PIPE_Data/" + tex.url.Remove(0,f + 1);


                            ServerData.FileCheckAndRequest(name, _from,dir);
                        }
                    }
                }



                          Console.WriteLine("Done");
                    ServerSend.SendTextFromServerToOne(_from, "Player Data good..");




                    // Start setup for every player but this one --------------------    
                    Console.WriteLine($"Telling others about this player..");
                    
                        foreach (Player c in Server.Players.Values.ToList())
                        {
                           if (c.RiderID != _from)
                           {
                             ServerSend.SetupNewPlayer(c.RiderID, Server.Players[_from]);
                   
                           }
                        }

                        

                      // if theres players, run setup all, collects playerinfo in groups of five max and sends, then send all player objects
                      if (Server.Players.Count > 1)
                      {
                           Console.WriteLine($"Telling {_player.Username} about {Server.Players.Count - 1} other players");
                           ServerSend.SetupAllOnlinePlayers(_from, Server.Players.Values.ToList());

                          
                      }

                       _player.ReadytoRoll = true;
                    ServerSend.SendTextFromServerToOne(_from, "Ready to Roll!");














                }


            }
            catch(Exception x)
            {
                Console.WriteLine($"Receive all parts error: {x}");
                ServerSend.DisconnectPlayer("Error with your Rider/Garage Data", _from);
            }



        }

        public static void PlayerRequestedFile(uint _from, Packet _packet)
        {
            // read data
            string name = _packet.ReadString();
            string dir = _packet.ReadString();
            int Listcount = _packet.ReadInt();
            List<int> Packetsowned = new List<int>();
            if (Listcount > 0)
            {
              for (int i = 0; i < Listcount; i++)
              {
                int e = _packet.ReadInt();
                Packetsowned.Add(e);
              }

            }


                // if index exists, isSending and has our id number, were already receiving this file
            foreach(SendReceiveIndex Ind in ServerData.OutgoingIndexes)
            {
                if(Ind.NameOfFile == name && Ind.isSending && Ind.PlayerTosendTo == _from)
                {
                  ServerSend.SendTextFromServerToOne(_from, "File already incoming");
                     return;
                          
                }

                // continue send
                    if (Ind.NameOfFile == name && !Ind.isSending && Ind.PlayerTosendTo == _from)
                    {
                        Ind.isSending = true;
                        ServerSend.SendTextFromServerToOne(_from, "Resumed");
                        return;

                    }

                }

            // if not, try to find and send
            ServerData.FileCheckAndSend(name,Packetsowned,_from,dir);

        }

        /// <summary>
        /// Receive a Texture
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_packet"></param>
        public static void FileSegmentReceive(uint _from, Packet _packet)
        {
            string name = _packet.ReadString();
            int segmentscount = _packet.ReadInt();
            int segmentno = _packet.ReadInt();
            int bytecount = _packet.ReadInt();
            byte[] bytes = _packet.ReadBytes(bytecount);
            long Totalbytes = _packet.ReadLong();
            string path = _packet.ReadString();

          
            ServerData.FileSaver(bytes, name,segmentscount,segmentno, _from, Totalbytes,path);

        }

        public static void FileStatus(uint _from, Packet _packet)
        {
            string name = _packet.ReadString();
            int Status = _packet.ReadInt();

            ThreadManager.ExecuteOnMainThread(() =>
            {
                foreach (SendReceiveIndex s in ServerData.OutgoingIndexes.ToList())
                {
                    if (s.NameOfFile == name && s.PlayerTosendTo == _from)
                    {
                        ServerData.OutgoingIndexes.Remove(s);
                    }
                }

            });

        }

        public static void ReceiveMapname(uint _from, Packet _packet)
        {
            string name = _packet.ReadString();

            try
            {
                if(Server.Players.TryGetValue(_from,out Player _player))
                {

                 _player.MapName = name;
                ServerSend.SendMapName(_from,name);
                Console.WriteLine($"{_player.Username} went to {name}");
                ServerData.FileCheckAndRequest(name, _from, "pipe_data/CustomMaps/");


                }

            }
            catch(Exception x)
            {
                Console.WriteLine($"Map name sync issue, player: {_from}:    " + x);
            }


        }

        public static void TurnPlayerOn(uint _from, Packet _packet)
        {
            try
            {
                foreach(Player p in Server.Players.Values)
                {
                    if(p.RiderID == _from)
                    {
                        p.ReadytoRoll = true;
                    }
                }

            }
            catch (Exception x )
            {
                Console.WriteLine("TurnPlayerON Error : " + x);
               
            }
        }

        public static void TurnPlayerOff(uint _from, Packet _packet)
        {
            try
            {
                foreach (Player p in Server.Players.Values)
                {
                    if (p.RiderID == _from)
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

        public static void KeepAlive(uint _from, Packet _packet)
        {
            // job done
           // Console.WriteLine($"Keep Alive : {(DateTime.Now - then).TotalMilliseconds}");
          //  then = DateTime.Now;


        }





        // user input

        public static void GearUpdate(uint _from, Packet _packet)
        {
          

            if(!Server.Players.TryGetValue(_from,out Player player))
            {
                return;
            }
            
            bool IsRiderUpdate = _packet.ReadBool();


            if (IsRiderUpdate)
            {

                // Rider update

                List<TextureInfo> RidersTextures = new List<TextureInfo>();
                int amount = _packet.ReadInt();

                for (int i = 0; i < amount; i++)
                {
                   
                    string nameoftex = _packet.ReadString();
                    string nameofgo = _packet.ReadString();
                    int matnum = _packet.ReadInt();
                    string dir = _packet.ReadString();

                    RidersTextures.Add(new TextureInfo(nameoftex, nameofgo, false, matnum,dir));
                }

                bool capforward = _packet.ReadBool();

                for (int i = 0; i < RidersTextures.Count; i++)
                {
                    ServerData.FileCheckAndRequest(RidersTextures[i].Nameoftexture, _from,RidersTextures[i].directory);
                }


                player.Gear.Capforward = capforward;
                player.Gear.RiderTextures = RidersTextures;

            }
            else
            {
                // bike ( garage )

                int bytecount = _packet.ReadInt();
                byte[] bytes = _packet.ReadBytes(bytecount);
                player.Gear.Garagesave = bytes;

                SaveList glist = new SaveList();
                glist = ServerData.DeserialiseGarage(bytes);

                if (glist != null && glist.partMeshes != null)
                {

                    foreach (PartMesh mesh in glist.partMeshes)
                    {
                        if (mesh.isCustom)
                        {
                            int indexer = mesh.fileName.LastIndexOf("/");
                            string shortname = mesh.fileName.Remove(0, indexer + 1);
                            string dir = ServerData.Rootdir + "GarageContent/" + mesh.fileName;
                            dir = dir.Replace(shortname, "");

                            ServerData.FileCheckAndRequest(shortname, _from,dir);
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
                            int f = tex.url.LastIndexOf("frostypmanager");
                            string dir = ServerData.Rootdir + tex.url.Remove(0, f + 1);

                            ServerData.FileCheckAndRequest(name, _from,dir);
                        }
                    }
                }




            }




            // send on to others
            using (Packet ServersPacket = new Packet((int)ServerPacket.GearUpdate))
            {
                ServersPacket.Write(_from);
                ServersPacket.Write(IsRiderUpdate);

                if (IsRiderUpdate)
                {
                    // pass on rider info

                    ServersPacket.Write(player.Gear.RiderTextures.Count);
                    if (player.Gear.RiderTextures.Count > 0)
                    {
                        for (int i = 0; i < player.Gear.RiderTextures.Count; i++)
                        {
                            ServersPacket.Write(player.Gear.RiderTextures[i].Nameoftexture);
                            ServersPacket.Write(player.Gear.RiderTextures[i].NameofparentGameObject);
                            ServersPacket.Write(player.Gear.RiderTextures[i].Matnum);
                            ServersPacket.Write(player.Gear.RiderTextures[i].directory);
                        }

                    }

                    ServersPacket.Write(player.Gear.Capforward);

                }
                else
                {
                    ServersPacket.Write(player.Gear.Garagesave.Length);
                    ServersPacket.Write(player.Gear.Garagesave);
                  
                }




                ServerSend.SendGearUpdate(_from, ServersPacket);

            }
            Console.WriteLine("Gear Update stored and relayed, player:" + player.Username);
        }

        public static void TransformReceive(uint _from, Packet _packet)
        {
            

            long ServerStamp = DateTime.Now.ToFileTimeUtc();
            ConnectionStatus info = new ConnectionStatus();
            Server.Connection.GetQuickConnectionStatus(_from, ref info);
            int _ping = info.ping;

            try
            {


                // if there's players, send on
            if (Server.Players.Count > 1)
            {
              ServerSend.SendATransformUpdate(_from, _packet,ServerStamp, _ping);
            }


                // store latest root for initialization
                long stamp = _packet.ReadLong();
                float ms = _packet.ReadFloat();
                Vector3 pos = _packet.ReadVector3();
                Vector3 rot = _packet.ReadVector3();

                if (Server.Players.TryGetValue(_from, out Player player))
                {
                    player.RiderRootPosition = pos;
                    player.RiderRootRotation = rot;
                }


            }
            catch (Exception)
            {

               
            }

        }

        public static void ReceiveAudioUpdate(uint _from, Packet _packet)
        {

           
            try
            {
            int code = _packet.ReadInt();


                if(code == 1)
                {

                    string nameofriser = _packet.ReadString();
                    int playstate = _packet.ReadInt();
                    float vol = _packet.ReadFloat();
                    float pitch = _packet.ReadFloat();
                    float vel = _packet.ReadFloat();
                    int flagcode = _packet.ReadInt();


                  if (Server.Players.Count > 1)
                  {
                    using (Packet outpacket = new Packet((int)ServerPacket.SendAudioUpdate))
                    {
                        outpacket.Write(_from);
                        outpacket.Write(code);
                        outpacket.Write(nameofriser);
                        outpacket.Write(playstate);
                        outpacket.Write(vol);
                        outpacket.Write(pitch);
                        outpacket.Write(vel);


                        if (flagcode == 1)
                        {
                          ServerSend.SendAudioUpdate(_from,outpacket.ToArray(),SendFlags.Reliable);
                        }
                        else if(flagcode == 2)
                        {
                            ServerSend.SendAudioUpdate(_from, outpacket.ToArray(), SendFlags.Unreliable);
                        }


                    }


                  }


                }
                if(code == 2)
                {
                  string nameofoneshot = _packet.ReadString();
                  float volofoneshot = _packet.ReadFloat();

                    if (Server.Players.Count > 1)
                    {
                        using (Packet outpacket = new Packet((int)ServerPacket.SendAudioUpdate))
                        {
                            outpacket.Write(_from);
                            outpacket.Write(code);
                            outpacket.Write(nameofoneshot);
                            outpacket.Write(volofoneshot);

                            ServerSend.SendAudioUpdate(_from, outpacket.ToArray(), SendFlags.Reliable);
                        }


                    }


                }


            }
            catch (Exception x)
            {
                Console.WriteLine("Failed audio relay, Player left? :   " + x);
            }
            
           




        }

        public static void RelayPlayerMessage(uint _from, Packet _packet)
        {
            string _mess = _packet.ReadString();
           
            foreach(string banword in ServerData.BannedWords)
            {
                if (_mess.ToLower().Contains(banword))
                {
                    Random Randomgenerator = new Random();
                    int num = Randomgenerator.Next(0, ServerData.BanMessageAlternates.Count - 1);
                            _mess = ServerData.BanMessageAlternates[num];
                }
            }
            
            ServerSend.SendTextMessageFromPlayerToAll(_from, _mess);
            
        }

        public static void VoteToRemoveObject(uint _from, Packet _packet)
        {
            int ObjectId = _packet.ReadInt();
            uint OwnerID = (uint)_packet.ReadLong();


            foreach(Player p in Server.Players.Values)
            {
                foreach(NetGameObject n in p.PlayerObjects.ToList())
                {
                    if(n.ObjectID == ObjectId)
                    {
                        foreach(uint voter in n.Votestoremove)
                        {
                            if(voter == _from)
                            {
                                ServerSend.SendTextFromServerToOne(_from, "Already voted for this object");
                                return;
                            }
                        }

                        n.Votestoremove.Add(_from);
                        ServerSend.SendTextFromServerToOne(_from, $"Ban vote at {n.Votestoremove.Count} of {(Server.Players.Count / 2)}");
                        if(n.Votestoremove.Count >= Server.Players.Count / 2 && n.Votestoremove.Count > 1)
                        {
                            ServerSend.DestroyAnObjectToAllButOwner(OwnerID, ObjectId);
                            p.PlayerObjects.Remove(n);
                            p.AmountofObjectBoots++;
                            ServerSend.SendTextFromServerToOne(OwnerID, $"Your {n.NameofObject} object was booted from the server, boot count {p.AmountofObjectBoots} of 5");
                            if(p.AmountofObjectBoots >= 5)
                            {
                                Valve.Sockets.ConnectionInfo info = new Valve.Sockets.ConnectionInfo();
                                Server.Connection.GetConnectionInfo(p.RiderID, ref info);
                                ServerData.BanPlayer(p.Username, info.address.GetIP(), OwnerID, 10);

                                ServerSend.DisconnectPlayer($"You were booted for {10} minutes", p.RiderID);
                                
                            }

                        }
                    }
                }
            }

        }

        public static void InviteToSpawn(uint _from, Packet _packet)
        {
            uint goingto = (uint)_packet.ReadLong();
            Vector3 pos = _packet.ReadVector3();
            Vector3 rot = _packet.ReadVector3();

            ServerSend.InviteToSpawn(_from, goingto, pos, rot);
        }






        // parkbuilder

        public static void SpawnNewObject(uint _ownerID, Packet _packet)
        {
            string NameofGO = _packet.ReadString();
            string NameofFile = _packet.ReadString();
            string NameofBundle = _packet.ReadString();

            Vector3 Position = _packet.ReadVector3();
            Vector3 Rotation = _packet.ReadVector3();
            Vector3 Scale = _packet.ReadVector3();
            int ObjectID = _packet.ReadInt();
            string directory = _packet.ReadString();

            NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID,directory);

            try
            {
                Server.Players[_ownerID].PlayerObjects.Add(OBJ);
                foreach(Player p in Server.Players.Values.ToList())
                {
                    if(p.RiderID != _ownerID)
                    {
                    ServerSend.SpawnAnObject(p.RiderID,_ownerID,OBJ);
                    }
                }
                Console.WriteLine($"{Server.Players[_ownerID].Username} spawned a {NameofGO}, objectID: {ObjectID}");

                ServerData.FileCheckAndRequest(NameofFile, _ownerID, directory);
                

            }
            catch (Exception x)
            {
                Console.WriteLine($"Spawn object error adding to player list : NameofGO: {NameofGO}, nameoffile: {NameofFile}, nameofbundle: {NameofBundle}, Player: {Server.Players[_ownerID].Username}: error: {x}");
            }

           

        }


        public static void DestroyObject(uint _ownerID, Packet _packet)
        {

            try
            {
            int ObjectID = _packet.ReadInt();

            // remove from players object list
            NetGameObject objtodel = null;

            foreach(NetGameObject n in Server.Players[_ownerID].PlayerObjects)
            {
                if(n.ObjectID == ObjectID)
                {
                    objtodel = n;
                }
            }
            if(objtodel != null)
            {
                Server.Players[_ownerID].PlayerObjects.Remove(objtodel);
            // Tell all players

            ServerSend.DestroyAnObjectToAllButOwner(_ownerID, ObjectID);

                Console.WriteLine($"{Server.Players[_ownerID].Username} destroyed an object, objectID: {ObjectID}");

            }
            //

            }
            catch (Exception X)
            {
                Console.WriteLine($"Destroy Object Error: player {Server.Players[_ownerID].Username}:  error:   {X}");
            }


        }


        public static void MoveObject(uint _ownerID, Packet _packet)
        {
            try
            {
            int objectID = _packet.ReadInt();
            Vector3 _newpos = _packet.ReadVector3();
            Vector3 _newrot = _packet.ReadVector3();
            Vector3 _newscale = _packet.ReadVector3();

            foreach(NetGameObject n in Server.Players[_ownerID].PlayerObjects)
            {
                if(n.ObjectID == objectID)
                {
                    n.Position = _newpos;
                    n.Rotation = _newrot;
                    n.Scale = _newscale;

                    ServerSend.MoveAnObject(_ownerID, n);
                }
            }

            }
            catch (Exception x)
            {
                Console.WriteLine($"Move Object error : {x}");
            }


        }














        // admin

        public static void ReceiveAdminlogin(uint _admin, Packet _packet)
        {
            string _password = _packet.ReadString();
            if(ServerData.AdminPassword == _password)
            {
                Server.Players[_admin].AdminLoggedIn = true;
                ServerSend.LoginGood(_admin);
                Server.Players[_admin].AdminStreamWatch.Start();
            }
            else
            {
                ServerSend.SendTextFromServerToOne(_admin, "Incorrect Password");
            }
        }

        public static void AdminLogOut(uint _admin, Packet _packet)
        {
            Server.Players[_admin].AdminLoggedIn = false;
            Server.Players[_admin].AdminStream = false;
            Server.Players[_admin].AdminStreamWatch.Stop();
            Server.Players[_admin].AdminStreamWatch.Reset();
        }

        public static void AdminBootPlayer(uint _admin, Packet _packet)
        {
            string _usertoboot = _packet.ReadString();
            int mins = _packet.ReadInt();
           


            foreach(Player p in Server.Players.Values)
            {
                if(p.Username == _usertoboot)
                {
                   
                    Valve.Sockets.ConnectionInfo info = new Valve.Sockets.ConnectionInfo();
                    Server.Connection.GetConnectionInfo(p.RiderID, ref info);
                    ServerData.BanPlayer(p.Username, info.address.GetIP(), _admin, mins);

                    ServerSend.DisconnectPlayer($"You were booted for {mins} minutes", p.RiderID);
                    ServerSend.SendTextFromServerToOne(_admin, $"{p.Username} was booted for {mins} mins");

                }
            }

           

        }

        public static void AdminRemoveObject(uint _admin, Packet _packet)
        {
            uint OwnerID = (uint)_packet.ReadLong();
            int ObjectId = _packet.ReadInt();


            foreach (Player p in Server.Players.Values)
            {
                foreach (NetGameObject n in p.PlayerObjects.ToList())
                {
                    if (n.ObjectID == ObjectId)
                    {
                       
                            ServerSend.DestroyAnObjectToAllButOwner(OwnerID, ObjectId);
                            p.PlayerObjects.Remove(n);
                           
                            ServerSend.SendTextFromServerToOne(OwnerID, $"Your {n.NameofObject} object was removed by admin");
                           

                        
                    }
                }
            }


        }

        public static void AdminAlterBanwords(uint _admin, Packet _packet)
        {
            string word = _packet.ReadString();
            bool addtolist = _packet.ReadBool();

            ServerData.AlterBanWords(_admin, addtolist, word);
        }






    }
}
