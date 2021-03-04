using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Frosty_Online_GameSide
{
    
    public class ClientSend : MonoBehaviour
    {
        /// <summary>Sends a packet to the server via TCP.</summary>
        /// <param name="_packet">The packet to send to the sever.</param>
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet);
        }

        /// <summary>Sends a packet to the server via UDP.</summary>
        /// <param name="_packet">The packet to send to the sever.</param>
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.udp.SendData(_packet);
        }





        #region Packets
        /// <summary>Lets the server know that the welcome message was received.</summary>
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(Ingame_UI.instance.Username);
                _packet.Write(Ingame_UI.instance._localplayer.RiderModelname);
                

                SendTCPData(_packet);
            }
        }

        /// <summary>
        ///Packets my positions and rotations and sends to SendUDPData as Vector3 arrays</summary>
        /// <param name="_inputs"></param>
        public static void SendMyTransforms(int TransformCount ,Vector3[] positions, Vector3[] rotations)
        {
            using (Packet _packet = new Packet((int)ClientPackets.TransformUpdate))
            {

                _packet.Write(TransformCount);

            

                for (int i = 0; i < TransformCount ; i++)
                {

                    _packet.Write(positions[i]);
                }

                for (int i = 0; i < TransformCount; i++)
                {
                    _packet.Write(rotations[i]); 

                }

                SendUDPData(_packet);
            }
        }


        public static void SendRiderInfo()
        {

        }








        public static void PlayerThrowItem(Vector3 _facing)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerThrowItem))
            {
                _packet.Write(_facing);

                SendTCPData(_packet);
            }
        }
        #endregion
    }
}