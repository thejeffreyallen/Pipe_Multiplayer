﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



namespace FrostyP_PIPE_MultiPlayer
{

    public class RemotePlayer : MonoBehaviour
    {

        NetworkIdentity netid;

        bool foundBike = false;
        bool foundPlayer = false;
        bool Foundboth = false;

        Rigidbody rbofbike;
       




        GameObject remotebikeAnchor;
        Transform bikeMain;

        Transform BarsofmyBike;
        Transform Crankofmybike;
        Transform Frameofmybike;
        Transform Frontwheelofmybike;
        Transform Backwheelofmybike;
        Transform pedalLofmybike;
        Transform pedalRofmybike;



        Transform SyncingbikeAnchor;
        Transform SyncBikeMain;
        Transform SyncingbikeBars;
        Transform SyncBikeCrank;
        Transform SyncBikeFrame;
        Transform SyncBikeFrontwheel;
        Transform SyncBikeBackwheel;
        Transform SyncPedalL;
        Transform SyncPedalR;



        GameObject RiderMain;
        Transform Upperlegleft;
        Transform upperlegright;
        Transform midlegleft;
        Transform midlegright;
        Transform footleft;
        Transform footright;
        Transform spine1;
        Transform spine2;
        Transform spine3;
        Transform leftshoulder;
        Transform rightshoulder;
        Transform leftarm;
        Transform rightarm;
        Transform leftforearm;
        Transform rightforearm;
        Transform lefthand;
        Transform righthand;
        Transform leftfingers;
        Transform rightfingers;
        Transform Hips;
        Transform Neck;
        Transform Head;



        Transform SyncRiderMain;
        Transform SyncUpperlegleft;
        Transform Syncupperlegright;
        Transform Syncmidlegleft;
        Transform Syncmidlegright;
        Transform Syncfootleft;
        Transform Syncfootright;
        Transform Syncspine1;
        Transform Syncspine2;
        Transform Syncspine3;
        Transform Syncleftshoulder;
        Transform Syncrightshoulder;
        Transform Syncleftarm;
        Transform Syncrightarm;
        Transform Syncleftforearm;
        Transform Syncrightforearm;
        Transform Synclefthand;
        Transform Syncrighthand;
        Transform Syncleftfingers;
        Transform Syncrightfingers;
        Transform SyncHips;
        Transform SyncNeck;
        Transform SyncHead;



        void Start()
        {
            netid = this.gameObject.GetComponent<NetworkIdentity>();

            // instatiate a bike and man and rename, send over string name of "Online prefab"? if found, instantiate that particular man and bike
            remotebikeAnchor = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX"));
            RiderMain = GameObject.Instantiate(UnityEngine.GameObject.Find("Daryien"));

            remotebikeAnchor.gameObject.name = "Bike " + netid.netId.Value.ToString();
            RiderMain.gameObject.name = "Daryien " + netid.netId.Value.ToString();



            //-------------------------------------------------------- Collisions ----------------------------------------------------------------------------------------------
           // mesh_Frame = remotebikeAnchor.transform.FindDeepChild("Frame Mesh").gameObject.GetComponent<MeshFilter>().mesh;
           // BoxCollider frame_Collider = Frameofmybike.gameObject.AddComponent<BoxCollider>();
          //  frame_Collider.convex = true;
           // frame_Collider.sharedMesh = mesh_Frame;

            /*
            mesh_fWheel = remotebikeAnchor.transform.FindDeepChild("Tire Mesh").gameObject.GetComponent<MeshFilter>().mesh;
            MeshCollider Fwheel_Collider = Frontwheelofmybike.gameObject.AddComponent<MeshCollider>();
            Fwheel_Collider.convex = true;
            Fwheel_Collider.sharedMesh = mesh_fWheel;


            mesh_BWheel = mesh_fWheel;
            MeshCollider Bwheel_Collider = Backwheelofmybike.gameObject.AddComponent<MeshCollider>();
            Bwheel_Collider.convex = true;
            Bwheel_Collider.sharedMesh = mesh_BWheel;

            */

            BoxCollider coll = remotebikeAnchor.AddComponent<BoxCollider>();
            coll.size = new Vector3(0.2f, 0.3f, 0.5f);
            coll.center = new Vector3(0, 0.3f, 0);
            rbofbike = remotebikeAnchor.AddComponent<Rigidbody>();
            rbofbike.isKinematic = true;
            // physics interactable layer for MG
            remotebikeAnchor.layer = 25;
            // -----------------------------------------------------  collisions end --------------------------------------------------------------------------------------------------------------





            // get network synced prefab and reference every transform child to its assigned part
            SyncingbikeAnchor = this.gameObject.transform;
            SyncBikeMain = SyncingbikeAnchor.GetChild(5);

            SyncingbikeBars = SyncingbikeAnchor.GetChild(0);
            SyncBikeCrank = SyncingbikeAnchor.GetChild(1);
            SyncBikeFrame = SyncingbikeAnchor.GetChild(2);
            SyncBikeFrontwheel = SyncingbikeAnchor.GetChild(3);
            SyncBikeBackwheel = SyncingbikeAnchor.GetChild(4);
            SyncPedalL = SyncingbikeAnchor.GetChild(27);
            SyncPedalR = SyncingbikeAnchor.GetChild(28);


            SyncRiderMain = SyncingbikeAnchor.GetChild(6);
            SyncUpperlegleft = SyncingbikeAnchor.GetChild(7);
            Syncupperlegright = SyncingbikeAnchor.GetChild(8);
            Syncmidlegleft = SyncingbikeAnchor.GetChild(9);
            Syncmidlegright = SyncingbikeAnchor.GetChild(10);
            Syncfootleft = SyncingbikeAnchor.GetChild(11);
            Syncfootright = SyncingbikeAnchor.GetChild(12);
            Syncspine1 = SyncingbikeAnchor.GetChild(13);
            Syncspine2 = SyncingbikeAnchor.GetChild(14);
            Syncspine3 = SyncingbikeAnchor.GetChild(15);
            Syncleftshoulder = SyncingbikeAnchor.GetChild(16);
            Syncrightshoulder = SyncingbikeAnchor.GetChild(17);
            Syncleftarm = SyncingbikeAnchor.GetChild(18);
            Syncrightarm = SyncingbikeAnchor.GetChild(19);
            Syncleftforearm = SyncingbikeAnchor.GetChild(20);
            Syncrightforearm = SyncingbikeAnchor.GetChild(21);
            Synclefthand = SyncingbikeAnchor.GetChild(22);
            Syncrighthand = SyncingbikeAnchor.GetChild(23);
            Syncleftfingers = SyncingbikeAnchor.GetChild(24);
            Syncrightfingers = SyncingbikeAnchor.GetChild(25);
            SyncHips = SyncingbikeAnchor.GetChild(26);
            SyncNeck = SyncingbikeAnchor.GetChild(29);
            SyncHead = SyncingbikeAnchor.GetChild(30);






            /*
            Component[] compstoremove = remotebikeAnchor.GetComponentsInChildren<Component>();
            
            foreach(Component component in compstoremove)
            {
                if(component.name.Contains("Additive"))
                {
                    Destroy(component);
                }
            }
            */
        }















        void Update()
        {
            if (!foundBike)
            {
                // if new bike exisits, assign all transforms
                if (UnityEngine.GameObject.Find("Bike " + netid.netId.Value.ToString()))
                {
                    remotebikeAnchor = UnityEngine.GameObject.Find("Bike " + netid.netId.Value.ToString());
                    bikeMain = remotebikeAnchor.transform.FindDeepChild("BMX:Bike_Joint");

                    BarsofmyBike = remotebikeAnchor.transform.FindDeepChild("BMX:Bars_Joint");

                    Crankofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:DriveTrain_Joint");

                    Frameofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:Frame_Joint");

                    Frontwheelofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:Wheel");

                    Backwheelofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:Wheel 1");

                    pedalLofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:LeftPedal_Joint");

                    pedalRofmybike = remotebikeAnchor.transform.FindDeepChild("BMX:RightPedal_Joint");

                }
            }



            // if new daryien exists, get references to all transforms
            if (UnityEngine.GameObject.Find("Daryien " + netid.netId.Value.ToString()))
            {

                RiderMain.GetComponent<Animation>().enabled = false;
                RiderMain.GetComponent<SkeletonReferenceValue>().enabled = false;
                RiderMain.GetComponent<BMXLimbTargetAdjust>().enabled = false;


                Upperlegleft = RiderMain.transform.FindDeepChild("mixamorig:LeftUpLeg");
                upperlegright = RiderMain.transform.FindDeepChild("mixamorig:RightUpLeg");
                midlegleft = RiderMain.transform.FindDeepChild("mixamorig:LeftLeg");
                midlegright = RiderMain.transform.FindDeepChild("mixamorig:RightLeg");
                footleft = RiderMain.transform.FindDeepChild("mixamorig:LeftFoot");
                footright = RiderMain.transform.FindDeepChild("mixamorig:RightFoot");
                spine1 = RiderMain.transform.FindDeepChild("mixamorig:Spine");
                spine2 = RiderMain.transform.FindDeepChild("mixamorig:Spine1");
                spine3 = RiderMain.transform.FindDeepChild("mixamorig:Spine2");
                leftshoulder = RiderMain.transform.FindDeepChild("mixamorig:LeftShoulder");
                rightshoulder = RiderMain.transform.FindDeepChild("mixamorig:RightShoulder");
                leftarm = RiderMain.transform.FindDeepChild("mixamorig:LeftArm");
                rightarm = RiderMain.transform.FindDeepChild("mixamorig:RightArm");
                leftforearm = RiderMain.transform.FindDeepChild("mixamorig:LeftForeArm");
                rightforearm = RiderMain.transform.FindDeepChild("mixamorig:RightForeArm");
                lefthand = RiderMain.transform.FindDeepChild("mixamorig:LeftHand");
                righthand = RiderMain.transform.FindDeepChild("mixamorig:RightHand");
                leftfingers = RiderMain.transform.FindDeepChild("mixamorig:LeftHandIndex1");
                rightfingers = RiderMain.transform.FindDeepChild("mixamorig:RightHandIndex1");
                Hips = RiderMain.transform.FindDeepChild("mixamorig:Hips");
                Neck = RiderMain.transform.FindDeepChild("mixamorig:Neck");
                Head = RiderMain.transform.FindDeepChild("mixamorig:Head");

            }





            // remote version of rider being given transform data
            remotebikeAnchor.transform.position = SyncingbikeAnchor.position;
            remotebikeAnchor.transform.rotation = SyncingbikeAnchor.rotation;
            bikeMain.localPosition = SyncBikeMain.localPosition;
            bikeMain.localRotation = SyncBikeMain.localRotation;

            BarsofmyBike.localRotation = SyncingbikeBars.localRotation;
            BarsofmyBike.localPosition = SyncingbikeBars.localPosition;

            Crankofmybike.localRotation = SyncBikeCrank.localRotation;

            Frameofmybike.localRotation = SyncBikeFrame.localRotation;
            Frameofmybike.localPosition = SyncBikeFrame.localPosition;

            Frontwheelofmybike.localRotation = SyncBikeFrontwheel.localRotation;
            Backwheelofmybike.localRotation = SyncBikeBackwheel.localRotation;

            pedalLofmybike.localRotation = SyncPedalL.localRotation;
            pedalRofmybike.localRotation = SyncPedalR.localRotation;



            // remote version of rider being given synced transform data
            RiderMain.transform.position = SyncRiderMain.position;
            RiderMain.transform.rotation = SyncRiderMain.rotation;

            Upperlegleft.localRotation = SyncUpperlegleft.localRotation;
            Upperlegleft.localPosition = SyncUpperlegleft.localPosition;
            upperlegright.localRotation = Syncupperlegright.localRotation;
            upperlegright.localPosition = Syncupperlegright.localPosition;

            midlegleft.localRotation = Syncmidlegleft.localRotation;
            midlegleft.localPosition = Syncmidlegleft.localPosition;
            midlegright.localRotation = Syncmidlegright.localRotation;
            midlegright.localPosition = Syncmidlegright.localPosition;

            footleft.localRotation = Syncfootleft.localRotation;
            footleft.localPosition = Syncfootleft.localPosition;
            footright.localRotation = Syncfootright.localRotation;
            footright.localPosition = Syncfootright.localPosition;

            spine1.localRotation = Syncspine1.localRotation;
            spine1.localPosition = Syncspine1.localPosition;
            spine2.localRotation = Syncspine2.localRotation;
            spine2.localPosition = Syncspine2.localPosition;
            spine3.localRotation = Syncspine3.localRotation;
            spine3.localPosition = Syncspine3.localPosition;

            leftshoulder.localRotation = Syncleftshoulder.localRotation;
            leftshoulder.localPosition = Syncleftshoulder.localPosition;
            rightshoulder.localRotation = Syncrightshoulder.localRotation;
            rightshoulder.localPosition = Syncrightshoulder.localPosition;

            leftarm.localRotation = Syncleftarm.localRotation;
            leftarm.localPosition = Syncleftarm.localPosition;
            rightarm.localRotation = Syncrightarm.localRotation;
            rightarm.localPosition = Syncrightarm.localPosition;

            leftforearm.localRotation = Syncleftforearm.localRotation;
            leftforearm.localPosition = Syncleftforearm.localPosition;
            rightforearm.localRotation = Syncrightforearm.localRotation;
            rightforearm.localPosition = Syncrightforearm.localPosition;

            lefthand.localRotation = Synclefthand.localRotation;
            lefthand.localPosition = Synclefthand.localPosition;
            righthand.localRotation = Syncrighthand.localRotation;
            righthand.localPosition = Syncrighthand.localPosition;

            leftfingers.localRotation = Syncleftfingers.localRotation;
            leftfingers.localPosition = Syncleftfingers.localPosition;
            rightfingers.localRotation = Syncrightfingers.localRotation;
            rightfingers.localPosition = Syncrightfingers.localPosition;


            Hips.localPosition = SyncHips.localPosition;
            Hips.localRotation = SyncHips.localRotation;

            Neck.localRotation = SyncNeck.localRotation;
            Neck.localPosition = SyncNeck.localPosition;

            Head.localPosition = SyncHead.localPosition;
            Head.localRotation = SyncHead.localRotation;












            if (!BarsofmyBike)
            {
                Debug.Log("No bars of my bike found");
            }
            if (!SyncingbikeBars)
            {
                Debug.Log("No syncingbikebars found");
            }
            if (!SyncingbikeAnchor)
            {
                Debug.Log("No syncingbikeMain found");
            }
            if (!SyncBikeCrank)
            {
                Debug.Log("No syncingbikecrank found");
            }
            if (!SyncBikeFrame)
            {
                Debug.Log("No syncingbikeframe found");
            }
            if (!SyncBikeFrontwheel)
            {
                Debug.Log("No syncingbikefrontwheel found");
            }
            if (!SyncBikeBackwheel)
            {
                Debug.Log("No syncingbikebackwheel found");
            }
            if (!Frameofmybike)
            {
                Debug.Log("No frame of mine found");
            }
            if (!Crankofmybike)
            {
                Debug.Log("No Crank of mine found");
            }
            if (!Frontwheelofmybike)
            {
                Debug.Log("No front wheel of mine found");
            }
            if (!Backwheelofmybike)
            {
                Debug.Log("No back wheel of mine found");
            }






        }



    }
}
