using System.Collections.Generic;
using UnityEngine;

namespace Frosty_Online_GameSide
{
    public class RemotePlayer : MonoBehaviour
    {
        public bool MasterActive = false;
        public int id;
        public string username;
        public string CurrentModelName;

        
        public GameObject RiderModel;
        public GameObject BMX;
        private Transform[] Riders_Transforms;
        public Vector3[] Riders_positions;
        public Vector3[] Riders_rotations;
        private Rigidbody Rider_RB;
        private Rigidbody BMX_RB;
        public BikeLoadOut _bikeloadout;
        public RemotePlayerAudio Audio;

        private GameObject[] wheelcolliders;


        private bool SetupSuccess;



        // Call initiation once on start, inititation to reoccur until resolved
        private void Start()
        {
            Initialize();
        }

        
      

        public void Initialize()
        {
            // this player has just been added to server, when this pc got the message to create the prefab and attach this class, it assigned the netid of this player, username, currentmodelname and inital pos and rot
            Debug.Log($"Remote Rider { id } Initialising.....");



            // Add Audio Component, audio component will receive data from master player
            Audio = gameObject.AddComponent<RemotePlayerAudio>();
            

            // decifer the rider and bmx specifics needed especially for daryien and bike colours
           RiderModel = GameObject.Instantiate(DecideRider(CurrentModelName)) as GameObject;
           RiderModel.name = "Model " + id;
           BMX = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX"));
            BMX.name = "BMX " + id;


            // remove or disable components
            if(RiderModel.GetComponent<Animation>())
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
            // remove any triggers?


            
            
           // create reference to all transforms of rider and bike (keep Seperate vector arrays to receive last update for use in interpolation?, pull eulers instead of quats to save 30 floats)
            Riders_Transforms = new Transform[32];
            Riders_positions = new Vector3[32];
            Riders_rotations = new Vector3[32];

            
           // Once models instatiated, run findriderparts to locate and assign all ridertransforms to models and children joints of models, sets masteractive on success
            SetupSuccess = RiderSetup();


           
            

        }

       
        private void FixedUpdate()
        {
            // if masteractive, start to update transform array with values of vector3 arrays which should now be taking in updates from server
            if (MasterActive)
            {
                UpdateAllRiderParts();
            }

            // loops ridersetup until it succeeds and marks masteractive as true
            if (!MasterActive)
            {
                RiderSetup();
            }

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

        // Called by DecideRider if Currentmodelname is Daryien, will then ask for more info from server about textures
        private GameObject DaryienSetup()
        {
            GameObject daz = UnityEngine.GameObject.Find("Daryien");
            Transform[] children = daz.GetComponentsInChildren<Transform>(true);
            foreach(Transform t in children)
            {
                t.gameObject.SetActive(true);
            }

            return daz;
        }

        // Called by DecideRider if Currentmodelname isnt Daryien
        private GameObject LoadRiderFromAssets()
        {
            GameObject loadedrider;
            bool found = false;

            // check if loaded already
            IEnumerable<AssetBundle> loadedalready = AssetBundle.GetAllLoadedAssetBundles();
            foreach(AssetBundle a in loadedalready)
            {
                if (a.name.Contains(CurrentModelName))
                {
                    Debug.Log("Matched bundle to requested model");
                    loadedrider = a.LoadAsset(CurrentModelName) as GameObject;
                    found = true;

                    
                    return loadedrider;
                }
            }

            if (!found)
            {

                Debug.Log("Didnt find loaded bundle matching requested rider model");
            AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/Custom Players/" + CurrentModelName);
            
            loadedrider = b.LoadAsset(CurrentModelName) as GameObject;
                
                return loadedrider;
            }
            else
            {
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

               




               _bikeloadout = BMX.GetComponentInChildren<BikeLoadOut>();
                

                // Collision setup
                /*
                if(wheelcolliders == null)
                {
                    wheelcolliders = new GameObject[2];
                    wheelcolliders[0] = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cylinder));
                    wheelcolliders[1] = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cylinder));
                }
                wheelcolliders[0].transform.position = Riders_Transforms[28].position;
                wheelcolliders[0].transform.parent = Riders_Transforms[28];
                    wheelcolliders[0].transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
               // wheelcolliders[0].GetComponent<MeshCollider>().convex = true;
                wheelcolliders[1].transform.position = Riders_Transforms[29].position;
                wheelcolliders[1].transform.parent = Riders_Transforms[29];
                    wheelcolliders[1].transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
                    //  wheelcolliders[1].GetComponent<MeshCollider>().convex = true;


                if(Rider_RB.GetComponent<Rigidbody>() == null)
                {
                Rider_RB = RiderModel.AddComponent<Rigidbody>();
                }
                Rider_RB.isKinematic = true;

                if(BMX_RB.GetComponent<Rigidbody>() == null)
                {
                BMX_RB = BMX.AddComponent<Rigidbody>();
                }
                BMX_RB.isKinematic = true;
                */
                



                MasterActive = true;

                Debug.Log("All Remote Rider Parts Assigned");
            return true;
            }
            catch(UnityException x)
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
          //  simply update to latest stored pos and rot

            // rider
            Riders_Transforms[0].position = Riders_positions[0];
            Riders_Transforms[0].eulerAngles = Riders_rotations[0];

            for (int i = 1; i < 23; i++)
            {
                Riders_Transforms[i].localPosition = Riders_positions[i];
                Riders_Transforms[i].localEulerAngles = Riders_rotations[i];

            }

            // Bmx
            Riders_Transforms[23].position = Riders_positions[23];
            Riders_Transforms[23].eulerAngles = Riders_rotations[23];
            for (int i = 24; i < 32; i++)
            {
                Riders_Transforms[i].localPosition = Riders_positions[i];
                Riders_Transforms[i].localEulerAngles = Riders_rotations[i];
            }


        }

       


        private void OnGUI()
        {
            GUILayout.Space(50);
            GUILayout.Label($"{username} is riding");
            GUILayout.Label(_bikeloadout.currentPreset.ToString());
           
        }

       
    }
}
