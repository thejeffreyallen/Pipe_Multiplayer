using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



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
        private Transform[] Riders_Transforms;
        public Vector3[] Riders_positions;
        public Vector3[] Riders_rotations;
        private Rigidbody Rider_RB;
        private Rigidbody BMX_RB;
        public float LerpSpeed = 0.999f;
        public float timeatlasttranformupdate;

        private GameObject[] wheelcolliders;
        public RemotePlayerAudio Audio;

        private bool SetupSuccess;



        public Vector3 FrameColour = new Vector3(1, 0, 0);
        public float FrameSmooth = 0;
        public byte[] FrameTex = new byte[0];

        public Vector3 ForksColour = new Vector3(0, 0, 0);
        public float ForksSmooth = 0;
        public byte[] ForksTex = new byte[0];

        public Vector3 BarsColour = new Vector3(1, 0, 0);
        public float BarsSmooth = 0;
        public byte[] BarsTex = new byte[0];

        public Vector3 SeatColour = new Vector3(0, 0, 0);
        public float SeatSmooth = 0;
        public byte[] SeatTex = new byte[0];

        public Vector3 FTireColour = new Vector3(0, 0, 0);
        public Vector3 RTireColour = new Vector3(0, 0, 0);
        public Vector3 FTireSideColour = new Vector3(0, 0, 0);
        public Vector3 RTireSideColour = new Vector3(0, 0, 0);
        public byte[] TiresTex = new byte[0];
        public byte[] TiresNormal = new byte[0];

        public string FrameTexname = "";
        public string ForkTexname = "";
        public string SeatTexname = "";
        public string BarTexName = "";
        public string TireTexName = "";
        public string TireNormalName = "";


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
		TextMesh tm;


        void Awake()
        {
            // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[32];
            Riders_positions = new Vector3[32];
            Riders_rotations = new Vector3[32];

        }


        // Call initiation once on start, inititation to reoccur until resolved
        private void Start()
        {
           

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

            tm.color = new Color(0.8f, 0.8f, 0.8f);
            tm.fontStyle = FontStyle.Bold;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.1f;
            tm.fontSize = 20;
            tm.text = username;



        }



        public void Initialize()
        {
            // this player has just been added to server, when this pc got the message to create the prefab and attach this class, it assigned the netid of this player, username, currentmodelname and inital pos and rot
            Debug.Log($"Remote Rider { username } Initialising.....");



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




            

            


            // Once models instatiated, run findriderparts to locate and assign all ridertransforms to models and children joints of models, sets masteractive on success
            SetupSuccess = RiderSetup();

            if (!SetupSuccess)
            {
                Debug.Log("Error with Ridersetup()");
            }


            StartCoroutine(Initialiseafterwait());
        }


        private void FixedUpdate()
        {
            if (nameSign != null && RiderModel != null && Camera.current != null)
            {
			nameSign.transform.rotation = Camera.current.transform.rotation;

            }
            // if masteractive, start to update transform array with values of vector3 arrays which should now be taking in updates from server
            if (MasterActive)
            {
                UpdateAllRiderParts();
				
            }


            if (!SetupSuccess)
            {
                SetupSuccess = RiderSetup();
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
            GameObject daz = GameObject.Instantiate(UnityEngine.GameObject.Find("Daryien"));

            // make sure meshes are active, pipeworks PI keeps daryien but turns off all his meshes, then tracks new models to daryiens bones
            foreach(Transform t in daz.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
                if(t.gameObject.name == "Daryen_Hair_Matt")
                {
                    Destroy(t.gameObject);
                }
            }
          
                     return daz;
        }




        /// <summary>
        /// Called to load a custom model, will return Daryien if nothing can be done
        /// </summary>
        /// <returns></returns>
        private GameObject LoadRiderFromAssets()
        {
            GameObject loadedrider = null;
            bool found = false;


          

                IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
                foreach (AssetBundle a in bundles)
                {
                    if (a.name.Contains(Modelbundlename.ToLower()))
                    {
                        Debug.Log("Matched bundle to requested model");
                        
                        found = true;


                      loadedrider = GameObject.Instantiate(a.LoadAsset(CurrentModelName) as GameObject);
                    }
                }


                if (!found)
                {
                    Debug.Log("Didnt find loaded bundle matching requested rider model, trying files");

                    AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/Custom Players/" + CurrentModelName);
                    loadedrider = b.LoadAsset(CurrentModelName) as GameObject;
                    found = true;

                  loadedrider = GameObject.Instantiate(b.LoadAsset(CurrentModelName) as GameObject);
                }



           // change to daryien?, change to random file from Custom Players? 
              if(!found)
              {
                loadedrider = DaryienSetup();
              }

            return loadedrider;  
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
        public void UpdateAllRiderParts()
        {

            try
            {
            //  Lerp all Positions to newest stored values rotations are just set

            // rider
            Riders_Transforms[0].position = Vector3.Lerp(Riders_Transforms[0].position, Riders_positions[0], LerpSpeed);
            Riders_Transforms[0].eulerAngles = Riders_rotations[0];

            for (int i = 1; i < 23; i++)
            {
                Riders_Transforms[i].localPosition = Vector3.Lerp(Riders_Transforms[i].localPosition, Riders_positions[i], LerpSpeed);
                Riders_Transforms[i].localEulerAngles = Riders_rotations[i];

            }

            // Bmx
            Riders_Transforms[23].position = Vector3.Lerp(Riders_Transforms[23].position, Riders_positions[23], LerpSpeed);
            Riders_Transforms[23].eulerAngles = Riders_rotations[23];
            for (int i = 24; i < 32; i++)
            {
                Riders_Transforms[i].localPosition = Vector3.Lerp(Riders_Transforms[i].localPosition, Riders_positions[i], LerpSpeed);
                Riders_Transforms[i].localEulerAngles = Riders_rotations[i];
            }

            }
            catch (System.Exception x)
            {
                Debug.Log("UpdateAllRiderParts Error   : " + x);
            }


        }



        /// <summary>
        /// Call this once Gamemanager.RiderTexinfos[id] has had its names updated
        /// </summary>
        public void UpdateDaryien()
        {
            // look through rider
            if (GameManager.RiderTexinfos[id].Count > 0)
            {
                Debug.Log($"Updating Textures for {id}");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updating {username}'s Rider..", 1, 1));


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
            yield return new WaitForSeconds(Random.Range(0.5f,1.5f));
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
            MasterActive = true;
            Debug.Log("Late init of remote rider done");
        }

       
       
      
    }
}