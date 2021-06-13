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
            }
            try
            {
                VersionNo = _pack.ReadFloat();
                Console.WriteLine($"player on version {VersionNo}");
            }
            catch (Exception x)
            {
                Console.WriteLine($"no Version number found from {name}  :" + x);
            }

            if (Ridermodel.ToLower().Contains("prefab"))
            {
                ServerSend.DisconnectPlayer($"player model:{Ridermodel}: isn't uniquely named", _from);
                Console.WriteLine($"Player refused due to ridermodel name: {Ridermodel}");
                    return;
            }

            if(VersionNo != Server.VERSIONNUMBER)
            {
                ServerSend.DisconnectPlayer($"Your version {VersionNo} does not match server verion {Server.VERSIONNUMBER}", _from);
               
                Console.WriteLine($"Player {name} refused due to versionNo:{VersionNo}");
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
                
            Console.WriteLine($"New rider {name} with connID {_from} received welcome and called back with rider model {Ridermodel} at {CurrentLevel} level");
            Console.WriteLine("Setting up Player on server..");
            Player p = new Player(_from, Ridermodel, name, RidermodelBundlename,CurrentLevel);
            Server.Players.Add(_from, p);


                    // add timeout watch for player and start
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    Server.TimeoutWatches.Add(_from, watch);



                // the receiving of all parts now triggers setup of players
                ServerSend.RequestAllParts(_from);
            

            }



            }
            catch (Exception x)
            {
                Console.WriteLine("Welcome error: " + x);
                Server.server.CloseConnection(_from);
            }




        }



        public static void ReceiveAllParts(uint _from, Packet _packet)
        {
            Console.WriteLine("Receiving Player Data");


            try
            {

            List<PlayerTextureInfo> Biketexnames = new List<PlayerTextureInfo>();
            List<PlayerTextureInfo> Bikenormalnames = new List<PlayerTextureInfo>();
            List<PlayerTextureInfo> RiderTexnames = new List<PlayerTextureInfo>();
            List<Vector3> Bikecols = new List<Vector3>();
            List<float> Bikesmooths = new List<float>();
            List<float> BikeMetallics = new List<float>();

            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                Bikecols.Add(vec);

            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                Bikesmooths.Add(f);

            }
            int Metallicscount = _packet.ReadInt();
            for (int i = 0; i < Metallicscount; i++)
            {
                float m = _packet.ReadFloat();
                BikeMetallics.Add(m);
            }

            int Biketexcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            for (int i = 0; i < Biketexcount; i++)
            {
                string n = _packet.ReadString();
                string e = _packet.ReadString();


                
                Biketexnames.Add(new PlayerTextureInfo(n, e));
            }


            int Ridertexcount = _packet.ReadInt();
                if (Ridertexcount > 0)
                {

                   for (int i = 0; i < Ridertexcount; i++)
                   {
                   string Texname = _packet.ReadString();
                   string ParentG_O = _packet.ReadString();
                   RiderTexnames.Add(new PlayerTextureInfo(Texname, ParentG_O));
                   }

                    
                }



                int bikenormalcount = _packet.ReadInt();
                if ( bikenormalcount > 0)
                {

                   for (int i = 0; i < bikenormalcount; i++)
                   {
                   string Texname = _packet.ReadString();
                   string ParentG_O = _packet.ReadString();
                   Bikenormalnames.Add(new PlayerTextureInfo(Texname, ParentG_O));
                   }

                    
                }

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

                    NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID);

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

            Server.Players[_from].Loadout.FrameColour = Bikecols[0];
            Server.Players[_from].Loadout.ForksColour = Bikecols[1];
            Server.Players[_from].Loadout.BarsColour = Bikecols[2];
            Server.Players[_from].Loadout.SeatColour = Bikecols[3];
            Server.Players[_from].Loadout.FTireColour = Bikecols[4];
            Server.Players[_from].Loadout.FTireSideColour = Bikecols[5];
            Server.Players[_from].Loadout.RTireColour = Bikecols[6];
            Server.Players[_from].Loadout.RTireSideColour = Bikecols[7];
            Server.Players[_from].Loadout.StemColour = Bikecols[8];
            Server.Players[_from].Loadout.FRimColour = Bikecols[9];
            Server.Players[_from].Loadout.RRimColour = Bikecols[10];


            Server.Players[_from].Loadout.FrameSmooth = Bikesmooths[0];
            Server.Players[_from].Loadout.ForksSmooth = Bikesmooths[1];
            Server.Players[_from].Loadout.BarsSmooth = Bikesmooths[2];
            Server.Players[_from].Loadout.SeatSmooth = Bikesmooths[3];
            Server.Players[_from].Loadout.StemSmooth = Bikesmooths[4];
            Server.Players[_from].Loadout.FRimSmooth = Bikesmooths[5];
            Server.Players[_from].Loadout.RRimSmooth = Bikesmooths[6];



            Server.Players[_from].Loadout.FrameMetallic = BikeMetallics[0];
            Server.Players[_from].Loadout.ForksMetallic = BikeMetallics[1];
            Server.Players[_from].Loadout.BarsMetallic = BikeMetallics[2];
            Server.Players[_from].Loadout.StemMetallic = BikeMetallics[3];
            Server.Players[_from].Loadout.FrimMetallic = BikeMetallics[4];
            Server.Players[_from].Loadout.RrimMetallic = BikeMetallics[5];




            Server.Players[_from].Loadout.BMXTextureInfos = Biketexnames;
                if (Ridertexcount > 0)
                {
                  Server.Players[_from].Loadout.RiderTextureInfos = RiderTexnames;
                }
            Server.Players[_from].Loadout.BMXNormalTexInfos = Bikenormalnames;











            Console.WriteLine($"Received Player Data: Bikeinfos: {Biketexnames.Count}, Riderinfos: {RiderTexnames.Count}");
            // Start setup for every player but this one --------------------    
            foreach (Player c in Server.Players.Values.ToList())
            {
                if (c.RiderID != _from)
                {
                    Console.WriteLine($"Sending Spawn command for {Server.Players[_from].Username} to {c.Username}");
                    ServerSend.SetupNewPlayer(c.RiderID, Server.Players[_from]);
                   
                }
            }

            foreach (Player p in Server.Players.Values.ToList())
            {
                if (p.RiderID != _from)
                {
                    foreach (NetGameObject n in Server.Players[_from].PlayerObjects)
                    {
                    ServerSend.SpawnAnObject(p.RiderID, _from, n);

                    }

                }
            }





            // if theres players, run setup all, collects playerinfo in groups of five max and sends, then send all player objects
            if (Server.Players.Count > 1)
            {
                Console.WriteLine($"Sending Setup info back to {Server.Players[_from].Username} about {Server.Players.Count - 1} other players");
                ServerSend.SetupAllOnlinePlayers(_from, Server.Players.Values.ToList());

                foreach(Player p in Server.Players.Values)
                {
                    if(p.RiderID != _from)
                    {
                    if (p.PlayerObjects.Count > 0)
                    {
                        foreach(NetGameObject n in p.PlayerObjects)
                        {
                            ServerSend.SpawnAnObject(_from, p.RiderID, n);
                        }
                    }

                    }
                }
            }

            Server.Players[_from].ReadytoRoll = true;


            }
            catch(Exception x)
            {
                Console.WriteLine($"Receive all parts error: {x}");
            }



        }


        public static void TexturenamesReceive(uint _fromclient, Packet _packet)
        {
            // receives names and stores in clients player

            int stringCount = _packet.ReadInt();
            Server.Players[_fromclient].Loadout.RiderTextureInfos = new List<PlayerTextureInfo>();

            for (int i = 0; i < stringCount; i++)
            {
                string name = _packet.ReadString();
                string nameofobj = _packet.ReadString();



                Server.Players[_fromclient].Loadout.RiderTextureInfos.Add(new PlayerTextureInfo(name,nameofobj));
                Console.WriteLine($"Received {name} texture name from {Server.Players[_fromclient].Username} ");

            }
            try
            {
            Server.Players[_fromclient].Gottexnames = true;
            // now checks the server has them
            Console.WriteLine("Checking I have them now..");
            Server.CheckForTextureFiles(Server.Players[_fromclient].Loadout.RiderTextureInfos, _fromclient);

            }
            catch (Exception x)
            {
                Console.WriteLine("Failed Texture name recieve, player not found:   " + x);
            }

            ServerSend.RequestAllParts(_fromclient);



         
           
        }


        public static void RequestforTex(uint _from, Packet _packet)
        {
            int count = _packet.ReadInt();
            List<string> names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string name = _packet.ReadString();
                names.Add(name);

            }

           // ServerSend.SendTextures(_from,names);

        }



        public static void ReceiveMapname(uint _from, Packet _packet)
        {
            string name = _packet.ReadString();

            try
            {
                Server.Players[_from].MapName = name;
                ServerSend.SendMapName(_from,name);
                Console.WriteLine($"Map name Sync for {_from}");
            }
            catch(Exception x)
            {
                Console.WriteLine($"Map name sync issue, player: {_from}:    " + x);
            }


        }



        /// <summary>
        /// Receive a Texture
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_packet"></param>
        public static void TextureReceive(uint _from, Packet _packet)
        {
            int segmentno = _packet.ReadInt();
            int segmentscount = _packet.ReadInt();
            int bytecount = _packet.ReadInt();
            string name = _packet.ReadString();
            byte[] bytes = _packet.ReadBytes(bytecount);

           // ServerData.SaveATexture(bytes, name,segmentscount,segmentno);
           Console.Write($" | Received {bytecount} bytes for {name}:  segment {segmentno} of {segmentscount-1} | ");

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

        public static void BikeDataQuickUpdate(uint _from, Packet _packet)
        {
            List<PlayerTextureInfo> Biketexnames = new List<PlayerTextureInfo>();
            List<PlayerTextureInfo> Bikenormalnames = new List<PlayerTextureInfo>();
            List<Vector3> Bikecols = new List<Vector3>();
            List<float> Bikesmooths = new List<float>();
            List<float> BikeMetallics = new List<float>();

            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                Bikecols.Add(vec);

            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                Bikesmooths.Add(f);

            }
            int Metallicscount = _packet.ReadInt();
            for (int i = 0; i < Metallicscount; i++)
            {
                float m = _packet.ReadFloat();
                BikeMetallics.Add(m);
            }

            int Biketexcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            for (int i = 0; i < Biketexcount; i++)
            {
                string n = _packet.ReadString();
                string e = _packet.ReadString();
                Biketexnames.Add(new PlayerTextureInfo(n, e));
            }



            int bikenormalcount = _packet.ReadInt();
            if (bikenormalcount > 0)
            {

                for (int i = 0; i < bikenormalcount; i++)
                {
                    string Texname = _packet.ReadString();
                    string ParentG_O = _packet.ReadString();
                    Bikenormalnames.Add(new PlayerTextureInfo(Texname, ParentG_O));
                }


            }




            ///// Add all data to the players Loadout created by the new playerscript assigned in Welcome

            Server.Players[_from].Loadout.FrameColour = Bikecols[0];
            Server.Players[_from].Loadout.ForksColour = Bikecols[1];
            Server.Players[_from].Loadout.BarsColour = Bikecols[2];
            Server.Players[_from].Loadout.SeatColour = Bikecols[3];
            Server.Players[_from].Loadout.FTireColour = Bikecols[4];
            Server.Players[_from].Loadout.FTireSideColour = Bikecols[5];
            Server.Players[_from].Loadout.RTireColour = Bikecols[6];
            Server.Players[_from].Loadout.RTireSideColour = Bikecols[7];
            Server.Players[_from].Loadout.StemColour = Bikecols[8];
            Server.Players[_from].Loadout.FRimColour = Bikecols[9];
            Server.Players[_from].Loadout.RRimColour = Bikecols[10];


            Server.Players[_from].Loadout.FrameSmooth = Bikesmooths[0];
            Server.Players[_from].Loadout.ForksSmooth = Bikesmooths[1];
            Server.Players[_from].Loadout.BarsSmooth = Bikesmooths[2];
            Server.Players[_from].Loadout.SeatSmooth = Bikesmooths[3];
            Server.Players[_from].Loadout.StemSmooth = Bikesmooths[4];
            Server.Players[_from].Loadout.FRimSmooth = Bikesmooths[5];
            Server.Players[_from].Loadout.RRimSmooth = Bikesmooths[6];



            Server.Players[_from].Loadout.FrameMetallic = BikeMetallics[0];
            Server.Players[_from].Loadout.ForksMetallic = BikeMetallics[1];
            Server.Players[_from].Loadout.BarsMetallic = BikeMetallics[2];
            Server.Players[_from].Loadout.StemMetallic = BikeMetallics[3];
            Server.Players[_from].Loadout.FrimMetallic = BikeMetallics[4];
            Server.Players[_from].Loadout.RrimMetallic = BikeMetallics[5];




            Server.Players[_from].Loadout.BMXTextureInfos = Biketexnames;
            Server.Players[_from].Loadout.BMXNormalTexInfos = Bikenormalnames;








            using (Packet packet = new Packet((int)ServerPacket.BikeQuickUpdate))
            {
                packet.Write(_from);

                packet.Write(Bikecols.Count);
                foreach(Vector3 v in Bikecols)
                {
                    packet.Write(v);
                }
                packet.Write(Bikesmooths.Count);
                foreach (float t in Bikesmooths)
                {
                    packet.Write(t);
                }
                packet.Write(BikeMetallics.Count);
                foreach (float m in BikeMetallics)
                {
                    packet.Write(m);
                }
                packet.Write(Biketexnames.Count);
                foreach (PlayerTextureInfo t in Biketexnames)
                {
                    packet.Write(t.Nameoftexture);
                    packet.Write(t.NameofparentGameObject);
                }
                packet.Write(Bikenormalnames.Count);
                foreach (PlayerTextureInfo t in Bikenormalnames)
                {
                    packet.Write(t.Nameoftexture);
                    packet.Write(t.NameofparentGameObject);
                }


                try
                {
                
                    // send to all but me
                        ServerSend.SendQuickBikeUpdate(_from, packet);

                }
                catch (Exception x)
                {
                    Console.WriteLine("Quick bike error: player: " + _from + "  : " + x);
                }





            }
            Console.WriteLine("Quick Bike Update stored and relayed, player: " + _from);
        }


        public static void RiderQuickUpdate(uint _from, Packet _packet)
        {
            List<PlayerTextureInfo> infos = new List<PlayerTextureInfo>();

            int count = _packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string Texname = _packet.ReadString();
                string ParentG_O = _packet.ReadString();
                infos.Add(new PlayerTextureInfo(Texname, ParentG_O));
            }

            using (Packet packet = new Packet((int)ServerPacket.RiderQuickUpdate))
            {
                packet.Write(_from);
                packet.Write(infos.Count);
                foreach (PlayerTextureInfo v in infos)
                {
                    packet.Write(v.Nameoftexture);
                    packet.Write(v.NameofparentGameObject);
                }

                try
                {
                foreach (Player p in Server.Players.Values.ToList())
                {
                    //store
                    if(p.RiderID == _from)
                    {
                        p.Loadout.BMXTextureInfos = infos;
                    }
                   
                    
                    

                }

                        ServerSend.SendQuickRiderUpdate(_from, packet);

                }
                catch (Exception x)
                {
                    Console.WriteLine("Quick Rider error, player: " + _from + "  : " + x);
                }





            }
            Console.WriteLine("Quick Rider Update received, stored and relayed to all");

        }



        /// <summary>
                /// Receives Transform info and plugs it into the matching player class on server, player class sends its data to all players at tick rate
                /// </summary>
                /// <param name="_from"></param>
                /// <param name="_packet"></param>
        public static void TransformReceive(uint _from, Packet _packet)
        {
            

            long ServerStamp = DateTime.Now.ToFileTimeUtc();
            ConnectionStatus info = new ConnectionStatus();
            Server.server.GetQuickConnectionStatus(_from, ref info);
            int _ping = info.ping;

           
            if (Server.Players.Count > 1)
            {
                try
                {

                    ServerSend.SendATransformUpdate(_from, _packet,ServerStamp, _ping);

                }
                catch (Exception x)
                {
                    Console.Write("Failed Transform relay! Player left?  : " + x);
                }

            }


        }



        /// <summary>
        /// Receive Audio state update from player and stores in Player on server, all players call ServerSend at tick rate
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_packet"></param>
        public static void ReceiveAudioUpdate(uint _from, Packet _packet)
        {
            
           
            try
            {
               
                if (Server.Players.Count > 1)
                {
               ServerSend.SendAudioUpdate(_from, _packet.ToArray());
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Failed audio relay, Player left? :   " + x);
            }
            
            // avoids the possiblilty of referencing player[_from] when it doesnt exist




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
                                Server.server.GetConnectionInfo(p.RiderID, ref info);
                                ServerData.BanPlayer(p.Username, info.address.GetIP(), OwnerID, 10);

                                ServerSend.DisconnectPlayer($"You were booted for {10} minutes", p.RiderID);
                                
                            }

                        }
                    }
                }
            }

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

            NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID);

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
                ServerSend.LoginGood(_admin);
            }
            else
            {
                ServerSend.SendTextFromServerToOne(_admin, "Incorrect Password");
            }
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
                    Server.server.GetConnectionInfo(p.RiderID, ref info);
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








    }
}
