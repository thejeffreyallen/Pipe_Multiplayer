using System.Collections.Generic;
using UnityEngine;


namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {
       
        

        #region Packets

        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.WelcomeReceived))
            {
                
                _packet.Write(InGameUI.instance.Username);
                _packet.Write(InGameUI.instance._localplayer.RiderModelname);


                GameNetworking.instance.client.SendMessageToConnection(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }


        public static void SendMyTransforms(int TransformCount, Vector3[] positions, Vector3[] rotations)
        {
            using (Packet _packet = new Packet((int)ClientPackets.TransformUpdate))
            {

                _packet.Write(TransformCount);



                for (int i = 0; i < TransformCount; i++)
                {

                    _packet.Write(positions[i]);
                }

                for (int i = 0; i < TransformCount; i++)
                {
                    _packet.Write(rotations[i]);

                }

                GameNetworking.instance.client.SendMessageToConnection(GameNetworking.instance.connection, _packet.ToArray());
            }
        }


        public static void SendDaryienTexNames()
        {
            using (Packet _packet = new Packet((int)ClientPackets.SendDaryienTexNames))
            {
                // write amount of names in list
                _packet.Write(GameManager.instance._localplayer.NamesOfDaryiensTextures.Count);

                // write each name
                foreach (string s in GameManager.instance._localplayer.NamesOfDaryiensTextures)
                {
                    _packet.Write(s);
                }

                
            }
        }


        public static void SendTexture(List<Texture2D> texs)
        {
            byte[] bytes = ByteMaker.Image(texs[1]);
            
            // Texture needs to be read/write enabled
        }






        #endregion
        
    }
}

