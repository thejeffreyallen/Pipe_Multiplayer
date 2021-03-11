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

            InGameUI.instance.Messages.Add($"Setting up {playerusername} as {CurrentModel}");
            Debug.Log($"Setting up Player model: {CurrentModel} for: {playerusername}");
            GameManager.instance.SpawnOnMyGame(playerid, playerusername, CurrentModel, Riderposition, RiderRotation);
        }


        public static void RequestforDaryienTexNamesReceive(Packet _packet)
        {
            // no need to read packet, the opcode is enough to know the server wants names

            ClientSend.SendDaryienTexNames();
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

            GameManager.instance._localplayer.SendTexturesToServer(names);


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
                        // Debug.Log("Position received:" + Positions[0].ToString());
                    }

                else
                {
                    Debug.Log($"Position received for unready player {FromId}");
                }
                }

            }





            // Debug.Log("Player to Update: " + FromId + ": Player to updates Transform count: " + count);
        }




        public static void PlayerDisconnected(Packet _packet)
        {
            uint _id = (uint)_packet.ReadLong();

            // delete rider, bike and then self and remove id from manager
            Destroy(GameManager.Players[_id].RiderModel);
            Destroy(GameManager.Players[_id].BMX);
            Destroy(GameManager.Players[_id].gameObject);
            GameManager.Players.Remove(_id);
            InGameUI.instance.Messages.Add($"{GameManager.Players[_id].username} Left");
        }
        
    }
}