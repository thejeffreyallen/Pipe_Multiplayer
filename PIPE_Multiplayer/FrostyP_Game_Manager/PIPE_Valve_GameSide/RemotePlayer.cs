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
        public List<TextureInfo> _texinfos;


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
        public RemotePlayer instance;



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

        public GameObject sign;
        public TextMesh tm;




        // Call initiation once on start, inititation to reoccur until resolved
        public void Start()
        {
            instance = this;
            Initialize();
            sign = new GameObject("player_label");

            tm = sign.AddComponent<TextMesh>();
            tm.color = new Color(0.8f, 0.8f, 0.8f);
            
            tm.fontStyle = FontStyle.Bold;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.065f;
            tm.fontSize = 20;

        }




        public void Initialize()
        {
            // this player has just been added to server, when this pc got the message to create the prefab and attach this class, it assigned the netid of this player, username, currentmodelname and inital pos and rot
            Debug.Log($"Remote Rider { id } Initialising.....");



            // decifer the rider and bmx specifics needed especially for daryien and bike colours
            RiderModel = DecideRider(CurrentModelName);
            
            BMX = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX"));
            DontDestroyOnLoad(BMX);
            DontDestroyOnLoad(RiderModel);

            FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
            ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
            BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
            SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
            FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();

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


            // Add Audio Component, audio component will receive data from master player
            Audio = gameObject.AddComponent<RemotePlayerAudio>();
            Audio.Rider = RiderModel;

            // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[32];
            Riders_positions = new Vector3[32];
            Riders_rotations = new Vector3[32];


            // Once models instatiated, run findriderparts to locate and assign all ridertransforms to models and children joints of models, sets masteractive on success
            SetupSuccess = RiderSetup();




            StartCoroutine(Initialiseafterwait());
        }


        private void LateUpdate()
        {
            // if masteractive, start to update transform array with values of vector3 arrays which should now be taking in updates from server
            if (MasterActive)
            {
                UpdateAllRiderParts();
                tm.text = username;
                sign.transform.rotation = Camera.main.transform.rotation; // Causes the text faces camera.
                sign.transform.position = RiderModel.transform.position + Vector3.up * 1.8f;

            }

            // loops ridersetup until it succeeds and marks masteractive as true
           

           

        }




        // decides whether to get daryien and start the texture process, or grab a custom model, Gives back gameobject to Initialise for instantiation
        private GameObject DecideRider(string modelname)
        {
            GameObject Rider;

            if (modelname == "Daryien")
            {
                Rider = DaryienSetup();
                return Rider;
            }
            else
            {
                Rider = LoadRiderFromAssets();
                return Rider;
            }
        }

        // Called by DecideRider if Currentmodelname is Daryien
        private GameObject DaryienSetup()
        {
            GameObject daz = GameObject.Instantiate(UnityEngine.GameObject.Find("Daryien"));

            // make sure meshes are active, pipeworks PI keeps daryien but turns off all his meshes, then tracks new models to daryiens bones
            Transform[] children = daz.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in children)
            {
                t.gameObject.SetActive(true);

            }
            SkinnedMeshRenderer[] r = daz.GetComponentsInChildren<SkinnedMeshRenderer>();

            // find all files that match with custom tex names,then load
            foreach (SkinnedMeshRenderer s in r)
            {
                if(_texinfos != null)
                {
                foreach (TextureInfo t in _texinfos)
                {
                    byte[] bytes = null;
                    if (s.gameObject.name == t.NameofparentGameObject)
                    {
                        
                        DirectoryInfo[] files = new DirectoryInfo(GameManager.instance.TexturesRootdir).GetDirectories();
                        foreach(DirectoryInfo i in files)
                        {
                            FileInfo[] images = i.GetFiles();
                            foreach(FileInfo f in images)
                            {
                                if(f.Name == t.Nameoftexture)
                                {
                                   bytes = File.ReadAllBytes(f.FullName);

                                }
                            }
                        }

                        if (bytes != null)
                        {
                            try
                            {
                        Texture2D image = new Texture2D(2, 2);
                       image.LoadImage(bytes);
                            image.Apply();
                        s.material.mainTexture = image;
                            }
                            catch (UnityException x)
                            {
                                Debug.Log("Daryien setup error : " + x);
                            }

                        }
                    }
                    


                }
                }
            }

                    return daz;
        }

        /// <summary>
        /// Called to load a custom model, will return Daryiensetup() if nothing can be done
        /// </summary>
        /// <returns></returns>
        private GameObject LoadRiderFromAssets()
        {
            GameObject loadedrider;
            bool found = false;



            IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in bundles)
            {
                    if (a.name.Contains(Modelbundlename.ToLower()))
                    {
                        Debug.Log("Matched bundle to requested model");
                        loadedrider = GameObject.Instantiate(a.LoadAsset(CurrentModelName) as GameObject);
                        found = true;


                        return loadedrider;
                    }
            }


            if (!found)
            {
                AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/Custom Players/" + CurrentModelName);
            IEnumerable<AssetBundle> loadedalready = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in loadedalready)
            {
                if (a.name.Contains(Modelbundlename))
                {
                    Debug.Log("Matched bundle to requested model");
                    loadedrider = GameObject.Instantiate(a.LoadAsset(CurrentModelName) as GameObject);
                    found = true;


                    return loadedrider;
                }
            }

            }
            

            if (!found)
            {

                Debug.Log("Didnt find loaded bundle matching requested rider model");
                AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/Custom Players/" + CurrentModelName);

                loadedrider = b.LoadAsset(CurrentModelName) as GameObject;
                found = true;
                return loadedrider;
            }
            else if(!found)
            {
                return DaryienSetup();
            }
            else
            {
                return DaryienSetup();
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
                // Result: if your still and your RB is asleep your a brick wall, if your moving, both riders take impact.

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



                if (RiderModel.GetComponent<Rigidbody>() == null)
                {
                    Rider_RB = RiderModel.AddComponent<Rigidbody>();
                }
                Rider_RB.isKinematic = true;

                if (BMX.GetComponent<Rigidbody>() == null)
                {
                    BMX_RB = BMX.AddComponent<Rigidbody>();
                }
                BMX_RB.isKinematic = true;



                // complete, send message that i have arrived and set masteractive true so i start to update myself

               
                MasterActive = true;

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


        public void UpdateTextures()
        {
            // look through rider
            if (GameManager.PlayersTexinfos[id].Count > 0)
            {
                Debug.Log($"Updating Textures for {id}");









            SkinnedMeshRenderer[] r = RiderModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer s in r)
            {
                foreach (TextureInfo t in GameManager.PlayersTexinfos[id])
                {
                    byte[] bytes = null;
                    if (s.gameObject.name == t.NameofparentGameObject)
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
                            s.material.mainTexture = image;
                                }
                                catch (System.Exception x)
                                {
                                    Debug.Log($"Failed to apply texture to {id}");
                                }

                        }
                    }



                }
            }

            }

            // look through bike
            if(_texinfos.Count > 0)
            {

            }


        }



        public void UpdateColours()
        {
            try
            {
            FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
            ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
            BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
            SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
            FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();


            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Updating {username}'s Bike Colours",(int)MessageColour.System,(uint)0));
            FrameRen.material.color = new Color(GameManager.PlayersColours[id][0].x, GameManager.PlayersColours[id][0].y, GameManager.PlayersColours[id][0].z);
           ForksRen.material.color = new Color(GameManager.PlayersColours[id][1].x, GameManager.PlayersColours[id][1].y, GameManager.PlayersColours[id][1].z);
            BarsRen.material.color = new Color(GameManager.PlayersColours[id][2].x, GameManager.PlayersColours[id][2].y, GameManager.PlayersColours[id][2].z);
            SeatRen.material.color = new Color(GameManager.PlayersColours[id][3].x, GameManager.PlayersColours[id][3].y, GameManager.PlayersColours[id][3].z);
            FTireRen.materials[1].color = new Color(GameManager.PlayersColours[id][4].x,GameManager.PlayersColours[id][4].y, GameManager.PlayersColours[id][4].z);
            FTireRen.materials[0].color = new Color(GameManager.PlayersColours[id][5].x, GameManager.PlayersColours[id][5].y, GameManager.PlayersColours[id][5].z);
            RTireRen.materials[1].color = new Color(GameManager.PlayersColours[id][6].x, GameManager.PlayersColours[id][6].y, GameManager.PlayersColours[id][6].z);
            RTireRen.materials[0].color = new Color(GameManager.PlayersColours[id][7].x, GameManager.PlayersColours[id][7].y, GameManager.PlayersColours[id][7].z);

            }
            catch(UnityException x)
            {
                Debug.Log("Update colours error  " + x);
            }



        }



        IEnumerator Initialiseafterwait()
        {

            yield return new WaitForSeconds(2);
            RiderModel.name = "Model " + id;
            BMX.name = "BMX " + id;
            Destroy(BMX.transform.Find("BMX Load Out").gameObject);
            FrameRen = BMX.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
            ForksRen = BMX.transform.FindDeepChild("Forks Mesh").gameObject.GetComponent<MeshRenderer>();
            BarsRen = BMX.transform.FindDeepChild("Bars Mesh").gameObject.GetComponent<MeshRenderer>();
            SeatRen = BMX.transform.FindDeepChild("Seat Mesh").gameObject.GetComponent<MeshRenderer>();
            FTireRen = BMX.transform.FindDeepChild("BMX:Wheel").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            RTireRen = BMX.transform.FindDeepChild("BMX:Wheel 1").gameObject.transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
            UpdateTextures();
            UpdateColours();
            Debug.Log("Late init of remote rider done");
            yield return null;
        }


    }
}