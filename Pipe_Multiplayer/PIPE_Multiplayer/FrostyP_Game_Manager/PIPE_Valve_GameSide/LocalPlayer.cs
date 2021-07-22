using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;



namespace PIPE_Valve_Console_Client
{
    public class LocalPlayer : MonoBehaviour
    {
        public static LocalPlayer instance;

        /// <summary>
        /// Master Switch to Send continuous data from FixedUpdate
        /// </summary>
        public bool ServerActive=false;
        public float MovementThreshold = 0.001f;

        //always daryien
        public GameObject DaryienOriginal;
        public GameObject Bmx_Root;
        public GameObject RiderRoot;
        // live model
        public GameObject ActiveModel;

        // Movement info
        public Transform[] Riders_Transforms;
        Vector3[] riderPositions;
        Vector3[] riderRotations;

        public string RiderModelname = "Daryien";
        public string RiderModelBundleName = "e";
        public List<string[]> Assetnames = new List<string[]>();

        

        LocalPlayerAudio Audio;
        DateTime LastTransformTime = DateTime.Now;
       
      

        void Awake()
        {
            instance = this;
            Riders_Transforms = new Transform[32];
            riderPositions = new Vector3[32];
            riderRotations = new Vector3[32];

        }




        private void Start()
        {
            InitialiseLocalRider();

            Audio = gameObject.AddComponent<LocalPlayerAudio>();
            Audio.Riderroot = DaryienOriginal;
            
        }

        private void Update()
        {
            // MUST BE IN UPDATE, LateUpdate Causes loss of something, IK movements seem to be lost?
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
            DaryienOriginal = UnityEngine.GameObject.Find("Daryien");
            RiderRoot = DaryienOriginal.transform.parent.gameObject;
            ActiveModel = DaryienOriginal;
            Bmx_Root = UnityEngine.GameObject.Find("BMX");

            Riders_Transforms[0] = DaryienOriginal.transform;
            Riders_Transforms[1] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
            Riders_Transforms[2] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
            Riders_Transforms[3] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftLeg").transform;
            Riders_Transforms[4] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightLeg").transform;
            Riders_Transforms[5] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftFoot").transform;
            Riders_Transforms[6] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightFoot").transform;
            Riders_Transforms[7] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine").transform;
            Riders_Transforms[8] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine1").transform;
            Riders_Transforms[9] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine2").transform;
            Riders_Transforms[10] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
            Riders_Transforms[11] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightShoulder").transform;
            Riders_Transforms[12] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftArm").transform;
            Riders_Transforms[13] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightArm").transform;
            Riders_Transforms[14] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
            Riders_Transforms[15] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightForeArm").transform;
            Riders_Transforms[16] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHand").transform;
            Riders_Transforms[17] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHand").transform;
            Riders_Transforms[18] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
            Riders_Transforms[19] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
            Riders_Transforms[20] = DaryienOriginal.transform.FindDeepChild("mixamorig:Hips").transform;
            Riders_Transforms[21] = DaryienOriginal.transform.FindDeepChild("mixamorig:Neck").transform;
            Riders_Transforms[22] = DaryienOriginal.transform.FindDeepChild("mixamorig:Head").transform;


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
            if (RiderRoot.GetComponentsInChildren<Transform>().Length < 70)
            {
                RiderModelname = "Daryien";
                ActiveModel = DaryienOriginal;

                Riders_Transforms[0] = DaryienOriginal.transform;
                Riders_Transforms[1] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
                Riders_Transforms[2] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
                Riders_Transforms[3] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftLeg").transform;
                Riders_Transforms[4] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightLeg").transform;
                Riders_Transforms[5] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftFoot").transform;
                Riders_Transforms[6] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightFoot").transform;
                Riders_Transforms[7] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine").transform;
                Riders_Transforms[8] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine1").transform;
                Riders_Transforms[9] = DaryienOriginal.transform.FindDeepChild("mixamorig:Spine2").transform;
                Riders_Transforms[10] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
                Riders_Transforms[11] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightShoulder").transform;
                Riders_Transforms[12] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftArm").transform;
                Riders_Transforms[13] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightArm").transform;
                Riders_Transforms[14] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
                Riders_Transforms[15] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightForeArm").transform;
                Riders_Transforms[16] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHand").transform;
                Riders_Transforms[17] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHand").transform;
                Riders_Transforms[18] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
                Riders_Transforms[19] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
                Riders_Transforms[20] = DaryienOriginal.transform.FindDeepChild("mixamorig:Hips").transform;
                Riders_Transforms[21] = DaryienOriginal.transform.FindDeepChild("mixamorig:Neck").transform;
                Riders_Transforms[22] = DaryienOriginal.transform.FindDeepChild("mixamorig:Head").transform;


                Riders_Transforms[23] = Bmx_Root.transform;
                Riders_Transforms[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
                Riders_Transforms[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
                Riders_Transforms[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
                Riders_Transforms[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
                Riders_Transforms[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
                Riders_Transforms[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
                Riders_Transforms[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
                Riders_Transforms[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");


            }
            else
            {
               
                Transform[] objs = RiderRoot.GetComponentsInChildren<Transform>();
                foreach (Transform i in objs)
                {
                    if (i.name.Contains("Clone"))
                    {
                        RiderModelname = i.name.Replace("(Clone)", "");
                       // InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Detected Custom Model: {RiderModelname} ", 1, 0));
                        ActiveModel = i.gameObject;
                        Riders_Transforms[0] = ActiveModel.transform;
                        Riders_Transforms[1] = ActiveModel.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
                        Riders_Transforms[2] = ActiveModel.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
                        Riders_Transforms[3] = ActiveModel.transform.FindDeepChild("mixamorig:LeftLeg").transform;
                        Riders_Transforms[4] = ActiveModel.transform.FindDeepChild("mixamorig:RightLeg").transform;
                        Riders_Transforms[5] = ActiveModel.transform.FindDeepChild("mixamorig:LeftFoot").transform;
                        Riders_Transforms[6] = ActiveModel.transform.FindDeepChild("mixamorig:RightFoot").transform;
                        Riders_Transforms[7] = ActiveModel.transform.FindDeepChild("mixamorig:Spine").transform;
                        Riders_Transforms[8] = ActiveModel.transform.FindDeepChild("mixamorig:Spine1").transform;
                        Riders_Transforms[9] = ActiveModel.transform.FindDeepChild("mixamorig:Spine2").transform;
                        Riders_Transforms[10] = ActiveModel.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
                        Riders_Transforms[11] = ActiveModel.transform.FindDeepChild("mixamorig:RightShoulder").transform;
                        Riders_Transforms[12] = ActiveModel.transform.FindDeepChild("mixamorig:LeftArm").transform;
                        Riders_Transforms[13] = ActiveModel.transform.FindDeepChild("mixamorig:RightArm").transform;
                        Riders_Transforms[14] = ActiveModel.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
                        Riders_Transforms[15] = ActiveModel.transform.FindDeepChild("mixamorig:RightForeArm").transform;
                        Riders_Transforms[16] = ActiveModel.transform.FindDeepChild("mixamorig:LeftHand").transform;
                        Riders_Transforms[17] = ActiveModel.transform.FindDeepChild("mixamorig:RightHand").transform;
                        Riders_Transforms[18] = ActiveModel.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
                        Riders_Transforms[19] = ActiveModel.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
                        Riders_Transforms[20] = ActiveModel.transform.FindDeepChild("mixamorig:HipsCustomChar").transform;
                        Riders_Transforms[21] = ActiveModel.transform.FindDeepChild("mixamorig:Neck").transform;
                        Riders_Transforms[22] = ActiveModel.transform.FindDeepChild("mixamorig:Head").transform;


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
                                if (name.ToLower().Contains(RiderModelname.ToLower()))
                                {
                                    RiderModelBundleName = a.name;
                                }
                            }
                        }

                    }
                   
                }



            }
           // InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Rider Tracking Done for {RiderModelname}", 1, 0));
        }


       

       
        
     
       




    }
   
}