using System.Collections.Generic;
using UnityEngine;
using System;


namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {

        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendToServer(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
                GameNetworking.instance.client.SendMessageToConnection(toclient, bytes, sendflag);
            });
           
        }
       























        #region Sendable_ messages

        public static void WelcomeReceived()
        {
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Established connection", 1, 0));
            
                using (Packet _packet = new Packet((int)ClientPackets.WelcomeReceived))
                {

                    _packet.Write(InGameUI.instance.Username);
                    _packet.Write(InGameUI.instance._localplayer.RiderModelname);
                    _packet.Write(InGameUI.instance._localplayer.RiderModelBundleName);


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
           

                using (Packet _packet = new Packet((int)ClientPackets.SendTextureNames))
                {
                    // write amount of names in list
                    _packet.Write(GameManager.instance._localplayer.RiderTextureInfoList.Count);

                    // write each name
                    foreach (TextureInfo s in GameManager.instance._localplayer.RiderTextureInfoList)
                    {
                    _packet.Write(s.Nameoftexture);
                    _packet.Write(s.NameofparentGameObject);
                    }
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

                }
           
        }


        public static void SendTextures(List<Texture2D> texs)
        {
            
           

            // For each image, split up and send each segment with a segment number, its total num of segments, tex name, segment length and segment
            foreach (Texture2D tex in texs)
            {
                byte[] bytesofimage = new byte[1];
                try
                {
                 bytesofimage = ByteMaker.Image(tex);
                }
                catch(Exception x)
                {
                    Debug.Log(x);
                }
                if(bytesofimage.Length==1)
                {
                    try
                    {
                        bytesofimage = tex.GetRawTextureData();
                    }
                    catch (Exception x)
                    {
                        Debug.Log(x);
                    }

                }

                if (bytesofimage.Length == 1)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Texture requested is incompatible with encoder,", (int)MessageColour.Server, 0));
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"try uncompressing and saving as different bitdepth .png", (int)MessageColour.Server, 0));
                    return;
                }




                if(bytesofimage.Length > 3000)
                {
                int sizeofbytes = bytesofimage.Length;
                int currentpos = 0;
                int n = sizeofbytes / 3000;
                int a = (n / 10) * 10;
                int b = a + 10;
                int divider = (n - a > b - n) ? b : a;


                for (int i = 0; i < divider; i++)
                {
                    byte[] segment = new byte[sizeofbytes/divider];

                    for (int _i = 0; _i < bytesofimage.Length/divider; _i++)
                    {
                    segment[_i] = bytesofimage[currentpos];
                    currentpos++;

                    }

                    using(Packet _packet = new Packet((int)ClientPackets.SendTexture))
                    {
                        _packet.Write(i);
                        _packet.Write(divider);
                        _packet.Write(segment.Length);
                        _packet.Write(tex.name);
                        _packet.Write(segment);
                
                        SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                        //InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"sent: {i} of {divider} packets at {segment.Length}", (int)MessageColour.Server, 0));
                    }
                }

                }
                else if (bytesofimage.Length > 0 && bytesofimage.Length < 3000)
                {





                    using (Packet _packet = new Packet((int)ClientPackets.SendTexture))
                    {
                        _packet.Write(0);
                        _packet.Write(1);
                        _packet.Write(bytesofimage.Length);
                        _packet.Write(tex.name);
                        _packet.Write(bytesofimage);

                        SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                        //InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"sent: {i} of {divider} packets at {segment.Length}", (int)MessageColour.Server, 0));
                    }

                }









               



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


        public static void SendBikeData(List<Vector3> vectors, List<float> floats, List<string> Texnames)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendBikeData))
            {
                for (int i = 0; i < vectors.Count; i++)
                {
                _packet.Write(vectors[i]);
                }
                for (int i = 0; i < floats.Count; i++)
                {
                    _packet.Write(floats[i]);
                }
                for (int i = 0; i < Texnames.Count; i++)
                {
                    _packet.Write(Texnames[i]);
                }
                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        public static void SendQuickBikeUpdate(List<Vector3> vectors, List<float> floats, List<string> Texnames)
        {
            using (Packet _packet = new Packet((int)ClientPackets.QuickBikeUpdate))
            {
                for (int i = 0; i < vectors.Count; i++)
                {
                    _packet.Write(vectors[i]);
                }
                for (int i = 0; i < floats.Count; i++)
                {
                    _packet.Write(floats[i]);
                }
                for (int i = 0; i < Texnames.Count; i++)
                {
                    _packet.Write(Texnames[i]);
                }
                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        public static void SendQuickRiderUpdate(List<TextureInfo> texnames)
        {
            using(Packet _packet = new Packet((int)ClientPackets.QuickRiderUpdate))
            {
                _packet.Write(texnames.Count);
                foreach(TextureInfo info in texnames)
                {
                    _packet.Write(info.Nameoftexture);
                    _packet.Write(info.NameofparentGameObject);
                }


                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }



        public static void RequestTextures(List<string> unfound)
        {
            using(Packet _packet = new Packet((int)ClientPackets.RequestforTex))
            {
                _packet.Write(unfound.Count);
            foreach(string t in unfound)
            {
              _packet.Write(t);

            }
                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }


        #endregion
        
    }
}

