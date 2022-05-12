using System;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    internal static class RiptideIncoming
    {
        static void GiveToUnity(UnityEngine.Events.UnityAction action)
        {
            SendToUnityThread.instance.ExecuteOnMainThread(action);
        }



        // Server Comms
        [MessageHandler((ushort)ServerPacket.Welcome)]
        public static void Welcome(Message mess)
        {
            try
            {
                string _msg = mess.GetString();
                string supported_garage_Ver = mess.GetString();
                int MaxTick = mess.GetInt();


                GiveToUnity(() => 
                {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(_msg, FrostyUIColor.Server, 0));
                    Debug.Log($"Message from server: {_msg}");
                    MultiplayerManager.EnableGarageSupport(supported_garage_Ver);
                    RiptideManager.instance.ServerMaxTick = MaxTick;
                    RipTideOutgoing.InitialTalk();
                }
                );


            }
            catch (Exception x)
            {
                GiveToUnity(() =>
                {
                    Debug.Log($"Welcome error : {x}");
                    MultiplayerManager.DisconnectMaster();
                });
            }

        }

        [MessageHandler((ushort)ServerPacket.SetupAPlayer)]
        public static void SetupPlayerSingle(Message mess)
        {
            LocalPlayer.instance.SendStream = false;

            GearUpdate _gear;
            List<NetTexture> RiderTextures = new List<NetTexture>();

            ushort playerid = mess.GetUShort();
            string playerusername = mess.GetString();


            Vector3 Riderposition = mess.GetVector3();
            Vector3 RiderRotation = mess.GetVector3();
            string CurrentModel = mess.GetString();
            string RidermodelBundlename = mess.GetString();
            string currentmap = mess.GetString();
            bool capforward = true;


            if (CurrentModel == "Daryien")
            {
                int Ridertexnamecount = mess.GetInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = mess.GetString();
                        string nameofGO = mess.GetString();
                        int matnum = mess.GetInt();
                        string dir = mess.GetString();
                        RiderTextures.Add(new NetTexture(nameoftex, nameofGO, false, matnum, dir));
                    }
                }

                capforward = mess.GetBool();
            }

            string garagesave = mess.GetString();
            string presetname = mess.GetString();

            int parkcount = mess.GetInt();

            List<NetGameObject> objects = new List<NetGameObject>();
            if (parkcount > 0)
            {

                for (int i = 0; i < parkcount; i++)
                {
                    string NameofGO = mess.GetString();
                    string NameofFile = mess.GetString();
                    string NameofBundle = mess.GetString();

                    Vector3 Position = mess.GetVector3();
                    Vector3 Rotation = mess.GetVector3();
                    Vector3 Scale = mess.GetVector3();
                    int ObjectID = mess.GetInt();
                    string objdir = mess.GetString();

                    objects.Add(new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null, objdir));

                }

            }

            // data gathered

            MultiplayerManager.NewRiderPacket riderpacket = new MultiplayerManager.NewRiderPacket();

            _gear = new GearUpdate();
            _gear.RiderTextures = RiderTextures;
            _gear.Capisforward = capforward;
            _gear.GarageSaveXML = garagesave;
            _gear.Presetname = presetname;

            riderpacket.Currentmap = currentmap;
            riderpacket._id = playerid;
            riderpacket._username = playerusername;
            riderpacket.currentmodel = CurrentModel;
            riderpacket.modelbundlename = RidermodelBundlename;
            riderpacket._position = Riderposition;
            riderpacket._rotation = RiderRotation;
            riderpacket.Gear = _gear;
            riderpacket.Objects = objects;

            GiveToUnity(() => 
            {
                Debug.Log("Setting up player");
                MultiplayerManager.instance.SpawnRider(riderpacket);

            });
                LocalPlayer.instance.SendStream = true;



        }

        [MessageHandler((ushort)ServerPacket.ReceiveSetupAllOnlinePlayers)]
        public static void SetupPlayerGroup(Message mess)
        {
            LocalPlayer.instance.SendStream = false;

            // amount of rider spawns received ( Max 5 per bundle)
            int amountinbundle = mess.GetInt();
            for (int _i = 0; _i < amountinbundle; _i++)
            {
                GearUpdate _gear;
                List<NetTexture> RiderTextures = new List<NetTexture>();



                ushort playerid = mess.GetUShort();
                string playerusername = mess.GetString();
                Vector3 Riderposition = mess.GetVector3();
                Vector3 RiderRotation = mess.GetVector3();
                string CurrentModel = mess.GetString();
                string RidermodelBundlename = mess.GetString();
                string currentmap = mess.GetString();
                bool capforward = true;





                if (CurrentModel == "Daryien")
                {
                    int Ridertexnamecount = mess.GetInt();
                    if (Ridertexnamecount > 0)
                    {
                        for (int i = 0; i < Ridertexnamecount; i++)
                        {
                            string nameoftex = mess.GetString();
                            string nameofGO = mess.GetString();
                            int matnum = mess.GetInt();
                            string dir = mess.GetString();
                            RiderTextures.Add(new NetTexture(nameoftex, nameofGO, false, matnum, dir));
                        }
                    }

                    capforward = mess.GetBool();
                }

                string garagesave = mess.GetString();
                string presetname = mess.GetString();

                int parkcount = mess.GetInt();

                List<NetGameObject> objects = new List<NetGameObject>();
                if (parkcount > 0)
                {

                    for (int i = 0; i < parkcount; i++)
                    {
                        string NameofGO = mess.GetString();
                        string NameofFile = mess.GetString();
                        string NameofBundle = mess.GetString();

                        Vector3 Position = mess.GetVector3();
                        Vector3 Rotation = mess.GetVector3();
                        Vector3 Scale = mess.GetVector3();
                        int ObjectID = mess.GetInt();
                        string objdir = mess.GetString();

                        objects.Add(new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null, objdir));

                    }

                }

                _gear = new GearUpdate();
                _gear.GarageSaveXML = garagesave;
                _gear.Presetname = presetname;
                _gear.Capisforward = capforward;
                _gear.RiderTextures = RiderTextures;
                MultiplayerManager.NewRiderPacket config = new MultiplayerManager.NewRiderPacket();
                config.Currentmap = currentmap;
                config._id = playerid;
                config._username = playerusername;
                config.currentmodel = CurrentModel;
                config.modelbundlename = RidermodelBundlename;
                config._position = Riderposition;
                config._rotation = RiderRotation;
                config.Gear = _gear;
                config.Objects = objects;


                GiveToUnity(() =>
                {
                    Debug.Log("Setting up player");
                    MultiplayerManager.instance.SpawnRider(config);

                });

            }
            LocalPlayer.instance.SendStream = true;

        }

        [MessageHandler((ushort)ServerPacket.requestFile)]
        public static void RequestForFile(Message mess)
        {
            string n = mess.GetString();
            string dir = mess.GetString();
            int Listcount = mess.GetInt();
            List<int> Packetsowned = new List<int>();

            for (int i = 0; i < Listcount; i++)
            {
                int e = mess.GetInt();
                Packetsowned.Add(e);
            }

            // convert to local path
            int slash = dir.ToLower().LastIndexOf("pipe_data");
            string fulldir = Application.dataPath + "/" + dir.Remove(0, slash + 10);



            // leave details including packets owned by server if any
            FileSyncing.OutGoingIndexes.Add(new SendReceiveIndex(n, Packetsowned, fulldir));
            //InGameUI.instance.NewMessage(20, new TextMessage($"Server requested {n}", FrostyUIColor.Server, 1));

        }

        [MessageHandler((ushort)ServerPacket.RequestForAllParts)]
        public static void RequestForAllParts(Message mess)
        {
            GiveToUnity(() =>
            {
                Debug.Log("Server requested all parts");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Sending Playerdata to server", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                MultiplayerManager.instance.SendAllParts();

            });

        }

        [MessageHandler((ushort)ServerPacket.SendFileSegment)]
        public static void ReceiveFileSegment(Message mess)
        {
            string name = mess.GetString();
            int segmentno = mess.GetInt();
            int segmentscount = mess.GetInt();
            int bytecount = mess.GetInt();
            byte[] bytes = mess.GetBytes(bytecount);
            long FileBytecount = mess.GetLong();
            string path = mess.GetString();

            FileSyncing.FileReceive(bytes, name, segmentscount, segmentno, FileBytecount, path);
        }

        [MessageHandler((ushort)ServerPacket.FileStatus)]
        public static void FileStatus(Message mess)
        {
            string name = mess.GetString();
            int status = mess.GetInt();

            // received
            if (status == (int)SyncStatus.Received)
            {

                foreach (SendReceiveIndex r in FileSyncing.OutGoingIndexes.ToArray())
                {
                    if (name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower().Contains(r.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower()))
                    {
                        FileSyncing.OutGoingIndexes.Remove(r);
                        InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Server Received {r.NameOfFile}", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                    }
                }



            }
        }

        [MessageHandler((ushort)ServerPacket.disconnectme)]
        public static void Disconnectme(Message mess)
        {
            string msg = mess.GetString();
            GiveToUnity(() =>
            {
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(msg + ": disconnecting", FrostyUIColor.Server, 0));
                MultiplayerManager.DisconnectMaster();
                InGameUI.instance.ShutdownAfterMessageFromServer();

            });
        }

        [MessageHandler((ushort)ServerPacket.DisconnectedPlayer)]
        public static void PlayerDisconnected(Message mess)
        {
            try
            {
                ushort _id = mess.GetUShort();
                GiveToUnity(() =>
                {
                    MultiplayerManager.PlayerDisconnect(_id);
                });

            }
            catch (Exception x)
            {
                Debug.Log("error with playerdisconnected: " + x);
            }



        }

        [MessageHandler((ushort)ServerPacket.Update)]
        public static void Update(Message mess)
        {
            List<string> FilesinUpdate = new List<string>();

            float Version = mess.GetFloat();
            int amountoffiles = mess.GetInt();
            for (int i = 0; i < amountoffiles; i++)
            {
                string file = mess.GetString();
                FilesinUpdate.Add(file);
            }
            InGameUI.instance.UpdateAvailable(Version, FilesinUpdate);
            Debug.Log("Update offer received");
        }

        [MessageHandler((ushort)ServerPacket.InviteToSpawn)]
        public static void InviteToSpawn(Message mess)
        {
            ushort player = mess.GetUShort();
            Vector3 pos = mess.GetVector3();
            Vector3 rot = mess.GetVector3();

            InGameUI.instance.ReceivedSpawnInvite(player, pos, rot);

        }


        // user input
        [MessageHandler((ushort)ServerPacket.GearUpdate)]
        public static void GearUpdate(Message mess)
        {
            Debug.Log("Received Gear update");

            ushort _from = mess.GetUShort();
            bool isRiderUpdate = mess.GetBool();

            if (isRiderUpdate)
            {
                List<NetTexture> RiderTextures = new List<NetTexture>();

                int Ridertexnamecount = mess.GetInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = mess.GetString();
                        string nameofGO = mess.GetString();
                        int matnum = mess.GetInt();
                        string dir = mess.GetString();
                        RiderTextures.Add(new NetTexture(nameoftex, nameofGO, false, matnum, dir));
                    }
                }

                bool Capforward = mess.GetBool();

                if (MultiplayerManager.Players.TryGetValue(_from, out RemotePlayer player))
                {
                    player.Gear.RiderTextures = RiderTextures;
                    player.Gear.Capisforward = Capforward;
                    GiveToUnity(() =>
                    {
                        player.UpdateDaryien();

                    });
                }




            }
            else
            {
                string xmlgarage = mess.GetString();
                string presetname = mess.GetString();
                if (MultiplayerManager.Players.TryGetValue(_from, out RemotePlayer player))
                {
                    player.Gear.GarageSaveXML = xmlgarage;
                    player.Gear.Presetname = presetname;
                    GiveToUnity(() =>
                    {
                        player.UpdateBMX();
                    });
                }

            }



        }

        [MessageHandler((ushort)ServerPacket.ReceiveMapName)]
        public static void ReceiveMapname(Message mess)
        {
            string Name = mess.GetString();
            ushort from = mess.GetUShort();

            try
            {
                if (MultiplayerManager.Players.TryGetValue(from, out RemotePlayer player))
                {
                    player.CurrentMap = Name;
                    FileSyncing.CheckForMap(Name, player.username);
                    MultiplayerManager.instance.ChangingLevel(Name, from);



                }


            }
            catch (System.Exception x)
            {
                Debug.Log("Map name error, player didnt exist :  " + x);
            }

        }

        [MessageHandler((ushort)ServerPacket.ReceiveTransformUpdate)]
        public static void PlayerPositionReceive(Message mess)
        {
            try
            {
                if (LocalPlayer.instance.SendStream)
            {
                // data server added to packet
                int Ping = mess.GetInt();
                long ServerTimestamp = mess.GetLong();
                ushort FromId = mess.GetUShort();
                

                    // to avoid truncation problem
                    int DividePos = (int)RipTideOutgoing.PosMult;
                    int DivideRot = (int)RipTideOutgoing.Rotmult;


                    // positions to be filled
                    Vector3[] Positions = new Vector3[28];
                    Vector3[] Rotations = new Vector3[34];


                    // Players machine timestamp
                    double elapsed = mess.GetDouble();
                    // riders root pos and rot
                    Positions[0] = mess.GetVector3();
                    Rotations[0] = mess.GetVector3();

                    // rider locals
                    for (int i = 1; i < 23; i++)
                    {
                        SystemHalf.Half _x = mess.GetShort();
                        SystemHalf.Half _y = mess.GetShort();
                        SystemHalf.Half _z = mess.GetShort();

                        if (SystemHalf.HalfHelper.IsInfinity(_x) | SystemHalf.HalfHelper.IsNaN(_x))
                        {
                            _x = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(_y) | SystemHalf.HalfHelper.IsNaN(_y))
                        {
                            _y = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(_z) | SystemHalf.HalfHelper.IsNaN(_z))
                        {
                            _z = 0;
                        }

                        Rotations[i] = new Vector3(SystemHalf.HalfHelper.HalfToSingle(_x) / DivideRot, SystemHalf.HalfHelper.HalfToSingle(_y) / DivideRot, SystemHalf.HalfHelper.HalfToSingle(_z) / DivideRot);
                    }

                    // hip joint
                    Positions[20] = mess.GetVector3();

                    // bike joint
                    Positions[24] = mess.GetVector3();
                    Rotations[24] = mess.GetVector3();

                    // bars joint
                    Positions[25] = mess.GetVector3();
                    Rotations[25] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));

                    // Drivetrain local X
                    Rotations[26] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));

                    // frame joint
                    Positions[27] = mess.GetVector3();
                    Rotations[27] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));

                    // front and back wheel local X
                    Rotations[28] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));
                    Rotations[29] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));

                    // left and right pedal local x
                    Rotations[30] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));
                    Rotations[31] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));

                    // left and right finger index2 local x
                    Rotations[32] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));
                    Rotations[33] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(mess.GetShort()) / DivideRot));


                    try
                    {
                        if (MultiplayerManager.Players.TryGetValue(FromId, out RemotePlayer player))
                        {
                            if (player.MasterActive)
                            {
                            // gives to unity thread once all calculations are done to avoid conflict when adding it incoming postion list
                                player.ProcessNewPositionUpdate(RemoteTransform.Create(Positions, Rotations, Ping, ServerTimestamp, elapsed));
                            }

                        }

                    }
                    catch (System.Exception x)
                    {
                        Debug.Log($"Position received for unready player {FromId}" + x);

                    }

                

            }

            }
            catch (Exception x)
            {
                Debug.Log($"Player position receive error : " + x);
                throw;
            }


        }

        [MessageHandler((ushort)ServerPacket.ReceiveAudioForPlayer)]
        public static void ReceiveAudioForaPlayer(Message mess)
        {

            if (LocalPlayer.instance.SendStream)
            {
                try
                {
                    ushort _from = mess.GetUShort();
                    int RiserorOneshot = mess.GetInt();

                    if (RiserorOneshot == 1)
                    {

                        string nameofriser = mess.GetString();
                        int playstate = mess.GetInt();
                        float volume = mess.GetFloat();
                        float pitch = mess.GetFloat();
                        float Velocity = mess.GetFloat();

                        AudioStateUpdate update = AudioStateUpdate.Create(volume, pitch, playstate, nameofriser, Velocity);


                        if (MultiplayerManager.Players.TryGetValue(_from, out RemotePlayer player))
                        {
                            if (player.MasterActive)
                            {
                                GiveToUnity(() =>
                                {
                                    player.Audio.IncomingRiserUpdates.Enqueue(update);
                                });

                            }

                        }



                    }

                    if (RiserorOneshot == 2)
                    {

                        string Pathofsound = mess.GetString();
                        float volume = mess.GetFloat();


                        AudioStateUpdate update = AudioStateUpdate.Create(volume, Pathofsound);

                        if (MultiplayerManager.Players.TryGetValue(_from, out RemotePlayer player))
                        {
                            if (player.MasterActive)
                            {
                                GiveToUnity(() =>
                                {
                                    player.Audio.IncomingOneShotUpdates.Enqueue(update);
                                });
                            }

                        }




                    }


                }
                catch (UnityException x)
                {
                    Debug.LogError("Audio Error : " + x);
                }

            }


        }

        [MessageHandler((ushort)ServerPacket.IncomingTextMessage)]
        public static void IncomingTextMessage(Message mess)
        {
            ushort fromcode = mess.GetUShort();
            string _m = mess.GetString();


            GiveToUnity(() =>
            {
                TextMessage tm = new TextMessage(_m, (FrostyUIColor)fromcode, (TextMessage.Textmessagemode)fromcode, fromcode);
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, tm);
            });

        }






        // Parkbuilder
        [MessageHandler((ushort)ServerPacket.SpawnAnObjectReceive)]
        public static void SpawnAnObjectReceive(Message mess)
        {
            string NameofGO = mess.GetString();
            string NameofFile = mess.GetString();
            string NameofBundle = mess.GetString();

            Vector3 Position = mess.GetVector3();
            Vector3 Rotation = mess.GetVector3();
            Vector3 Scale = mess.GetVector3();
            int ObjectID = mess.GetInt();
            string dir = mess.GetString();

            ushort OwnerID = mess.GetUShort();

            NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null, dir);
            OBJ.OwnerID = OwnerID;

           
            GiveToUnity(() =>
            {
                MultiplayerManager.instance.SpawnObject(OBJ);
            });
            
           
        }

        [MessageHandler((ushort)ServerPacket.DestroyAnObject)]
        public static void DestroyAnObject(Message mess)
        {
            try
            {
                ushort _ownerID = mess.GetUShort();
                int ObjectId = mess.GetInt();



                if (MultiplayerManager.Players.TryGetValue(_ownerID, out RemotePlayer player))
                {
                    for (int i = 0; i < player.Objects.Count; i++)
                    {
                        if (player.Objects[i].ObjectID == ObjectId)
                        {
                            GiveToUnity(() =>
                            {
                                MultiplayerManager.DestroyObj(player.Objects[i]._Gameobject);
                                player.Objects.RemoveAt(i);
                            });
                        }
                    }


                }




                PlacedObject objinsavelist = null;
                foreach (PlacedObject p in ParkBuilder.instance.ObjectstoSave)
                {
                    if (p.OwnerID == _ownerID && p.ObjectId == ObjectId)
                    {
                        objinsavelist = p;
                    }
                }
                if (objinsavelist != null)
                {
                    ParkBuilder.instance.ObjectstoSave.Remove(objinsavelist);
                }



            }
            catch (Exception x)
            {
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Could find player object to destroy it", FrostyUIColor.Server, 0));
            }






        }

        [MessageHandler((ushort)ServerPacket.MoveAnObject)]
        public static void MoveAnObject(Message mess)
        {
            try
            {
                Vector3 _newpos = mess.GetVector3();
                Vector3 _newrot = mess.GetVector3();
                Vector3 _newscale = mess.GetVector3();
                int objectID = mess.GetInt();

                ushort OwnerID = mess.GetUShort();

                if (MultiplayerManager.Players.TryGetValue(OwnerID, out RemotePlayer player))
                {

                    foreach (NetGameObject n in player.Objects)
                    {
                        if (n.ObjectID == objectID)
                        {
                            GiveToUnity(() =>
                            {
                                MultiplayerManager.instance.MoveObject(n, _newpos, _newrot, _newscale);
                            });
                        }
                    }

                }

            }
            catch (Exception x)
            {
                Debug.Log($"Moveobject error :  " + x);
            }


        }





        // Admin
        [MessageHandler((ushort)ServerPacket.Logingood)]
        public static void LoginGood(Message mess)
        {
            InGameUI.instance.AdminLoggedin = true;
        }

        [MessageHandler((ushort)ServerPacket.AdminStream)]
        public static void AdminStream(Message mess)
        {
            float bytesinpsec = mess.GetFloat();
            float bytesoutpsec = mess.GetFloat();

            int inindexes = mess.GetInt();
            int outindexes = mess.GetInt();

            int ServerPendingRel = mess.GetInt();
            int ServerpendingUnrel = mess.GetInt();

            int Playercount = mess.GetInt();

            InGameUI.instance.ServerPendingRel = ServerPendingRel;
            InGameUI.instance.ServerPendingUnRel = ServerpendingUnrel;
            InGameUI.instance.serverbytesinpersec = bytesinpsec;
            InGameUI.instance.serverbytesoutpersec = bytesoutpsec;
            InGameUI.instance.ServerPlayercount = Playercount;
            InGameUI.instance.Serverinindexes = inindexes;
            InGameUI.instance.Serveroutindexes = outindexes;
        }

    }
}
