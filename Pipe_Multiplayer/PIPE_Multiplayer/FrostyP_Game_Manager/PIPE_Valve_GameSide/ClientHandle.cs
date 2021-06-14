using System.Collections.Generic;
using UnityEngine;
using System;
using FrostyP_Game_Manager;


namespace PIPE_Valve_Console_Client
{
    public class ClientHandle : MonoBehaviour
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
            GameManager.instance._localplayer.ServerActive = false;
           


            // optional
            List<TextureInfo> BmxTexinfos = new List<TextureInfo>();
            List<TextureInfo> BmxNormalinfos = new List<TextureInfo>();
            List<TextureInfo> riderinfos = new List<TextureInfo>();

            uint playerid =(uint)_packet.ReadLong();
            string playerusername = _packet.ReadString();


            Vector3 Riderposition = _packet.ReadVector3();
            Vector3 RiderRotation = _packet.ReadVector3();
            string CurrentModel = _packet.ReadString();
            string RidermodelBundlename = _packet.ReadString();
            string currentmap = _packet.ReadString();



            Vector3 Framecol = _packet.ReadVector3();
            Vector3 forkcol =_packet.ReadVector3();
            Vector3 barscol = _packet.ReadVector3();
            Vector3 seatcol = _packet.ReadVector3();
            Vector3 ftirecol = _packet.ReadVector3();
            Vector3 rtirecol =  _packet.ReadVector3();
            Vector3 ftiresidecol = _packet.ReadVector3();
            Vector3 rtiresidecol = _packet.ReadVector3();

            Vector3 Stemcol = _packet.ReadVector3();
            Vector3 FRimcol = _packet.ReadVector3();
            Vector3 RRimcol = _packet.ReadVector3();


            float framesmooth = _packet.ReadFloat();
            float forksmooth = _packet.ReadFloat();
           float seatsmooth = _packet.ReadFloat();
           float barssmooth = _packet.ReadFloat();

            float stemsmooth =_packet.ReadFloat();
            float FRimsmooth = _packet.ReadFloat();
            float RRimsmooth = _packet.ReadFloat();



            float Framemett = _packet.ReadFloat();
            float Forksmett = _packet.ReadFloat();
            float barsmett = _packet.ReadFloat();
            float stemmett = _packet.ReadFloat();
            float FRimmett = _packet.ReadFloat();
            float RRimmett = _packet.ReadFloat();

            int Ridertexnamecount = _packet.ReadInt();
            if (Ridertexnamecount > 0)
            {
                for (int i = 0; i < Ridertexnamecount; i++)
                {
                    string nameoftex = _packet.ReadString();
                    string nameofGO = _packet.ReadString();
                    riderinfos.Add(new TextureInfo(nameoftex, nameofGO));
                }
            }



            int Biketexnamecount = _packet.ReadInt();
            if (Biketexnamecount > 0)
            {
                for (int i = 0; i < Biketexnamecount; i++)
                {
                    string nameoftex = _packet.ReadString();
                    string nameofGO = _packet.ReadString();
                    BmxTexinfos.Add(new TextureInfo(nameoftex, nameofGO));
                }
            }

            int bikenormalnamecount = _packet.ReadInt();
            if (bikenormalnamecount > 0)
            {
                for (int i = 0; i < bikenormalnamecount; i++)
                {
                    string nameoftex = _packet.ReadString();
                    string nameofGO = _packet.ReadString();
                    BmxNormalinfos.Add(new TextureInfo(nameoftex, nameofGO));
                }
            }





            Debug.Log("Setting up player");
            List<Vector3> bikecols = new List<Vector3>();
            bikecols.Add(Framecol);
            bikecols.Add(forkcol);
            bikecols.Add(barscol);
            bikecols.Add(seatcol);
            bikecols.Add(ftirecol);
            bikecols.Add(ftiresidecol);
            bikecols.Add(rtirecol);
            bikecols.Add(rtiresidecol);
            bikecols.Add(Stemcol);
            bikecols.Add(FRimcol);
            bikecols.Add(RRimcol);
          
            List<float> bikesmooths = new List<float>();
            bikesmooths.Add(framesmooth);
            bikesmooths.Add(forksmooth);
            bikesmooths.Add(barssmooth);
            bikesmooths.Add(seatsmooth);
            bikesmooths.Add(stemsmooth);
            bikesmooths.Add(FRimsmooth);
            bikesmooths.Add(RRimsmooth);
            List<float> bikemetts = new List<float>();
            bikemetts.Add(Framemett);
            bikemetts.Add(Forksmett);
            bikemetts.Add(barsmett);
            bikemetts.Add(stemmett);
            bikemetts.Add(FRimmett);
            bikemetts.Add(RRimmett);

            GameManager.instance.SpawnRider(playerid, playerusername, CurrentModel,RidermodelBundlename, Riderposition, RiderRotation, BmxTexinfos,bikecols,bikesmooths, bikemetts,currentmap,riderinfos, BmxNormalinfos);

            GameManager.instance._localplayer.ServerActive = true;
           


        }


        
        public static void SetupAllOnlinePlayers(Packet _packet)
        {
            GameManager.instance._localplayer.ServerActive = false;
            bool builderopen = ParkBuilder.instance.openflag;


            if (builderopen)
            {
                ParkBuilder.instance.Player.SetActive(false);
            }

            int amountinbundle = _packet.ReadInt();
            for (int _i = 0; _i < amountinbundle; _i++)
            {
                // optional
                List<TextureInfo> BmxTexinfos = new List<TextureInfo>();
                List<TextureInfo> BmxNormalinfos = new List<TextureInfo>();
                List<TextureInfo> riderinfos = new List<TextureInfo>();

                uint playerid = (uint)_packet.ReadLong();
                string playerusername = _packet.ReadString();


                Vector3 Riderposition = _packet.ReadVector3();
                Vector3 RiderRotation = _packet.ReadVector3();
                string CurrentModel = _packet.ReadString();
                string RidermodelBundlename = _packet.ReadString();
                string currentmap = _packet.ReadString();



                Vector3 Framecol = _packet.ReadVector3();
                Vector3 forkcol = _packet.ReadVector3();
                Vector3 barscol = _packet.ReadVector3();
                Vector3 seatcol = _packet.ReadVector3();
                Vector3 ftirecol = _packet.ReadVector3();
                Vector3 rtirecol = _packet.ReadVector3();
                Vector3 ftiresidecol = _packet.ReadVector3();
                Vector3 rtiresidecol = _packet.ReadVector3();

                Vector3 Stemcol = _packet.ReadVector3();
                Vector3 FRimcol = _packet.ReadVector3();
                Vector3 RRimcol = _packet.ReadVector3();


                float framesmooth = _packet.ReadFloat();
                float forksmooth = _packet.ReadFloat();
                float seatsmooth = _packet.ReadFloat();
                float barssmooth = _packet.ReadFloat();

                float stemsmooth = _packet.ReadFloat();
                float FRimsmooth = _packet.ReadFloat();
                float RRimsmooth = _packet.ReadFloat();



                float Framemett = _packet.ReadFloat();
                float Forksmett = _packet.ReadFloat();
                float barsmett = _packet.ReadFloat();
                float stemmett = _packet.ReadFloat();
                float FRimmett = _packet.ReadFloat();
                float RRimmett = _packet.ReadFloat();

                int Ridertexnamecount = _packet.ReadInt();
                if (Ridertexnamecount > 0)
                {
                    for (int i = 0; i < Ridertexnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        riderinfos.Add(new TextureInfo(nameoftex, nameofGO));
                    }
                }



                int Biketexnamecount = _packet.ReadInt();
                if (Biketexnamecount > 0)
                {
                    for (int i = 0; i < Biketexnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        BmxTexinfos.Add(new TextureInfo(nameoftex, nameofGO));
                    }
                }

                int bikenormalnamecount = _packet.ReadInt();
                if (bikenormalnamecount > 0)
                {
                    for (int i = 0; i < bikenormalnamecount; i++)
                    {
                        string nameoftex = _packet.ReadString();
                        string nameofGO = _packet.ReadString();
                        BmxNormalinfos.Add(new TextureInfo(nameoftex, nameofGO));
                    }
                }





                Debug.Log("Setting up player");
                List<Vector3> bikecols = new List<Vector3>();
                bikecols.Add(Framecol);
                bikecols.Add(forkcol);
                bikecols.Add(barscol);
                bikecols.Add(seatcol);
                bikecols.Add(ftirecol);
                bikecols.Add(ftiresidecol);
                bikecols.Add(rtirecol);
                bikecols.Add(rtiresidecol);
                bikecols.Add(Stemcol);
                bikecols.Add(FRimcol);
                bikecols.Add(RRimcol);

                List<float> bikesmooths = new List<float>();
                bikesmooths.Add(framesmooth);
                bikesmooths.Add(forksmooth);
                bikesmooths.Add(barssmooth);
                bikesmooths.Add(seatsmooth);
                bikesmooths.Add(stemsmooth);
                bikesmooths.Add(FRimsmooth);
                bikesmooths.Add(RRimsmooth);
                List<float> bikemetts = new List<float>();
                bikemetts.Add(Framemett);
                bikemetts.Add(Forksmett);
                bikemetts.Add(barsmett);
                bikemetts.Add(stemmett);
                bikemetts.Add(FRimmett);
                bikemetts.Add(RRimmett);

                try
                {
                GameManager.instance.SpawnRider(playerid, playerusername, CurrentModel, RidermodelBundlename, Riderposition, RiderRotation, BmxTexinfos, bikecols, bikesmooths, bikemetts, currentmap, riderinfos, BmxNormalinfos);

                }
                catch (Exception x )
                {
                    Debug.Log("Spawn error from clienthandle  :  " + x);
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"SpawnRider Issue for {playerusername}", (int)MessageColour.Server, 0));
                   
                }





            }
                GameManager.instance._localplayer.ServerActive = true;
            if (builderopen)
            {
                ParkBuilder.instance.Player.SetActive(true);
            }


        }
        


        public static void RequestforDaryienTexNamesReceive(Packet _packet)
        {
           
            // no need to read packet, the opcode is enough to know the server wants names
           // InGameUI.instance.Messages.Add("Server Requesting Textures");
            ClientSend.SendDaryienTexNames();
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Custom textures detected on daryien, checking server has them..", (int)MessageColour.System, 0));

        }


        public static void RequestForTextures(Packet _packet)
        {
            
                List<string> names = new List<string>();

                int amountmissing = _packet.ReadInt();
                for (int i = 0; i < amountmissing; i++)
                {
                    string n = _packet.ReadString();
                    names.Add(n);
                }

            // let player choose whether to upload or send message to just set to default texture?
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Request for {amountmissing} unknown textures", (int)MessageColour.Server, 0));
           
           // GameManager.instance._localplayer.SendTexturesToServer(names);

            

        }


        public static void RequestForAllParts(Packet _packet)
        {
           
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Sending Playerdata to server", 1, 0));
            GameManager.instance.SendAllParts();
           
        }


        public static void ReceiveTexture(Packet _packet)
        {
            int segmentno = _packet.ReadInt();
            int segmentscount = _packet.ReadInt();
            int bytecount = _packet.ReadInt();
            string name = _packet.ReadString();
            byte[] bytes = _packet.ReadBytes(bytecount);

            GameData.SaveImage(bytes, name, segmentscount, segmentno);
        }


        public static void Disconnectme(Packet _packet)
        {
            string msg = _packet.ReadString();
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(msg + ": disconnecting", 4, 0));
            InGameUI.instance.Disconnect();
            InGameUI.instance.Connected = false;
            InGameUI.instance.Waittoend();
        }


        public static void ReceiveMapname(Packet _packet)
        {
            string Name = _packet.ReadString();
            uint from = (uint)_packet.ReadLong();

            try
            {
                GameManager.Players[from].CurrentMap = Name;
            }
            catch(System.Exception x)
            {
                Debug.Log("Map name error, player didnt exist :  " + x);
            }

        }


        public static void PlayerDisconnected(Packet _packet)
        {
            try
            {
            GameObject todelete = null;
            RemotePlayer toremove = null;

                uint _id = (uint)_packet.ReadLong();
            GameManager.PlayersColours.Remove(_id);
            GameManager.PlayersSmooths.Remove(_id);
            GameManager.PlayersMetals.Remove(_id);
            GameManager.BikeTexinfos.Remove(_id);
            GameManager.RiderTexinfos.Remove(_id);
            GameManager.Bikenormalinfos.Remove(_id);

            try
            {
            if (GameManager.PlayersObjects[_id].Count > 0)
            {
            foreach(NetGameObject n in GameManager.PlayersObjects[_id])
            {
                if(n._Gameobject != null)
                {
                    Destroy(n._Gameobject);
                }
            }

            }
            GameManager.PlayersObjects.Remove(_id);

            }
            catch (Exception x)
            {
                Debug.Log("Player disconnect error  : " + x);
            }

            try
            {
                if (ParkBuilder.instance.ObjectstoSave.Count > 0)
                {
                    foreach (PlacedObject n in ParkBuilder.instance.ObjectstoSave.ToArray())
                    {
                        if (n.OwnerID == _id)
                        {
                            ParkBuilder.instance.ObjectstoSave.Remove(n);
                        }
                    }

                }
                GameManager.PlayersObjects.Remove(_id);

            }
            catch (Exception x)
            {
                Debug.Log("Player disconnect error  : " + x);
            }







            if (InGameUI.instance.IsSpectating)
            {
                foreach(RemotePlayer remo in InGameUI.instance.cycleplayerslist)
                {
                    if(remo.id == _id)
                    {
                        toremove = remo;

                    }
                }

                if (toremove)
                {
                    if(InGameUI.instance.Targetrider == toremove.RiderModel)
                    {
                        InGameUI.instance.SpectateExit();
                    }
                    else
                    {
                    InGameUI.instance.cycleplayerslist.Remove(toremove);
                    }
                }

            }

            // delete rider, bike and then self and remove id from manager
                foreach (RemotePlayer player in GameManager.Players.Values)
                {
                    if (player.id == _id)
                    {
                      player.Audio.ShutdownAllSounds();
                      Destroy(player.RiderModel);
                      Destroy(player.BMX);
                      Destroy(player.Audio);
                      Destroy(player.nameSign);
                    
                    todelete = player.gameObject;
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(player.username + " Left the game", 4, 0));
                    }
                }
                GameManager.Players.Remove(_id);

            if (todelete != null)
            {
                Destroy(todelete);
            }
            else
            {
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Couldnt find last player that left", 4, 0));
                try
                {
                    GameManager.instance.CleanUpOldPlayer(_id);
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }

            }
            catch (Exception x)
            {
                Debug.Log("error with playerdisconnected: " + x);
            }



        }


        








        // user input

        public static void BikeQuickupdate(Packet _packet)
        {
            Debug.Log("Received Quick bike update");

            uint _from =(uint)_packet.ReadLong();

            List<TextureInfo> Biketexnames = new List<TextureInfo>();
            List<TextureInfo> Bikenormalnames = new List<TextureInfo>();
            List<Vector3> Bikecols = new List<Vector3>();
            List<float> Bikesmooths = new List<float>();
            List<float> BikeMetallics = new List<float>();

            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                Bikecols.Add(vec);

            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                Bikesmooths.Add(f);

            }
            int Metallicscount = _packet.ReadInt();
            for (int i = 0; i < Metallicscount; i++)
            {
                float m = _packet.ReadFloat();
                BikeMetallics.Add(m);
            }

            int Biketexcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            for (int i = 0; i < Biketexcount; i++)
            {
                string n = _packet.ReadString();
                string e = _packet.ReadString();
                Biketexnames.Add(new TextureInfo(n, e));
            }



            int bikenormalcount = _packet.ReadInt();
            if (bikenormalcount > 0)
            {

                for (int i = 0; i < bikenormalcount; i++)
                {
                    string Texname = _packet.ReadString();
                    string ParentG_O = _packet.ReadString();
                    Bikenormalnames.Add(new TextureInfo(Texname, ParentG_O));
                }


            }



            GameManager.PlayersColours[_from] = Bikecols;
            GameManager.PlayersSmooths[_from] = Bikesmooths;
            GameManager.PlayersMetals[_from] = BikeMetallics;
            GameManager.BikeTexinfos[_from] = Biketexnames;
            GameManager.Bikenormalinfos[_from] = Bikenormalnames;

            GameManager.Players[_from].UpdateBike();
        

        }


        public static void RiderQuickupdate(Packet _packet)
        {
            Debug.Log("Received Quick rider update");
            List<TextureInfo> infos = new List<TextureInfo>();

            uint _from = (uint)_packet.ReadLong();
            int count = _packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string Texname = _packet.ReadString();
                string ParentG_O = _packet.ReadString();
                if(Texname != "" && Texname != " ")
                infos.Add(new TextureInfo(Texname, ParentG_O));
                Debug.Log(Texname + "  " + ParentG_O);
            }

            try
            {
                
                if(GameManager.Players[_from].CurrentModelName == "Daryien")
                {
                GameManager.RiderTexinfos[_from] = infos;
                GameManager.Players[_from].UpdateDaryien();
                }
                else
                {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"RiderUpdate for custom model!", (int)MessageColour.Server, 1));
                }
            }
            catch(UnityException x)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Couldnt do UpdateTextures for {GameManager.Players[_from].username}", (int)MessageColour.Server, 1));
                Debug.Log("Error updating player tex list :  " + x);
            }

        }



       public static void PlayerPositionReceive(Packet _packet)
        {

            if (GameManager.instance._localplayer.ServerActive)
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
                    int DividePos = 4500;
                    int DivideRot = 80;


                    // positions to be filled
                Vector3[] Positions = new Vector3[32];
                Vector3[] Rotations = new Vector3[32];


                    // Players machine timestamp
                    long _playertime = ClientPacket.ReadLong();
                    // riders root pos and rot
                Positions[0] = ClientPacket.ReadVector3();
                Rotations[0] = ClientPacket.ReadVector3();

                    // rider locals
                for (int i = 1; i < 23; i++)
                {
                    SystemHalf.Half x = ClientPacket.ReadShort();
                    SystemHalf.Half y = ClientPacket.ReadShort();
                    SystemHalf.Half z = ClientPacket.ReadShort();

                        
                        if (SystemHalf.HalfHelper.IsInfinity(x) | SystemHalf.HalfHelper.IsNaN(x))
                        {
                            x = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(y) | SystemHalf.HalfHelper.IsNaN(y))
                        {
                            y = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(z) | SystemHalf.HalfHelper.IsNaN(z))
                        {
                            z = 0;
                        }
                        
                        Positions[i] = new Vector3(SystemHalf.HalfHelper.HalfToSingle(x) / DividePos, SystemHalf.HalfHelper.HalfToSingle(y) / DividePos, SystemHalf.HalfHelper.HalfToSingle(z) / DividePos);
                       
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


                // bike root pos and rot
                Positions[23] = ClientPacket.ReadVector3();
                Rotations[23] = ClientPacket.ReadVector3();

                    // bike locals
                for (int i = 24; i < 32; i++)
                {
                    SystemHalf.Half x = ClientPacket.ReadShort();
                    SystemHalf.Half y = ClientPacket.ReadShort();
                    SystemHalf.Half z = ClientPacket.ReadShort();


                        if (SystemHalf.HalfHelper.IsInfinity(x) | SystemHalf.HalfHelper.IsNaN(x))
                        {
                            x = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(y) | SystemHalf.HalfHelper.IsNaN(y))
                        {
                            y = 0;
                        }
                        if (SystemHalf.HalfHelper.IsInfinity(z) | SystemHalf.HalfHelper.IsNaN(z))
                        {
                            z = 0;
                        }


                        Positions[i] = new Vector3(SystemHalf.HalfHelper.HalfToSingle(x) / DividePos, SystemHalf.HalfHelper.HalfToSingle(y) / DividePos, SystemHalf.HalfHelper.HalfToSingle(z) / DividePos);
                       
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




                try
                {
                        if(GameManager.Players[FromId] != null)
                        {
                        if (GameManager.Players[FromId].MasterActive)
                        {
                            GameManager.Players[FromId].IncomingTransformUpdates.Add(new IncomingTransformUpdate(Positions, Rotations, Ping, ServerTimestamp, _playertime));
                  
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

            if (GameManager.instance._localplayer.ServerActive)
            {
                try
                {
                    uint _from = (uint)_packet.ReadLong();
                    int senderspacketcode = _packet.ReadInt();

                    int count = _packet.ReadInt();
                int code = _packet.ReadInt();

                if(code == 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string nameofriser = _packet.ReadString();
                        int playstate = _packet.ReadInt();
                            // float volume = SystemHalf.HalfHelper.HalfToSingle(_packet.ReadShort() / 1000);
                            //float pitch = SystemHalf.HalfHelper.HalfToSingle(_packet.ReadShort() / 1000);
                            //float Velocity = SystemHalf.HalfHelper.HalfToSingle(_packet.ReadShort() / 1000);
                            float volume = _packet.ReadFloat();
                            float pitch = _packet.ReadFloat();
                            float Velocity = _packet.ReadFloat();

                            AudioStateUpdate update = new AudioStateUpdate(volume, pitch, playstate, nameofriser, Velocity);
                       
                                try
                                {

                                  if(GameManager.Players[_from].Audio != null)
                                  {
                                    GameManager.Players[_from].Audio.IncomingRiserUpdates.Add(update);
                                  }


                                }
                                catch (Exception e) {
                                    Debug.Log("Error in foreach loop in ClientHandle.RecieveAudioForAPlayer :" + e.Message + e.StackTrace);
                                }
                            
                        

                    }

                }

                if(code == 2)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string Pathofsound = _packet.ReadString();
                        float volume = _packet.ReadFloat();


                        AudioStateUpdate update = new AudioStateUpdate(volume, Pathofsound);
                        foreach (RemotePlayer player in GameManager.Players.Values)
                        {
                            if (player.id == _from)
                            {
                                GameManager.Players[_from].Audio.IncomingOneShotUpdates.Add(update);

                            }
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

            uint OwnerID = (uint)_packet.ReadLong();

            NetGameObject OBJ = new NetGameObject(NameofGO, NameofFile, NameofBundle, Rotation, Position, Scale, false, ObjectID, null);
            OBJ.OwnerID = OwnerID;

            try
            {
                GameManager.instance.SpawnObject(OwnerID, OBJ);
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

               NetGameObject todel = null;

               foreach(NetGameObject n in GameManager.PlayersObjects[_ownerID])
               {
                if(n.ObjectID == ObjectId)
                {
                    todel = n;
                }
               }

              if(todel != null)
              {
                Destroy(todel._Gameobject);
                GameManager.PlayersObjects[_ownerID].Remove(todel);
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
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Could find player object to destroy it", (int)MessageColour.Server, 0));
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

            foreach(NetGameObject n in GameManager.PlayersObjects[OwnerID])
            {
                if(n.ObjectID == objectID)
                {
                    GameManager.instance.MoveObject(n, _newpos, _newrot, _newscale);
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

        
    }
}