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



        public static void RiderInfoReceive(int _fromclient, Packet _packet)
        {

        }



    }
}

