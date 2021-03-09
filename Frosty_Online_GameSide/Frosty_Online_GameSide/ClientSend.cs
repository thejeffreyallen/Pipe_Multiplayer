using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Frosty_Online_GameSide
{
    // the top two functions are used by all the other once they have created a packet, packet is constructed with the int OPcode at the beginning, the server has a handle function attached to this particular int which reads the packet just as the function in here wrote it
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


       public static void SendDaryienTexNames()
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendDaryienTexNames))
            {
                // write amount of names in list
                _packet.Write(GameManager.instance._localplayer.NamesOfDaryiensTextures.Count);
               
                // write each name
                foreach(string s in GameManager.instance._localplayer.NamesOfDaryiensTextures)
                {
                    _packet.Write(s);
                }

                SendTCPData(_packet);
            }
        }


        public static void SendTexture(List<Texture2D> texs)
        {
          byte[] bytes = ByteMaker.Image(texs[1]);
            Ingame_UI.instance.lastmsgfromServer = $"Sending {bytes.Length} bytes";
            // Texture needs to be read/write enabled
        }





       
        #endregion
    }
}