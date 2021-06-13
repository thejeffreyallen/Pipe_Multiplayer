using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;



namespace PIPE_Valve_Console_Client
{
    public class LocalPlayer : MonoBehaviour
    {
        private bool initsuccess;

        /// <summary>
        /// Master Switch to Send continuous data from FixedUpdate
        /// </summary>
        public bool ServerActive=false;
        public float MovementThreshold = 0.0001f;

        // GameObject roots: Rider_Root is always Daryien, ridermodel can be daryien or the custom rider model, in that case, Daryien is still there hes just invisible and ridermodel is using him
        public GameObject Rider_Root;
        public GameObject Bmx_Root;
        GameObject ridermodel;

        // Movement info
        private Transform[] Riders_Transforms;
        Vector3[] riderPositions;
        Vector3[] riderRotations;

        public string RiderModelname = "Daryien";
        public string RiderModelBundleName = "e";
        public List<string[]> Assetnames = new List<string[]>();

        // null unless InGameUI.Connect sees that you are Daryien, then GrabTextures is called.
        public List<TextureInfo> RiderTextureInfoList = new List<TextureInfo>();
        public List<Texture2D> RidersTextures = new List<Texture2D>();
        public List<Texture2D> BikesTextures = new List<Texture2D>();


        LocalPlayerAudio Audio;
        DateTime LastTransformTime = DateTime.Now;
       
      


        private void Start()
        {

            Riders_Transforms = new Transform[32];
            riderPositions = new Vector3[32];
            riderRotations = new Vector3[32];
            initsuccess = InitialiseLocalRider();

            Audio = gameObject.AddComponent<LocalPlayerAudio>();
            Audio.Riderroot = Rider_Root;
            
        }

        private void Update()
        {
            // MUST BE IN UPDATE, LateUpdate Causes loss of something, IK movements seem to be lost
            // Independently measures timespan and if met, creates a transform update with timestamp and stores in SendToServer thread's outbox, Aims for 60Fps
            if (InGameUI.instance.Connected && ServerActive)
            {
                if((DateTime.Now - LastTransformTime).TotalMilliseconds + (Time.deltaTime * 1000 / 3) >= 16f)
                {
                    CheckThreshold();
                    LastTransformTime = DateTime.Now;
                }
               
            }

        }
       



        // Grabs all of Daryiens Bones on Start, and the bikes, stores in Rider_Transforms[] for sending
        public bool InitialiseLocalRider()
        {
            Rider_Root = UnityEngine.GameObject.Find("Daryien");
            ridermodel = Rider_Root;
            Bmx_Root = UnityEngine.GameObject.Find("BMX");

            Riders_Transforms[0] = Rider_Root.transform;
            Riders_Transforms[1] = Rider_Root.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
            Riders_Transforms[2] = Rider_Root.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
            Riders_Transforms[3] = Rider_Root.transform.FindDeepChild("mixamorig:LeftLeg").transform;
            Riders_Transforms[4] = Rider_Root.transform.FindDeepChild("mixamorig:RightLeg").transform;
            Riders_Transforms[5] = Rider_Root.transform.FindDeepChild("mixamorig:LeftFoot").transform;
            Riders_Transforms[6] = Rider_Root.transform.FindDeepChild("mixamorig:RightFoot").transform;
            Riders_Transforms[7] = Rider_Root.transform.FindDeepChild("mixamorig:Spine").transform;
            Riders_Transforms[8] = Rider_Root.transform.FindDeepChild("mixamorig:Spine1").transform;
            Riders_Transforms[9] = Rider_Root.transform.FindDeepChild("mixamorig:Spine2").transform;
            Riders_Transforms[10] = Rider_Root.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
            Riders_Transforms[11] = Rider_Root.transform.FindDeepChild("mixamorig:RightShoulder").transform;
            Riders_Transforms[12] = Rider_Root.transform.FindDeepChild("mixamorig:LeftArm").transform;
            Riders_Transforms[13] = Rider_Root.transform.FindDeepChild("mixamorig:RightArm").transform;
            Riders_Transforms[14] = Rider_Root.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
            Riders_Transforms[15] = Rider_Root.transform.FindDeepChild("mixamorig:RightForeArm").transform;
            Riders_Transforms[16] = Rider_Root.transform.FindDeepChild("mixamorig:LeftHand").transform;
            Riders_Transforms[17] = Rider_Root.transform.FindDeepChild("mixamorig:RightHand").transform;
            Riders_Transforms[18] = Rider_Root.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
            Riders_Transforms[19] = Rider_Root.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
            Riders_Transforms[20] = Rider_Root.transform.FindDeepChild("mixamorig:Hips").transform;
            Riders_Transforms[21] = Rider_Root.transform.FindDeepChild("mixamorig:Neck").transform;
            Riders_Transforms[22] = Rider_Root.transform.FindDeepChild("mixamorig:Head").transform;


            Riders_Transforms[23] = Bmx_Root.transform;
            Riders_Transforms[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
            Riders_Transforms[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
            Riders_Transforms[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
            Riders_Transforms[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
            Riders_Transforms[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
            Riders_Transforms[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
            Riders_Transforms[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
            Riders_Transforms[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");
            return true;
        }


       
       
        /// <summary>
        /// triggers a transformupdate to be sent if any ridertransform moves more than MovementThreshold
        /// </summary>
        public void CheckThreshold()
        {
            List<float> distances = new List<float>();
           
            distances.Add(Vector3.Distance(riderPositions[0], Riders_Transforms[0].position));
            bool send = false;
            for (int i = 1; i < 23 ; i++)
            {
                distances.Add(Vector3.Distance(riderPositions[i], Riders_Transforms[i].localPosition));
            }
            distances.Add(Vector3.Distance(riderPositions[23], Riders_Transforms[23].position));
            for (int i = 24; i < 32; i++)
            {
                distances.Add(Vector3.Distance(riderPositions[i], Riders_Transforms[i].localPosition));
            }

            for (int i = 0; i < distances.Count; i++)
            {
                if (distances[i] > MovementThreshold)
                {
                    send = true;
                }
            }

            if (send)
            {
              PackTransformsandSend();
            }

        }



        /// <summary>Sends player Movement to the server.</summary>
        public void PackTransformsandSend()
        {
            //Debug.Log("Trans going out ::  ");
            // pack world pos and rot first, then for loop from 1 through all children grabbing the local pos and rot
            riderPositions[0] = Riders_Transforms[0].position;
            riderRotations[0] = Riders_Transforms[0].eulerAngles;

            for (int i = 1; i < 23; i++)
            {
                    riderPositions[i] = Riders_Transforms[i].localPosition;
                    riderRotations[i] = Riders_Transforms[i].localEulerAngles;
            }


            riderPositions[23] = Riders_Transforms[23].position;
            riderRotations[23] = Riders_Transforms[23].eulerAngles;

            for (int i = 24; i < 32; i++)
            {
                    riderPositions[i] = Riders_Transforms[i].localPosition;
                    riderRotations[i] = Riders_Transforms[i].localEulerAngles;
            }

           
           ClientSend.SendMyTransforms(Riders_Transforms.Length, riderPositions, riderRotations, DateTime.Now.ToFileTimeUtc());
           
        }






        // called by GUI on connect, so leaving and changing rider will fire this again. For net, 
        //if component count is less than 70 theres no extra mixamorig attached so make ridermodel name Daryien, if more, rename Ridermodelname to new character,
        //grab new character reference and realign Rider_Transforms to the new bones,
        // then grabs assetbundle name associated with ridermodelname, needed for checking whether,
        // the bundle is already loaded at any point and accessing that bundle if it is,
        // G-O name and filename are not reliable
        public void RiderTrackingSetup()
        {
            // if theres less than 75 parts, the PI rig is not there, must be daryien, if not, go and get GO name and Assetbundle name as they can be different
            if (Rider_Root.transform.parent.gameObject.GetComponentsInChildren<Transform>().Length < 75)
            {
                RiderModelname = "Daryien";
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Detected Daryien", 1, 0));
            }
            else
            {
               
                Transform[] objs = Rider_Root.transform.parent.gameObject.GetComponentsInChildren<Transform>();
                foreach (Transform i in objs)
                {
                    if (i.name.Contains("Clone"))
                    {
                        RiderModelname = i.name.Replace("(Clone)", "");
                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Detected Custom Model: {RiderModelname} ", 1, 0));
                        ridermodel = i.gameObject;
                        Riders_Transforms[0] = ridermodel.transform;
                        Riders_Transforms[1] = ridermodel.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
                        Riders_Transforms[2] = ridermodel.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
                        Riders_Transforms[3] = ridermodel.transform.FindDeepChild("mixamorig:LeftLeg").transform;
                        Riders_Transforms[4] = ridermodel.transform.FindDeepChild("mixamorig:RightLeg").transform;
                        Riders_Transforms[5] = ridermodel.transform.FindDeepChild("mixamorig:LeftFoot").transform;
                        Riders_Transforms[6] = ridermodel.transform.FindDeepChild("mixamorig:RightFoot").transform;
                        Riders_Transforms[7] = ridermodel.transform.FindDeepChild("mixamorig:Spine").transform;
                        Riders_Transforms[8] = ridermodel.transform.FindDeepChild("mixamorig:Spine1").transform;
                        Riders_Transforms[9] = ridermodel.transform.FindDeepChild("mixamorig:Spine2").transform;
                        Riders_Transforms[10] = ridermodel.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
                        Riders_Transforms[11] = ridermodel.transform.FindDeepChild("mixamorig:RightShoulder").transform;
                        Riders_Transforms[12] = ridermodel.transform.FindDeepChild("mixamorig:LeftArm").transform;
                        Riders_Transforms[13] = ridermodel.transform.FindDeepChild("mixamorig:RightArm").transform;
                        Riders_Transforms[14] = ridermodel.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
                        Riders_Transforms[15] = ridermodel.transform.FindDeepChild("mixamorig:RightForeArm").transform;
                        Riders_Transforms[16] = ridermodel.transform.FindDeepChild("mixamorig:LeftHand").transform;
                        Riders_Transforms[17] = ridermodel.transform.FindDeepChild("mixamorig:RightHand").transform;
                        Riders_Transforms[18] = ridermodel.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
                        Riders_Transforms[19] = ridermodel.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
                        Riders_Transforms[20] = ridermodel.transform.FindDeepChild("mixamorig:HipsCustomChar").transform;
                        Riders_Transforms[21] = ridermodel.transform.FindDeepChild("mixamorig:Neck").transform;
                        Riders_Transforms[22] = ridermodel.transform.FindDeepChild("mixamorig:Head").transform;


                        Riders_Transforms[23] = Bmx_Root.transform;
                        Riders_Transforms[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
                        Riders_Transforms[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
                        Riders_Transforms[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
                        Riders_Transforms[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
                        Riders_Transforms[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
                        Riders_Transforms[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
                        Riders_Transforms[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
                        Riders_Transforms[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");


                        IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
                        foreach (AssetBundle a in bundles)
                        {
                            string[] names = a.GetAllAssetNames();
                            Assetnames.Add(names);
                            foreach (string name in names)
                            {
                                if (name.Contains(RiderModelname.ToLower()))
                                {
                                    RiderModelBundleName = a.name;
                                }
                            }
                        }

                    }
                   
                }



            }
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Rider Tracking Done for {RiderModelname}", 1, 0));
        }


        // Called By Gui on connect if RiderTrackingSetup set name to Daryien
        public int GrabRiderTextures()
        {
            RiderTextureInfoList.Clear();
            RidersTextures.Clear();


            try
            {
            SkinnedMeshRenderer[] r = ridermodel.GetComponentsInChildren<SkinnedMeshRenderer>();

            // add all main textures and names to lists
            foreach (SkinnedMeshRenderer s in r)
            {
                foreach (Material m in s.materials)
                {

                    if(m.mainTexture != null)
                    {
                    byte[] bytes = new byte[0];
                        Texture2D t = m.mainTexture as Texture2D;


                        // just filters out default tex's as their not readable and cause errors
                    try
                    {
                       bytes = t.GetRawTextureData();
                    }
                    catch(UnityException x)
                    {
                                Debug.Log($"Error getting {t.name}: must be stock daryien tex, locked in editor:  " + x);

                    }

                        if(bytes.Length > 1 && t.name.Length > 1)
                        {
                    RiderTextureInfoList.Add(new TextureInfo(t.name,s.gameObject.name));
                    RidersTextures.Add(t);

                        }

                        
                       

                    }

                }
            }

            }
            catch (Exception x)
            {
                Debug.Log(x);
                
                return 0;
            }

            return RidersTextures.Count;
        }


        // assumes daryien textures
        public void SendTexturesToServer(List<string> names)
        {

            List<Texture2D> matchedtexs = new List<Texture2D>();


            // Search Riders list of textures
            foreach (Texture2D t in RidersTextures)
            {
                foreach (string n in names)
                {
                    // if matched, add to list of textures
                    if (t.name == n)
                    {
                        matchedtexs.Add(t);
                       
                    }
                }


            }

            // Search
            string[] files = Directory.GetFiles(CharacterModding.instance.Texturerootdir, "*.png", SearchOption.AllDirectories);
            DirectoryInfo info = new DirectoryInfo(CharacterModding.instance.Texturerootdir);
            DirectoryInfo[] dirs = info.GetDirectories();
            foreach(DirectoryInfo d in dirs)
            {
                FileInfo[] f = d.GetFiles();
                foreach(FileInfo _f in f)
                {
                    foreach(string n in names)
                    {
                        if (_f.Name.Contains(n))
                        {
                            
                            byte[] array2 = File.ReadAllBytes(_f.FullName);

                            Texture2D texture2D = new Texture2D(1024, 1024);
                            ImageConversion.LoadImage(texture2D, array2);
                            texture2D.name = name;

                            matchedtexs.Add(texture2D);
                        }
                    }
                }
            }


            foreach(Renderer r in CharacterModding.instance.BMX_Materials.Values)
            {
                foreach(string n in names)
                {
                if(r.material.mainTexture != null && r.material.mainTexture.name == n)
                {

                        matchedtexs.Add((Texture2D)r.material.mainTexture);
                }

                }
            }


           
            if (matchedtexs.Count > 0)
            {
            ClientSend.SendTextures(matchedtexs);

            }
            else
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldnt locate Textures the server wants",(int) MessageColour.Server, (uint) 1));
            }


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