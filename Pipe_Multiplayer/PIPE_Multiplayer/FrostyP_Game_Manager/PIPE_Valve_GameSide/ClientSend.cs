using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using Valve.Sockets;


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
                try
                {
                  GameNetworking.instance.Socket.SendMessageToConnection(GameNetworking.instance.ServerConnection, bytes, sendflag);

                }
                catch (Exception x)
                {
                    SendToUnityThread.instance.ExecuteOnMainThread(() =>
                    {
                        Debug.Log("Valve send issue: " + x);
                    });



                }
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


                    SendToServer(_packet.ToArray(), SendFlags.Reliable);
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
                Vector3 currentpos = LocalPlayer.instance.ActiveModel.transform.position;
                Vector3 currentrot = LocalPlayer.instance.ActiveModel.transform.eulerAngles;

                _packet.Write(currentpos);
                _packet.Write(currentrot);


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
                    _packet.Write(FullGear.RiderTextures[i].Directory);
                }

                }


                    _packet.Write(FullGear.Capisforward);

                }


                // Garage
                try
                {
                    if (FullGear.GarageSave.Length > 0)
                    {
                       _packet.Write(FullGear.GarageSave.Length);
                       _packet.Write(FullGear.GarageSave);
                    }
                    else
                    {
                        InGameUI.instance.NewMessage(6, new TextMessage("couldn't send Garage Save, ensure preset loads as intended", (int)MessageColourByNum.Server, 1));
                        InGameUI.instance.NewMessage(6, new TextMessage("Disconnecting..", (int)MessageColourByNum.Server, 1));
                        InGameUI.instance.ShutdownAfterMessageFromServer();
                        return;
                    }

                }
                catch (Exception)
                {

                  
                }





                _packet.Write(FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count);

               
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
                            _packet.Write(_netobj.Directory);
                        }
                    }
                


               
                SendToServer(_packet.ToArray(),SendFlags.Reliable);
                Debug.Log($"Sent all gear and {FrostyP_Game_Manager.ParkBuilder.instance.NetgameObjects.Count} objects");
                LocalPlayer.instance.ServerActive = true;
                LocalPlayer.instance.SendWatch.Start();
            }

        }

       
        public static void SendMyTransforms(Vector3[] positions, Vector3[] rotations, long _TimeStamp, float elapsed)
        {

            using (Packet _packet = new Packet((int)ClientPackets.TransformUpdate))
            {

                _packet.Write(_TimeStamp);
                _packet.Write(elapsed);


                // rider root and locals
                _packet.Write(positions[0]);
                _packet.Write(rotations[0]);
                for (int i = 1; i < 23; i++)
                {
                  _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].x * (int)Rotmult))));
                  _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].y * (int)Rotmult))));
                  _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].z * (int)Rotmult))));

                }
                // Hip Joint
                _packet.Write(positions[20]);


                // bike joint
                _packet.Write(positions[24]);
                _packet.Write(rotations[24]);

                //bars joint
                _packet.Write(positions[25]);
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].z * Rotmult))));



                // drivetrain
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].z * Rotmult))));

                // frame joint
                _packet.Write(positions[27]);
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].z * Rotmult))));



                // front and back wheel joint local X
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].z * Rotmult))));

                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].z * Rotmult))));


                // left and right pedal joint local X
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].z * Rotmult))));

                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].z * Rotmult))));

                // left and right finger index2 local X
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].z * Rotmult))));

                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[33].x * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].y * Rotmult))));
                _packet.Write((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].z * Rotmult))));

               
                SendToServer(_packet.ToArray(), SendFlags.Unreliable);
               
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

                    SendToServer(_packet.ToArray(), SendFlags.NoNagle);

                }

                Debug.Log($"packet {FileSegment.this_segment_num} of {FileSegment.segment_count} outgoing");


            


        }
        
       
        public static void RequestFile(string unfound, List<int> _packetsihave,string dir)
        {
            using(Packet _packet = new Packet((int)ClientPackets.RequestFileFromServer))
            {
               
                _packet.Write(unfound);
                _packet.Write(dir);
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



        public static void SendAudioUpdate(List<AudioStateUpdate> updates, int Soundtype)
        {

            // Risers
            if(Soundtype == 1)
            {
                foreach (AudioStateUpdate update in updates)
                {
                    using (Packet _packet = new Packet((int)ClientPackets.SendAudioUpdate))
                    {

                        _packet.Write(Soundtype);
                        _packet.Write(update.nameofriser);
                        _packet.Write(update.playstate);
                        _packet.Write(update.Volume);
                        _packet.Write(update.pitch);
                        _packet.Write(update.Velocity);

                        if(update.Sendflag == SendFlags.Reliable)
                        {
                            _packet.Write(1);
                        }
                        if (update.Sendflag == SendFlags.Unreliable)
                        {
                            _packet.Write(2);
                        }


                        SendToServer(_packet.ToArray(),update.Sendflag);

                    }
                }

            }

            // One Shots
            if(Soundtype == 2)
            {
                foreach (AudioStateUpdate update in updates)
                {
                    using (Packet _packet = new Packet((int)ClientPackets.SendAudioUpdate))
                    {
                        _packet.Write(Soundtype);
                        _packet.Write(update.Path);
                        _packet.Write(update.Volume);

                        SendToServer(_packet.ToArray(), update.Sendflag);

                    }
                }
            }
           
               



        }


        public static void SendTextMessage(string _message)
        {
            using(Packet _packet = new Packet((int)ClientPackets.SendTextMessage))
            {
                _message = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_message));
                
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
                        _packet.Write(gear.RiderTextures[i].Directory);
                    }

                }

                    _packet.Write(gear.Capisforward);
                }
                else
                {
                    LocalPlayer.instance.ServerActive = false;
                    //bike
                    _packet.Write(gear.GarageSave.Length);
                    _packet.Write(gear.GarageSave);
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Sent Garage preset: {PlayerPrefs.GetString("lastPreset")}", (int)MessageColourByNum.System, 1));
                    LocalPlayer.instance.ServerActive = true;
                }


                


                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }


        public static void SendMapName(string name)
        {
            using (Packet _packet = new Packet((int)ClientPackets.SendMapName))
            {
              
                Debug.Log($"Sent {name}");
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

        public static void InviteToSpawn(uint invitee,Vector3 pos, Vector3 rot)
        {
            using (Packet _packet = new Packet((int)ClientPackets.InviteToSpawn))
            {
                _packet.Write(invitee);
                _packet.Write(pos);
                _packet.Write(rot);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
         }

        public static void OverrideAMapMatch(uint player, bool value)
        {
            using(Packet packet = new Packet((int)ClientPackets.OverrideMapMatch))
            {
                packet.Write(player);
                packet.Write(value);

                SendToServer(packet.ToArray(), SendFlags.Reliable);


            }
        }





        // Parkbuilder

        public static void SpawnObjectOnServer(FrostyP_Game_Manager.NetGameObject _netobj)
        {
            string nameoffileinput = _netobj.NameOfFile;
            string NameofFileUnicode = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(nameoffileinput));
            NameofFileUnicode = NameofFileUnicode.Trim(Path.GetInvalidFileNameChars());
            NameofFileUnicode = NameofFileUnicode.Trim(Path.GetInvalidPathChars());

            string nameofassetbuninput = _netobj.NameofAssetBundle;
            string NameofBundleUnicode = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(nameofassetbuninput));
            NameofBundleUnicode = NameofBundleUnicode.Trim(Path.GetInvalidFileNameChars());
            NameofBundleUnicode = NameofBundleUnicode.Trim(Path.GetInvalidPathChars());

            using (Packet _packet = new Packet((int)ClientPackets.SpawnObject))
            {
                _packet.Write(_netobj.NameofObject);
                _packet.Write(NameofFileUnicode);
                _packet.Write(NameofBundleUnicode);

                _packet.Write(_netobj.Position);
                _packet.Write(_netobj.Rotation);
                _packet.Write(_netobj.Scale);
                _packet.Write(_netobj.ObjectID);
                _packet.Write(_netobj.Directory);

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

        public static void AdminAlterBanWords(bool addtolist, string word)
        {
            using (Packet _packet = new Packet((int)ClientPackets.AlterBanwords))
            {
                _packet.Write(word.ToLower());
                _packet.Write(addtolist);

                SendToServer(_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        #endregion
        
    }
}




