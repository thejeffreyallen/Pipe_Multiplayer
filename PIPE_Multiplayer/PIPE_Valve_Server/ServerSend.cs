using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Functions that format and send data
    /// </summary>
    class ServerSend
    {
        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendtoOne(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            Server.server.SendMessageToConnection(toclient, bytes,sendflag);
        }
        private static void SendToAll(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            foreach(Player client in Server.Players.Values)
            {

            Server.server.SendMessageToConnection(client.clientID, bytes, sendflag);
            }
        }
        private static void SendToAll(uint Exceptthis, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            foreach (Player client in Server.Players.Values)
            {
                if(client.clientID != Exceptthis)
                {

                Server.server.SendMessageToConnection(client.clientID, bytes, sendflag);
                }
            }

        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////







        #region Send Fuctions



        /// <summary>
        ///  send inital welcome on connection obtained to the server
        /// </summary>
        /// <param name="ClientID"></param>
        public static void Welcome(uint ClientID)
        {
            using (Packet _packet = new Packet((int)ServerPacket.Welcome))
            {
                _packet.Write("Welcome To the Server");
             


                Server.server.SendMessageToConnection(ClientID, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

           
        }





        /// <summary>
        /// Fires once enough info has been obtained about player, sends command to a player to instantiate new player
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_player"></param>
        public static void SetupPlayer(uint _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPacket.SetupAPlayer))
            {
                _packet.Write(_player.clientID);
                _packet.Write(_player.Username);
                _packet.Write(_player.RiderPositions[0]);
                _packet.Write(_player.RiderRotations[0]);
                _packet.Write(_player.Ridermodel);

                Server.server.SendMessageToConnection(_toClient, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }






        /// <summary>
        /// Sends to all players except this one
        /// </summary>
        /// <param name="_aboutplayer"></param>
        /// <param name="count"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public static void SendATransformUpdate(uint _aboutplayer,int count, Vector3[] pos, Vector3[] rot)
        {
            using (Packet _Packet = new Packet((int)ServerPacket.SendTransformUpdate))
            {
                _Packet.Write(_aboutplayer);
                _Packet.Write(count);

                for (int i = 0; i < count; i++)
                {
                    _Packet.Write(pos[i]);
                }

                for (int i = 0; i < count; i++)
                {
                    _Packet.Write(rot[i]);
                }



                SendToAll(_aboutplayer, _Packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);



            }
        }






        /// <summary>
        /// send connection int that disconnected to all remaining connections
        /// </summary>
        /// <param name="ClientThatDisconnected"></param>
        public static void DisconnectTellAll(uint ClientThatDisconnected)
        {
            using (Packet _packet = new Packet((int)ServerPacket.DisconnectedPlayer))
            {
                _packet.Write(ClientThatDisconnected);

                SendToAll(ClientThatDisconnected, _packet.ToArray(),Valve.Sockets.SendFlags.Reliable);
            }
        }


        #endregion




    }
}
