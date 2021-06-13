using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;



namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Master Script for a Remote Player, when gamemanager spawns a new playerprefab, it has a remoteplayer attached with info populated by info from server
    /// </summary>
    public class RemotePlayer : MonoBehaviour
    {
        /// <summary>
        /// Should incoming updates be processed
        /// </summary>
        public bool MasterActive = false;
        public uint id;
        public string username;
        public string CurrentModelName;
        public string Modelbundlename;
        public string CurrentMap = "Unknown";


        public GameObject RiderModel;
        public GameObject BMX;
        private Transform[] Riders_Transforms;
       
        public List<IncomingTransformUpdate> IncomingTransformUpdates;
       

        private Rigidbody Rider_RB;
        private Rigidbody BMX_RB;
       
       

        /// <summary>
        /// flipped on and off by interpolation logic to keep behind realtime
        /// </summary>
        bool MoveRiderOn;

        private GameObject[] wheelcolliders;
        public RemotePlayerAudio Audio;

        private bool SetupSuccess;

        MeshRenderer FrameRen;
        MeshRenderer ForksRen;
        MeshRenderer BarsRen;
        MeshRenderer SeatRen;
        MeshRenderer FTireRen;
        MeshRenderer RTireRen;
        MeshRenderer StemRen;
        MeshRenderer FRimRen;
        MeshRenderer RRimRen;
            

        SkinnedMeshRenderer shirtren;
        SkinnedMeshRenderer bottomsren;
        MeshRenderer hatren;
        SkinnedMeshRenderer shoesren;
        SkinnedMeshRenderer bodyren;
        

        public GameObject nameSign;
		public TextMesh tm;

        public float PlayersFrameRate;
        private float _playerframerate;
        public float R2RPing;
        public float MovementTimer;
        public float LastGamePing;



        void Awake()
        {
            // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[32];
           
           
            IncomingTransformUpdates = new List<IncomingTransformUpdate>();
           
        }


        // Call initiation once on start, inititation to reoccur until resolved
        private void Start()
        {
            bool builderopen = FrostyP_Game_Manager.ParkBuilder.instance.openflag;

            if (builderopen)
            {
                FrostyP_Game_Manager.ParkBuilder.instance.Player.SetActive(true);
            }

            Initialize();
            RiderModel.name = "Model " + id;
            BMX.name = "BMX " + id;
           
            DontDestroyOnLoad(BMX);
            DontDestroyOnLoad(RiderModel);
            

           
            FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
            ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
            BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
            SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
            FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            StemRen = BMX.transform.FindDeepChild("Stem Mesh").gameObject.GetComponent<MeshRenderer>();
            FRimRen = BMX.transform.FindDeepChild("BMX:Wheel").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();
            RRimRen = BMX.transform.FindDeepChild("BMX:Wheel 1").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();

            if (CurrentModelName == "Daryien")
            {
            shirtren = RiderModel.transform.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>();
            bottomsren = RiderModel.transform.Find("pants_geo").GetComponent<SkinnedMeshRenderer>();
            shoesren = RiderModel.transform.Find("shoes_geo").GetComponent<SkinnedMeshRenderer>();
            hatren = RiderModel.transform.FindDeepChild("Baseball Cap_R").GetComponent<MeshRenderer>();
            bodyren = RiderModel.transform.Find("body_geo").GetComponent<SkinnedMeshRenderer>();

            }


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
            tm.fontSize = 20;
            tm.text = username;

            
            MasterActive = true;

            if (builderopen)
            {
               FrostyP_Game_Manager.ParkBuilder.instance.Player.SetActive(false);
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
            // Add Audio Component
            Audio = gameObject.AddComponent<RemotePlayerAudio>();
            Audio.Rider = RiderModel;

            BMX = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX"));
            Destroy(BMX.transform.Find("BMX Load Out").gameObject);

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


            }
            catch(Exception x)
            {
                Debug.Log($"RemotePlayer.Initialize error: {x}");
            }




            

            


            // Once models instatiated, run findriderparts to locate and assign all ridertransforms to models and children joints of models, sets masteractive on success
            SetupSuccess = RiderSetup();

            if (!SetupSuccess)
            {
                Debug.Log("Error with Ridersetup()");
            }


            StartCoroutine(Initialiseafterwait());
        }


        private void LateUpdate()
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


           
            MoveRider();
            

            
            if (IncomingTransformUpdates.Count > 0)
            {
                if (Vector3.Distance(Riders_Transforms[0].position, IncomingTransformUpdates[0].Positions[0]) < 0.0005f && IncomingTransformUpdates.Count >=3)
                {     
                  IncomingTransformUpdates.RemoveAt(0);
                }
            }
            
            if(_playerframerate > 0 && _playerframerate < 120)
            {
                PlayersFrameRate = _playerframerate;
            }


            
        }



      



        // decides whether to get daryien and start the texture process, or grab a custom model, Gives back gameobject to Initialise for instantiation
        private GameObject DecideRider(string modelname)
        {
            

            if (modelname == "Daryien")
            {
                
                return DaryienSetup();
            }
            else
            {

                return LoadRiderFromAssets();
            }
        }





        // Called by DecideRider if Currentmodelname is Daryien
        private GameObject DaryienSetup()
        {
            GameObject daz = GameObject.Instantiate(Component.FindObjectOfType<SkeletonReferenceValue>().gameObject);

            // make sure meshes are active, pipeworks PI keeps daryien but turns off all his meshes, then tracks new models to daryiens bones
            foreach(Transform t in daz.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
                if(t.gameObject.name == "Daryen_Hair_Matt")
                {
                    Destroy(t.gameObject);
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }

            }
            daz.SetActive(true);
          
           return daz;
        }




        /// <summary>
        /// Called to load a custom model
        /// </summary>
        /// <returns></returns>
        private GameObject LoadRiderFromAssets()
        {
            if (BundleIsLoaded())
            {
                return LoadFromBundle();

            }
            else
            {
                return LoadFromAssets();
            }
        }

        private GameObject LoadFromBundle()
        {
            
            IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in bundles)
            {
                if (a.name.ToLower().Contains(Modelbundlename.ToLower()))
                {
                    Debug.Log("Matched bundle to requested model");

                    
                   return GameObject.Instantiate(a.LoadAsset(CurrentModelName) as GameObject);
                  
                }
               
            }
            return null;

        }
        private bool BundleIsLoaded()
        {
            IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in bundles)
            {
                if (a.name.ToLower().Contains(Modelbundlename.ToLower()))
                {
                   


                    return true;

                }
                

            }

            return false;


        }


        private GameObject LoadFromAssets()
        {
           
         
            if(File.Exists(Application.dataPath + "/Custom Players/" + CurrentModelName))
            {

                    AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/Custom Players/" + CurrentModelName);

                   
                    return GameObject.Instantiate(b.LoadAsset(CurrentModelName) as GameObject);
            }
            else
            {
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"{username} is using a model called {CurrentModelName} that you dont have", (int)MessageColour.Server, 0));
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"{username} will be removed from your game", (int)MessageColour.Server, 0));
                return null;
            }

            
           
              
              

        }



        /// <summary>
        /// Couple Ridertransforms array with transforms of ridermodel and bike, setup colliders, Rigidbodies, grab BikeLoadout script of bike, then set MasterActive to True
        /// </summary>
        /// <returns></returns>
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


                Rider_RB = RiderModel.AddComponent<Rigidbody>();
                Rider_RB.isKinematic = true;

                SphereCollider spherehead = RiderModel.transform.FindDeepChild("mixamorig:Head").gameObject.AddComponent<SphereCollider>();
                spherehead.radius = 0.012f;
                spherehead.center = new Vector3(0, 0.1f, 0.03f);
                spherehead.transform.localPosition = new Vector3(0, 0.1028246f, 0.0511784f);


                BMX_RB = BMX.AddComponent<Rigidbody>();
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




        /// <summary>
        /// Called by FixedUpdate if MasterActive is true, Updates transforms to latest received by my Master player
        /// </summary>
        public void MoveRider()
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
                    float MyPingDifference = InGameUI.instance.Ping - LastGamePing;


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
                    Riders_Transforms[0].eulerAngles = Vector3.Lerp(Riders_Transforms[0].eulerAngles, IncomingTransformUpdates[0].Rotations[0],1);
                    // rider locals
                   for (int i = 1; i < 23; i++)
                   {
                      Riders_Transforms[i].localPosition = Vector3.MoveTowards(Riders_Transforms[i].localPosition, IncomingTransformUpdates[0].Positions[i], (float)(Vector3.Distance(Riders_Transforms[i].localPosition, IncomingTransformUpdates[1].Positions[i]) / timespan / (1 / Time.deltaTime)));
                      Riders_Transforms[i].localEulerAngles = Vector3.Lerp(Riders_Transforms[i].localEulerAngles, IncomingTransformUpdates[0].Rotations[i],1);
                   }



                    // Bmx
                        Riders_Transforms[23].position = Vector3.MoveTowards(Riders_Transforms[23].position, IncomingTransformUpdates[0].Positions[23], (float)(Vector3.Distance(Riders_Transforms[23].position, IncomingTransformUpdates[1].Positions[23]) / timespan / (1 / Time.deltaTime)));
                        Riders_Transforms[23].eulerAngles = Vector3.Lerp(Riders_Transforms[23].eulerAngles, IncomingTransformUpdates[0].Rotations[23], 1);
                    // bmx locals
                    for (int i = 24; i < 32; i++)
                    {
                         Riders_Transforms[i].localPosition = Vector3.MoveTowards(Riders_Transforms[i].localPosition, IncomingTransformUpdates[0].Positions[i], (float)(Vector3.Distance(Riders_Transforms[i].localPosition, IncomingTransformUpdates[1].Positions[i]) / timespan / (1 / Time.deltaTime)));
                        Riders_Transforms[i].localEulerAngles = Vector3.Lerp(Riders_Transforms[i].localEulerAngles, IncomingTransformUpdates[0].Rotations[i], 1);
                    

                    //Vector3.Lerp(Riders_Transforms[i].localEulerAngles, IncomingTransformUpdates[0].Rotations[i],1);
                    }

                }

                LastGamePing = InGameUI.instance.Ping;

            }
            catch (System.Exception x)
            {
                Debug.Log("MoveRider Error   : " + x);
            }

           
        }





        /// <summary>
        /// Dead Reckoning
        /// </summary>
        public void Extrapolate()
        {


        }




        /// <summary>
        /// Called by incoming rider update packet once it has updated Gamemanager.RiderTexinfos[id] 
        /// </summary>
        public void UpdateDaryien()
        {
            // look through rider
            if (GameManager.RiderTexinfos[id].Count > 0)
            {
                Debug.Log($"Updating Textures for {username}");
                //InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updating {username}'s Rider..", 1, 1));


                try
                {
                    
                foreach (TextureInfo t in GameManager.RiderTexinfos[id])
                {
                        bool found = false;
                    byte[] bytes = null;
                    if (t.NameofparentGameObject == "Daryien_Head")
                    {

                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name == t.Nameoftexture)
                                {
                                    bytes = File.ReadAllBytes(f.FullName);
                                        

                                }
                            }
                        }

                        if (bytes != null)
                        {
                                try
                                {
                            Texture2D image = new Texture2D(1024, 1024);
                            
                                    ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                    bodyren.materials[0].mainTexture = image;
                                    found = true;
                                }
                                catch (System.Exception x)
                                {
                                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                    Debug.Log($"Failed to apply texture to {id}  " + x);
                                }

                        }
                            else
                            {
                                bodyren.materials[0].mainTexture = CharacterModding.instance.Heads[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Heads.Length - 1))];
                            }
                    }
                    if (t.NameofparentGameObject == "Daryien_Body")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name.Contains(t.Nameoftexture))
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                bodyren.materials[1].mainTexture = image;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                                bodyren.materials[0].mainTexture = CharacterModding.instance.Bodies[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Bodies.Length - 1))];
                            }
                        }
                    if (t.NameofparentGameObject == "Daryien_HandsFeet")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name.Contains(t.Nameoftexture))
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                bodyren.materials[2].mainTexture = image;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                                bodyren.materials[0].mainTexture = CharacterModding.instance.Hands_feet[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Hands_feet.Length - 1))];
                            }
                        }
                    if (t.NameofparentGameObject == "shirt_geo")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name == t.Nameoftexture)
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                shirtren.material.mainTexture = image;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                               shirtren.material.mainTexture = CharacterModding.instance.Shirts[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Shirts.Length - 1))];
                            }
                        }
                    if (t.NameofparentGameObject == "pants_geo")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name.Contains(t.Nameoftexture))
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                               bottomsren.material.mainTexture = image;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                                bottomsren.material.mainTexture = CharacterModding.instance.Bottoms[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Bottoms.Length - 1))];
                            }
                        }
                    if (t.NameofparentGameObject == "Baseball Cap_R")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name.Contains(t.Nameoftexture))
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                hatren.material.mainTexture = image;
                                hatren.material.color = Color.white;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                                hatren.material.mainTexture = CharacterModding.instance.Hats[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Hats.Length - 1))];
                            }
                    }
                    if (t.NameofparentGameObject == "shoes_geo")
                    {




                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach (DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach (FileInfo f in images)
                            {
                                if (f.Name.Contains(t.Nameoftexture))
                                {
                                    bytes = File.ReadAllBytes(f.FullName);


                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                                Texture2D image = new Texture2D(1024, 1024);

                                ImageConversion.LoadImage(image, bytes);
                                image.name = t.Nameoftexture;
                                shoesren.material.mainTexture = image;
                                    found = true;
                                }
                            catch (System.Exception x)
                            {
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                Debug.Log($"Failed to apply texture to {id}  " + x);
                            }

                        }
                            else
                            {
                                shoesren.material.mainTexture = CharacterModding.instance.Shoes[Mathf.RoundToInt(UnityEngine.Random.Range(0, CharacterModding.instance.Shoes.Length - 1))];
                            }
                        }


                        if (!found)
                        {
                            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} wasnt found in textures for {username}'s Daryien", 1, 1));

                        }


                }

                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
           
            

            }

           
        }


        

        /// <summary>
        /// Call once Gamemanager.PlayerColours[id], Gamemanager.PlayerMetals[id] etc has been updated
        /// </summary>
        public void UpdateBike()
        {

            // colours
            try
            {
            FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
            ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
            BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
            SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
            FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
                StemRen = BMX.transform.FindDeepChild("Stem Mesh").gameObject.GetComponent<MeshRenderer>();
                FRimRen = BMX.transform.FindDeepChild("BMX:Wheel").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();
                RRimRen = BMX.transform.FindDeepChild("BMX:Wheel 1").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();


                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updating {username}'s Bike Colours",(int)MessageColour.System,(uint)0));
            FrameRen.material.color = new Color(GameManager.PlayersColours[id][0].x, GameManager.PlayersColours[id][0].y, GameManager.PlayersColours[id][0].z);
                FrameRen.material.SetInt("_SmoothnessTextureChannel", 0);
                FrameRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][0]);
                FrameRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][0]);

                ForksRen.material.color = new Color(GameManager.PlayersColours[id][1].x, GameManager.PlayersColours[id][1].y, GameManager.PlayersColours[id][1].z);
                ForksRen.material.SetInt("_SmoothnessTextureChannel", 0);
                ForksRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][1]);
                ForksRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][1]);

                BarsRen.material.color = new Color(GameManager.PlayersColours[id][2].x, GameManager.PlayersColours[id][2].y, GameManager.PlayersColours[id][2].z);
                BarsRen.material.SetInt("_SmoothnessTextureChannel", 0);
                BarsRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][2]);
                BarsRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][2]);


                SeatRen.material.color = new Color(GameManager.PlayersColours[id][3].x, GameManager.PlayersColours[id][3].y, GameManager.PlayersColours[id][3].z);
            FTireRen.materials[0].color = new Color(GameManager.PlayersColours[id][4].x,GameManager.PlayersColours[id][4].y, GameManager.PlayersColours[id][4].z);
            FTireRen.materials[1].color = new Color(GameManager.PlayersColours[id][5].x, GameManager.PlayersColours[id][5].y, GameManager.PlayersColours[id][5].z);
            RTireRen.materials[0].color = new Color(GameManager.PlayersColours[id][6].x, GameManager.PlayersColours[id][6].y, GameManager.PlayersColours[id][6].z);
            RTireRen.materials[1].color = new Color(GameManager.PlayersColours[id][7].x, GameManager.PlayersColours[id][7].y, GameManager.PlayersColours[id][7].z);


                StemRen.material.color = new Color(GameManager.PlayersColours[id][8].x, GameManager.PlayersColours[id][8].y, GameManager.PlayersColours[id][8].z);
               StemRen.material.SetInt("_SmoothnessTextureChannel", 0);
                StemRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][3]);
                StemRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][3]);


               FRimRen.material.color = new Color(GameManager.PlayersColours[id][9].x, GameManager.PlayersColours[id][9].y, GameManager.PlayersColours[id][9].z);
                FRimRen.material.SetInt("_SmoothnessTextureChannel", 0);
                FRimRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][4]);
                FRimRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][4]);


                RRimRen.material.color = new Color(GameManager.PlayersColours[id][10].x, GameManager.PlayersColours[id][10].y, GameManager.PlayersColours[id][10].z);
                RRimRen.material.SetInt("_SmoothnessTextureChannel", 0);
                RRimRen.material.SetFloat("_Glossiness", GameManager.PlayersSmooths[id][5]);
                RRimRen.material.SetFloat("_Metallic", GameManager.PlayersMetals[id][5]);



            }
            catch(UnityException x)
            {
                Debug.Log("Update colours error  " + x);
            }


            // textures
            try
            {
                FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
                ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
                BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
                SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
                FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
                RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
                StemRen = BMX.transform.FindDeepChild("Stem Mesh").gameObject.GetComponent<MeshRenderer>();
                FRimRen = BMX.transform.FindDeepChild("BMX:Wheel").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();
                RRimRen = BMX.transform.FindDeepChild("BMX:Wheel 1").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();

                FrameRen.material.EnableKeyword("_NORMALMAP");
                ForksRen.material.EnableKeyword("_NORMALMAP");
                BarsRen.material.EnableKeyword("_NORMALMAP");
                SeatRen.material.EnableKeyword("_NORMALMAP");
                StemRen.material.EnableKeyword("_NORMALMAP");
                FTireRen.material.EnableKeyword("_NORMALMAP");
                RTireRen.material.EnableKeyword("_NORMALMAP");
                FRimRen.material.EnableKeyword("_NORMALMAP");
                RRimRen.material.EnableKeyword("_NORMALMAP");

                if (GameManager.BikeTexinfos[id].Count > 0)
                {
                    foreach(TextureInfo t in GameManager.BikeTexinfos[id])
                    {


                        byte[] bytes = null;

                        if (t.NameofparentGameObject == "Frame Mesh")
                        {

                            if(t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                            DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.FrameDir);
                           
                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }
                            

                            if (bytes != null)
                            {
                                try
                                {
                                    Texture2D image = new Texture2D(1024, 1024);

                                    ImageConversion.LoadImage(image, bytes);
                                    image.name = t.Nameoftexture;
                                    FrameRen.material.mainTexture = image;
                                }
                                catch (System.Exception x)
                                {
                                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                    Debug.Log($"Failed to apply texture to {id}  " + x);
                                }

                            }




                            }
                            else
                            {
                                FrameRen.material.mainTexture = null; // if no registered name, take any exisiting material back off with update
                            }



                           
                        }

                        if (t.NameofparentGameObject == "Forks Mesh")
                        {
                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.ForksDir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        ForksRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                ForksRen.material.mainTexture = null;
                            }






                        }

                        if (t.NameofparentGameObject == "Stem Mesh")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Stemdir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        StemRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }

                            }
                            else
                            {
                                StemRen.material.mainTexture = null;
                            }





                        }


                        if (t.NameofparentGameObject == "Bars Mesh")
                        {


                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.BarsDir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        BarsRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }

                            }
                            else
                            {
                                BarsRen.material.mainTexture = null;
                            }





                        }


                        if (t.NameofparentGameObject == "Seat Mesh")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.SeatDir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        SeatRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                SeatRen.material.mainTexture = null;
                            }






                        }


                        if (t.NameofparentGameObject == "Front Rim")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Rimdir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        FRimRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                FRimRen.material.mainTexture = null;
                            }






                        }


                        if (t.NameofparentGameObject == "Rear Rim")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Rimdir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        RRimRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                RRimRen.material.mainTexture = null;
                            }






                        }


                        if (t.NameofparentGameObject == "Tire Mesh")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.TiresDir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        FTireRen.material.mainTexture = image;
                                        RTireRen.material.mainTexture = image;
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                FTireRen.material.mainTexture = null;
                                RTireRen.material.mainTexture = null;
                            }






                        }


                    }



                }


                if (GameManager.Bikenormalinfos[id].Count > 0)
                {
                    foreach (TextureInfo t in GameManager.Bikenormalinfos[id])
                    {
                        byte[] bytes = null;

                        if (t.NameofparentGameObject == "Frame Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Framenormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        FrameRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                FrameRen.material.SetTexture("_BumpMap", null);
                            }






                        }

                        if (t.NameofparentGameObject == "Forks Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Forksnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        ForksRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                ForksRen.material.SetTexture("_BumpMap", null);
                            }






                        }

                        if (t.NameofparentGameObject == "Stem Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Stemnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        StemRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                StemRen.material.SetTexture("_BumpMap", null);
                            }






                        }


                        if (t.NameofparentGameObject == "Bars Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Barsnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        BarsRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                BarsRen.material.SetTexture("_BumpMap", null);
                            }






                        }


                        if (t.NameofparentGameObject == "Seat Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Seatnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        SeatRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                SeatRen.material.SetTexture("_BumpMap", null);
                            }






                        }


                        if (t.NameofparentGameObject == "FRim Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Rimnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        FRimRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                FRimRen.material.SetTexture("_BumpMap", null);
                            }







                        }


                        if (t.NameofparentGameObject == "RRim Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Rimnormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        RRimRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }

                            }
                            else
                            {
                                RRimRen.material.SetTexture("_BumpMap", null);
                            }





                        }


                        if (t.NameofparentGameObject == "Tires Normal")
                        {

                            if (t.Nameoftexture != "" && t.Nameoftexture != " " && t.Nameoftexture != "e")
                            {
                                DirectoryInfo files = new DirectoryInfo(CharacterModding.instance.Tirenormaldir);

                                FileInfo[] images = files.GetFiles();
                                foreach (FileInfo f in images)
                                {
                                    if (f.Name == t.Nameoftexture)
                                    {
                                        bytes = File.ReadAllBytes(f.FullName);


                                    }
                                }


                                if (bytes != null)
                                {
                                    try
                                    {
                                        Texture2D image = new Texture2D(1024, 1024);

                                        ImageConversion.LoadImage(image, bytes);
                                        image.name = t.Nameoftexture;
                                        FTireRen.material.SetTexture("_BumpMap", image);
                                        RTireRen.material.SetTexture("_BumpMap", image);
                                    }
                                    catch (System.Exception x)
                                    {
                                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"{t.Nameoftexture} unsuitable for rebuild", 1, 1));
                                        Debug.Log($"Failed to apply texture to {id}  " + x);
                                    }

                                }
                            }
                            else
                            {
                                FTireRen.material.SetTexture("_BumpMap", null);
                                RTireRen.material.SetTexture("_BumpMap", null);
                            }






                        }


                    }



                }





            }
            catch (System.Exception x)
            {
                Debug.Log("Error with Updating Bike Textures");
            }

        }



        /// <summary>
        /// Give everything a moment before trying to go live and update
        /// </summary>
        /// <returns></returns>
        IEnumerator Initialiseafterwait()
        {
          // stagger out the initial rider build in case many are spawning at once somehow?
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f,1.5f));
            if (CurrentModelName == "Daryien")
            {
                shirtren = RiderModel.transform.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>();
                bottomsren = RiderModel.transform.Find("pants_geo").GetComponent<SkinnedMeshRenderer>();
                shoesren = RiderModel.transform.Find("shoes_geo").GetComponent<SkinnedMeshRenderer>();
                hatren = RiderModel.transform.FindDeepChild("Baseball Cap_R").GetComponent<MeshRenderer>();
                bodyren = RiderModel.transform.Find("body_geo").GetComponent<SkinnedMeshRenderer>();
                UpdateDaryien();
            }


            UpdateBike();



            // update bike textures too
           
            Debug.Log("Late init of remote rider done");
        }

       
        

       
      
    }



    /// <summary>
    /// Incoming list of received transform updates, with or without timestamp ( v2.1 support )
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