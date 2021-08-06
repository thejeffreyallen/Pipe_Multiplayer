using System.Collections;
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
        public bool InviteToSpawnLive;
        public Vector3 spawnpos;
        public Vector3 spawnrot;
        public Vector3 StartupPos = new Vector3();
        public Vector3 StartupRot = new Vector3();

        public float PlayersFrameRate;
        public float R2RPing;

        // last transform data
        public float MyLastPing;
        public float PlayerLastPing;
        public DateTime LastServerStamp;
        public DateTime LastPlayerStamp;
        public Vector3[] lastpos = new Vector3[28];
        private Vector3[] lastrot = new Vector3[34];
        private DateTime LastTimeAtReceive = DateTime.Now;
        private float deltatime = Time.deltaTime;
        bool MoveTwice = false;
        float RemainingTimeSpan = 0.001f;

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
           
            nameSign = new GameObject($"{username} label");
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
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(10, 10, 2, 2);
                style.fontStyle = FontStyle.Bold;
                style.normal.background = InGameUI.instance.whiteTex;
                style.hover.background = InGameUI.instance.BlackTex;
                style.hover.textColor = Color.white;


            }
            catch (Exception)
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

                RiderModel.transform.position = StartupPos;
                RiderModel.transform.eulerAngles = StartupRot;
                BMX.transform.position = StartupPos;
                BMX.transform.eulerAngles = StartupRot;
                partMaster = gameObject.GetComponent<RemotePartMaster>(); // initialize the part master part list.
                brakesManager = gameObject.GetComponent<RemoteBrakesManager>();
                StartCoroutine(Initialiseafterwait());
            }

            catch (Exception x)
            {
                Debug.Log($"RemotePlayer.Initialize error: {x}");
            }


        }

        private void Update()
        {
            if (MasterActive)
            {
                try
                {
                   
                  if (nameSign != null && RiderModel != null && Camera.current!= null)
                  {
			       nameSign.transform.rotation = Camera.current.transform.rotation;
                  }

                  if (!SetupSuccess)
                  {
                   SetupSuccess = RiderSetup();
                  }

                    if (MoveTwice)
                    {
                        InterpolateRider();
                        CheckThresholds();
                        MoveTwice = false;
                    }
                    else
                    {
                     CheckThresholds();
                    }

                  InterpolateRider();
                  CheckThresholds();


                  // whole second behind, dump most updates causing jump to latest
                  if (IncomingTransformUpdates.Count > 60)
                  {
                        IncomingTransformUpdates.RemoveRange(0, IncomingTransformUpdates.Count - 10);
                  }



                }
                catch (Exception x )
                {
                    Debug.Log($"Rider LateUpdate Error" + x);
                }
            }
           
        }

        void CheckThresholds()
        {
            bool value = false;
            if (IncomingTransformUpdates.Count > 1)
            {
                value = Vector3.Distance(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0]) < (Vector3.Distance(lastpos[0], IncomingTransformUpdates[0].Positions[0]) / 100);
            }

            if (value)
            {
                // copy update data to LastData
                Array.Copy(IncomingTransformUpdates[0].Positions, lastpos, IncomingTransformUpdates[0].Positions.Length);
                Array.Copy(IncomingTransformUpdates[0].Rotations, lastrot, IncomingTransformUpdates[0].Rotations.Length);
                MyLastPing = InGameUI.instance.Ping;
                LastPlayerStamp = DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].Playertimestamp);
                LastServerStamp = DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].ServerTimeStamp);
                PlayerLastPing = IncomingTransformUpdates[0].Ping;
                LastTimeAtReceive = IncomingTransformUpdates[0].TimeAtReceive;

               

                // Remove Update weve reached
                IncomingTransformUpdates.RemoveAt(0);

                // if FPS of this machine is not high enough to keep up, dump more than one update
                if (IncomingTransformUpdates.Count > 20)
                {
                    // if deltatime is bigger than deltatime of this players framerate 
                    if (Time.deltaTime > (1/PlayersFrameRate))
                    {
                        IncomingTransformUpdates.RemoveAt(0);
                    }
                }


                // Alter real world Velocity to buffer lag out

                float PlayerPingDifference = IncomingTransformUpdates[0].Ping - PlayerLastPing;
                float MyPingDifference = InGameUI.instance.Ping - MyLastPing;
                RemainingTimeSpan = RemainingTimeSpan + (float)(DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].Playertimestamp) - LastPlayerStamp).TotalSeconds;
                if (IncomingTransformUpdates.Count < 10)
                {
                   RemainingTimeSpan = RemainingTimeSpan + ((10 - IncomingTransformUpdates.Count) / 1000);
                }
                // if were more than 10 updates behind, remove half difference in ms
                if (IncomingTransformUpdates.Count >= 10 && IncomingTransformUpdates.Count < 20)
                {
                    RemainingTimeSpan = RemainingTimeSpan + ((19 - IncomingTransformUpdates.Count) / 1000 / 2);
                }

                // if player current ping is larger than last ping, add half the difference in ms to the timespan within reason
                if (PlayerPingDifference > 0 && PlayerPingDifference < 5)
                {
                    RemainingTimeSpan = RemainingTimeSpan + ((PlayerPingDifference / 1000)/2);
                }

                if (MyPingDifference > 0 && MyPingDifference < 5)
                {
                    RemainingTimeSpan = RemainingTimeSpan + ((MyPingDifference / 1000) /2);
                }

                RemainingTimeSpan = Mathf.Clamp(RemainingTimeSpan, 0.00001f, 0.032f);

            }

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

                if (IncomingTransformUpdates.Count > 0)
                {

                    #region build data

                    int PingCurrent = IncomingTransformUpdates[0].Ping;
                    if(LastPlayerStamp== null)
                    {
                        LastPlayerStamp = DateTime.Now.Subtract(TimeSpan.FromSeconds(0.016f));
                    }

                      R2RPing = IncomingTransformUpdates[IncomingTransformUpdates.Count-1].Ping + InGameUI.instance.Ping;
                    float rate = (1.000f / (float)(DateTime.FromFileTimeUtc(IncomingTransformUpdates[0].Playertimestamp).Subtract(TimeSpan.FromMilliseconds(IncomingTransformUpdates[0].Ping)) - LastPlayerStamp.Subtract(TimeSpan.FromMilliseconds(PlayerLastPing))).TotalSeconds);
                    PlayersFrameRate = rate;

                    #endregion


                    #region Do Movement
                    RemainingTimeSpan = Mathf.Clamp(RemainingTimeSpan, 0.00001f, 0.032f);

                    // will this movement make it to the destination, if not, keep track of remaining time span
                    if(RemainingTimeSpan > Time.deltaTime && RemainingTimeSpan < Time.deltaTime * 2)
                    {
                        MoveTwice = true;
                    }
                    
                    


                    // rider
                    Riders_Transforms[0].position = Vector3.MoveTowards(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0], (float)(Vector3.Distance(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0]) / RemainingTimeSpan * Time.deltaTime));
                    Riders_Transforms[0].rotation = Quaternion.RotateTowards(Riders_Transforms[0].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[0]), (float)Quaternion.Angle(Riders_Transforms[0].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[0])) / RemainingTimeSpan * Time.deltaTime);
                    
                    // rider locals
                   for (int i = 1; i < 23; i++)
                   { 
                     Riders_Transforms[i].localRotation = Quaternion.RotateTowards(Riders_Transforms[i].localRotation,Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i]), (float)Quaternion.Angle(Riders_Transforms[i].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[i])) / RemainingTimeSpan * Time.deltaTime);
                   }

                    // hip joint
                    Riders_Transforms[20].localPosition = Vector3.MoveTowards(Riders_Transforms[20].localPosition, IncomingTransformUpdates[0].Positions[20], (float)Vector3.Distance(Riders_Transforms[20].localPosition, IncomingTransformUpdates[0].Positions[20]) / RemainingTimeSpan * Time.deltaTime);

                    // bike joint
                    Riders_Transforms[24].position = Vector3.MoveTowards(Riders_Transforms[24].position, IncomingTransformUpdates[0].Positions[24], (float)(Vector3.Distance(Riders_Transforms[24].position, IncomingTransformUpdates[0].Positions[24]) / RemainingTimeSpan * Time.deltaTime));
                    Riders_Transforms[24].rotation = Quaternion.RotateTowards(Riders_Transforms[24].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[24]), (float)Quaternion.Angle(Riders_Transforms[24].rotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[24])) / RemainingTimeSpan * Time.deltaTime);

                    // bars joint
                    Riders_Transforms[25].position = Vector3.MoveTowards(Riders_Transforms[25].position, IncomingTransformUpdates[0].Positions[25], (float)(Vector3.Distance(Riders_Transforms[25].position, IncomingTransformUpdates[0].Positions[25]) / RemainingTimeSpan * Time.deltaTime));
                    Riders_Transforms[25].localRotation = Quaternion.RotateTowards(Riders_Transforms[25].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[25]), (float)Quaternion.Angle(Riders_Transforms[25].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[25])) / RemainingTimeSpan * Time.deltaTime);

                    // Drivetrain
                    Riders_Transforms[26].localRotation = Quaternion.RotateTowards(Riders_Transforms[26].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[26]), (float)Quaternion.Angle(Riders_Transforms[26].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[26])) / RemainingTimeSpan * Time.deltaTime);

                    // frame joint
                    Riders_Transforms[27].position = Vector3.MoveTowards(Riders_Transforms[27].position, IncomingTransformUpdates[0].Positions[27], (float)(Vector3.Distance(Riders_Transforms[27].position, IncomingTransformUpdates[0].Positions[27]) / RemainingTimeSpan * Time.deltaTime));
                    Riders_Transforms[27].localRotation = Quaternion.RotateTowards(Riders_Transforms[27].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[27]), (float)Quaternion.Angle(Riders_Transforms[27].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[27])) / RemainingTimeSpan * Time.deltaTime);

                    // front wheel
                    Riders_Transforms[28].localRotation = Quaternion.RotateTowards(Riders_Transforms[28].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[28]), (float)Quaternion.Angle(Riders_Transforms[28].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[28])) / RemainingTimeSpan * Time.deltaTime);

                    // back wheel
                    Riders_Transforms[29].localRotation = Quaternion.RotateTowards(Riders_Transforms[29].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[29]), (float)Quaternion.Angle(Riders_Transforms[29].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[29])) / RemainingTimeSpan * Time.deltaTime);

                    // left pedal
                    Riders_Transforms[30].localRotation = Quaternion.RotateTowards(Riders_Transforms[30].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[30]), (float)Quaternion.Angle(Riders_Transforms[30].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[30])) / RemainingTimeSpan * Time.deltaTime);

                    // right pedal
                    Riders_Transforms[31].localRotation = Quaternion.RotateTowards(Riders_Transforms[31].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[31]), (float)Quaternion.Angle(Riders_Transforms[31].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[31])) / RemainingTimeSpan * Time.deltaTime);

                    // left fingers index2
                    Riders_Transforms[32].localRotation = Quaternion.RotateTowards(Riders_Transforms[32].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[32]), (float)Quaternion.Angle(Riders_Transforms[32].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[32])) / RemainingTimeSpan * Time.deltaTime);

                    // right fingers index 2
                    Riders_Transforms[33].localRotation = Quaternion.RotateTowards(Riders_Transforms[33].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[33]), (float)Quaternion.Angle(Riders_Transforms[33].localRotation, Quaternion.Euler(IncomingTransformUpdates[0].Rotations[33])) / RemainingTimeSpan * Time.deltaTime);


                    RemainingTimeSpan = Mathf.Clamp(RemainingTimeSpan - Time.deltaTime ,0.00001f,0.032f);
                    #endregion

                }


            }
            catch (System.Exception x)
            {
                Debug.Log("MoveRider Error   : " + x.Message + " : " + x.StackTrace);
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
            MasterActive = value;
            PlayerIsVisible = value;
            GameManager.instance.UpdatePlayersOnMyLevelToggledOff();
        }

        public void UpdateDaryien()
        {
            
            if (Gear.RiderTextures.Count > 0)
            {
                Debug.Log($"Updating Textures for {username}");


                try
                {
                    
                   foreach (TextureInfo t in Gear.RiderTextures)
                   {
                        if(t.Nameoftexture.ToLower() != "e")
                        {
                        if (FileSyncing.CheckForFile(t.Nameoftexture))
                        {
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].EnableKeyword("_ALPHATEST_ON");
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().enabled = true;
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].mainTexture = GameManager.GetTexture(t.Nameoftexture);
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].color = Color.white;
                        }
                        else
                        {
                            FileSyncing.AddToRequestable(1, t.Nameoftexture, id);
                        }
                        }
                        else
                        {
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().enabled = false;
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].mainTexture = null;
                        }
                       

                   }
            
                  InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updated {username}'s Rider..", 1, 1));
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
                MasterActive = false;
            // do garage setup
            GameManager.DoGarageSetup(this, Gear.GarageSave);
                MasterActive = true;
            }
            catch (Exception x )
            {

                Debug.Log("dogaragesetup error :   " + x.Message);
            }

        }

        public void MasterShutdown()
        {
            if (Audio)
            {
            Audio.ShutdownAllSounds();
            }
            if (RiderModel)
            {
                Destroy(RiderModel);
            }
            if (BMX)
            {
                Destroy(BMX);
            }
            if (nameSign)
            {
                Destroy(nameSign);
            }

            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i]._Gameobject != null)
                {
                    Destroy(Objects[i]._Gameobject);
                }
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
                RiderModel.transform.FindDeepChild("pants_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
                RiderModel.transform.FindDeepChild("shirt_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
                UpdateDaryien();
            }

            // do bike
            DestroyNormal();
            UpdateBMX();

            

            ChangePlayerVisibilty(CurrentMap == GameManager.instance.MycurrentLevel);
            ChangeCollideStatus(InGameUI.instance.CollisionsToggle);

            Debug.Log($"{username} completed setup"); MasterActive = true;
           
        }

        private void DestroyNormal()
        {
            Material[] defaultMats = partMaster.GetMaterials(partMaster.frame);
            defaultMats[0].SetTexture("_DetailNormalMap", null);
            defaultMats[0].color = Color.black;
            Debug.Log("Destroying detail normal maps on remote rider");
            foreach (KeyValuePair<int, GameObject> pair in partMaster.partList)
            {
                Material[] m = partMaster.GetMaterials(pair.Key);
                if (m == null)
                    continue;
                if (m[0].name.ToLower().Contains("a_glossy") || m[0].name.ToLower().Contains("forks"))
                {
                    partMaster.SetMaterials(pair.Key, defaultMats);
                }
            }
        }

        public IEnumerator InvitedToSpawn()
        {
            InviteToSpawnLive = true;
            yield return new WaitForSeconds(60);
            InviteToSpawnLive = false;

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
        public DateTime TimeAtReceive;

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
            TimeAtReceive = DateTime.Now;
        }

    }



}