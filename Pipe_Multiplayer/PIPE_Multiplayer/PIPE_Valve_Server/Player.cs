using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;


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

       

        public Vector3 RiderRootPosition;
        public Vector3 RiderRootRotation;

        /// <summary>
        /// contains everything about bike
        /// </summary>
        
        public GearUpdate Gear;
        public bool ReadytoRoll = false;


        // 5 gets a 10 mins ban
        public int AmountofObjectBoots;
        public List<NetGameObject> PlayerObjects;

        public bool AdminLoggedIn = false;
        public bool AdminStream = false;
        public Stopwatch AdminStreamWatch = new Stopwatch();









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

            PlayerObjects = new List<NetGameObject>();
            Gear = new GearUpdate();
            Gear.RiderTextures = new List<TextureInfo>();
            

        }
    }


    /// <summary>
    /// Used for keeping track of texture name and the gameobject its on for when it reaches remote players
    /// </summary>
    public class TextureInfo
    {
        public string Nameoftexture;
        public string NameofparentGameObject;
        public bool isNormal;
        public int Matnum;

        public TextureInfo(string nameoftex, string nameofG_O, bool isnormal, int matnum)
        {
            Nameoftexture = nameoftex;
            NameofparentGameObject = nameofG_O;
            isNormal = isnormal;
            Matnum = matnum;
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


    public class GearUpdate
    {
        public bool isRiderUpdate;
        public List<TextureInfo> RiderTextures;
        public bool Capforward;
        public byte[] Garagesave;



        /// <summary>
        /// to Send Just Riders gear
        /// </summary>
        /// <param name="ridertextures"></param>
        public GearUpdate(List<TextureInfo> ridertextures)
        {
            isRiderUpdate = true;
            RiderTextures = ridertextures;

        }


        public GearUpdate()
        {
        }

    }



}
