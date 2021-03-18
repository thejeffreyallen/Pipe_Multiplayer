using System.Collections.Generic;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
    public class ClientHandle : MonoBehaviour
    {

        public static void Welcome(Packet _packet)
        {
            
            string _msg = _packet.ReadString();



          
            InGameUI.instance.Messages.Add(_msg);
            
            
            Debug.Log($"Message from server: {_msg}");
           
            ClientSend.WelcomeReceived();

            

           
        }
        
        public static void SetupPlayerReceive(Packet _packet)
        {

           
            uint playerid =(uint)_packet.ReadLong();
            string playerusername = _packet.ReadString();


            Vector3 Riderposition = _packet.ReadVector3();
            Vector3 RiderRotation = _packet.ReadVector3();
            string CurrentModel = _packet.ReadString();

            InGameUI.instance.Messages.Add($"{playerusername} is here riding as {CurrentModel}");
            GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel, Riderposition, RiderRotation);

           

        }


        public static void RequestforDaryienTexNamesReceive(Packet _packet)
        {
           
            // no need to read packet, the opcode is enough to know the server wants names
           // InGameUI.instance.Messages.Add("Server Requesting Textures");
          //  ClientSend.SendDaryienTexNames();

           
        }


        public static void RequestForTextures(Packet _packet)
        {
            /*
                List<string> names = new List<string>();

                int amountmissing = _packet.ReadInt();
                for (int i = 0; i < amountmissing; i++)
                {
                    string n = _packet.ReadString();
                    names.Add(n);
                }
            */
                // let player choose whether to upload or send message to just set to default texture?

                //GameManager.instance._localplayer.SendTexturesToServer(names);

            

        }


        public static void ReceiveTexture(Packet _packet)
        {

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
                                GameManager.Players[_from].Audio.IncomingStateUpdates.Add(update);

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
            

                uint _id = (uint)_packet.ReadLong();

                // delete rider, bike and then self and remove id from manager
                foreach (RemotePlayer player in GameManager.Players.Values)
                {
                    if (player.id == _id)
                    {
                        Destroy(GameManager.Players[_id].RiderModel);
                        Destroy(GameManager.Players[_id].BMX);
                        InGameUI.instance.Messages.Add($"{GameManager.Players[_id].username} Left");
                    }
                }
                GameManager.Players.Remove(_id);
                Destroy(GameManager.Players[_id].gameObject);

        }


        public static void IncomingTextMessage(Packet _packet)
        {
            // handle player and server messages differently

        }
        
    }
}