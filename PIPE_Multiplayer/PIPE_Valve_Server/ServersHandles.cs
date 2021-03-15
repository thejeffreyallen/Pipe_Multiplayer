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


        public static void WelcomeReceived(uint _from,Packet _pack)
        {

            string name = _pack.ReadString();
            string Ridermodel = _pack.ReadString();

            Console.WriteLine($"New Client {name} with connID {_from} received welcome and called back with rider model {Ridermodel}");
            Console.WriteLine("Setting up Player on server..");
            Player p = new Player(_from, Ridermodel, name);
            Server.Players.Add(_from, p);

            ServerSend.RequestTexturenames(_from);

            // send this player info to everyone online
            foreach (Player c in Server.Players.Values)
            {
                if (c != null)
                {
                    if (c.clientID != _from)
                    {
                        Console.WriteLine($"Sending Setup command from {_from} to {c.clientID}");
                        ServerSend.SetupPlayer(c.clientID, p);

                    }

                }



            }



            // Send All online players to this new player
            foreach (Player _client in Server.Players.Values)
            {
                if (_client != null && _client.clientID != _from)
                {
                    Console.WriteLine($"Sending Setup command back to new player about active player: {_client.Username}");
                    ServerSend.SetupPlayer(_from, _client);

                }
            }


        }





        public static void RiderInfoReceive(uint _from, Packet _pack)
        {

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


            // If Player is active checker, if so, update his Player info on the server
            foreach(Player p in Server.Players.Values)
            {
            if (p.clientID == _from)
            {
                p.RiderPositions = _pos;
                p.RiderRotations = _rot;



            }

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
            Server.Players[_fromclient].RidersTexturenames = new List<string>();

            for (int i = 0; i < stringCount; i++)
            {
                string name = _packet.ReadString();
                Server.Players[_fromclient].RidersTexturenames.Add(name);
                Console.WriteLine($"Received {name} texture name from {_fromclient} ");

            }
            // now checks the server has them
            Console.WriteLine("Checking I have them now..");
            Server.CheckForTextureFiles(Server.Players[_fromclient].RidersTexturenames, _fromclient);

        }




        /// <summary>
        /// Receive a Texture
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_pack"></param>
        public static void TextureReceive(uint _from, Packet _pack)
        {

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
            for (int i = 0; i < amount; i++)
            {
                string name = _packet.ReadString();
                int state = _packet.ReadInt();
                float vol = _packet.ReadFloat();
                float pitch = _packet.ReadFloat();
                float Velo = _packet.ReadFloat();
            Console.WriteLine($"Audio Packet from {_from}: {_packet.ToArray().Length}: riser {name} in state {state}");
            }


            // avoids the possiblilty of referencing player[_from] when it doesnt exist
           foreach(Player p in Server.Players.Values)
            {
                if(p.clientID == _from)
                {
                  p.LastAudioUpdate = newbytes;

                }
            }


          
        }







    }
}
