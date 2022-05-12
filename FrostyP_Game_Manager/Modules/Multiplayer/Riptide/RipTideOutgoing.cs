using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.IO;
using RiptideNetworking;

namespace FrostyP_Game_Manager
{
    public static class RipTideOutgoing
    {
        public static float PosMult = 4500;
        public static float Rotmult = 80;

        static void SendToServer(Message mess)
        {
            SendToServerThread.ExecuteOnMainThread(() =>
            {
              RiptideManager.instance.client.Send(mess);
            });
        }



        #region Sendable_ messages

        public static void InitialTalk()
        {

            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Talking with server..", FrostyUIColor.System, 0));

            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.WelcomeReceived);
           

                _packet.AddString(InGameUI.instance.Username);
                _packet.AddString(InGameUI.instance.LocalPlayer.RiderModelname);
                _packet.AddString(InGameUI.instance.LocalPlayer.RiderModelBundleName);
                _packet.AddString(MultiplayerManager.MycurrentLevel);
                _packet.AddFloat(RiptideManager.instance.VERSIONNUMBER);
                _packet.AddBool(MultiplayerManager.GarageEnabled);
                _packet.AddString(MultiplayerManager.GarageVersion);

                SendToServer(_packet);
            
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
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SendAllParts);
            
                // rider
                Vector3 currentpos = LocalPlayer.instance.ActiveModel.transform.position;
                Vector3 currentrot = LocalPlayer.instance.ActiveModel.transform.eulerAngles;

                message.AddVector3(currentpos);
                message.AddVector3(currentrot);

            // daryien customs
                if (LocalPlayer.instance.RiderModelname == "Daryien")
                {

                    message.AddInt(FullGear.RiderTextures.Count);
                    if (FullGear.RiderTextures.Count > 0)
                    {
                        for (int i = 0; i < FullGear.RiderTextures.Count; i++)
                        {
                            message.AddString(FullGear.RiderTextures[i].Nameoftexture);
                            message.AddString(FullGear.RiderTextures[i].NameofparentGameObject);
                            message.AddInt(FullGear.RiderTextures[i].Matnum);
                            message.AddString(FullGear.RiderTextures[i].Directory);
                        }

                    }


                    message.AddBool(FullGear.Capisforward);

                }


                // Garage
                try
                {
                    message.AddBool(MultiplayerManager.GarageEnabled);
                    if (MultiplayerManager.GarageEnabled)
                    {
                        message.AddString(FullGear.GarageSaveXML);
                        message.AddString(FullGear.Presetname);
                    }
                }
                catch (Exception x)
                {
                    Debug.Log("garage fail" + x);
                return;
                }


            // park objects
                message.AddInt(ParkBuilder.instance.NetgameObjects.Count);

                if (ParkBuilder.instance.NetgameObjects.Count > 0)
                {
                    foreach (NetGameObject _netobj in ParkBuilder.instance.NetgameObjects)
                    {


                        message.AddString(_netobj.NameofObject);
                        message.AddString(_netobj.NameOfFile);
                        message.AddString(_netobj.NameofAssetBundle);

                        message.AddVector3(_netobj.Position);
                        message.AddVector3(_netobj.Rotation);
                        message.AddVector3(_netobj.Scale);
                        message.AddInt(_netobj.ObjectID);
                        message.AddString(_netobj.Directory);
                    }
                }




                SendToServer(message);
                Debug.Log($"Sent all gear and {ParkBuilder.instance.NetgameObjects.Count} objects");
                LocalPlayer.instance.SendStream = true;
                LocalPlayer.instance.SendWatch.Start();
            

        }

        public static void SendMyTransforms(Vector3[] positions, Vector3[] rotations, long _TimeStamp, double elapsed)
        {
            Message _packet = Message.Create(MessageSendMode.unreliable, (ushort)ClientPackets.TransformUpdate);
           
                _packet.AddDouble(elapsed);


                // rider root and locals
                _packet.AddVector3(positions[0]);
                _packet.AddVector3(rotations[0]);
                for (int i = 1; i < 23; i++)
                {
                    _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].x * (int)Rotmult))));
                    _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].y * (int)Rotmult))));
                    _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[i].z * (int)Rotmult))));

                }
                // Hip Joint
                _packet.AddVector3(positions[20]);


                // bike joint
                _packet.AddVector3(positions[24]);
                _packet.AddVector3(rotations[24]);

                //bars joint
                _packet.AddVector3(positions[25]);
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[25].z * Rotmult))));



                // drivetrain
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[26].z * Rotmult))));

                // frame joint
                _packet.AddVector3(positions[27]);
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[27].z * Rotmult))));



                // front and back wheel joint local X
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[28].z * Rotmult))));

                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[29].z * Rotmult))));


                // left and right pedal joint local X
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[30].z * Rotmult))));

                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[31].z * Rotmult))));

                // left and right finger index2 local X
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].z * Rotmult))));

                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[33].x * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].y * Rotmult))));
                _packet.AddShort((short)(SystemHalf.HalfHelper.SingleToHalf((rotations[32].z * Rotmult))));


                SendToServer(_packet);

        }

        public static void SendFileSegment(FileSegment FileSegment)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SendFileSegment);
            
                _packet.AddString(FileSegment.NameofFile);
                _packet.AddInt(FileSegment.segment_count);
                _packet.AddInt(FileSegment.this_segment_num);
                _packet.AddInt(FileSegment.segment.Length);
                _packet.AddBytes(FileSegment.segment);
                _packet.AddLong(FileSegment.ByteCount);
                _packet.AddString(FileSegment.path);

                SendToServer(_packet);

            

            Debug.Log($"packet {FileSegment.this_segment_num} of {FileSegment.segment_count} outgoing");





        }

        public static void RequestFile(string unfound, List<int> _packetsihave, string dir)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.RequestFileFromServer);
            

                _packet.AddString(unfound);
                _packet.AddString(dir);
                _packet.AddInt(_packetsihave.Count);
                for (int i = 0; i < _packetsihave.Count; i++)
                {
                    _packet.AddInt(_packetsihave[i]);
                }

                SendToServer(_packet);
            
        }

        public static void FileStatus(string name, int Status)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.FileStatus);
           
                _packet.AddString(name);
                _packet.AddInt(Status);

                SendToServer(_packet);
            
        }

        public static void SendAudioUpdate(List<AudioStateUpdate> updates, int Soundtype)
        {

            // Risers
            if (Soundtype == 1)
            {
                foreach (AudioStateUpdate update in updates)
                {
                    Message _packet = Message.Create(update.Sendflag, (ushort)ClientPackets.SendAudioUpdate);
                   

                        _packet.AddInt(Soundtype);
                        _packet.AddString(update.nameofriser);
                        _packet.AddInt(update.playstate);
                        _packet.AddFloat(update.Volume);
                        _packet.AddFloat(update.pitch);
                        _packet.AddFloat(update.Velocity);
                        _packet.AddByte((byte)update.Sendflag);
                     
                        SendToServer(_packet);

                    
                }

            }

            // One Shots
            if (Soundtype == 2)
            {
                foreach (AudioStateUpdate update in updates)
                {
                    Message _packet = Message.Create(MessageSendMode.unreliable, (ushort)ClientPackets.SendAudioUpdate);
                   
                        _packet.AddInt(Soundtype);
                        _packet.AddString(update.Path);
                        _packet.AddFloat(update.Volume);

                        SendToServer(_packet);

                    
                }
            }


        }

        public static void SendTextMessage(string _message)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SendTextMessage);
           
                _message = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_message));

                _packet.AddString(_message);


                SendToServer(_packet);
            
        }

        public static void SendGearUpdate(GearUpdate gear)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.GearUpdate);
            
                _packet.AddBool(gear.isRiderUpdate);
                // rider
                if (gear.isRiderUpdate)
                {
                    _packet.AddInt(gear.RiderTextures.Count);
                    if (gear.RiderTextures.Count > 0)
                    {
                        for (int i = 0; i < gear.RiderTextures.Count; i++)
                        {
                            _packet.AddString(gear.RiderTextures[i].Nameoftexture);
                            _packet.AddString(gear.RiderTextures[i].NameofparentGameObject);
                            _packet.AddInt(gear.RiderTextures[i].Matnum);
                            _packet.AddString(gear.RiderTextures[i].Directory);
                        }

                    }

                    _packet.AddBool(gear.Capisforward);
                }
                else
                {
                    LocalPlayer.instance.SendStream = false;
                    //bike
                    _packet.AddString(gear.GarageSaveXML);
                    _packet.AddString(gear.Presetname);
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Sent Garage preset: {PlayerPrefs.GetString("lastPreset")}", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                    LocalPlayer.instance.SendStream = true;
                }

                SendToServer(_packet);
            

        }

        public static void SendMapName(string name)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SendMapName);
            

                Debug.Log($"Sent {name}");
                _packet.AddString(name);

                SendToServer(_packet);
            

        }

        public static void TurnMeOn()
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.Turnmeon);
           
                SendToServer(_packet);
        }

        public static void TurnMeOff()
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.Turnmeoff);

            SendToServer(_packet);
        }

        public static void KeepActive()
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.KeepActive);

            SendToServer(_packet);
        }

        public static void InviteToSpawn(uint invitee, Vector3 pos, Vector3 rot)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.InviteToSpawn);

            
                _packet.AddUInt(invitee);
                _packet.AddVector3(pos);
                _packet.AddVector3(rot);

            SendToServer(_packet);
                
            
        }

        public static void OverrideAMapMatch(uint player, bool value)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.OverrideMapMatch);

           
                _packet.AddUInt(player);
                _packet.AddBool(value);

            SendToServer(_packet);
            
        }





        // Parkbuilder

        public static void SpawnObjectOnServer(NetGameObject _netobj)
        {
            string nameoffileinput = _netobj.NameOfFile;
            string NameofFileUnicode = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(nameoffileinput));
            NameofFileUnicode = NameofFileUnicode.Trim(Path.GetInvalidFileNameChars());
            NameofFileUnicode = NameofFileUnicode.Trim(Path.GetInvalidPathChars());

            string nameofassetbuninput = _netobj.NameofAssetBundle;
            string NameofBundleUnicode = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(nameofassetbuninput));
            NameofBundleUnicode = NameofBundleUnicode.Trim(Path.GetInvalidFileNameChars());
            NameofBundleUnicode = NameofBundleUnicode.Trim(Path.GetInvalidPathChars());

            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SpawnObject);


                _packet.AddString(_netobj.NameofObject);
                _packet.AddString(NameofFileUnicode);
                _packet.AddString(NameofBundleUnicode);

                _packet.AddVector3(_netobj.Position);
                _packet.AddVector3(_netobj.Rotation);
                _packet.AddVector3(_netobj.Scale);
                _packet.AddInt(_netobj.ObjectID);
                _packet.AddString(_netobj.Directory);

            SendToServer(_packet);
        }

        public static void DestroyAnObject(int objectid)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.DestroyAnObject);

            
                _packet.AddInt(objectid);
               
            SendToServer(_packet);
            
        }

        public static void ObjectTransformUpdate(PlacedObject _placedobject)
        {
            Message _packet = Message.Create(MessageSendMode.unreliable, (ushort)ClientPackets.MoveAnObject);

           
                _packet.AddInt(_placedobject.ObjectId);
                _packet.AddVector3(_placedobject.Object.transform.position);
                _packet.AddVector3(_placedobject.Object.transform.eulerAngles);
                _packet.AddVector3(_placedobject.Object.transform.localScale);

            SendToServer(_packet);
            
        }

        public static void VoteToRemoveObject(int ObjectID, uint _OwnerID)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.VoteToRemoveObject);

           
                _packet.AddInt(ObjectID);
                _packet.AddUInt(_OwnerID);
                
            SendToServer(_packet);
           
        }


        // ------------------------------------------------------------ Admin Mode --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AdminModeOn(string _password)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.AdminModeOn);

            _packet.AddString(_password);
            SendToServer(_packet);
            
        }

        public static void SendBootPlayer(string _username, int mins)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.SendBootPlayer);

                _packet.AddString(_username);
                _packet.AddInt(mins);

            SendToServer(_packet);
               
        }

        public static void AdminLogOut()
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.LogOut);

            SendToServer(_packet);
           
            
        }

        public static void AdminRemoveObject(uint _owner, int ObjectID)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.Turnmeon);

          
                _packet.AddUInt(_owner);
                _packet.AddInt(ObjectID);

            SendToServer(_packet);

        }

        public static void AdminAlterBanWords(bool addtolist, string word)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, (ushort)ClientPackets.Turnmeon);

            
                _packet.AddString(word.ToLower());
                _packet.AddBool(addtolist);

            SendToServer(_packet);
             
        }

        #endregion

    }
}
