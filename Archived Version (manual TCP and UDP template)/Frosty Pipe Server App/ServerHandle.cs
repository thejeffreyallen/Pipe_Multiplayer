using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Frosty_Pipe_Server
{
    class ServerHandle
    {
        // in repsonse to inital welcome sent from here, begins setup of established player
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            string ridermodel = _packet.ReadString();

            Console.WriteLine($"{_username} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }

            // setup using this client
            Server.clients[_fromClient].SetupPlayer(_username, ridermodel);
        }

        public static void TransformsReceive(int _fromClient, Packet _packet)
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
            if(Server.clients[_fromClient].player != null)
            {
            Server.clients[_fromClient].player.RiderPositions = _pos;
            Server.clients[_fromClient].player.RiderRotations = _rot;
               
                

            }

           // Console.WriteLine(_pos[0].ToString() + "from " + _fromClient);

           

        }

        public static void ReceiveDaryienTexNames(int _fromclient, Packet _packet)
        {
            // receives names and stores in clients player

            int stringCount = _packet.ReadInt();
            Server.clients[_fromclient].player.NamesofDaryiensTextures = new List<string>();

            for (int i = 0; i < stringCount; i++)
            {
                string name = _packet.ReadString();
                Server.clients[_fromclient].player.NamesofDaryiensTextures.Add(name);
                Console.WriteLine($"Received {name} texture name from {_fromclient} ");

            }
            // now checks the server has them
                Console.WriteLine("Checking I have them now..");
            Server.CheckForTextureFiles(Server.clients[_fromclient].player.NamesofDaryiensTextures, _fromclient);
        }

      



    }
}

