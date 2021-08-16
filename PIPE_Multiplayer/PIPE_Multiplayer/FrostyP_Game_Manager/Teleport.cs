using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{
    public class Teleport : MonoBehaviour
    {
        public static Teleport instance;
        GameObject RiderModel;
        GameObject Bmx_Root;
        MGInputManager mg;
        GameObject Cam;
        bool isOpen;
        Transform[] MyRidersTrans;
        Vector3[] DefaultPos;
        Vector3[] DefaultRot;
        float movespeed = 7;
        GUIStyle labels = new GUIStyle();
        GUIStyle boxstyle = new GUIStyle();


        void Awake()
        {
            labels.fixedWidth = 100;
            labels.fontStyle = FontStyle.Bold;
            labels.normal.textColor = Color.black;

            boxstyle.padding = new RectOffset(10, 10, 5, 5);
            boxstyle.normal.background = InGameUI.instance.whiteTex;
            boxstyle.alignment = TextAnchor.MiddleCenter;

            MyRidersTrans = new Transform[32];
            DefaultRot = new Vector3[32];
            DefaultPos = new Vector3[32];
            instance = this;
            Cam = Instantiate(Camera.main.gameObject);
            DontDestroyOnLoad(Cam);
            Cam.SetActive(false);

        }
        void Start()
        {
           StartCoroutine(GrabDefault());
        }





        void Update()
        {
            if (isOpen)
            {
                // lock menu till A press
                FrostyPGamemanager.instance.MenuShowing = 2;
                Controls();
            }
        }



        public void Show()
        {
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 10 + (Screen.height/50)), new Vector2(Screen.width / 2, Screen.height / 20)),boxstyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("LB/RB Rotate : A to Place : Pad shortcut (Xbox) - LT + RT + RS hold then LS ",labels);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Movespeed",labels);
            movespeed = GUILayout.HorizontalSlider(movespeed, 4, 15);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }


        void Controls()
        {


            Cam.transform.LookAt(RiderModel.transform.FindDeepChild("mixamorig:Head"));

            Cam.transform.position = Vector3.Lerp(Cam.transform.position, RiderModel.transform.position + (RiderModel.transform.up * 2) + (-RiderModel.transform.forward * 4), movespeed * Time.deltaTime);

            if(MGInputManager.LStickX()>0.1f | MGInputManager.LStickX() < -0.1f | MGInputManager.LStickY() > 0.1f | MGInputManager.LStickY() < -0.1f | MGInputManager.RStickX() > 0.1f | MGInputManager.RStickX() < -0.1f | MGInputManager.RStickY() > 0.1f | MGInputManager.RStickY() < -0.1f)
            {

            RiderModel.transform.Translate(MGInputManager.LStickX() * Time.deltaTime * movespeed, MGInputManager.RStickY() * Time.deltaTime * movespeed, MGInputManager.LStickY() * Time.deltaTime * movespeed);

            }
          

            if (MGInputManager.RB_Down())
            {
                RiderModel.transform.Rotate(0, 22.5f, 0);
            }
            if (MGInputManager.LB_Down())
            {
                RiderModel.transform.Rotate(0, -22.5f, 0);
            }



            if (MGInputManager.A_Down())
            {
                SessionMarker marker = FindObjectOfType<SessionMarker>();
                marker.marker.position = RiderModel.transform.position;
                marker.marker.rotation = RiderModel.transform.rotation;
                Close();
                marker.ResetPlayerAtMarker();


            }

        }




        public void Open()
        {
            if (isOpen)
            {
                return;
            }
            RiderModel = GetPlayer();
            Bmx_Root = GetBmx();
            Vector3 currentpos = LocalPlayer.instance.ActiveModel.transform.position;
            Vector3 currentrot = LocalPlayer.instance.ActiveModel.transform.eulerAngles;
            Cam.transform.position = LocalPlayer.instance.ActiveModel.transform.position + (-LocalPlayer.instance.ActiveModel.transform.transform.forward * 2) + (LocalPlayer.instance.ActiveModel.transform.transform.up * 2);
            StartCoroutine(SetupPlayer(currentpos,currentrot));

            GameManager.TogglePlayerComponents(false);

            mg = new MGInputManager();




            
            Cam.SetActive(true);
            isOpen = true;
            FrostyPGamemanager.instance.MenuShowing = 2;

        }
        public void Close()
        {
            Destroy(RiderModel);
            Destroy(Bmx_Root);
            GameManager.TogglePlayerComponents(true);
            mg = null;
            Cam.SetActive(false);
            FrostyPGamemanager.instance.MenuShowing = 0;
            isOpen = false;

        }



        IEnumerator SetupPlayer(Vector3 pos,Vector3 rot)
        {
           
            while (RiderModel == null && Bmx_Root == null)
            {
                yield return new WaitForFixedUpdate();
            }
            MyRidersTrans[0] = RiderModel.transform;
            MyRidersTrans[1] = RiderModel.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
            MyRidersTrans[2] = RiderModel.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
            MyRidersTrans[3] = RiderModel.transform.FindDeepChild("mixamorig:LeftLeg").transform;
            MyRidersTrans[4] = RiderModel.transform.FindDeepChild("mixamorig:RightLeg").transform;
            MyRidersTrans[5] = RiderModel.transform.FindDeepChild("mixamorig:LeftFoot").transform;
            MyRidersTrans[6] = RiderModel.transform.FindDeepChild("mixamorig:RightFoot").transform;
            MyRidersTrans[7] = RiderModel.transform.FindDeepChild("mixamorig:Spine").transform;
            MyRidersTrans[8] = RiderModel.transform.FindDeepChild("mixamorig:Spine1").transform;
            MyRidersTrans[9] = RiderModel.transform.FindDeepChild("mixamorig:Spine2").transform;
            MyRidersTrans[10] = RiderModel.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
            MyRidersTrans[11] = RiderModel.transform.FindDeepChild("mixamorig:RightShoulder").transform;
            MyRidersTrans[12] = RiderModel.transform.FindDeepChild("mixamorig:LeftArm").transform;
            MyRidersTrans[13] = RiderModel.transform.FindDeepChild("mixamorig:RightArm").transform;
            MyRidersTrans[14] = RiderModel.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
            MyRidersTrans[15] = RiderModel.transform.FindDeepChild("mixamorig:RightForeArm").transform;
            MyRidersTrans[16] = RiderModel.transform.FindDeepChild("mixamorig:LeftHand").transform;
            MyRidersTrans[17] = RiderModel.transform.FindDeepChild("mixamorig:RightHand").transform;
            MyRidersTrans[18] = RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
            MyRidersTrans[19] = RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
            MyRidersTrans[20] = RiderModel.transform.FindDeepChild("mixamorig:Hips").transform;
            MyRidersTrans[21] = RiderModel.transform.FindDeepChild("mixamorig:Neck").transform;
            MyRidersTrans[22] = RiderModel.transform.FindDeepChild("mixamorig:Head").transform;

            MyRidersTrans[23] = Bmx_Root.transform;
            MyRidersTrans[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
            MyRidersTrans[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
            MyRidersTrans[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
            MyRidersTrans[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
            MyRidersTrans[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
            MyRidersTrans[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
            MyRidersTrans[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
            MyRidersTrans[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");



            for (int i = 0; i < 23; i++)
            {
                MyRidersTrans[i].localPosition = DefaultPos[i];
                MyRidersTrans[i].localEulerAngles = DefaultRot[i];
            }

            for (int i = 24; i < 32; i++)
            {
                MyRidersTrans[i].localPosition = DefaultPos[i];
                MyRidersTrans[i].localEulerAngles = DefaultRot[i];
            }
            RiderModel.transform.position = pos;
            Bmx_Root.transform.position = pos;
            RiderModel.transform.eulerAngles = new Vector3(0, rot.y, 0);
            Bmx_Root.transform.eulerAngles = new Vector3(0, rot.y, 0);
            Bmx_Root.transform.parent = RiderModel.transform;
            yield return null;


        }


        IEnumerator GrabDefault()
        {
            yield return new WaitForSeconds(1f);



            GameObject RiderModel = GameObject.Find("Daryien");
            GameObject Bmx_Root = GameObject.Find("BMX");

            while (RiderModel == null && Bmx_Root == null)
            {
                yield return new WaitForFixedUpdate();
            }
            MyRidersTrans[0] = RiderModel.transform;
            MyRidersTrans[1] = RiderModel.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
            MyRidersTrans[2] = RiderModel.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
            MyRidersTrans[3] = RiderModel.transform.FindDeepChild("mixamorig:LeftLeg").transform;
            MyRidersTrans[4] = RiderModel.transform.FindDeepChild("mixamorig:RightLeg").transform;
            MyRidersTrans[5] = RiderModel.transform.FindDeepChild("mixamorig:LeftFoot").transform;
            MyRidersTrans[6] = RiderModel.transform.FindDeepChild("mixamorig:RightFoot").transform;
            MyRidersTrans[7] = RiderModel.transform.FindDeepChild("mixamorig:Spine").transform;
            MyRidersTrans[8] = RiderModel.transform.FindDeepChild("mixamorig:Spine1").transform;
            MyRidersTrans[9] = RiderModel.transform.FindDeepChild("mixamorig:Spine2").transform;
            MyRidersTrans[10] = RiderModel.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
            MyRidersTrans[11] = RiderModel.transform.FindDeepChild("mixamorig:RightShoulder").transform;
            MyRidersTrans[12] = RiderModel.transform.FindDeepChild("mixamorig:LeftArm").transform;
            MyRidersTrans[13] = RiderModel.transform.FindDeepChild("mixamorig:RightArm").transform;
            MyRidersTrans[14] = RiderModel.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
            MyRidersTrans[15] = RiderModel.transform.FindDeepChild("mixamorig:RightForeArm").transform;
            MyRidersTrans[16] = RiderModel.transform.FindDeepChild("mixamorig:LeftHand").transform;
            MyRidersTrans[17] = RiderModel.transform.FindDeepChild("mixamorig:RightHand").transform;
            MyRidersTrans[18] = RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
            MyRidersTrans[19] = RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
            MyRidersTrans[20] = RiderModel.transform.FindDeepChild("mixamorig:Hips").transform;
            MyRidersTrans[21] = RiderModel.transform.FindDeepChild("mixamorig:Neck").transform;
            MyRidersTrans[22] = RiderModel.transform.FindDeepChild("mixamorig:Head").transform;

            MyRidersTrans[23] = Bmx_Root.transform;
            MyRidersTrans[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
            MyRidersTrans[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
            MyRidersTrans[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
            MyRidersTrans[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
            MyRidersTrans[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
            MyRidersTrans[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
            MyRidersTrans[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
            MyRidersTrans[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");



            Vector3[] pos = new Vector3[32];
            Vector3[] rot = new Vector3[32];

            pos[0] = new Vector3(0, 0, 0);
            rot[0] = new Vector3(0, 0, 0);
            for (int i = 1; i < 23; i++)
            {
                pos[i] = MyRidersTrans[i].localPosition;
                rot[i] = MyRidersTrans[i].localEulerAngles;
            }
            pos[23] = new Vector3(0, 0, 0);
            rot[23] = new Vector3(0, 0, 0);
            for (int i = 24; i < 32; i++)
            {
                pos[i] = MyRidersTrans[i].localPosition;
                rot[i] = MyRidersTrans[i].localEulerAngles;
            }

            Array.Copy(pos, DefaultPos, pos.Length);
            Array.Copy(rot, DefaultRot, rot.Length);


            yield return null;

        }


        GameObject GetPlayer()
        {
            LocalPlayer.instance.RiderTrackingSetup();
            Debug.Log("Making player");
            GameObject _riderModel = null;
            if (LocalPlayer.instance.RiderModelname == "Daryien")
            {
                _riderModel = Instantiate(LocalPlayer.instance.ActiveModel);
            }
            else
            {
                _riderModel = GameManager.GetPlayerModel(LocalPlayer.instance.RiderModelname, LocalPlayer.instance.RiderModelBundleName);
            }


            if (_riderModel.GetComponent<Animation>())
            {
                _riderModel.GetComponent<Animation>().enabled = false;
            }
            if (_riderModel.GetComponent<BMXLimbTargetAdjust>())
            {
                _riderModel.GetComponent<BMXLimbTargetAdjust>().enabled = false;

            }
            if (_riderModel.GetComponent<SkeletonReferenceValue>())
            {
                _riderModel.GetComponent<SkeletonReferenceValue>().enabled = false;

            }
            // remove any triggers, Ontrigger events will cause local player to bail
            foreach (Transform t in _riderModel.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("Trigger"))
                {
                    Destroy(t.gameObject);
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }


                t.gameObject.SetActive(true);
            }
            if (_riderModel.GetComponent<Rigidbody>())
            {
                _riderModel.GetComponent<Rigidbody>().isKinematic = true;
            }

            return _riderModel;
        }

        GameObject GetBmx()
        {
            GameObject Bmx_Root = Instantiate(PIPE_Valve_Console_Client.LocalPlayer.instance.Bmx_Root);
            if (Bmx_Root.GetComponent<Rigidbody>())
            {
                Bmx_Root.GetComponent<Rigidbody>().isKinematic = true;
            }

            foreach (Transform t in Bmx_Root.GetComponentsInChildren<Transform>())
            {
                t.gameObject.SetActive(true);

                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
                if (t.gameObject.GetComponent<BikeLoadOut>())
                {
                    t.gameObject.GetComponent<BikeLoadOut>().enabled = false;
                }
                if (t.gameObject.GetComponent<Hub>())
                {
                    t.gameObject.GetComponent<Hub>().enabled = false;
                }


            }

            return Bmx_Root;

        }




    }
}
