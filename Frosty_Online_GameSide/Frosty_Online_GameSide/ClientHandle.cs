using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Frosty_Online_GameSide
{

    // all functions here are called by an incoming packet whose "OP code" matched that of a dictionary key setup in Client.Initilaise, also see Packet.cs for setup of OP codes
    public class ClientHandle : MonoBehaviour
    {

        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();


            Ingame_UI.instance.lastmsgfromServer = _msg;
            Debug.Log($"Message from server: {_msg}");
            Client.instance.myId = _myId;
            ClientSend.WelcomeReceived();

            // Now that we have the client's id, connect UDP
            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void SetupPlayerReceive(Packet _packet)
        {
           int playerid = _packet.ReadInt();
           string playerusername = _packet.ReadString();

            
           Vector3 Riderposition = _packet.ReadVector3();
            Vector3 RiderRotation = _packet.ReadVector3();
            string CurrentModel = _packet.ReadString();

            Debug.Log($"Setting up Player model: {CurrentModel} for: {playerusername}");
            GameManager.instance.SpawnOnMyGame(playerid,playerusername, CurrentModel,Riderposition, RiderRotation);
        }


        public static void PlayerPositionReceive(Packet _packet)
        {
            int FromId = _packet.ReadInt();
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

          
            foreach(RemotePlayer player in GameManager.players.Values)
            {

           if(player.id == FromId)
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

             }
            else
            {
                Debug.Log($"Position received for unready player {FromId}");
            }

            }


            


           // Debug.Log("Player to Update: " + FromId + ": Player to updates Transform count: " + count);
        }

       


        public static void PlayerDisconnected(Packet _packet)
        {
            int _id = _packet.ReadInt();

            // delete rider, bike and then self and remove id from manager
            Destroy(GameManager.players[_id].RiderModel);
            Destroy(GameManager.players[_id].BMX);
            Destroy(GameManager.players[_id].gameObject);
            GameManager.players.Remove(_id);
        }

        

        


  
    }
}




