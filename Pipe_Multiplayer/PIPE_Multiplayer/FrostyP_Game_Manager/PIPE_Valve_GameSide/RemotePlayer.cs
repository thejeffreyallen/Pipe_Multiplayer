﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Master Script for a Remote Player, when gamemanager spawns a new playerprefab, it has a remoteplayer attached with info populated by info from server
    /// </summary>
    public class RemotePlayer : MonoBehaviour
    {
      
        public bool MasterActive = false;
        public uint id;
        public string username;
        public string CurrentModelName;
        public string Modelbundlename;
        public string CurrentMap = "Unknown";


        public GameObject RiderModel;
        public GameObject BMX;
        public Transform[] Riders_Transforms;
        private Rigidbody Rider_RB;
        private Rigidbody BMX_RB;
        private GameObject[] wheelcolliders;
        public RemotePlayerAudio Audio;
        public GearUpdate Gear;
        public List<FrostyP_Game_Manager.NetGameObject> Objects = new List<FrostyP_Game_Manager.NetGameObject>();


        public GameObject nameSign;
		public TextMesh tm;


        public List<IncomingTransformUpdate> IncomingTransformUpdates;
        public List<IncomingTransformUpdate> ReplayPostions;
       
        public GUIStyle style = new GUIStyle();
       
        private bool SetupSuccess;


        public float PlayersFrameRate;
        private float _playerframerate;
        public float R2RPing;
        public float LastPing;


        public bool PlayerTagVisible = true;
        public bool PlayerCollides = true;
        public bool PlayerObjectsVisible = true;
        public bool PlayerIsVisible = true;

        public RemotePartMaster partMaster;
        public RemoteBrakesManager brakesManager;


        void Awake()
        {
            // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[34];
           
           
            IncomingTransformUpdates = new List<IncomingTransformUpdate>();
            ReplayPostions = new List<IncomingTransformUpdate>();
           
        }


        // Call initiation once on start, inititation to reoccur until resolved
        private void Start()
        {
            
            try
            {
            Initialize();
           
            nameSign = new GameObject("player_label");
            DontDestroyOnLoad(nameSign);
            nameSign.transform.position = RiderModel.transform.position + Vector3.up * 1.8f;
            nameSign.transform.parent = RiderModel.transform;
            tm = nameSign.AddComponent<TextMesh>();

            tm.color = new Color(UnityEngine.Random.Range(0.01f, 0.99f), UnityEngine.Random.Range(0.01f, 0.99f), UnityEngine.Random.Range(0.01f, 0.99f));
            tm.fontStyle = FontStyle.Bold;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.1f;
            tm.fontSize = 10;
            tm.text = username;

                style.normal.textColor = tm.color;
                style.alignment = TextAnchor.MiddleRight;
                style.padding = new RectOffset(10, 10, 2, 2);
                style.fontStyle = FontStyle.Bold;
                style.hover.background = InGameUI.instance.GreyTex;


            }
            catch (Exception x)
            {

            }

            
        }



        public void Initialize()
        {
            // this player has just been added to server, when this pc got the message to create the prefab and attach this class, it assigned the netid of this player, username, currentmodelname and inital pos and rot
            Debug.Log($"Remote Rider { username } Initialising.....");

            try
            {
                // decifer the rider and bmx specifics needed especially for daryien and bike colours

                RiderModel = DecideRider(CurrentModelName);
                RiderModel.name = (username + " " + id).ToString();
                // Add Audio Component
                Audio = gameObject.AddComponent<RemotePlayerAudio>();
                Audio.Rider = RiderModel;
                Audio.player = this;

                BMX = GameManager.GetNewBMX();
                BMX.name = "BMX " + id;
           
                SetupSuccess = RiderSetup();

                if (!SetupSuccess)
                {
                Debug.Log("Error with Ridersetup()");
                }


                partMaster = gameObject.GetComponent<RemotePartMaster>(); // initialize the part master part list.
                brakesManager = gameObject.GetComponent<RemoteBrakesManager>();
                StartCoroutine(Initialiseafterwait());
            }

            catch (Exception x)
            {
                Debug.Log($"RemotePlayer.Initialize error: {x}");
            }


        }


        private void LateUpdate()
        {
            if (MasterActive)
            {
                try
                {

                  if (nameSign != null && RiderModel != null)
                  {
			       nameSign.transform.rotation = Camera.current.transform.rotation;
                  }

                  if (!SetupSuccess)
                  {
                   SetupSuccess = RiderSetup();
                  }

                  // keeps players up to date if stall occurs for level change, player spawn etc, list typically runs with 1-2 new positions at any one time
                  if (IncomingTransformUpdates.Count > 60)
                  {
                    IncomingTransformUpdates.Clear();
                  }

                  if (CheckThresholds())
                  {     
                   IncomingTransformUpdates.RemoveAt(0);
                  }

           
                  InterpolateRider();
            
            
            
                  if(_playerframerate > 0 && _playerframerate < 120)
                  {
                   PlayersFrameRate = _playerframerate;
                  }

                }
                catch (Exception x )
                {
                    Debug.Log($"Rider LateUpdate Error" + x);
                }
            }

        }


        bool CheckThresholds()
        {
            bool value = false;
            if (IncomingTransformUpdates.Count > 0)
            {
            value = Vector3.Distance(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0]) < 0.01f;
            }
            return value;
        }


        public GameObject DecideRider(string modelname)
        {
           
            if (modelname == "Daryien")
            {
                
                return GameManager.GetNewDaryien();
            }
            else
            {

                return GameManager.GetPlayerModel(modelname,Modelbundlename);
            }



        }



        private bool RiderSetup()
        {

            try
            {

                Riders_Transforms[0] = RiderModel.transform;
                Riders_Transforms[1] = RiderModel.transform.FindDeepChild("mixamorig:LeftUpLeg");
                Riders_Transforms[2] = RiderModel.transform.FindDeepChild("mixamorig:RightUpLeg");
                Riders_Transforms[3] = RiderModel.transform.FindDeepChild("mixamorig:LeftLeg");
                Riders_Transforms[4] = RiderModel.transform.FindDeepChild("mixamorig:RightLeg");
                Riders_Transforms[5] = RiderModel.transform.FindDeepChild("mixamorig:LeftFoot");
                Riders_Transforms[6] = RiderModel.transform.FindDeepChild("mixamorig:RightFoot");
                Riders_Transforms[7] = RiderModel.transform.FindDeepChild("mixamorig:Spine");
                Riders_Transforms[8] = RiderModel.transform.FindDeepChild("mixamorig:Spine1");
                Riders_Transforms[9] = RiderModel.transform.FindDeepChild("mixamorig:Spine2");
                Riders_Transforms[10] = RiderModel.transform.FindDeepChild("mixamorig:LeftShoulder");
                Riders_Transforms[11] = RiderModel.transform.FindDeepChild("mixamorig:RightShoulder");
                Riders_Transforms[12] = RiderModel.transform.FindDeepChild("mixamorig:LeftArm");
                Riders_Transforms[13] = RiderModel.transform.FindDeepChild("mixamorig:RightArm");
                Riders_Transforms[14] = RiderModel.transform.FindDeepChild("mixamorig:LeftForeArm");
                Riders_Transforms[15] = RiderModel.transform.FindDeepChild("mixamorig:RightForeArm");
                Riders_Transforms[16] = RiderModel.transform.FindDeepChild("mixamorig:LeftHand");
                Riders_Transforms[17] = RiderModel.transform.FindDeepChild("mixamorig:RightHand");
                Riders_Transforms[18] = RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex1");
                Riders_Transforms[19] = RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex1");

                Riders_Transforms[20] = RiderModel.transform.FindDeepChild("mixamorig:Hips");
                Riders_Transforms[21] = RiderModel.transform.FindDeepChild("mixamorig:Neck");
                Riders_Transforms[22] = RiderModel.transform.FindDeepChild("mixamorig:Head");


                Riders_Transforms[23] = BMX.transform;
                Riders_Transforms[24] = BMX.transform.FindDeepChild("BMX:Bike_Joint");
                Riders_Transforms[25] = BMX.transform.FindDeepChild("BMX:Bars_Joint");
                Riders_Transforms[26] = BMX.transform.FindDeepChild("BMX:DriveTrain_Joint");
                Riders_Transforms[27] = BMX.transform.FindDeepChild("BMX:Frame_Joint");
                Riders_Transforms[28] = BMX.transform.FindDeepChild("BMX:Wheel");
                Riders_Transforms[29] = BMX.transform.FindDeepChild("BMX:Wheel 1");
                Riders_Transforms[30] = BMX.transform.FindDeepChild("BMX:LeftPedal_Joint");
                Riders_Transforms[31] = BMX.transform.FindDeepChild("BMX:RightPedal_Joint");

                Riders_Transforms[32] = RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex2");
                Riders_Transforms[33] = RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex2");








                // Collision setup, Add Rigidbodies last as they require colliders.
                // All remote rider rigidboides are kinematic so they should plow down any local rider they touch, but the remote rider is being updated by its local counterpart, and on that machine its them that has the live rigidbody and they too have hit a kinematic remote rider. 
                // Result: if your still and your RB is asleep your a brick wall, if your moving, both riders take impact(lag dependant)

                if (wheelcolliders == null)
                {
                    wheelcolliders = new GameObject[2];
                }
                wheelcolliders[0] = GameObject.Instantiate(GameManager.instance.wheelcolliderobj);
                wheelcolliders[1] = GameObject.Instantiate(GameManager.instance.wheelcolliderobj);


                //back wheel
                wheelcolliders[0].transform.position = Riders_Transforms[29].position;
                wheelcolliders[0].transform.parent = Riders_Transforms[29];


                // front wheel
                wheelcolliders[1].transform.position = Riders_Transforms[28].position;
                wheelcolliders[1].transform.parent = Riders_Transforms[28];

                foreach (Rigidbody r in BMX.GetComponentsInChildren<Rigidbody>())
                {
                    r.isKinematic = true;
                }

                if (!RiderModel.GetComponent<Rigidbody>())
                {
                Rider_RB = RiderModel.AddComponent<Rigidbody>();
                }
                Rider_RB.isKinematic = true;

                SphereCollider spherehead = RiderModel.transform.FindDeepChild("mixamorig:Head").gameObject.AddComponent<SphereCollider>();
                spherehead.radius = 0.012f;
                spherehead.center = new Vector3(0, 0.1f, 0.03f);
                spherehead.transform.localPosition = new Vector3(0, 0.1028246f, 0.0511784f);

                if (!BMX.GetComponent<Rigidbody>())
                {
                BMX_RB = BMX.AddComponent<Rigidbody>();

                }
                BMX_RB.isKinematic = true;









               
                Debug.Log("All Remote Rider Parts Assigned");
                return true;
            }
            catch (UnityException x)
            {

                Debug.Log("FindRidersParts() fail: " + x);
                return false;
            }


        }


        public void InterpolateRider()
        {
            
            try
            {

                if (IncomingTransformUpdates.Count > 1)
                {


                    DateTime ServerCurrent = DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].ServerTimeStamp);
                    DateTime PlayertimeCurrent = DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].Playertimestamp);

                    DateTime ServerTarget = DateTime.FromFileTimeUtc(IncomingTransformUpdates[1].ServerTimeStamp);
                    DateTime PlayertimeTarget = DateTime.FromFileTimeUtc(IncomingTransformUpdates[1].Playertimestamp);

                    int PingCurrent = IncomingTransformUpdates[0].Ping;
                    int PingTarget = IncomingTransformUpdates[1].Ping;

                      R2RPing = IncomingTransformUpdates[IncomingTransformUpdates.Count-1].Ping + InGameUI.instance.Ping;
                    _playerframerate = (1 / (float)(DateTime.FromFileTimeUtc(IncomingTransformUpdates[IncomingTransformUpdates.Count - 1].Playertimestamp).Subtract(TimeSpan.FromMilliseconds(IncomingTransformUpdates[IncomingTransformUpdates.Count - 1].Ping)) - DateTime.FromFileTimeUtc(IncomingTransformUpdates[IncomingTransformUpdates.Count - 2].Playertimestamp).Subtract(TimeSpan.FromMilliseconds(IncomingTransformUpdates[IncomingTransformUpdates.Count - 2].Ping))).TotalSeconds);

                    float timespan = (float)(PlayertimeTarget - PlayertimeCurrent).TotalSeconds;
                    float PlayerPingDifference = PingTarget - PingCurrent;
                    float MyPingDifference = InGameUI.instance.Ping - LastPing;


                    if (PlayerPingDifference > 0 && PlayerPingDifference < 50)
                    {
                        timespan = timespan + (PlayerPingDifference / 1000);
                    }
                    if(MyPingDifference >0 && MyPingDifference < 50)
                    {
                        timespan = timespan + (MyPingDifference / 1000);
                    }



                    // rider
                    Riders_Transforms[0].position = Vector3.MoveTowards(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0], (float)(Vector3.Distance(Riders_Transforms[0].position, IncomingTransformUpdates[1].Positions[0]) / timespan / (1 / Time.deltaTime)));
                    Riders_Transforms[0].rotation = Quaternion.RotateTowards(Riders_Transforms[0].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[0]), Quaternion.Angle(Riders_Transforms[0].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[0])) / timespan / (1 / Time.deltaTime));
                    // rider locals
                   for (int i = 1; i < 23; i++)
                   { 
                     Riders_Transforms[i].localPosition = Vector3.MoveTowards(Riders_Transforms[i].localPosition, IncomingTransformUpdates[0].Positions[i], (float)(Vector3.Distance(Riders_Transforms[i].localPosition, IncomingTransformUpdates[1].Positions[i]) / timespan / (1 / Time.deltaTime)));
                     Riders_Transforms[i].localRotation = Quaternion.RotateTowards(Riders_Transforms[i].localRotation,Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i]),Quaternion.Angle(Riders_Transforms[i].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i])) /timespan/(1/Time.deltaTime));
                   }



                       // Bmx
                        Riders_Transforms[23].position = Vector3.MoveTowards(Riders_Transforms[23].position, IncomingTransformUpdates[0].Positions[23], (float)(Vector3.Distance(Riders_Transforms[23].position, IncomingTransformUpdates[1].Positions[23]) / timespan / (1 / Time.deltaTime)));
                        Riders_Transforms[23].rotation = Quaternion.RotateTowards(Riders_Transforms[23].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[23]),Quaternion.Angle(Riders_Transforms[23].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[23])) / timespan / (1 / Time.deltaTime));
                    // bike joint
                    Riders_Transforms[24].position = Vector3.MoveTowards(Riders_Transforms[24].position, IncomingTransformUpdates[0].Positions[24], (float)(Vector3.Distance(Riders_Transforms[24].position, IncomingTransformUpdates[1].Positions[24]) / timespan / (1 / Time.deltaTime)));
                    Riders_Transforms[24].rotation = Quaternion.RotateTowards(Riders_Transforms[24].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[24]), Quaternion.Angle(Riders_Transforms[24].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[24])) / timespan / (1 / Time.deltaTime));
                    // bmx locals
                        for (int i = 25; i < 32; i++)
                        {   
                        Riders_Transforms[i].localRotation = Quaternion.RotateTowards(Riders_Transforms[i].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i]), Quaternion.Angle(Riders_Transforms[i].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i])) / timespan / (1 / Time.deltaTime));
                        }


                        // fingers
                        Riders_Transforms[32].localRotation = Quaternion.RotateTowards(Riders_Transforms[32].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[32]), Quaternion.Angle(Riders_Transforms[32].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[32])) / timespan / (1 / Time.deltaTime));
                        Riders_Transforms[33].localRotation = Quaternion.RotateTowards(Riders_Transforms[33].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[33]), Quaternion.Angle(Riders_Transforms[33].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[33])) / timespan / (1 / Time.deltaTime));




                        LastPing = InGameUI.instance.Ping;
                }


            }
            catch (System.Exception x)
            {
                Debug.Log("MoveRider Error   : " + x);
            }


            
          
        }


        public void ChangeCollideStatus(bool value)
        {
            PlayerCollides = value;
            if (value)
            {
                this.gameObject.layer = 25;

                foreach(Transform t in RiderModel.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = 25;
                }
                RiderModel.layer = 25;

                foreach (Transform t in BMX.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = 25;
                }
                BMX.layer = 25;
            }
            if (!value)
            {
                this.gameObject.layer = 1;

                foreach (Transform t in RiderModel.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = 1;
                }
                RiderModel.layer = 1;

                foreach (Transform t in BMX.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = 1;
                }
                BMX.layer = 1;
            }

        }

        public void ChangeObjectsVisible(bool value)
        {
            PlayerObjectsVisible = value;
            
                foreach(FrostyP_Game_Manager.NetGameObject n in Objects)
                {
                    if(n._Gameobject != null)
                    {

                    if(n._Gameobject.activeInHierarchy != value)
                    {
                        n._Gameobject.SetActive(value);
                    }

                    }

                    if(n._Gameobject == null && value)
                    {
                    GameManager.instance.GetObject(n);
                    }

                }
            



        }

        public void ChangePlayerTagVisible(bool value)
        {
            PlayerTagVisible = value;
           
            tm.gameObject.SetActive(value);
            
        }
       
        public void ChangePlayerVisibilty(bool value)
        {
            RiderModel.SetActive(value);
            BMX.SetActive(value);
            ChangePlayerTagVisible(value);
            ChangeObjectsVisible(value);
            PlayerIsVisible = value;
        }
        
        public void UpdateDaryien()
        {
            
            // look through rider
            if (Gear.RiderTextures.Count > 0)
            {
                Debug.Log($"Updating Textures for {username}");
                //InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updating {username}'s Rider..", 1, 1));


                try
                {
                    
                   foreach (TextureInfo t in Gear.RiderTextures)
                   {
                       
                        if (FileSyncing.CheckForFile(t.Nameoftexture))
                        {
                           
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].mainTexture = GameManager.GetTexture(t.Nameoftexture);
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].color = Color.white;
                        }
                        else
                        {
                            FileSyncing.AddToRequestable(1, t.Nameoftexture, id);
                        }

                   }
            
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }

                Debug.Log($"textures updated for {username}");

            }

           
        }

        public void UpdateBMX()
        {
           
            try
            {
            // do garage setup
            GameManager.DoGarageSetup(this, Gear.GarageSave);

            }
            catch (Exception x )
            {

                Debug.Log("dogaragesetup error :   " + x.Message);
            }

        }

        

      


        /// <summary>
        /// Give everything a moment before trying to go live and update
        /// </summary>
        /// <returns></returns>
        IEnumerator Initialiseafterwait()
        {
          // stagger out the initial rider build in case many are spawning at once somehow?
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f,1.2f));

            partMaster.InitPartList(BMX);
            partMaster.AccessoriesSetup();

            if (CurrentModelName == "Daryien")
            {
                UpdateDaryien();
            }

            // do bike
            UpdateBMX();

            

            ChangePlayerVisibilty(CurrentMap == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            ChangeCollideStatus(InGameUI.instance.CollisionsToggle);

            Debug.Log($"{username} completed setup"); MasterActive = true;
        }




        // called by receiving model file after failure to load
        public void UpdateModel()
        {
            MasterActive = false;
            StartCoroutine(UpdateModelAfterWait());

        }

        IEnumerator UpdateModelAfterWait()
        {
            
                GameObject newmodel = GameManager.GetPlayerModel(CurrentModelName,Modelbundlename);
                while(newmodel == null)
                {
                  Debug.Log("Waiting for model");
                 
                 yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.4f));
                }


            try
            {


                newmodel.transform.position = RiderModel.transform.position;
                Destroy(RiderModel);

                RiderModel = newmodel;
                if (!RiderSetup())
                {
                    Debug.Log("Failed to setup model");
                }

                // decifer the rider and bmx specifics needed especially for daryien and bike colours

                

                Audio.Rider = RiderModel;




                // remove or disable components
                if (RiderModel.GetComponent<Animation>())
                {
                    RiderModel.GetComponent<Animation>().enabled = false;
                }
                if (RiderModel.GetComponent<BMXLimbTargetAdjust>())
                {
                    RiderModel.GetComponent<BMXLimbTargetAdjust>().enabled = false;

                }
                if (RiderModel.GetComponent<SkeletonReferenceValue>())
                {
                    RiderModel.GetComponent<SkeletonReferenceValue>().enabled = false;

                }
                // remove any triggers, Ontrigger events will cause local player to bail
                foreach (Transform t in RiderModel.GetComponentsInChildren<Transform>())
                {
                    if (t.name.Contains("Trigger"))
                    {
                        Destroy(t.gameObject);
                    }
                }


                MasterActive = true;

            }
            catch (Exception x)
            {
                Debug.Log($"UpdateModel Error: {x}");
            }

            yield return null;
        }
        

       
      
    }



    /// <summary>
    /// Incoming list of received transform updates, with or without timestamp
    /// </summary>
    public class IncomingTransformUpdate
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public int Ping;
        public long ServerTimeStamp;
        public long Playertimestamp;


        /// <summary>
        /// used to store transform update for this player along with timestamp from the moment this player sent it. Class found in RemotePlayer
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_rot"></param>
        /// <param name="_time"></param>
        public IncomingTransformUpdate(Vector3[] _pos, Vector3[] _rot)
        {
            Positions = _pos;
            Rotations = _rot;
            
           
        }

        public IncomingTransformUpdate(Vector3[] _pos, Vector3[] _rot, int _ping, long _serverstamp, long _playertimestamp)
        {
            Positions = _pos;
            Rotations = _rot;
            Ping = _ping;
            ServerTimeStamp = _serverstamp;
            Playertimestamp = _playertimestamp;
        }

    }



}