using System.Collections.Generic;
using UnityEngine;


namespace Frosty_Online_GameSide
{
    public class LocalPlayer : MonoBehaviour
    {
        private bool initsuccess;

        public GameObject Rider_Root;
        public GameObject Bmx_Root;

        private Transform[] Riders_Transforms;
        Vector3[] riderPositions;
        Vector3[] riderRotations;

        public string RiderModelname;
        GameObject ridermodel;
        





        private void Start()
        {
            

            Riders_Transforms = new Transform[32];
            riderPositions = new Vector3[32];
            riderRotations = new Vector3[32];
           initsuccess = InitialiseLocalRider();

        }

      

        private void FixedUpdate()
        {
           

            if (!Rider_Root)
            {
                Rider_Root = UnityEngine.GameObject.Find("Daryien");
               
            }
            
            



            if (Ingame_UI.instance.Connected)
            {
                if(Riders_Transforms != null && Rider_Root != null && initsuccess)
                {
            PackTransformsandSend();

                }

            }
        }

        /// <summary>Sends player Movement to the server.</summary>
        private void PackTransformsandSend()
        {
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


            ClientSend.SendMyTransforms(Riders_Transforms.Length, riderPositions,riderRotations);
        }

        // called by GUI on connect, so leaving and changing rider will fire this again. For net, if component count is less than 70 theres no extra mixamorig attached so make ridermodel name Daryien, if more, rename Ridermodelname to new character, grab new character reference and realign Rider_Transforms to the new bones
        public void RiderTracking()
        {
            if(Rider_Root.transform.parent.gameObject.GetComponentsInChildren<Transform>().Length < 70)
            {
                RiderModelname = "Daryien";
            }
            else
            {
                Transform[] objs = Rider_Root.transform.parent.gameObject.GetComponentsInChildren<Transform>();
                foreach(Transform i in objs)
                {
                    if (i.name.Contains("Clone"))
                    {
                        RiderModelname = i.name.Replace("(Clone)", "");
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
                    }
                }


               
            }
        }



        // Grabs all of Daryiens Bones on Start, and the bikes, stores in Rider_Transforms[] for sending
        public bool InitialiseLocalRider()
        {
            Rider_Root = UnityEngine.GameObject.Find("Daryien");
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



        private void OnGUI()
        {

        }

    }
}