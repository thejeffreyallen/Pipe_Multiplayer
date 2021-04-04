using System.Collections.Generic;
using UnityEngine;
using System;



namespace PIPE_Valve_Console_Client
{
    public class ClientSend : MonoBehaviour
    {

        // These top three functions are used by the send functions, give connection number (Server), bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendToServer(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            // Sends to outgoing thread once use of Unity API is done with, Really need system.numerics (NET 4.5) to be able to do all processing of numbers in and out on a different thread
            SendToServerThread.ExecuteOnMainThread(() =>
            {
                GameNetworking.instance.client.SendMessageToConnection(GameNetworking.instance.connection, bytes, sendflag);
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


                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                }

           
           
            
        }


        public static void SendMyTransforms(int TransformCount, Vector3[] positions, Vector3[] rotations)
        {
            

                using (Packet _packet = new Packet((int)ClientPackets.TransformUpdate))
                {

                    _packet.Write(TransformCount);
              


                    for (int i = 0; i < TransformCount; i++)
                    {
                    Vector3 rounded = new Vector3((float)Math.Round(positions[i].x * 1000) / 1000, (float)Math.Round(positions[i].y * 1000) / 1000, (float)Math.Round(positions[i].z * 1000) / 1000);

                        _packet.Write(rounded);
                    }

                    for (int i = 0; i < TransformCount; i++)
                    {
                    Vector3 rounded = new Vector3((float)Math.Round(rotations[i].x * 1000) / 1000, (float)Math.Round(rotations[i].y * 1000) / 1000, (float)Math.Round(rotations[i].z * 1000) / 1000);
                    _packet.Write(rounded);

                    }
                
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.NoDelay | Valve.Sockets.SendFlags.Unreliable);
                
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
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

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
                        _packet.Write((float)Math.Round(update.Volume * 100) / 100);
                        _packet.Write((float)Math.Round(update.pitch * 100) / 100);
                        _packet.Write((float)Math.Round(update.Velocity * 100) / 100);
                    }
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Unreliable | Valve.Sockets.SendFlags.NoDelay);

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
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Unreliable | Valve.Sockets.SendFlags.NoDelay);

                }
            }
           
               



        }


        public static void SendTextMessage(string _message)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendTextMessage))
            {
                _packet.Write(_message);


                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        /// <summary>
        /// Send Bike and Rider, Triggers spawn command to you and everyone else
        /// </summary>
        /// <param name="Bikecolours"></param>
        /// <param name="BikeSmooths"></param>
        /// <param name="_BikeTexname"></param>
        /// <param name="_RiderTexnames"></param>
        /// <param name="bikemetallics"></param>
        /// <param name="bikenormalnames"></param>
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
               
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                GameManager.instance._localplayer.ServerActive = true;
            }

        }

        /// <summary>
        /// Called by User click
        /// </summary>
        /// <param name="Colours"></param>
        /// <param name="Smooths"></param>
        /// <param name="Metallics"></param>
        /// <param name="Texnames"></param>
        public static void SendQuickBikeUpdate(List<Vector3> Colours, List<float> Smooths, List<float> Metallics, List<TextureInfo> Texnames, List<TextureInfo> bikenormalsinfo)
        {
            using (Packet _packet = new Packet((int)ClientPackets.QuickBikeUpdate))
            {
                _packet.Write(Colours.Count);
                for (int i = 0; i < Colours.Count; i++)
                {
                    _packet.Write(Colours[i]);
                }
                _packet.Write(Smooths.Count);
                for (int i = 0; i < Smooths.Count; i++)
                {
                    _packet.Write(Smooths[i]);
                }
                 _packet.Write(Metallics.Count);
                for (int i = 0; i < Metallics.Count; i++)
                {
                    _packet.Write(Metallics[i]);
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


                _packet.Write(bikenormalsinfo.Count);
                if (bikenormalsinfo.Count > 0)
                {
                    for (int i = 0; i < bikenormalsinfo.Count; i++)
                    {
                        _packet.Write(bikenormalsinfo[i].Nameoftexture);
                        _packet.Write(bikenormalsinfo[i].NameofparentGameObject);
                    }
                }



                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }

        /// <summary>
        /// Called by user click
        /// </summary>
        /// <param name="texnames"></param>
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


                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        /// <summary>
        /// A packet from server triggers this giving list of filenames it doesnt have
        /// </summary>
        /// <param name="unfound"></param>
        public static void RequestTextures(List<string> unfound)
        {
            using(Packet _packet = new Packet((int)ClientPackets.RequestforTex))
            {
                _packet.Write(unfound.Count);
            foreach(string t in unfound)
            {
              _packet.Write(t);

            }
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        /// <summary>
        /// called by user click
        /// </summary>
        /// <param name="name"></param>
        public static void SendMapName(string name)
        {
            using (Packet _packet = new Packet((int)ClientPackets.SendMapName))
            {
                _packet.Write(name);
              
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        #endregion
        
    }
}

