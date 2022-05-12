﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrostyP_Game_Manager
{
    /// <summary>
    /// Master Script for a Remote Player, when gamemanager spawns a new playerprefab, it has a remoteplayer attached with info populated by info from server
    /// </summary>
    public class RemotePlayer : MonoBehaviour
    {
      
        public bool MasterActive = false;
        public ushort id;
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
        public List<NetGameObject> Objects = new List<NetGameObject>();

        // Overhead name
        public GameObject nameSign;
		public TextMesh tm;

        // Received position updates for this player
        public Queue<RemoteTransform> IncomingTransformUpdates = new Queue<RemoteTransform>();
        // Positions captured in the same instant as all others including me
        public List<RemoteTransform> ReplayPositions = new List<RemoteTransform>();
       
        public GUIStyle style = new GUIStyle();
       
        private bool SetupSuccess;
        public bool InviteToSpawnLive;
        public Vector3 spawnpos;
        public Vector3 spawnrot;
        public Vector3 StartupPos = new Vector3();
        public Vector3 StartupRot = new Vector3();

        public float FPS;
        public float R2RPing;
        public int ConnectionQuality;
        public long NetmessagetimeReceived;

        // last position data
        double CurrentMoveRemainingTime = 0.0001f;
        RemoteTransform currentUpdate;
        RemoteTransform LastUpdate;
        double TimetoAbsorb;


        public bool PlayerTagVisible = true;
        public bool PlayerCollides = true;
        public bool PlayerObjectsVisible = true;
        public bool PlayerIsVisible = true;

        public RemotePartMaster partMaster;
        public RemoteBrakesManager brakesManager;

        public bool OverrideMapSync = false;

        void Awake()
        {
            // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[34];
            IncomingTransformUpdates = new Queue<RemoteTransform>();
            ReplayPositions = new List<RemoteTransform>();
        }

        // Call initiation once on start, inititation to reoccur until resolved
        private void Start()
        {
            try
            {
            Initialize();
           
            nameSign = new GameObject($"{username} label");
            DontDestroyOnLoad(nameSign);
            tm = nameSign.AddComponent<TextMesh>();

            tm.color = new Color(UnityEngine.Random.Range(0.3f, 0.9f), UnityEngine.Random.Range(0.3f, 0.9f), UnityEngine.Random.Range(0.3f, 0.9f));
            tm.fontStyle = FontStyle.Bold;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.1f;
            tm.fontSize = 40;
            tm.text = username;

                style.normal.textColor = tm.color;
                style.padding = new RectOffset(10, 10, 2, 2);
                style.fontStyle = FontStyle.Bold;
                style.normal.background = InGameUI.instance.BlackTex;
                style.hover.background = InGameUI.instance.whiteTex;
                style.hover.textColor = Color.black;

                StartCoroutine(Initialiseafterwait());
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

                BMX = MultiplayerManager.GetNewBMX();
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
                   
                  if (nameSign != null && RiderModel != null && Camera.current!= null)
                  {
			       nameSign.transform.rotation = Camera.current.transform.rotation;
                   float dist = Mathf.Clamp(Vector3.Distance(nameSign.transform.position, Camera.current.gameObject.transform.position) * 0.05f,0.05f,0.8f);
                   nameSign.transform.localScale = new Vector3(dist, dist, 0.1f);
                  }


                    if (currentUpdate != null)
                    {
                        InterpolateRider();
                    }
                   
                 

                  // whole second behind, dump most updates causing jump to latest
                  if (IncomingTransformUpdates.Count > 60)
                  {
                        currentUpdate = IncomingTransformUpdates.Dequeue();
                        IncomingTransformUpdates.Clear();
                  }



                }
                catch (Exception x )
                {
                    Debug.Log($"Rider LateUpdate Error" + x);
                }
            }
           
        }

        public GameObject DecideRider(string modelname)
        {
            if (modelname == "Daryien")
            {
                return MultiplayerManager.GetNewDaryien();
            }
            else
            {
                return MultiplayerManager.GetPlayerModel(modelname,Modelbundlename);
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
                wheelcolliders[0] = GameObject.Instantiate(MultiplayerManager.instance.wheelcolliderobj);
                wheelcolliders[1] = GameObject.Instantiate(MultiplayerManager.instance.wheelcolliderobj);


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

        public void ProcessNewPositionUpdate(RemoteTransform newupdate)
        {
            // is our framerate higher than this riders framerate
            newupdate.LowFPS = newupdate.PlayerMsFromLast < (long)(Time.deltaTime * 1000);
            R2RPing = newupdate.Ping + MultiplayerManager.Ping;
            FPS = 1 / ((float)newupdate.PlayerMsFromLast/1000);
            CalculateBuffers(newupdate);

        }

        public void CalculateBuffers(RemoteTransform update)
        {
           
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
              IncomingTransformUpdates.Enqueue(update);
                
            });

        }

        // calculate how to move smoothly considering our framerate, their framerate, their velocity and lag
        public void InterpolateRider()
        {
            try
            {
                // is our framerate high enough to process every update received
                if (!currentUpdate.LowFPS)
                {
                    // will we make a full deltatime's worth of movement at this given velocity, if not, change to the next move but blend the velocity
                    if(CurrentMoveRemainingTime >= Time.deltaTime)
                    {
                     DoMovement((float)CurrentMoveRemainingTime,Time.deltaTime);
                     CurrentMoveRemainingTime -= Time.deltaTime;
                    }
                    else
                    {
                        DoneWithUpdate(currentUpdate);
                        CurrentMoveRemainingTime += ((currentUpdate.PlayerMsFromLast / 1000) - Time.deltaTime);
                        DoMovement((float)CurrentMoveRemainingTime, Time.deltaTime);

                    }
                }
                else
                {
                    // our Fps is too low to use all updates
                    DoMovement(1,1);
                    DoneWithUpdate(currentUpdate);
                    if (IncomingTransformUpdates.Count > 1)
                    {
                     DoneWithUpdate(currentUpdate);
                    }
                }

               
            }
            catch (System.Exception x)
            {
                Debug.Log("MoveRider Error: " + x.Message + " : " + x.StackTrace);
            }


            
          
        }

        public void DoMovement(float movetime, float deltatime)
        {
            // rider
            Riders_Transforms[0].position = Vector3.MoveTowards(Riders_Transforms[0].position, currentUpdate.Positions[0], (float)(Vector3.Distance(Riders_Transforms[0].position, currentUpdate.Positions[0]) / movetime * deltatime));
            Riders_Transforms[0].rotation = Quaternion.RotateTowards(Riders_Transforms[0].rotation, Quaternion.Euler(currentUpdate.Rotations[0]), (float)Quaternion.Angle(Riders_Transforms[0].rotation, Quaternion.Euler(currentUpdate.Rotations[0])) / movetime * deltatime);

            // rider locals
            for (int i = 1; i < 23; i++)
            {
                Riders_Transforms[i].localRotation = Quaternion.RotateTowards(Riders_Transforms[i].localRotation, Quaternion.Euler(currentUpdate.Rotations[i]), (float)Quaternion.Angle(Riders_Transforms[i].localRotation, Quaternion.Euler(currentUpdate.Rotations[i])) / movetime * deltatime);
            }

            // hip joint
            Riders_Transforms[20].localPosition = Vector3.MoveTowards(Riders_Transforms[20].localPosition, currentUpdate.Positions[20], (float)Vector3.Distance(Riders_Transforms[20].localPosition, currentUpdate.Positions[20]) / movetime * deltatime);

            // bike joint
            Riders_Transforms[24].position = Vector3.MoveTowards(Riders_Transforms[24].position, currentUpdate.Positions[24], (float)Vector3.Distance(Riders_Transforms[24].position, currentUpdate.Positions[24]) / movetime * deltatime);
            Riders_Transforms[24].rotation = Quaternion.RotateTowards(Riders_Transforms[24].rotation, Quaternion.Euler(currentUpdate.Rotations[24]), (float)Quaternion.Angle(Riders_Transforms[24].rotation, Quaternion.Euler(currentUpdate.Rotations[24])) / movetime * deltatime);

            // bars joint
            Riders_Transforms[25].localPosition = Vector3.MoveTowards(Riders_Transforms[25].localPosition, currentUpdate.Positions[25], (float)Vector3.Distance(Riders_Transforms[25].localPosition, currentUpdate.Positions[25]) / movetime * deltatime);
            Riders_Transforms[25].localRotation = Quaternion.RotateTowards(Riders_Transforms[25].localRotation, Quaternion.Euler(currentUpdate.Rotations[25]), (float)Quaternion.Angle(Riders_Transforms[25].localRotation, Quaternion.Euler(currentUpdate.Rotations[25])) / movetime * deltatime);

            // Drivetrain
            Riders_Transforms[26].localRotation = Quaternion.RotateTowards(Riders_Transforms[26].localRotation, Quaternion.Euler(currentUpdate.Rotations[26]), (float)Quaternion.Angle(Riders_Transforms[26].localRotation, Quaternion.Euler(currentUpdate.Rotations[26])) / movetime * deltatime);

            // frame joint
            Riders_Transforms[27].localPosition = Vector3.MoveTowards(Riders_Transforms[27].localPosition, currentUpdate.Positions[27], (float)Vector3.Distance(Riders_Transforms[27].localPosition, currentUpdate.Positions[27]) / movetime * deltatime);
            Riders_Transforms[27].localRotation = Quaternion.RotateTowards(Riders_Transforms[27].localRotation, Quaternion.Euler(currentUpdate.Rotations[27]), (float)Quaternion.Angle(Riders_Transforms[27].localRotation, Quaternion.Euler(currentUpdate.Rotations[27])) / movetime * deltatime);

            // front wheel
            Riders_Transforms[28].localRotation = Quaternion.RotateTowards(Riders_Transforms[28].localRotation, Quaternion.Euler(currentUpdate.Rotations[28]), (float)Quaternion.Angle(Riders_Transforms[28].localRotation, Quaternion.Euler(currentUpdate.Rotations[28])) / movetime * deltatime);

            // back wheel
            Riders_Transforms[29].localRotation = Quaternion.RotateTowards(Riders_Transforms[29].localRotation, Quaternion.Euler(currentUpdate.Rotations[29]), (float)Quaternion.Angle(Riders_Transforms[29].localRotation, Quaternion.Euler(currentUpdate.Rotations[29])) / movetime * deltatime);

            // left pedal
            Riders_Transforms[30].localRotation = Quaternion.RotateTowards(Riders_Transforms[30].localRotation, Quaternion.Euler(currentUpdate.Rotations[30]), (float)Quaternion.Angle(Riders_Transforms[30].localRotation, Quaternion.Euler(currentUpdate.Rotations[30])) / movetime * deltatime);

            // right pedal
            Riders_Transforms[31].localRotation = Quaternion.RotateTowards(Riders_Transforms[31].localRotation, Quaternion.Euler(currentUpdate.Rotations[31]), (float)Quaternion.Angle(Riders_Transforms[31].localRotation, Quaternion.Euler(currentUpdate.Rotations[31])) / movetime * deltatime);

            // left fingers index2
            Riders_Transforms[32].localRotation = Quaternion.RotateTowards(Riders_Transforms[32].localRotation, Quaternion.Euler(currentUpdate.Rotations[32]), (float)Quaternion.Angle(Riders_Transforms[32].localRotation, Quaternion.Euler(currentUpdate.Rotations[32])) / movetime * deltatime);

            // right fingers index 2
            Riders_Transforms[33].localRotation = Quaternion.RotateTowards(Riders_Transforms[33].localRotation, Quaternion.Euler(currentUpdate.Rotations[33]), (float)Quaternion.Angle(Riders_Transforms[33].localRotation, Quaternion.Euler(currentUpdate.Rotations[33])) / movetime * deltatime);

            currentUpdate.MoveCount++;
           
        }

        void DoneWithUpdate(RemoteTransform update)
        {
            if(LastUpdate != null) LastUpdate.Release();
            LastUpdate = update;
            currentUpdate = IncomingTransformUpdates.Count > 0 ? IncomingTransformUpdates.Dequeue() : update;
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
            MultiplayerManager.PlayersInvisibleOnMyMap();
        }

        public void UpdateDaryien()
        {
            if (Gear.RiderTextures.Count > 0)
            {
                Debug.Log($"Updating Textures for {username}");


                try
                {
                    CharacterModding.instance.FlipCap(!Gear.Capisforward, this);
                    
                   foreach (NetTexture t in Gear.RiderTextures)
                   {
                        // if stock, do nothing
                        if(t.Nameoftexture != "stock")
                        {

                        if(t.Nameoftexture.ToLower() != "e")
                        {
                        if (FileSyncing.CheckForFile(t.Nameoftexture,t.Directory))
                        {
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].EnableKeyword("_ALPHATEST_ON");
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().enabled = true;
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].mainTexture = MultiplayerManager.GetTexture(t.Nameoftexture,t.Directory);
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].color = Color.white;
                        }
                        else
                        {
                            FileSyncing.AddToRequestable(1, t.Nameoftexture.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", ""), id,t.Directory);
                        }
                        }
                        else
                        {
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().enabled = false;
                            RiderModel.transform.FindDeepChild(t.NameofparentGameObject).gameObject.GetComponent<Renderer>().materials[t.Matnum].mainTexture = null;
                        }
                       

                        }

                   }


            
                  InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updated {username}'s Rider..", FrostyUIColor.System, TextMessage.Textmessagemode.system));
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
            if (!MultiplayerManager.GarageEnabled) return;
           

            try
            {
                MasterActive = false;
            // do garage setup
            
            MultiplayerManager.DoGarageSetup(this);
                MasterActive = true;
            }
            catch (Exception x )
            {

                Debug.Log("dogaragesetup error :   " + x.Message);
            }

        }

        public void MasterShutdown()
        {
            MasterActive = false;

            if (Audio)
            {
            Audio.Shutdown();
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f,3f));
            try
            {
            if (CurrentModelName == "Daryien")
            {
                RiderModel.transform.FindDeepChild("pants_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
                RiderModel.transform.FindDeepChild("shirt_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
                UpdateDaryien();
            }
                

            }
            catch (Exception x)
            {
                Debug.Log($"Init after wait for {username} had an issue : " + x);
            }


            try
            {
                if (MultiplayerManager.GarageEnabled)
                {
                    partMaster.InitPartList(BMX);
                    partMaster.AccessoriesSetup();

                }
            }
            catch (Exception x )
            {
                Debug.Log("Garage init issue in Initafterwait : " + x);
            }



            // do bike update
            try
            {
                if (MultiplayerManager.GarageEnabled)
                {
                    DestroyNormal();
                    UpdateBMX();
                }

            }
            catch (Exception x)
            {
                Debug.Log("Initial setup after wait error: Destroynormal or UpdateBMX: " + x);
            }

            // do park objects
            try
            {
                if (Objects.Count > 0)
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        MultiplayerManager.instance.GetObject(Objects[i]);
                        if (Objects[i]._Gameobject != null)
                        {
                            if (!InGameUI.instance.AllPlayerObjectsVisibleToggle)
                            {
                                Objects[i]._Gameobject.SetActive(false);
                            }
                            else
                            {
                                Objects[i]._Gameobject.SetActive(PlayerObjectsVisible);
                            }

                        }

                    }
                }
            }
            catch (Exception x)
            {

                Debug.Log($"Init after wait for {username} had an Object spawn issue : " + x);
            }



            nameSign.transform.parent = RiderModel.transform.FindDeepChild("mixamorig:Head");
            nameSign.transform.localPosition = new Vector3(0, 0.37f, 0);

            ChangePlayerVisibilty(MultiplayerManager.instance.RiderOnMyMap(this));
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
            
                GameObject newmodel = MultiplayerManager.GetPlayerModel(CurrentModelName,Modelbundlename);
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



}