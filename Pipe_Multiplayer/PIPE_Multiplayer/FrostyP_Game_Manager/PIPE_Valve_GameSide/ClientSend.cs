using System.Collections.Generic;
using UnityEngine;
using System;



namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Choose a message type, give connection number (Server), bytes data and specify a send mode from Valve.sockets.sendflags, the message will then be sent to the server
    /// </summary>
    public class ClientSend
    {
        public static float PosMult = 4500;
        public static float Rotmult = 80;


        


        
        private static void SendToServer(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            
            SendToServerThread.ExecuteOnMainThread(() =>
            {
                GameNetworking.instance.Socket.SendMessageToConnection(GameNetworking.instance.ServerConnection, bytes, sendflag);
            });
           
           
        }
       










        #region Sendable_ messages

        public static void WelcomeReceived()
        {
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Established connection", 1, 0));
            
                using (Packet _packet = new Packet((int)ClientPackets.WelcomeReceived))
                {

                    _packet.Write(InGameUI.instance.Username);
                    _packet.Write(InGameUI.instance.LocalPlayer.RiderModelname);
                    _packet.Write(InGameUI.instance.LocalPlayer.RiderModelBundleName);
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
        public static void SendAllParts(GearUpdate FullGear)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendAllParts))
            {
                // rider
                if(LocalPlayer.instance.RiderModelname == "Daryien")
                {

                _packet.Write(FullGear.RiderTextures.Count);
                if (FullGear.RiderTextures.Count > 0)
                {
                for (int i = 0; i < FullGear.RiderTextures.Count; i++)
                {
                    _packet.Write(FullGear.RiderTextures[i].Nameoftexture);
                    _packet.Write(FullGear.RiderTextures[i].NameofparentGameObject);
                    _packet.Write(FullGear.RiderTextures[i].Matnum);
                }

                }


                }


                // Garage
                _packet.Write(FullGear.GarageSave.Length);
                _packet.Write(FullGear.GarageSave);





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


               
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                Debug.Log($"Sent all gear and {FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count} objects");
                LocalPlayer.instance.ServerActive = true;
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

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);

            }



        }



        
        public static void SendFileSegment(FileSegment FileSegment)
        {
            
                using (Packet _packet = new Packet((int)ClientPackets.SendFileSegment))
                {
                    _packet.Write(FileSegment.NameofFile);
                    _packet.Write(FileSegment.segment_count);
                    _packet.Write(FileSegment.this_segment_num);
                    _packet.Write(FileSegment.segment.Length);
                    _packet.Write(FileSegment.segment);
                    _packet.Write(FileSegment.ByteCount);
                    _packet.Write(FileSegment.path);

                    SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

                }

                Debug.Log($"packet {FileSegment.this_segment_num} of {FileSegment.segment_count} outgoing");


            


        }
        
       
        public static void RequestFile(string unfound, List<int> _packetsihave)
        {
            using(Packet _packet = new Packet((int)ClientPackets.RequestFileFromServer))
            {
               
                _packet.Write(unfound);
                _packet.Write(_packetsihave.Count);
                for (int i = 0; i < _packetsihave.Count; i++)
                {
                    _packet.Write(_packetsihave[i]);
                }
            
                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }



        public static void FileStatus(string name, int Status)
        {
            using (Packet _packet = new Packet((int)ClientPackets.FileStatus))
            {
                _packet.Write(name);
                _packet.Write(Status);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
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

       
        public static void SendGearUpdate(GearUpdate gear)
        {
            using (Packet _packet = new Packet((int)ClientPackets.GearUpdate))
            {
                _packet.Write(gear.isRiderUpdate);
                // rider
                if (gear.isRiderUpdate)
                {
                _packet.Write(gear.RiderTextures.Count);
                if (gear.RiderTextures.Count > 0)
                {
                    for (int i = 0; i < gear.RiderTextures.Count; i++)
                    {
                        _packet.Write(gear.RiderTextures[i].Nameoftexture);
                        _packet.Write(gear.RiderTextures[i].NameofparentGameObject);
                        _packet.Write(gear.RiderTextures[i].Matnum);
                    }

                }

                }
                else
                {
                    //bike
                    _packet.Write(gear.GarageSave.Length);
                    _packet.Write(gear.GarageSave);

                }


                


                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


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

        public static void AdminLogOut()
        {
            using(Packet _packet = new Packet((int)ClientPackets.LogOut))
            {
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




