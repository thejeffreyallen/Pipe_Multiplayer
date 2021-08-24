using System.Collections.Generic;
using UnityEngine;
using System;
using FrostyP_Game_Manager;


namespace PIPE_Valve_Console_Client
{
    public class ClientHandle
    {




        // Server Comms
        public static void Welcome(Packet _packet)
        {
            
            string _msg = _packet.ReadString();




            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(_msg, 4, 0));


            Debug.Log($"Message from server: {_msg}");
           
            ClientSend.WelcomeReceived();

            

           
        }
        
        public static void SetupPlayerReceive(Packet _packet)
        {
            LocalPlayer.instance.ServerActive = false;

            GearUpdate _gear;
            List<TextureInfo> RiderTextures = new List<TextureInfo>();

            uint playerid =(uint)_packet.ReadLong();
            string playerusername = _packet.ReadString();


            Vector3 Riderposition = _packet.ReadVector3();
            Vector3 RiderRotation = _packet.ReadVector3();
            string CurrentModel = _packet.ReadString();
            string RidermodelBundlename = _packet.ReadString();
            string currentmap = _packet.ReadString();
            bool capforward = true;


            if (CurrentModel == "Daryien")
            {
                int Ridertexnamecount = _packet.ReadInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        int matnum = _packet.ReadInt();
                        string dir = _packet.ReadString();
                        RiderTextures.Add(new TextureInfo(nameoftex, nameofGO, false, matnum,dir));
                    }
                }

               capforward = _packet.ReadBool();
            }

            int garagesaveByteSize = _packet.ReadInt();
            byte[] garagesave = _packet.ReadBytes(garagesaveByteSize);

            int parkcount = _packet.ReadInt();

            List<NetGameObject> objects = new List<NetGameObject>();
            if (parkcount > 0)
            {

                for (int i = 0; i < parkcount; i++)
                {
                    string NameofGO = _packet.ReadString();
                    string NameofFile = _packet.ReadString();
                    string NameofBundle = _packet.ReadString();

                    Vector3 Position = _packet.ReadVector3();
                    Vector3 Rotation = _packet.ReadVector3();
                    Vector3 Scale = _packet.ReadVector3();
                    int ObjectID = _packet.ReadInt();
                    string objdir = _packet.ReadString();

                     objects.Add(new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID,null, objdir));

                }

            }


            Debug.Log("Setting up player");


            _gear = new GearUpdate();
            _gear.RiderTextures = RiderTextures;
            _gear.Capisforward = capforward;
            _gear.GarageSave = garagesave;
            GameManager.instance.SpawnRider(playerid, playerusername, CurrentModel,RidermodelBundlename, Riderposition, RiderRotation,currentmap,_gear,objects);

            LocalPlayer.instance.ServerActive = true;
           


        }

        public static void SetupAllOnlinePlayers(Packet _packet)
        {
           LocalPlayer.instance.ServerActive = false;
          
            // amount of rider spawns received ( Max 5 per bundle)
            int amountinbundle = _packet.ReadInt();
            for (int _i = 0; _i < amountinbundle; _i++)
            {
                GearUpdate _gear;
                List<TextureInfo> RiderTextures = new List<TextureInfo>();



                uint playerid = (uint)_packet.ReadLong();
                string playerusername = _packet.ReadString();
                Vector3 Riderposition = _packet.ReadVector3();
                Vector3 RiderRotation = _packet.ReadVector3();
                string CurrentModel = _packet.ReadString();
                string RidermodelBundlename = _packet.ReadString();
                string currentmap = _packet.ReadString();
                bool capforward = true;


               


                if(CurrentModel == "Daryien")
                {
                int Ridertexnamecount = _packet.ReadInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        int matnum = _packet.ReadInt();
                        string dir = _packet.ReadString();
                       RiderTextures.Add(new TextureInfo(nameoftex, nameofGO, false, matnum,dir));
                    }
                }

                    capforward = _packet.ReadBool();
                }

                int garagesaveByteSize = _packet.ReadInt();
                byte[] garagesave = _packet.ReadBytes(garagesaveByteSize);

                int parkcount = _packet.ReadInt();

                List<NetGameObject> objects = new List<NetGameObject>();
                if (parkcount > 0)
                {

                    for (int i = 0; i < parkcount; i++)
                    {
                        string NameofGO = _packet.ReadString();
                        string NameofFile = _packet.ReadString();
                        string NameofBundle = _packet.ReadString();

                        Vector3 Position = _packet.ReadVector3();
                        Vector3 Rotation = _packet.ReadVector3();
                        Vector3 Scale = _packet.ReadVector3();
                        int ObjectID = _packet.ReadInt();
                        string objdir = _packet.ReadString();

                        objects.Add(new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null, objdir));

                    }

                }





                Debug.Log("Setting up player");
               

                _gear = new GearUpdate();
                _gear.GarageSave = garagesave;
                _gear.Capisforward = capforward;
                _gear.RiderTextures = RiderTextures;
                GameManager.instance.SpawnRider(playerid, playerusername, CurrentModel, RidermodelBundlename, Riderposition, RiderRotation, currentmap, _gear,objects);


            }
                LocalPlayer.instance.ServerActive = true;
            
        }
        
        public static void RequestForFile(Packet _packet)
        {
            
                
               
                    string n = _packet.ReadString();
            string dir = _packet.ReadString();
            int Listcount = _packet.ReadInt();
            List<int> Packetsowned = new List<int>();

            for (int i = 0; i < Listcount; i++)
            {
                int e = _packet.ReadInt();
                Packetsowned.Add(e);
            }

            // convert to local path
            int slash = dir.ToLower().LastIndexOf("pipe_data");
            string fulldir = Application.dataPath + "/" + dir.Remove(0, slash + 10);



            // leave details including packets owned by server if any

            FileSyncing.OutGoingIndexes.Add(new SendReceiveIndex(n, Packetsowned,fulldir));
            InGameUI.instance.NewMessage(20, new TextMessage($"Server requested {n}", (int)MessageColourByNum.Server, 1));

        }

        public static void RequestForAllParts(Packet _packet)
        {
           
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Sending Playerdata to server", 1, 0));
            GameManager.instance.SendAllParts();
           
        }

        public static void ReceiveFileSegment(Packet _packet)
        {
            string name = _packet.ReadString();
            int segmentno = _packet.ReadInt();
            int segmentscount = _packet.ReadInt();
            int bytecount = _packet.ReadInt();
            byte[] bytes = _packet.ReadBytes(bytecount);
            long FileBytecount = _packet.ReadLong();
            string path = _packet.ReadString();

            FileSyncing.FileReceive(bytes, name, segmentscount, segmentno, FileBytecount, path);
        }

        public static void FileStatus(Packet _packet)
        {
            string name = _packet.ReadString();
            int status = _packet.ReadInt();

            // received
            if(status == (int)PIPE_Valve_Console_Client.FileStatus.Received)
            {
            
            foreach(SendReceiveIndex r in FileSyncing.OutGoingIndexes.ToArray())
            {
                if (name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower().Contains(r.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower()))
                {
                  FileSyncing.OutGoingIndexes.Remove(r);
                  InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Server Received {r.NameOfFile}",(int)MessageColourByNum.System, 1));
                }
            }
            


            }
        }

        public static void Disconnectme(Packet _packet)
        {
            string msg = _packet.ReadString();
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(msg + ": disconnecting", 4, 0));
            InGameUI.instance.Disconnect();
            InGameUI.instance.Connected = false;
            InGameUI.instance.ShutdownAfterMessageFromServer();
        }

        public static void PlayerDisconnected(Packet _packet)
        {
            try
            {
           
            

                uint _id = (uint)_packet.ReadLong();




                bool exists = GameManager.Players.TryGetValue(_id, out RemotePlayer player);

                if (exists)
                {

                    if (InGameUI.instance.IsSpectating)
                    {
                        for (int i = 0; i < InGameUI.instance.cycleplayerslist.Count; i++)
                        {
                            if (InGameUI.instance.cycleplayerslist[i].id == _id)
                            {
                                if (InGameUI.instance.Targetrider == player.RiderModel)
                                {
                                   InGameUI.instance.SpectateExit();
                                }
                                else
                                {
                                 InGameUI.instance.cycleplayerslist.RemoveAt(i);

                                }


                            }
                        }


                    }


                    player.MasterShutdown();



                    PlacedObject objinsavelist = null;
                    foreach(NetGameObject n in player.Objects)
                    {
                        if(n._Gameobject != null)
                        {
                            GameManager.instance.DestroyObj(n._Gameobject);
                        }
                   
                        foreach (PlacedObject p in ParkBuilder.instance.ObjectstoSave)
                        {
                        if (p.OwnerID == player.id  && p.ObjectId == n.ObjectID)
                        {
                            objinsavelist = p;
                        }
                        }


                    if (objinsavelist != null)
                    {
                        ParkBuilder.instance.ObjectstoSave.Remove(objinsavelist);
                    }

                    }






                try
                {
                 GameManager.Players.Remove(_id);
                 GameManager.instance.DestroyObj(player.gameObject);
                }
                catch (Exception)
                {

                }


                }
                
                foreach(Waitingrequest w in FileSyncing.WaitingRequests.ToArray())
                {
                    if(w.player == _id)
                    {
                        FileSyncing.WaitingRequests.Remove(w);
                    }
                }

                GameManager.instance.UpdatePlayersOnMyLevelToggledOff();
            

            }
            catch (Exception x)
            {
                Debug.Log("error with playerdisconnected: " + x);
            }



        }

        public static void Update(Packet _packet)
        {
            List<string> FilesinUpdate = new List<string>();

            float Version = _packet.ReadFloat();
            int amountoffiles = _packet.ReadInt();
            for (int i = 0; i < amountoffiles; i++)
            {
                string file = _packet.ReadString();
                FilesinUpdate.Add(file);
            }
            InGameUI.instance.UpdateAvailable(Version,FilesinUpdate);
            Debug.Log("Update offer received");
        }

        public static void InviteToSpawn(Packet _packet)
        {
            uint player = (uint)_packet.ReadLong();
            Vector3 pos = _packet.ReadVector3();
            Vector3 rot = _packet.ReadVector3();

            InGameUI.instance.ReceivedSpawnInvite(player, pos, rot);

        }






        // user input
        public static void GearUpdate(Packet _packet)
        {
            Debug.Log("Received Gear update");

            uint _from =(uint)_packet.ReadLong();
            bool isRiderUpdate = _packet.ReadBool();

            if (isRiderUpdate)
            {
                List<TextureInfo> RiderTextures = new List<TextureInfo>();

                int Ridertexnamecount = _packet.ReadInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        int matnum = _packet.ReadInt();
                        string dir = _packet.ReadString();
                        RiderTextures.Add(new TextureInfo(nameoftex, nameofGO, false, matnum,dir));
                    }
                }

                bool Capforward = _packet.ReadBool();

                if(GameManager.Players.TryGetValue(_from, out RemotePlayer player))
                {
                    player.Gear.RiderTextures = RiderTextures;
                    player.Gear.Capisforward = Capforward;
                    player.UpdateDaryien();
                }




            }
            else
            {
                int bytecount = _packet.ReadInt();
                byte[] bytes = _packet.ReadBytes(bytecount);
                if (GameManager.Players.TryGetValue(_from, out RemotePlayer player))
                {
                    player.Gear.GarageSave = bytes;
                    player.UpdateBMX();
                }

            }

        

        }

        public static void ReceiveMapname(Packet _packet)
        {
            string Name = _packet.ReadString();
            uint from = (uint)_packet.ReadLong();

            try
            {
                if(GameManager.Players.TryGetValue(from,out RemotePlayer player))
                {
                   player.CurrentMap = Name;
                   FileSyncing.CheckForMap(Name, player.username);
                   GameManager.instance.ChangingLevel(Name,from);
                    


                }


            }
            catch(System.Exception x)
            {
                Debug.Log("Map name error, player didnt exist :  " + x);
            }

        }

       public static void PlayerPositionReceive(Packet _packet)
        {

            if (LocalPlayer.instance.ServerActive)
            {
                // data server added to packet
            int Ping = _packet.ReadInt();
            long ServerTimestamp = _packet.ReadLong();
            uint FromId = (uint)_packet.ReadLong();
            int length = _packet.ReadInt();

                // client packet in bytes
            byte[] _clientspacket = _packet.ReadBytes(length);




                // packet the client sent. servers transformupdate code, FromId and length have been read from the start
            using (Packet ClientPacket = new Packet(_clientspacket))
            {

                    // Transformupdate code the client placed at start when sending to server
                int sendcode = ClientPacket.ReadInt();


                    // to avoid truncation problem
                    int DividePos = (int)ClientSend.PosMult;
                    int DivideRot = (int)ClientSend.Rotmult;


                    // positions to be filled
                Vector3[] Positions = new Vector3[28];
                Vector3[] Rotations = new Vector3[34];


                    // Players machine timestamp
                    long _playertime = ClientPacket.ReadLong();
                    float elapsed = ClientPacket.ReadFloat();
                    // riders root pos and rot
                Positions[0] = ClientPacket.ReadVector3();
                Rotations[0] = ClientPacket.ReadVector3();

                    // rider locals
                for (int i = 1; i < 23; i++)
                {
                    SystemHalf.Half _x = ClientPacket.ReadShort();
                    SystemHalf.Half _y = ClientPacket.ReadShort();
                    SystemHalf.Half _z = ClientPacket.ReadShort();

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
                    Positions[20] = ClientPacket.ReadVector3();

                    // bike joint
                    Positions[24] = ClientPacket.ReadVector3();
                    Rotations[24] = ClientPacket.ReadVector3();

                    // bars joint
                    Positions[25] = ClientPacket.ReadVector3();
                    Rotations[25] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                    // Drivetrain local X
                    Rotations[26] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                    // frame joint
                    Positions[27] = ClientPacket.ReadVector3();
                    Rotations[27] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                    // front and back wheel local X
                    Rotations[28] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));
                    Rotations[29] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                    // left and right pedal local x
                    Rotations[30] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));
                    Rotations[31] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                    // left and right finger index2 local x
                    Rotations[32] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));
                    Rotations[33] = new Vector3((float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot), (float)(SystemHalf.HalfHelper.HalfToSingle(ClientPacket.ReadShort()) / DivideRot));

                   
                    try
                    {
                        if(GameManager.Players.TryGetValue(FromId,out RemotePlayer player))
                        {
                        if (player.MasterActive)
                        {
                          player.ProcessNewPositionUpdate(new IncomingTransformUpdate(Positions, Rotations, Ping, ServerTimestamp, _playertime,elapsed));
                        }
                    
                        }



                    }
                    catch (System.Exception x)
                    {
                    Debug.Log($"Position received for unready player {FromId}" + x);

                    }

            }

            }





        }

        public static void ReceiveAudioForaPlayer(Packet _packet)
        {

            if (LocalPlayer.instance.ServerActive)
            {
                try
                {
                    uint _from = (uint)_packet.ReadLong();
                    int RiserorOneshot = _packet.ReadInt();

                if(RiserorOneshot == 1)
                {
                   
                            string nameofriser = _packet.ReadString();
                            int playstate = _packet.ReadInt();
                            float volume = _packet.ReadFloat();
                            float pitch = _packet.ReadFloat();
                            float Velocity = _packet.ReadFloat();

                            AudioStateUpdate update = new AudioStateUpdate(volume, pitch, playstate, nameofriser, Velocity);
                       
                               
                                if (GameManager.Players.TryGetValue(_from, out RemotePlayer player))
                                {
                                    if (player.MasterActive)
                                    {
                                        player.Audio.IncomingRiserUpdates.Add(update);

                                    }

                                }

                    

                }

                if(RiserorOneshot == 2)
                {
                    
                        string Pathofsound = _packet.ReadString();
                        float volume = _packet.ReadFloat();


                        AudioStateUpdate update = new AudioStateUpdate(volume, Pathofsound);
                       
                            if(GameManager.Players.TryGetValue(_from,out RemotePlayer player))
                            {
                            if (player.MasterActive)
                            {
                                player.Audio.IncomingOneShotUpdates.Add(update);

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
          
        public static void IncomingTextMessage(Packet _packet)
        {
            uint _from = (uint)_packet.ReadLong();
            string _m = _packet.ReadString();
            int fromcode = _packet.ReadInt();

           

            if (fromcode == 3)
            {
                TextMessage tm = new TextMessage(GameManager.Players[_from].username + " : " + _m, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.PlayerMessageTime, tm);
            }
            if(fromcode == 2)
            {
                TextMessage tm = new TextMessage("You : " + _m, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.PlayerMessageTime, tm);
            }
            if (fromcode == 1)
            {
                TextMessage tm = new TextMessage("System : " + _m, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, tm);
            }
            if (fromcode == 4)
            {
                TextMessage tm = new TextMessage("Server : " + _m, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, tm);
            }

                


            
        }






        // Parkbuilder
        public static void SpawnAnObjectReceive(Packet _packet)
        {
            string NameofGO = _packet.ReadString();
            string NameofFile = _packet.ReadString();
            string NameofBundle = _packet.ReadString();

            Vector3 Position = _packet.ReadVector3();
            Vector3 Rotation = _packet.ReadVector3();
            Vector3 Scale = _packet.ReadVector3();
            int ObjectID = _packet.ReadInt();
            string dir = _packet.ReadString();

            uint OwnerID = (uint)_packet.ReadLong();

            NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null,dir);
            OBJ.OwnerID = OwnerID;

            try
            {
                GameManager.instance.SpawnObject(OBJ);
            }
            catch (UnityException x)
            {
                Debug.Log("Spawnobj receive error :  " + x);
            }


        }

        public static void DestroyAnObject(Packet _packet)
        {
            try
            {
               uint _ownerID = (uint)_packet.ReadLong();
               int ObjectId = _packet.ReadInt();

              

              if(GameManager.Players.TryGetValue(_ownerID, out RemotePlayer player))
              {
                    for (int i = 0; i < player.Objects.Count; i++)
                    {
                        if(player.Objects[i].ObjectID == ObjectId)
                        {
                            GameManager.instance.DestroyObj(player.Objects[i]._Gameobject);
                            player.Objects.RemoveAt(i);
                        }
                    }


              }




                PlacedObject objinsavelist = null;
                foreach(PlacedObject p in ParkBuilder.instance.ObjectstoSave)
                {
                    if(p.OwnerID == _ownerID && p.ObjectId == ObjectId)
                    {
                        objinsavelist = p;
                    }
                }
                if(objinsavelist != null)
                {
                    ParkBuilder.instance.ObjectstoSave.Remove(objinsavelist);
                }



            }
            catch (Exception x)
            {
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Could find player object to destroy it", (int)MessageColourByNum.Server, 0));
            }






        }

        public static void MoveAnObject(Packet _packet)
        {
            try
            {
            Vector3 _newpos = _packet.ReadVector3();
            Vector3 _newrot = _packet.ReadVector3();
            Vector3 _newscale = _packet.ReadVector3();
            int objectID = _packet.ReadInt();

            uint OwnerID = (uint)_packet.ReadLong();

                if(GameManager.Players.TryGetValue(OwnerID, out RemotePlayer player))
                {

            foreach(NetGameObject n in player.Objects)
            {
                if(n.ObjectID == objectID)
                {
                    GameManager.instance.MoveObject(n, _newpos, _newrot, _newscale);
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
        public static void LoginGood(Packet _packet)
        {
            InGameUI.instance.AdminLoggedin = true;
        }

        public static void AdminStream(Packet _packet)
        {
            float bytesinpsec = _packet.ReadFloat();
            float bytesoutpsec = _packet.ReadFloat();

            int inindexes = _packet.ReadInt();
            int outindexes = _packet.ReadInt();

            int ServerPendingRel = _packet.ReadInt();
            int ServerpendingUnrel = _packet.ReadInt();

            int Playercount = _packet.ReadInt();

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