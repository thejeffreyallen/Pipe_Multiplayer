using System.Collections.Generic;
using UnityEngine;


namespace PIPE_Valve_Console_Client
{
    public class ClientHandle : MonoBehaviour
    {

        public static void Welcome(Packet _packet)
        {
            
            string _msg = _packet.ReadString();




            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(_msg, 4, 0));


            Debug.Log($"Message from server: {_msg}");
           
            ClientSend.WelcomeReceived();

            

           
        }
        


        public static void SetupPlayerReceive(Packet _packet)
        {
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

            GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel,RidermodelBundlename, Riderposition, RiderRotation, BmxTexinfos,bikecols,bikesmooths, bikemetts,currentmap,riderinfos, BmxNormalinfos);


           
           

        }


        
        public static void SetupAllOnlinePlayers(Packet _packet)
        {
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

                GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel, RidermodelBundlename, Riderposition, RiderRotation, BmxTexinfos, bikecols, bikesmooths, bikemetts, currentmap, riderinfos, BmxNormalinfos);





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


        public static void BikeQuickupdate(Packet _packet)
        {
            Debug.Log("Received Quick bike update");

            uint _from =(uint)_packet.ReadLong();

            List<TextureInfo> Texturenames = new List<TextureInfo>();
            List<Vector3> vecs = new List<Vector3>();
            List<float> floats = new List<float>();
            List<float> BikeMetallics = new List<float>();
            int veccount = _packet.ReadInt();
            // read colours
            for (int i = 0; i < veccount; i++)
            {
                Vector3 vec = _packet.ReadVector3();
                vecs.Add(vec);

            }
            int floatcount = _packet.ReadInt();
            // read smoothnesses
            for (int i = 0; i < floatcount; i++)
            {
                float f = _packet.ReadFloat();
                floats.Add(f);

            }
            int Metallicscount = _packet.ReadInt();
            for (int i = 0; i < Metallicscount; i++)
            {
                float m = _packet.ReadFloat();
                BikeMetallics.Add(m);
            }
            int texcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            if (texcount > 0)
            {
            for (int i = 0; i < texcount; i++)
            {
                string n = _packet.ReadString();
                string G_O = _packet.ReadString();
                if (n != "e")
                {
                  
                Texturenames.Add(new TextureInfo(n,G_O));
                }
            }

            }


            GameManager.PlayersColours[_from] = vecs;
            GameManager.PlayersSmooths[_from] = floats;
            GameManager.PlayersMetals[_from] = BikeMetallics;
          
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


        public static void ReceiveTexture(Packet _packet)
        {
            int segmentno = _packet.ReadInt();
            int segmentscount = _packet.ReadInt();
            int bytecount = _packet.ReadInt();
            string name = _packet.ReadString();
            byte[] bytes = _packet.ReadBytes(bytecount);

            GameData.SaveImage(bytes, name, segmentscount, segmentno);
        }


        public static void PlayerPositionReceive(Packet _packet)
        {

           
            uint FromId = (uint)_packet.ReadLong();
            int count = _packet.ReadInt();

            Vector3[] Positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Positions[i] = _packet.ReadVector3();
            }

            Vector3[] Rotations = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Rotations[i] = _packet.ReadVector3();
            }


            foreach (RemotePlayer player in GameManager.Players.Values)
            {

                if (player.id == FromId)
                {

                    if (player.Riders_positions != null)
                    {
                        for (int i = 0; i < Positions.Length; i++)
                        {
                            player.Riders_positions[i] = Positions[i];

                            player.Riders_rotations[i] = Rotations[i];

                        }
                        //player.timeatlasttranformupdate = Time.time;
                    }

                    else
                    {
                        Debug.Log($"Position received for unready player {FromId}");
                    }
                }
            }

           




           
        }


        public static void ReceiveAudioForaPlayer(Packet _packet)
        {
           

                try
                {
                    uint _from = (uint)_packet.ReadLong();


                    int count = _packet.ReadInt();
                int code = _packet.ReadInt();

                if(code == 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string nameofriser = _packet.ReadString();
                        int playstate = _packet.ReadInt();
                        float volume = _packet.ReadFloat();
                        float pitch = _packet.ReadFloat();
                        float Velocity = _packet.ReadFloat();

                        AudioStateUpdate update = new AudioStateUpdate(volume, pitch, playstate, nameofriser, Velocity);
                        foreach (RemotePlayer player in GameManager.Players.Values)
                        {
                            if (player.id == _from)
                            {
                                GameManager.Players[_from].Audio.IncomingRiserUpdates.Add(update);

                            }
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
                    Debug.LogError(x);
                }




            
            




        }
          

        public static void PlayerDisconnected(Packet _packet)
        {
            GameObject todelete = null;

                uint _id = (uint)_packet.ReadLong();
            GameManager.PlayersColours.Remove(_id);
            GameManager.PlayersSmooths.Remove(_id);
            GameManager.BikeTexinfos.Remove(_id);
            GameManager.RiderTexinfos.Remove(_id);

            // delete rider, bike and then self and remove id from manager
            foreach (RemotePlayer player in GameManager.Players.Values)
                {
                    if (player.id == _id)
                    {
                        Destroy(GameManager.Players[_id].RiderModel);
                        Destroy(GameManager.Players[_id].BMX);
                        Destroy(GameManager.Players[_id].Audio);
                        Destroy(GameManager.Players[_id].nameSign);
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
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Couldnt Clean up last player that left", 4, 0));
            }

        }


        public static void IncomingTextMessage(Packet _packet)
        {
            uint _from = (uint)_packet.ReadLong();
            string _message = _packet.ReadString();
            int fromcode = _packet.ReadInt();

            if(fromcode == 3)
            {
            TextMessage tm = new TextMessage(GameManager.Players[_from].username + " : " + _message, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.PlayerMessageTime, tm);
            }
            if(fromcode == 2)
            {
                TextMessage tm = new TextMessage("You : " + _message, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.PlayerMessageTime, tm);
            }
            if (fromcode == 1)
            {
                TextMessage tm = new TextMessage("System : " + _message, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, tm);
            }
            if (fromcode == 4)
            {
                TextMessage tm = new TextMessage("Server : " + _message, fromcode, _from);
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, tm);
            }



            
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
                Debug.Log("Map name error " + x);
            }

        }


        public static void Disconnectme(Packet _packet)
        {
            string msg = _packet.ReadString();
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(msg + ": disconnecting", 4, 0));
            InGameUI.instance.Disconnect();
            InGameUI.instance.Connected = false;
            InGameUI.instance.Waittoend();
        }

        
    }
}