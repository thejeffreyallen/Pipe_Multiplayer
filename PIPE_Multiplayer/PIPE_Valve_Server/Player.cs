using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;


namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Created for Each Live Connection to hold Player data on Server
    /// </summary>
    class Player
    {
        public string Username;
        public uint clientID;
        public string Ridermodel;
        public string MapName;

        public List<string> RidersTexturenames;

        public Vector3[] RiderPositions;
        public Vector3[] RiderRotations;


        // audio
        public byte[] LastAudioUpdate;









        /// <summary>
        /// Constructor to initiailise with Connection id and rider info, also initialises vectors for storage
        /// </summary>
        public Player(uint connofPlayer, string _riderModel, string _username)
        {
            clientID = connofPlayer;
            Username = _username;
            Ridermodel = _riderModel;

            RiderRotations = new Vector3[32];
            RiderPositions = new Vector3[32];

            for (int i = 0; i < RiderPositions.Length; i++)
            {
                RiderPositions[i].X = 0;
                RiderPositions[i].Y = 0;
                RiderPositions[i].Z = 0;
            }
            for (int i = 0; i < RiderRotations.Length; i++)
            {
                RiderRotations[i].X = 0;
                RiderRotations[i].Y = 0;
                RiderRotations[i].Z = 0;
            }

        }


        // called on tick rate
        public void Update()
        {
            if (RiderPositions != null && RiderRotations != null && LastAudioUpdate != null && LastAudioUpdate.Length > 0)
            {
                SendTransformInfoToAll();
                ServerSend.SendAudioToAllPlayers(clientID, LastAudioUpdate);
            }

            

        }

        public void SendTransformInfoToAll()
        {

            ServerSend.SendATransformUpdate(clientID, RiderPositions.Length, RiderPositions, RiderRotations);
        }


    }
}
