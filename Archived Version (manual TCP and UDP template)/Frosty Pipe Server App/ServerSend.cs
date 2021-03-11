
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Frosty_Pipe_Server
{
    class ServerSend
    {
        /// <summary>Sends a packet to a client via TCP.</summary>
        /// <param name="_toClient">The client to send the packet the packet to.</param>
        /// <param name="_packet">The packet to send to the client.</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        /// <summary>Sends a packet to a client via UDP.</summary>
        /// <param name="_toClient">The client to send the packet the packet to.</param>
        /// <param name="_packet">The packet to send to the client.</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>Sends a packet to all clients via TCP.</summary>
        /// <param name="_packet">The packet to send.</param>
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        /// <summary>Sends a packet to all clients except one via TCP.</summary>
        /// <param name="_exceptClient">The client to NOT send the data to.</param>
        /// <param name="_packet">The packet to send.</param>
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        /// <summary>Sends a packet to all clients via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        /// <summary>Sends a packet to all clients except one via UDP.</summary>
        /// <param name="_exceptClient">The client to NOT send the data to.</param>
        /// <param name="_packet">The packet to send.</param>
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }










        ///   Functions that the server can send to clients, called usually by a ServerHandle being received, Requires a packet code on server and client that match





        #region Packets
      
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        







      /// <summary>
      /// Fires once enough info has been obtained about player, sends command to players to instantiate new player
      /// </summary>
      /// <param name="_toClient"></param>
      /// <param name="_player"></param>
        public static void SetupPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SetupPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.RiderPositions[0]);
                _packet.Write(_player.RiderRotations[0]);
                _packet.Write(_player.CurrentRiderModel);

                SendTCPData(_toClient, _packet);
            }
        }







        /// <summary>
        /// Fires if a client connects with ridermodelname daryien,
        /// </summary>
        /// <param name="_toclient"></param>
        public static void RequestTextureNamesForDaryien(int _toclient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.RequestTexnamesforDaryien))
            {
                // doesnt need data just triggers the request

                SendTCPData(_toclient, _packet);
            }

        }





        /// <summary>
        /// Fires if a client started as daryien with textures the server doesnt have, sends list of unfound texture names to the client
        /// </summary>
        /// <param name="_toclient"></param>
        /// <param name="unfoundlist"></param>
        public static void RequestTextures(int _toclient, List<string> unfoundlist)
        {
            using(Packet _packet = new Packet((int)ServerPackets.RequestTextures))
            {
                _packet.Write(unfoundlist.Count);

                foreach(string s in unfoundlist)
                {
                    _packet.Write(s);
                }
                SendTCPData(_toclient, _packet);
            }

        }








        public static void RelayTransformsToAllButSender(int _from,int count,Vector3[] pos,Vector3[] rot)
        {

            using (Packet _Packet = new Packet((int)ServerPackets.TransformInfo))
            {
                _Packet.Write(_from);
                _Packet.Write(count);

            for (int i = 0; i < count; i++)
            {
                    _Packet.Write(pos[i]);
            }
                
            for (int i = 0; i < count; i++)
            {
                 _Packet.Write(rot[i]);
            }


                
                    SendUDPDataToAll(_from, _Packet);
                
               

            }
           


        }









        public static void DisconnectTellAll(int ClientThatDisconnected)
        {
            using (Packet _packet = new Packet((int)ServerPackets.DisconnectTellAll))
            {
                _packet.Write(ClientThatDisconnected);

                SendTCPDataToAll(ClientThatDisconnected, _packet);
            }
        }

        

        #endregion
    }
}