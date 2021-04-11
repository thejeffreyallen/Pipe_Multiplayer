using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Functions attached to ConnectionCallBacks, Fired by receiving matching packetCode
    /// </summary>
    class ServersHandles
    {

        /// <summary>
        /// Connection happened, i replied, now they've confirmed they're connected
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_pack"></param>
        public static void WelcomeReceived(uint _from,Packet _pack)
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
            Console.WriteLine($"Player {name} using old version, couldnt get map name, using {CurrentLevel} in their package: {x}");
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

            if(VersionNo != Server.VERSIONNUMBER)
            {
                ServerSend.DisconnectPlayer($"Your version {VersionNo} does not match server verion {Server.VERSIONNUMBER}", _from);
               
                Console.WriteLine("Player refused");
            }
            else
            {


                /// Version is correct, continue setup...
                
            Console.WriteLine($"New Client {name} with connID {_from} received welcome and called back with rider model {Ridermodel} at {CurrentLevel} level");
            Console.WriteLine("Setting up Player on server..");
            Player p = new Player(_from, Ridermodel, name, RidermodelBundlename,CurrentLevel);
            Server.Players.Add(_from, p);



                // the receiving of all parts now triggers setup of players
                ServerSend.RequestAllParts(_from);
            



            }




        }



        public static void ReceiveAllParts(uint _from, Packet _packet)
        {
            Console.WriteLine("Receiving Player Data");

            List<TextureInfo> Biketexnames = new List<TextureInfo>();
            List<TextureInfo> Bikenormalnames = new List<TextureInfo>();
            List<TextureInfo> RiderTexnames = new List<TextureInfo>();
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


                
                Biketexnames.Add(new TextureInfo(n, e));
            }


            int Ridertexcount = _packet.ReadInt();
                if (Ridertexcount > 0)
                {

                   for (int i = 0; i < Ridertexcount; i++)
                   {
                   string Texname = _packet.ReadString();
                   string ParentG_O = _packet.ReadString();
                   RiderTexnames.Add(new TextureInfo(Texname, ParentG_O));
                   }

                    
                }



                int bikenormalcount = _packet.ReadInt();
                if ( bikenormalcount > 0)
                {

                   for (int i = 0; i < bikenormalcount; i++)
                   {
                   string Texname = _packet.ReadString();
                   string ParentG_O = _packet.ReadString();
                   Bikenormalnames.Add(new TextureInfo(Texname, ParentG_O));
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




            Server.Players[_from].Loadout.bikeTexnames = Biketexnames;
            Server.Players[_from].Loadout.RiderTexnames = RiderTexnames;
            Server.Players[_from].Loadout.Bikenormalnames = Bikenormalnames;











            Console.WriteLine($"Received Player Data: Bikeinfos: {Biketexnames.Count}, Riderinfos: {RiderTexnames.Count}");
            // Start setup for every player but this one --------------------    
            foreach (Player c in Server.Players.Values.ToList())
            {
                if (c.clientID != _from)
                {
                    Console.WriteLine($"Sending Setup command for {Server.Players[_from].Username} to {c.Username}");
                    ServerSend.SetupNewPlayer(c.clientID, Server.Players[_from]);
                   
                }
            }


            // if theres players, run setup all, collects playerinfo in groups of five max and sends
            if (Server.Players.Count > 1)
            {
                Console.WriteLine($"Sending Setup info back to {Server.Players[_from].Username} player about {Server.Players.Count - 1} other players");
                ServerSend.SetupAllOnlinePlayers(_from, Server.Players.Values.ToList());
            }






        }





        public static void BikeDataQuickUpdate(uint _from, Packet _packet)
        {
            List<TextureInfo> Biketexnames = new List<TextureInfo>();
            List<TextureInfo> Bikenormalnames = new List<TextureInfo>();
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
                Biketexnames.Add(new TextureInfo(n, e));
            }



            int bikenormalcount = _packet.ReadInt();
            if (bikenormalcount > 0)
            {

                for (int i = 0; i < bikenormalcount; i++)
                {
                    string Texname = _packet.ReadString();
                    string ParentG_O = _packet.ReadString();
                    Bikenormalnames.Add(new TextureInfo(Texname, ParentG_O));
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




            Server.Players[_from].Loadout.bikeTexnames = Biketexnames;
            Server.Players[_from].Loadout.Bikenormalnames = Bikenormalnames;








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
                foreach (TextureInfo t in Biketexnames)
                {
                    packet.Write(t.Nameoftexture);
                    packet.Write(t.NameofparentGameObject);
                }
                packet.Write(Bikenormalnames.Count);
                foreach (TextureInfo t in Bikenormalnames)
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
            List<TextureInfo> infos = new List<TextureInfo>();

            int count = _packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string Texname = _packet.ReadString();
                string ParentG_O = _packet.ReadString();
                infos.Add(new TextureInfo(Texname, ParentG_O));
            }

            using (Packet packet = new Packet((int)ServerPacket.RiderQuickUpdate))
            {
                packet.Write(_from);
                packet.Write(infos.Count);
                foreach (TextureInfo v in infos)
                {
                    packet.Write(v.Nameoftexture);
                    packet.Write(v.NameofparentGameObject);
                }

                try
                {
                foreach (Player p in Server.Players.Values.ToList())
                {
                    //store
                    if(p.clientID == _from)
                    {
                        p.Loadout.bikeTexnames = infos;
                    }
                   
                    
                    

                }

                        ServerSend.SendQuickRiderUpdate(_from, packet);

                }
                catch (Exception x)
                {
                    Console.WriteLine("Quick Rider error, player: " + _from);
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

            if(_packet.ToArray().Length > 500)
            {
                ServerSend.DisconnectPlayer("Please Update Multiplayer mod", _from);
                return;
            }


            if (Server.Players.Count > 1)
            {
                try
                {

                    ServerSend.SendATransformUpdate(_from, _packet);

                }
                catch (Exception x)
                {
                    Console.Write("Failed Transform relay! Player left?  : " + x);
                }

            }




            /*
            // collect transform count, then use that count to read all position vectors of rider, then all rotation
            int count = _packet.ReadInt();

            Vector3[] _pos = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                _pos[i] = _packet.ReadVector3();

            }

            Vector3[] _rot = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                _rot[i] = _packet.ReadVector3();

            }

            try
            {
           
             ServerSend.SendATransformUpdate(_from, count, _pos, _rot);

            }
            catch (Exception x)
            {
                Console.Write("Failed Transform relay! Player left?  : " + x);
            }
            */
        }




        /// <summary>
        /// Receive names as inital search info
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_pack"></param>
        public static void TexturenamesReceive(uint _fromclient, Packet _packet)
        {
            // receives names and stores in clients player

            int stringCount = _packet.ReadInt();
            Server.Players[_fromclient].Loadout.RiderTexnames = new List<TextureInfo>();

            for (int i = 0; i < stringCount; i++)
            {
                string name = _packet.ReadString();
                string nameofobj = _packet.ReadString();



                Server.Players[_fromclient].Loadout.RiderTexnames.Add(new TextureInfo(name,nameofobj));
                Console.WriteLine($"Received {name} texture name from {Server.Players[_fromclient].Username} ");

            }
            try
            {
            Server.Players[_fromclient].Gottexnames = true;
            // now checks the server has them
            Console.WriteLine("Checking I have them now..");
            Server.CheckForTextureFiles(Server.Players[_fromclient].Loadout.RiderTexnames, _fromclient);

            }
            catch (Exception x)
            {
                Console.WriteLine("Failed Texture name recieve, player not found");
            }

            ServerSend.RequestAllParts(_fromclient);



         
           
        }




        /// <summary>
        /// Receive a Texture
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_pack"></param>
        public static void TextureReceive(uint _from, Packet _pack)
        {
            int segmentno = _pack.ReadInt();
            int segmentscount = _pack.ReadInt();
            int bytecount = _pack.ReadInt();
            string name = _pack.ReadString();
            byte[] bytes = _pack.ReadBytes(bytecount);

           // ServerData.SaveATexture(bytes, name,segmentscount,segmentno);
           Console.Write($" | Received {bytecount} bytes for {name}:  segment {segmentno} of {segmentscount-1} | ");

        }

        

        /// <summary>
        /// Receive Audio state update from player and stores in Player on server, all players call ServerSend at tick rate
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_packet"></param>
        public static void ReceiveAudioUpdate(uint _from, Packet _packet)
        {
            
            // receives and takes the receiving PacketId off the start of the array
            byte[] oldbytes = _packet.ToArray();

            byte[] newbytes = new byte[_packet.ToArray().Length - 4];
            int count = 0;
            for (int i = 4; count < newbytes.Length; i++)
            {

                newbytes[count] = oldbytes[i];
                count++;
            }


            int amount = _packet.ReadInt();
            int code = _packet.ReadInt();

            if (code == 1)
            {
                for (int i = 0; i < amount; i++)
                {
                    string nameofriser = _packet.ReadString();
                    int playstate = _packet.ReadInt();
                    float volume = _packet.ReadFloat();
                    float pitch = _packet.ReadFloat();
                    float Velocity = _packet.ReadFloat();

                    

                }

            }

            if (code == 2)
            {
                for (int i = 0; i < amount; i++)
                {
                    string Pathofsound = _packet.ReadString();
                    float volume = _packet.ReadFloat();


                   
                }

            }

            try
            {
                foreach(Player p in Server.Players.Values.ToList())
                {
                    if(p.clientID == _from)
                    {
                      Server.Players[_from].LastAudioUpdate = newbytes;
                Server.Players[_from].newAudioReceived = true;

                    }
                }

                // Console.WriteLine("Received audio");

                if (Server.Players.Count > 1)
                {
               ServerSend.SendAudioToAllPlayers(_from, newbytes);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Failed audio relay, Player left?");
            }
            
            // avoids the possiblilty of referencing player[_from] when it doesnt exist




        }




        public static void RelayPlayerMessage(uint _from, Packet _packet)
        {
            string _mess = _packet.ReadString();
            
            ServerSend.SendTextMessageToAll(_from, _mess);
            
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
                Console.WriteLine($"Map name sync issue, player: {_from}");
            }


        }



    }
}
