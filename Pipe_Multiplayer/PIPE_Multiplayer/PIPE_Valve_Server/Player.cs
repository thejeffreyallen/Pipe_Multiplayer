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
        public uint RiderID;
        public string Ridermodel;
        public string Ridermodelbundlename;
        public string MapName;

       

        public Vector3[] RiderPositions;
        public Vector3[] RiderRotations;

        /// <summary>
        /// contains everything about bike
        /// </summary>
        public BMXLoadout Loadout;

        

        public bool Gottexnames;
        
        public bool ReadytoRoll = false;


        // 5 gets a 10 mins ban
        public int AmountofObjectBoots;
        public List<NetGameObject> PlayerObjects;


        /// <summary>
        /// Constructor to initiailise with Connection id and rider info, also initialises vectors for storage
        /// </summary>
        public Player(uint connofPlayer, string _riderModel, string _username, string _ridermodelbundlename, string currentlevel)
        {
            RiderID = connofPlayer;
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
            PlayerObjects = new List<NetGameObject>();
            Loadout = new BMXLoadout();
            

        }


      

      

    }


    /// <summary>
    /// Used for keeping track of texture name and the gameobject its on for when it reaches remote players
    /// </summary>
    public class PlayerTextureInfo
    {
        public string Nameoftexture;
        public string NameofparentGameObject;


        public PlayerTextureInfo(string nameoftex, string nameofG_O)
        {
            Nameoftexture = nameoftex;
            NameofparentGameObject = nameofG_O;
        }


    }



    public class NetGameObject
    {
        public string NameofObject;
        public string NameofAssetBundle;
        public string NameOfFile;
        public Vector3 Rotation;
        public Vector3 Position;
        public Vector3 Scale;
        public bool IsPhysics;
        public int ObjectID;
        public List<uint> Votestoremove = new List<uint>();

        public NetGameObject(string _nameofobject, string _nameoffile, string _nameofassetbundle, Vector3 _rotation, Vector3 _position, Vector3 _scale, bool _IsPhysicsenabled, int Objectid)
        {
            NameofObject = _nameofobject;
            NameOfFile = _nameoffile;
            NameofAssetBundle = _nameofassetbundle;
            Rotation = _rotation;
            Position = _position;
            Scale = _scale;
            IsPhysics = _IsPhysicsenabled;
            ObjectID = Objectid;
        }


    }





}
