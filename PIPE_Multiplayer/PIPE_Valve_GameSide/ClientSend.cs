using System.Collections.Generic;
using UnityEngine;


namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {

        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendtoOne(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
           GameNetworking.instance.client.SendMessageToConnection(toclient, bytes, sendflag);
        }
        private static void SendToAll(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            foreach (RemotePlayer client in GameManager.Players.Values)
            {

                GameNetworking.instance.client.SendMessageToConnection(client.id, bytes, sendflag);
            }
        }
        private static void SendToAll(uint Exceptthis, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            foreach (RemotePlayer client in GameManager.Players.Values)
            {
                if (client.id != Exceptthis)
                {

                    GameNetworking.instance.client.SendMessageToConnection(client.id, bytes, sendflag);
                }
            }

        }
























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
            using (Packet _packet = new Packet((int)ClientPackets.SendTextureNames))
            {
                // write amount of names in list
                _packet.Write(GameManager.instance._localplayer.RidersTexturenames.Count);

                // write each name
                foreach (string s in GameManager.instance._localplayer.RidersTexturenames)
                {
                    _packet.Write(s);
                }
                SendtoOne(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                
            }
        }


        public static void SendTextures(List<Texture2D> texs)
        {
            foreach(Texture2D tex in texs)
            {
            byte[] bytes = ByteMaker.Image(tex);

            }
            
            // Texture needs to be read/write enabled
        }


        public static void SendAudioUpdate(List<AudioStateUpdate> updates)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendAudioUpdate))
            {
                _packet.Write(updates.Count);
            foreach(AudioStateUpdate update in updates)
            {
                    _packet.Write(update.nameofriser);
                    _packet.Write(update.playstate);
                    _packet.Write(update.Volume);
                    _packet.Write(update.pitch);
                    _packet.Write(update.Velocity);
            }

                SendtoOne(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }



        }



        #endregion
        
    }
}

