using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;



namespace FrostyP_Game_Manager
{
    public class LocalPlayer : MonoBehaviour
    {
        public static LocalPlayer instance;

        /// <summary>
        /// Master Switch to Send continuous data from FixedUpdate
        /// </summary>
        public bool SendStream=false;
        public float DistanceThreshold = 0.01f;
        float AnglesThreshold = 1f;

        //always daryien
        public GameObject DaryienOriginal;
        public GameObject Bmx_Root;
        public GameObject RiderParentobj;
        // live model
        public GameObject ActiveModel;

        // Movement info
        public Transform[] Riders_Transforms;
        Vector3[] sendPositions;
        Vector3[] sendRotations;

        public string RiderModelname = "Daryien";
        public string RiderModelBundleName = "e";

        LocalPlayerAudio Audio;
        public System.Diagnostics.Stopwatch SendWatch = new System.Diagnostics.Stopwatch();
        DateTime LastTransformTime = DateTime.Now;

       
      

        void Awake()
        {
            instance = this;
            Riders_Transforms = new Transform[34];
            sendPositions = new Vector3[32];
            sendRotations = new Vector3[34];

        }

        private void Start()
        {
            InitialiseLocalRider();
            Audio = gameObject.AddComponent<LocalPlayerAudio>();
            Audio.Riderroot = DaryienOriginal;
        }

        private void Update()
        {
            // MUST BE IN UPDATE, LateUpdate Causes loss of something, IK movements seem to be lost? OnAnimatorIK() being used and that an issue maybe
            // Independently measures timespan to guarantee position updates are spaced appropriately
            if (MultiplayerManager.isConnected() && SendStream)
            {
                if (!SendWatch.IsRunning)
                {
                    SendWatch.Start();
                }

                if((float)SendWatch.ElapsedMilliseconds >= 16.00f)
                {
                    if (!CheckThreshold())
                    {
                        SendWatch.Reset();
                        SendWatch.Start();
                    }
                }
               
            }
            else if (SendWatch.IsRunning)
            {
                SendWatch.Stop();
            }

        }

        // Grabs all of Daryiens Bones on Start, and the bikes, stores in Rider_Transforms[] for sending
        public bool InitialiseLocalRider()
        {
            DaryienOriginal = UnityEngine.GameObject.Find("Daryien");
            RiderParentobj = DaryienOriginal.transform.parent.gameObject;
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


            Riders_Transforms[32] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHandIndex2");
            Riders_Transforms[33] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHandIndex2");
            return true;
        }

        public bool CheckThreshold()
        {
            List<float> distances = new List<float>();
            List<float> angles = new List<float>();
           
            distances.Add(Vector3.Distance(sendPositions[0], Riders_Transforms[0].position));
            bool send = false;
            for (int i = 1; i < 23 ; i++)
            {
                angles.Add(Vector3.Angle(sendRotations[i], Riders_Transforms[i].localEulerAngles));
            }
            distances.Add(Vector3.Distance(sendPositions[25], Riders_Transforms[25].localPosition));
            distances.Add(Vector3.Distance(sendPositions[27], Riders_Transforms[27].localPosition));
            for (int i = 25; i < 32; i++)
            {
                angles.Add(Vector3.Angle(sendRotations[i], Riders_Transforms[i].localEulerAngles));
            }

            for (int i = 0; i < distances.Count; i++)
            {
                if (distances[i] > DistanceThreshold)
                {
                    send = true;
                }
            }
            for (int i = 0; i < angles.Count; i++)
            {
                if (angles[i] > AnglesThreshold)
                {
                    send = true;
                }
            }

            if (send)
            {
              PackTransformsandSend();
                return true;
            }
            else
            {
                return false;
            }

        }

        public void PackTransformsandSend()
        {
           // rider
            sendPositions[0] = Riders_Transforms[0].position;
            sendRotations[0] = Riders_Transforms[0].eulerAngles;

            for (int i = 1; i < 23; i++)
            {
                sendRotations[i] = Riders_Transforms[i].localEulerAngles;
            }

            // bmx
            sendPositions[23] = Riders_Transforms[23].position;
            sendRotations[23] = Riders_Transforms[23].eulerAngles;

            sendPositions[24] = Riders_Transforms[24].position;
            sendRotations[24] = Riders_Transforms[24].eulerAngles;

            sendPositions[25] = Riders_Transforms[25].localPosition;
            sendPositions[27] = Riders_Transforms[27].localPosition;

            for (int i = 25; i < 32; i++)
            {    
               sendRotations[i] = Riders_Transforms[i].localEulerAngles;
            }

            // rot of fingers index2
            sendRotations[32] = Riders_Transforms[32].localEulerAngles;
            sendRotations[33] = Riders_Transforms[33].localEulerAngles;
            RipTideOutgoing.SendMyTransforms(sendPositions, sendRotations, DateTime.Now.ToFileTimeUtc(),SendWatch.Elapsed.TotalMilliseconds);
            SendWatch.Reset();
            SendWatch.Start();
           
        }

        public void RiderTrackingSetup()
        {

            try
            {

            // if theres less than 75 parts, the PI rig is not there, must be daryien, if not, go and get GO name and Assetbundle name as they can be different
            if (RiderParentobj.GetComponentsInChildren<Transform>().Length < 70)
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


                Riders_Transforms[32] = DaryienOriginal.transform.FindDeepChild("mixamorig:LeftHandIndex2");
                Riders_Transforms[33] = DaryienOriginal.transform.FindDeepChild("mixamorig:RightHandIndex2");


            }
            else
            {
               
                Transform[] objs = RiderParentobj.GetComponentsInChildren<Transform>();
                foreach (Transform i in objs)
                {
                    if (i.name.Contains("Clone"))
                    {
                        string inputString = i.name.Replace("(Clone)","");
                        string asciifile = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback()), Encoding.UTF8.GetBytes(inputString)));
                        asciifile = asciifile.Trim(Path.GetInvalidFileNameChars());
                        asciifile = asciifile.Trim(Path.GetInvalidPathChars());


                        RiderModelname = asciifile;
                       InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Detected Custom Model: {RiderModelname} ", FrostyUIColor.System, 0));
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


                        Riders_Transforms[32] = ActiveModel.transform.FindDeepChild("mixamorig:LeftHandIndex2");
                        Riders_Transforms[33] = ActiveModel.transform.FindDeepChild("mixamorig:RightHandIndex2");


                        IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
                        foreach (AssetBundle a in bundles)
                        {
                           
                            foreach (string name in a.GetAllAssetNames())
                            {
                                string assetinput = name;
                                string assetascii = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback()), Encoding.UTF8.GetBytes(assetinput)));
                                assetascii = assetascii.Trim(Path.GetInvalidFileNameChars());
                                assetascii = assetascii.Trim(Path.GetInvalidPathChars());

                                if (assetascii.ToLower().Contains(RiderModelname.ToLower()))
                                {
                                    string bundleinput = a.name;
                                    string bundleascii = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback()), Encoding.UTF8.GetBytes(bundleinput)));
                                    bundleascii = bundleascii.Trim(Path.GetInvalidFileNameChars());
                                    bundleascii = bundleascii.Trim(Path.GetInvalidPathChars());
                                    RiderModelBundleName = bundleascii;
                                }
                            }
                        }

                    }
                   
                }



            }
           
            }
            catch (Exception x)
            {
                Debug.Log("Rider tracking error: " + x);
            }

        }

    }
   
}