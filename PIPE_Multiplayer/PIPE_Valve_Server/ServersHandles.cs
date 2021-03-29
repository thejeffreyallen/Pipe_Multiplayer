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
            string CurrentLevel;
            try
            {
            CurrentLevel = _pack.ReadString();
            }
            catch(Exception x)
            {
                Server.server.CloseConnection(_from);
                Console.WriteLine($"Player {name} refused, old version, couldnt get map name");
                return;
            }

            Console.WriteLine($"New Client {name} with connID {_from} received welcome and called back with rider model {Ridermodel} at {CurrentLevel} level");
            Console.WriteLine("Setting up Player on server..");
            Player p = new Player(_from, Ridermodel, name, RidermodelBundlename,CurrentLevel);
            Server.Players.Add(_from, p);


            // if daryien, request more info, if not, model name is enough
            if(Ridermodel == "Daryien")
            {
                Console.WriteLine("Detected Daryien, asking for texture info now..");
            ServerSend.RequestTexturenames(_from);
            }

            // Request bike

            ServerSend.RequestBike(_from);

            // Start setup for every player but this one --------------------    
            foreach (Player c in Server.Players.Values)
            {

                if (c.clientID != _from)
                {
                    Console.WriteLine($"Sending Setup command for {_from} to {c.clientID}");
                    ServerSend.SetupNewPlayer(c.clientID, Server.Players[_from]);

                }


            }



            if (Server.Players.Count > 1)
            {
                    Console.WriteLine($"Sending Setup info back to {name} player about {Server.Players.Count} players");
                    ServerSend.SetupAllOnlinePlayers(_from, Server.Players.Values.ToList());

            }
           

        }


        public static void BikeDataReceive(uint _from, Packet _packet)
        {
            Console.Write("Receiving Inital Bike Data..");
            List<TextureInfo> Texturenames = new List<TextureInfo>();
            List<Vector3> vecs = new List<Vector3>();
            List<float> floats = new List<float>();

            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                vecs.Add(vec);

            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                floats.Add(f);

            }
         

            Server.Players[_from].GotBikeData = true;
            Console.WriteLine("Bike Data Sound");

            /*
            using (Packet packet = new Packet((int)ServerPacket.send))
            {
                packet.Write(_from);

                packet.Write(vecs.Count);
                foreach (Vector3 v in vecs)
                {
                    packet.Write(v);
                }
                packet.Write(floatcount);
                foreach (float t in floats)
                {
                    packet.Write(t);
                }
               

                foreach (Player p in Server.Players.Values)
                {
                    //store
                    if (p.clientID == _from)
                    {
                        p.Loadout.TexInfos = Texturenames;
                        p.Loadout.Colours = vecs;
                        p.Loadout.Smooths = floats;
                    }

                    // send
                    if (p.clientID != _from)
                    {
                        ServerSend.SendQuickBikeUpdate(p.clientID, packet);


                    }
                }

            }

            */
        }


        public static void BikeDataQuickUpdate(uint _from, Packet _packet)
        {
            List<TextureInfo> Texturenames = new List<TextureInfo>();
            List<Vector3> vecs = new List<Vector3>();
            List<float> floats = new List<float>();

            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                vecs.Add(vec);
                
            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                floats.Add(f);
                
            }
            int texcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            for (int i = 0; i < texcount; i++)
            {
                string n = _packet.ReadString();
                string e = _packet.ReadString();

                Texturenames.Add(new TextureInfo(n,e));
            }



            using(Packet packet = new Packet((int)ServerPacket.BikeQuickUpdate))
            {
                packet.Write(_from);

                packet.Write(vecs.Count);
                foreach(Vector3 v in vecs)
                {
                    packet.Write(v);
                }
                packet.Write(floatcount);
                foreach (float t in floats)
                {
                    packet.Write(t);
                }
                packet.Write(texcount);
                foreach (TextureInfo t in Texturenames)
                {
                    packet.Write(t.Nameoftexture);
                    packet.Write(t.NameofparentGameObject);
                }

                try
                {
                foreach (Player p in Server.Players.Values.ToList())
                {
                    //store
                    if (p.clientID == _from)
                    {
                        p.Loadout.TexInfos = Texturenames;
                        p.Loadout.Colours = vecs;
                        p.Loadout.Smooths = floats;
                    }
                }

                    // send to all but me
                        ServerSend.SendQuickBikeUpdate(_from, packet);

                }
                catch (Exception x)
                {
                    Console.WriteLine("Quick bike error: player: " + _from);
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
                        p.Loadout.TexInfos = infos;
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
           
            foreach(Player p in Server.Players.Values)
            {
            if (p.clientID == _from)
            {
                p.RiderPositions = _pos;
                p.RiderRotations = _rot;

                    
                        ServerSend.SendATransformUpdate(_from, count, _pos, _rot);
                    


                }

            }

            }
            catch (Exception x)
            {
                Console.Write("Failed Transform relay! Player left?");
            }
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
            Server.Players[_fromclient].RiderTextureInfoList = new List<TextureInfo>();

            for (int i = 0; i < stringCount; i++)
            {
                string name = _packet.ReadString();
                string nameofobj = _packet.ReadString();



                Server.Players[_fromclient].RiderTextureInfoList.Add(new TextureInfo(name,nameofobj));
                Console.WriteLine($"Received {name} texture name from {Server.Players[_fromclient].Username} ");

            }
            try
            {
            Server.Players[_fromclient].Gottexnames = true;
            // now checks the server has them
            Console.WriteLine("Checking I have them now..");
            Server.CheckForTextureFiles(Server.Players[_fromclient].RiderTextureInfoList, _fromclient);

            }
            catch (Exception x)
            {
                Console.WriteLine("Failed Texture name recieve, player not found");
            }

           



         
           
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

                foreach (Player p in Server.Players.Values.ToList())
                {
                    if (p.clientID == _from)
                    {
                        // Console.WriteLine("Received audio");
                        p.LastAudioUpdate = newbytes;
                        p.newAudioReceived = true;
                        
                            ServerSend.SendAudioToAllPlayers(_from, newbytes);
                    

                    }
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
            Server.Players[_from].MapName = name;

            try
            {
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
