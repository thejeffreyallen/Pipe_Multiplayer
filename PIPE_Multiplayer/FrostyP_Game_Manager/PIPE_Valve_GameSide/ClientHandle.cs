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
            List<TextureInfo> infos = new List<TextureInfo>();
           
            uint playerid =(uint)_packet.ReadLong();
            string playerusername = _packet.ReadString();


            Vector3 Riderposition = _packet.ReadVector3();
            Vector3 RiderRotation = _packet.ReadVector3();
            string CurrentModel = _packet.ReadString();
            string RidermodelBundlename = _packet.ReadString();
            string currentmap = _packet.ReadString();

            if (CurrentModel == "Daryien")
            {
                infos = new List<TextureInfo>();
                int count = _packet.ReadInt();
                if (count > 0)
                {
                for (int i = 0; i < count; i++)
                {
                    string name = _packet.ReadString();
                   string nameofobj = _packet.ReadString();
                        if(name != "")
                        {
                    TextureInfo t = new TextureInfo(name, nameofobj);
                    infos.Add(t);

                        }

                }

                }
            }


            Vector3 Framecol = _packet.ReadVector3();
            Vector3 forkcol =_packet.ReadVector3();
            Vector3 barscol = _packet.ReadVector3();
            Vector3 seatcol = _packet.ReadVector3();
            Vector3 ftirecol = _packet.ReadVector3();
            Vector3 rtirecol =  _packet.ReadVector3();
            Vector3 ftiresidecol = _packet.ReadVector3();
            Vector3 rtiresidecol = _packet.ReadVector3();

            float framesmooth = _packet.ReadFloat();
            float forksmooth = _packet.ReadFloat();
           float seatsmooth = _packet.ReadFloat();
           float barssmooth = _packet.ReadFloat();

           string frametexname = _packet.ReadString();
           string forktexname = _packet.ReadString();
           string Bartexname = _packet.ReadString();
           string Seattexname = _packet.ReadString();
           string Tiretexname = _packet.ReadString();
           string Tirenormalname = _packet.ReadString();

           
                TextureInfo Frame = new TextureInfo(frametexname, "Frame Mesh");
                infos.Add(Frame);
            TextureInfo fork = new TextureInfo(forktexname, "Forks Mesh");
            infos.Add(fork);
            TextureInfo Bar = new TextureInfo(Bartexname, "Bars Mesh");
            infos.Add(Bar);
            TextureInfo seat = new TextureInfo(Seattexname, "Seat Mesh");
            infos.Add(seat);
            TextureInfo tire = new TextureInfo(Tiretexname, "Tire Mesh");
            infos.Add(tire);
            TextureInfo tirenormal = new TextureInfo(Tirenormalname, "Tire Mesh");
            infos.Add(tirenormal);

            Debug.Log("Setting up player");
            List<Vector3> vecs = new List<Vector3>();
            vecs.Add(Framecol);
            vecs.Add(forkcol);
            vecs.Add(barscol);
            vecs.Add(seatcol);
            vecs.Add(ftirecol);
            vecs.Add(ftiresidecol);
            vecs.Add(rtirecol);
            vecs.Add(rtiresidecol);
            GameManager.PlayersColours.Add(playerid, vecs);
            List<float> floats = new List<float>();
            floats.Add(framesmooth);
            floats.Add(forksmooth);
            floats.Add(barssmooth);
            floats.Add(seatsmooth);
            GameManager.PlayersSmooths.Add(playerid, floats);
            GameManager.PlayersTexinfos.Add(playerid, infos);
            
            GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel,RidermodelBundlename, Riderposition, RiderRotation, infos,vecs,floats,currentmap);

           
              
            

            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{playerusername} is riding", 4, 0));
           

        }



        public static void SetupAllOnlinePlayers(Packet _packet)
        {
            int amountinbundle = _packet.ReadInt();
            for (int _i = 0; _i < amountinbundle; _i++)
            {
                List<TextureInfo> infos = new List<TextureInfo>();

                uint playerid = (uint)_packet.ReadLong();
                string playerusername = _packet.ReadString();


                Vector3 Riderposition = _packet.ReadVector3();
                Vector3 RiderRotation = _packet.ReadVector3();
                string CurrentModel = _packet.ReadString();
                string RidermodelBundlename = _packet.ReadString();
                string CurrentMap = _packet.ReadString();

                if (CurrentModel == "Daryien")
                {
                    infos = new List<TextureInfo>();
                    int count = _packet.ReadInt();
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            string name = _packet.ReadString();
                            string nameofobj = _packet.ReadString();
                            if (name != "")
                            {
                                TextureInfo t = new TextureInfo(name, nameofobj);
                                infos.Add(t);

                            }

                        }

                    }
                }


                Vector3 Framecol = _packet.ReadVector3();
                Vector3 forkcol = _packet.ReadVector3();
                Vector3 barscol = _packet.ReadVector3();
                Vector3 seatcol = _packet.ReadVector3();
                Vector3 ftirecol = _packet.ReadVector3();
                Vector3 rtirecol = _packet.ReadVector3();
                Vector3 ftiresidecol = _packet.ReadVector3();
                Vector3 rtiresidecol = _packet.ReadVector3();

                float framesmooth = _packet.ReadFloat();
                float forksmooth = _packet.ReadFloat();
                float seatsmooth = _packet.ReadFloat();
                float barssmooth = _packet.ReadFloat();

                string frametexname = _packet.ReadString();
                string forktexname = _packet.ReadString();
                string Bartexname = _packet.ReadString();
                string Seattexname = _packet.ReadString();
                string Tiretexname = _packet.ReadString();
                string Tirenormalname = _packet.ReadString();


                TextureInfo Frame = new TextureInfo(frametexname, "Frame Mesh");
                infos.Add(Frame);
                TextureInfo fork = new TextureInfo(forktexname, "Forks Mesh");
                infos.Add(fork);
                TextureInfo Bar = new TextureInfo(Bartexname, "Bars Mesh");
                infos.Add(Bar);
                TextureInfo seat = new TextureInfo(Seattexname, "Seat Mesh");
                infos.Add(seat);
                TextureInfo tire = new TextureInfo(Tiretexname, "Tire Mesh");
                infos.Add(tire);
                TextureInfo tirenormal = new TextureInfo(Tirenormalname, "Tire Mesh");
                infos.Add(tirenormal);

                Debug.Log("Setting up player");
                List<Vector3> vecs = new List<Vector3>();
                vecs.Add(Framecol);
                vecs.Add(forkcol);
                vecs.Add(barscol);
                vecs.Add(seatcol);
                vecs.Add(ftirecol);
                vecs.Add(ftiresidecol);
                vecs.Add(rtirecol);
                vecs.Add(rtiresidecol);
                GameManager.PlayersColours.Add(playerid, vecs);
                List<float> floats = new List<float>();
                floats.Add(framesmooth);
                floats.Add(forksmooth);
                floats.Add(barssmooth);
                floats.Add(seatsmooth);
                GameManager.PlayersSmooths.Add(playerid, floats);
                GameManager.PlayersTexinfos.Add(playerid, infos);

                GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel, RidermodelBundlename, Riderposition, RiderRotation, infos, vecs, floats, CurrentMap);





                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{playerusername} is riding on {CurrentMap} level", 4, 0));


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


        public static void RequestforMyBike(Packet _packet)
        {
            GameManager.instance.SendMyBikeToServer();
        }


        public static void BikeQuickupdate(Packet _packet)
        {
            Debug.Log("Received Quick bike update");

            uint _from =(uint)_packet.ReadLong();

            List<TextureInfo> Texturenames = new List<TextureInfo>();
            List<Vector3> vecs = new List<Vector3>();
            List<float> floats = new List<float>();

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
            int texcount = _packet.ReadInt();
            // read texture names, empty if tex is null
            for (int i = 0; i < texcount; i++)
            {
                string n = _packet.ReadString();
                string G_O = _packet.ReadString();
                if (n != "e")
                {
                    // construct textureinfo !!!!
                Texturenames.Add(new TextureInfo(n,G_O));
                }
            }


            GameManager.PlayersColours[_from] = vecs;
            GameManager.PlayersSmooths[_from] = floats;
            GameManager.PlayersTexinfos[_from] = Texturenames;
            GameManager.Players[_from].UpdateColours();
            GameManager.Players[_from].UpdateTextures();

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
                infos.Add(new TextureInfo(Texname, ParentG_O));
                Debug.Log(Texname + "  " + ParentG_O);
            }

            try
            {
            GameManager.PlayersTexinfos[_from] = infos;
                GameManager.Players[_from].UpdateTextures();
            }
            catch(UnityException x)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Couldnt do UpdateTextures for {GameManager.Players[_from].username}", (int)MessageColour.Server, 1));
                Debug.Log("Error updating player tex list");
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
                        // Debug.Log("Position received:" + Positions[0].ToString());
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
            GameManager.PlayersTexinfos.Remove(_id);

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
                Debug.Log("Map name error");
            }

        }

        
    }
}