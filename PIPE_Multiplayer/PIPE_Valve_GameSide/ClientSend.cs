using System.Collections.Generic;
using UnityEngine;


namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {

        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendToServer(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
           GameNetworking.instance.client.SendMessageToConnection(toclient, bytes, sendflag);
        }
       























        #region Sendable_ messages

        public static void WelcomeReceived()
        {
            InGameUI.instance.NewMessage(Constants.SystemMessage, new TextMessage("Established connection", 1, 0));
            
                using (Packet _packet = new Packet((int)ClientPackets.WelcomeReceived))
                {

                    _packet.Write(InGameUI.instance.Username);
                    _packet.Write(InGameUI.instance._localplayer.RiderModelname);


                    GameNetworking.instance.client.SendMessageToConnection(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                }

            GameManager.instance._localplayer.ServerActive = true;
           
            
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

                    GameNetworking.instance.client.SendMessageToConnection(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.NoDelay);
                }
            
            
        }


        public static void SendDaryienTexNames()
        {
            SendToServerThread.ExecuteOnMainThread(() =>
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
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

                }
            });
        }


        public static void SendTextures(List<Texture2D> texs)
        {
           
                foreach (Texture2D tex in texs)
                {
                    byte[] bytes = ByteMaker.Image(tex);

                }
          
            
            // Texture needs to be read/write enabled
        }


        public static void SendAudioUpdate(List<AudioStateUpdate> updates, int code)
        {

            // Risers
            if(code == 1)
            {
                using (Packet _packet = new Packet((int)ClientPackets.SendAudioUpdate))
                {
                    _packet.Write(updates.Count);
                        _packet.Write(code);

                    foreach (AudioStateUpdate update in updates)
                    {
                        _packet.Write(update.nameofriser);
                        _packet.Write(update.playstate);
                        _packet.Write(update.Volume);
                        _packet.Write(update.pitch);
                        _packet.Write(update.Velocity);
                    }
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

                }

            }

            // One Shots
            if(code == 2)
            {
                using (Packet _packet = new Packet((int)ClientPackets.SendAudioUpdate))
                {
                    _packet.Write(updates.Count);
                        _packet.Write(code);
                    foreach (AudioStateUpdate update in updates)
                    {
                        _packet.Write(update.Path);
                        _packet.Write(update.Volume);
                    }
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

                }
            }
           
               



        }


        public static void SendTextMessage(string _message)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendTextMessage))
            {
                _packet.Write(_message);


                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }


        #endregion
        
    }
}

