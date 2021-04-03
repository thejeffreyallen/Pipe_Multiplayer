﻿using System.Collections.Generic;
using UnityEngine;
using System;



namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {

        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendToServer(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            SendToServerThread.ExecuteOnMainThread(() =>
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
                    _packet.Write(GameManager.instance.MycurrentLevel);
                    _packet.Write(GameNetworking.instance.VERSIONNUMBER);


                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
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
                
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.NoDelay | Valve.Sockets.SendFlags.Unreliable);
                
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
                if(bytesofimage.Length<2)
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

                if (bytesofimage.Length < 2)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Texture requested is incompatible with encoder,", (int)MessageColour.Server, 0));
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"try uncompressing and saving as different bitdepth .png", (int)MessageColour.Server, 0));
                    return;
                }




               
                int sizeofbytes = bytesofimage.Length;
                int currentpos = 0;
                int n = sizeofbytes / 3000;
                int a = (n / 10) * 10;
                int b = a + 10;
                int divider = 10;

                /*
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
                
                       // SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                        //InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"sent: {i} of {divider} packets at {segment.Length}", (int)MessageColour.Server, 0));
                    }
                }

                */
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
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);

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
                    SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);

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


        public static void SendAllParts(List<Vector3> Bikecolours, List<float> BikeSmooths, List<TextureInfo> _BikeTexname, List<TextureInfo> _RiderTexnames, List<float> bikemetallics, List<TextureInfo> bikenormalnames)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendAllParts))
            {
                _packet.Write(Bikecolours.Count);
                for (int i = 0; i < Bikecolours.Count; i++)
                {
                _packet.Write(Bikecolours[i]);
                }


                _packet.Write(BikeSmooths.Count);
                for (int i = 0; i < BikeSmooths.Count; i++)
                {
                    _packet.Write(BikeSmooths[i]);
                }


                _packet.Write(bikemetallics.Count);
                for (int i = 0; i < bikemetallics.Count; i++)
                {
                    _packet.Write(bikemetallics[i]);
                }


                _packet.Write(_BikeTexname.Count);
                for (int i = 0; i < _BikeTexname.Count; i++)
                {
                    _packet.Write(_BikeTexname[i].Nameoftexture);
                    _packet.Write(_BikeTexname[i].NameofparentGameObject);

                }


                _packet.Write(_RiderTexnames.Count);
                if (_RiderTexnames.Count > 0)
                {
                for (int i = 0; i < _RiderTexnames.Count; i++)
                {
                    _packet.Write(_RiderTexnames[i].Nameoftexture);
                    _packet.Write(_RiderTexnames[i].NameofparentGameObject);
                }

                }


                _packet.Write(bikenormalnames.Count);
                if(bikenormalnames.Count > 0)
                {
                    for (int i = 0; i < bikenormalnames.Count; i++)
                    {
                        _packet.Write(bikenormalnames[i].Nameoftexture);
                        _packet.Write(bikenormalnames[i].NameofparentGameObject);

                    }
                }



                Debug.Log($"Send all parts: biketexnames count: {_BikeTexname.Count}, Ridertexname count: {_RiderTexnames.Count}, Bikenormal count: {bikenormalnames}");
               
                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                GameManager.instance._localplayer.ServerActive = true;
            }

        }


        public static void SendQuickBikeUpdate(List<Vector3> vectors, List<float> floats, List<float> bikemetallics, List<TextureInfo> Texnames)
        {
            using (Packet _packet = new Packet((int)ClientPackets.QuickBikeUpdate))
            {
                _packet.Write(vectors.Count);
                for (int i = 0; i < vectors.Count; i++)
                {
                    _packet.Write(vectors[i]);
                }
                _packet.Write(floats.Count);
                for (int i = 0; i < floats.Count; i++)
                {
                    _packet.Write(floats[i]);
                }
                 _packet.Write(bikemetallics.Count);
                for (int i = 0; i < bikemetallics.Count; i++)
                {
                    _packet.Write(bikemetallics[i]);
                }

                _packet.Write(Texnames.Count);
                if (Texnames.Count > 0)
                {
                for (int i = 0; i < Texnames.Count; i++)
                {
                    _packet.Write(Texnames[i].Nameoftexture);
                    _packet.Write(Texnames[i].NameofparentGameObject);
                }
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


        public static void SendMapName(string name)
        {
            using (Packet _packet = new Packet((int)ClientPackets.SendMapName))
            {
                _packet.Write(name);
              
                SendToServer(GameNetworking.instance.connection, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        #endregion
        
    }
}
