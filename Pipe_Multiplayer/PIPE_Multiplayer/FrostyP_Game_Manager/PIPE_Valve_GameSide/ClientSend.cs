using System.Collections.Generic;
using UnityEngine;
using System;



namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Choose a message type, give connection number (Server), bytes data and specify a send mode from Valve.sockets.sendflags, the message will then be sent to the server
    /// </summary>
    public class ClientSend : MonoBehaviour
    {
        public static float PosMult = 4500;
        public static float Rotmult = 80;
        
        private static void SendToServer(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            
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

                

                _packet.Write(FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count);

                if(FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects!= null)
                {
                    if (FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count > 0)
                    {
                        foreach(FrostyP_Game_Manager.NetGameObject _netobj in FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects)
                        {
                            _packet.Write(_netobj.NameofObject);
                            _packet.Write(_netobj.NameOfFile);
                            _packet.Write(_netobj.NameofAssetBundle);

                            _packet.Write(_netobj.Position);
                            _packet.Write(_netobj.Rotation);
                            _packet.Write(_netobj.Scale);
                            _packet.Write(_netobj.ObjectID);
                        }
                    }
                }


                Debug.Log($"Send all parts: biketexnames count: {_BikeTexname.Count}, Ridertexname count: {_RiderTexnames.Count}, Bikenormal count: {bikenormalnames.Count}, Object count: {FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count}");
               
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                GameManager.instance._localplayer.ServerActive = true;
            }

        }



        public static void SendMyTransforms(int TransformCount, Vector3[] positions, Vector3[] rotations, long _TimeStamp)
        {

            using (Packet _packet = new Packet((int)ClientPackets.TransformUpdate))
            {

                _packet.Write(_TimeStamp);
                _packet.Write(positions[0]);
                _packet.Write(rotations[0]);
                for (int i = 1; i < 23; i++)
                {

                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].x * PosMult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].y * PosMult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].z * PosMult))));

                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].x * Rotmult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].y * Rotmult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].z * Rotmult))));

                }




                _packet.Write(positions[23]);
                _packet.Write(rotations[23]);

                for (int i = 24; i < 32; i++)
                {

                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].x * PosMult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].y * PosMult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((positions[i].z * PosMult))));

                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].x * Rotmult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].y * Rotmult))));
                    _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].z * Rotmult))));


                }

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

            }



        }


        public static void SendDaryienTexNames()
        {
           

                using (Packet _packet = new Packet((int)ClientPackets.SendTextureNames))
                {
                if (GameManager.instance._localplayer == null) {
                    Debug.Log("GameManager.instance._localplayer == null");
                }
                if (GameManager.instance._localplayer.RiderTextureInfoList == null)
                {
                    Debug.Log("GameManager.instance._localplayer.RiderTextureInfoList == null");
                }
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
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"try uncompressing {tex.name}", (int)MessageColour.Server, 0));
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
                        /// _packet.Write((short)SystemHalf.HalfHelper.SingleToHalf(update.Volume * 1000));
                        // _packet.Write((short)SystemHalf.HalfHelper.SingleToHalf(update.pitch * 1000));
                        // _packet.Write((short)SystemHalf.HalfHelper.SingleToHalf(update.Velocity * 1000));
                        _packet.Write(update.Volume);
                        _packet.Write(update.pitch);
                        _packet.Write(update.Velocity);
                    }
                    
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable | Valve.Sockets.SendFlags.NoDelay);

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
                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable | Valve.Sockets.SendFlags.NoDelay);

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
        /// called by user click or map change
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

        public static void TurnMeOn()
        {
            using(Packet _packet = new Packet((int)ClientPackets.Turnmeon))
            {
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void TurnMeOff()
        {
            using (Packet _packet = new Packet((int)ClientPackets.Turnmeoff))
            {
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void KeepActive()
        {
            using (Packet _packet = new Packet((int)ClientPackets.KeepActive))
            {
                // receiving any packet from my connectionId within 10 seconds resets timeout watch, no data needed

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }






        // Parkbuilder

        public static void SpawnObjectOnServer(FrostyP_Game_Manager.NetGameObject _netobj)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SpawnObject))
            {
                _packet.Write(_netobj.NameofObject);
                _packet.Write(_netobj.NameOfFile);
                _packet.Write(_netobj.NameofAssetBundle);

                _packet.Write(_netobj.Position);
                _packet.Write(_netobj.Rotation);
                _packet.Write(_netobj.Scale);
                _packet.Write(_netobj.ObjectID);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }


        }

      
        public static void DestroyAnObject(int objectid)
        {
            using(Packet _packet = new Packet((int)ClientPackets.DestroyAnObject))
            {
                _packet.Write(objectid);
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void ObjectTransformUpdate(FrostyP_Game_Manager.PlacedObject _placedobject)
        {
            using(Packet _packet = new Packet((int)ClientPackets.MoveAnObject))
            {
                _packet.Write(_placedobject.ObjectId);
                _packet.Write(_placedobject.Object.transform.position);
                _packet.Write(_placedobject.Object.transform.eulerAngles);
                _packet.Write(_placedobject.Object.transform.localScale);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void VoteToRemoveObject(int ObjectID, uint _OwnerID)
        {
            using(Packet _packet = new Packet((int)ClientPackets.VoteToRemoveObject))
            {
                _packet.Write(ObjectID);
                _packet.Write(_OwnerID);
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }









        // ------------------------------------------------------------ Admin Mode --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AdminModeOn(string _password)
        {
            using (Packet _packet = new Packet((int)ClientPackets.AdminModeOn))
            {
                _packet.Write(_password);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }


        }


        public static void SendBootPlayer(string _username, int mins)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendBootPlayer))
            {
                _packet.Write(_username);
                _packet.Write(mins);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }





        public static void AdminRemoveObject(uint _owner, int ObjectID)
        {
            using (Packet _packet = new Packet((int)ClientPackets.AdminRemoveObject))
            {
                _packet.Write(_owner);
                _packet.Write(ObjectID);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }



        #endregion
        
    }
}




