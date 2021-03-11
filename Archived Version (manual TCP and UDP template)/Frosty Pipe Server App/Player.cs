using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Frosty_Pipe_Server
{
    class Player
    {
        public bool active;
        public int id;
        public string username;
        public Vector3 SpawnPosition;

        // Character model
        public string CurrentRiderModel;
      
        // Movement
        public Vector3[] RiderPositions;
        public Vector3[] RiderRotations;

        // null unless Daryien is selected
        public List<string> NamesofDaryiensTextures;
        // if the server detects it doesnt have these it will request them from client and save locally
        

       


        public Player(int _id, string _username, Vector3 _spawnPosition, string Ridermodel)
        {
            id = _id;
            username = _username;
            SpawnPosition = _spawnPosition;


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


            CurrentRiderModel = Ridermodel;
            active = false;
        }

        /// <summary>If all check are met, Update rider.</summary>
        public void Update()
        {
            if(RiderPositions != null && RiderRotations != null)
            {

            SendTransformInfoToAll();
            }
        }


        public void SendTransformInfoToAll()
        {

            ServerSend.RelayTransformsToAllButSender(id,RiderPositions.Length, RiderPositions, RiderRotations);
        }

       

      
        
    }
}