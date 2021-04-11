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
        public string Ridermodelbundlename;
        public string MapName;

       

        public Vector3[] RiderPositions;
        public Vector3[] RiderRotations;

        /// <summary>
        /// contains everything about bike
        /// </summary>
        public BMXLoadout Loadout;

        // audio
        /// <summary>
        /// updated here every time an update in received
        /// </summary>
        public byte[] LastAudioUpdate;
        public bool newAudioReceived;

        public bool Gottexnames;
        public bool GotBikeData;
        public bool Ready;







        /// <summary>
        /// Constructor to initiailise with Connection id and rider info, also initialises vectors for storage
        /// </summary>
        public Player(uint connofPlayer, string _riderModel, string _username, string _ridermodelbundlename, string currentlevel)
        {
            clientID = connofPlayer;
            Username = _username;
            Ridermodel = _riderModel;
            Ridermodelbundlename = _ridermodelbundlename;
            MapName = currentlevel;

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

            Loadout = new BMXLoadout();
            Loadout.Setup();

        }


        // called on tick rate
        public void Update()
        {
            if (Ready)
            {
                //SendTransformInfoToAll();
                
            }

           if(Gottexnames)
            {
                Ready = true;
            }

            

        }

        public void SendTransformInfoToAll()
        {

           // ServerSend.SendATransformUpdate(clientID, RiderPositions.Length, RiderPositions, RiderRotations);
        }


    }
    /// <summary>
    /// Used for keeping track of texture name and the gameobject its on for when it reaches remote players
    /// </summary>
    public class TextureInfo
    {
        public string Nameoftexture;
        public string NameofparentGameObject;


        public TextureInfo(string nameoftex, string nameofG_O)
        {
            Nameoftexture = nameoftex;
            NameofparentGameObject = nameofG_O;
        }


    }
}
