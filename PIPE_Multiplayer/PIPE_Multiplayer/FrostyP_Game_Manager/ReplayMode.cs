using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{
    class ReplayMode : MonoBehaviour
    {
        public static ReplayMode instance;
        GUIStyle BottomPanelStyle = new GUIStyle();
        public GameObject ReplayCam;
        MGInputManager _MG;
        GameObject MyMan;
        GameObject MyBmx;
        Vector3[] Currentpositions;
        Vector3[] Currentrotations;
        Transform[] MyRidersTrans;
        public static List<ReplayPosition> MyPlayersPoisitions = new List<ReplayPosition>();
        public static List<ReplayPosition> CamMarkers = new List<ReplayPosition>();
        public bool ReplayOpen = false;
        public bool Tracking = false;
        public int CurrentShowingPosition;
        public int CurrentCamPosition;
        public int CurrentCamTargetPosition = 1;
        public bool PlayThrough;
        public int StartFrame;
        public int EndFrame;
        public bool PlayEditToggle;
        public string ModeDisplay = "Play Mode";
        public bool OpenCamSettings;

        float PercentAong;
        Vector3 pos;
        int framestotal;
        float _time;
        float camspeed;
        float Playspeed;


        // Camera position based on frame and markers
        Vector3 currentrot;
        Vector3 Targetrot;
        Vector3 RotAxis;
        float angle;

        Vector3 Rot;




        public void Open()
        {
            Debug.Log("Replay opening.");
            if (InGameUI.instance.Connected)
            {
                GameManager.instance.TurnOffNetUpdates();
            }


            StartFrame = 0;
            EndFrame = MyPlayersPoisitions.Count - 1;
            

            
            ReplayCam.SetActive(true);
            ReplayCam.transform.position = Camera.main.gameObject.transform.position;
            ReplayCam.transform.rotation = Camera.main.gameObject.transform.rotation;

            CurrentShowingPosition = MyPlayersPoisitions.Count - 1;
            MyMan = GetPlayer();
            MyBmx = GetBmx();
            MyMan.transform.position = LocalPlayer.instance.ActiveModel.transform.position;
            MyBmx.transform.position = LocalPlayer.instance.Bmx_Root.transform.position;

            Debug.Log("Player created");
            GameManager.TogglePlayerComponents(false);
            _MG = new MGInputManager();




            // if online, pause comms but send keepalive packets


            ReplayOpen = true;
            Debug.Log("open");
        }
        public void Close()
        {
            if (InGameUI.instance.Connected)
            {
                GameManager.instance.TurnOnNetUpdates();
            }
            Destroy(MyMan);
            Destroy(MyBmx);
            GameManager.TogglePlayerComponents(true);
            ReplayCam.SetActive(false);
            ReplayOpen = false;
            _MG = null;
            FrostyPGamemanager.instance.MenuShowing = 0;
            FrostyPGamemanager.instance.OpenMenu = true;
        }


        void PlayerFreeCam()
        {
            camspeed = 5;

            if(MGInputManager.LStickX() > 0.2f | MGInputManager.LStickX() < -0.2f | MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() <-0.2f | MGInputManager.RStickX() > 0.2f | MGInputManager.RStickX() < -0.2f | MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
            {
            ReplayCam.transform.Translate(MGInputManager.LStickX()* Time.deltaTime * camspeed, MGInputManager.RStickY() * Time.deltaTime * camspeed, MGInputManager.LStickY() * Time.deltaTime * camspeed);
            ReplayCam.transform.Rotate(0, MGInputManager.RStickX() * Time.deltaTime * camspeed, 0);
            }
            ReplayCam.transform.LookAt(MyMan.transform.FindDeepChild("mixamorig:Head"));
            
        }

        void TriggerScrollEditMode()
        {
           
            if (MGInputManager.LTrigger() > 0.1f)
            {
                if (Vector3.Distance(MyRidersTrans[0].position, MyPlayersPoisitions[CurrentShowingPosition].Positions[0])<0.1f)
                {
                    if(CurrentShowingPosition<= StartFrame)
                    {
                        CurrentShowingPosition = StartFrame;
                    }
                    else
                    {
                        CurrentShowingPosition--;
                    }
                }
               
            }
            if (MGInputManager.RTrigger() > 0.1f)
            {
                if (Vector3.Distance(MyRidersTrans[0].position, MyPlayersPoisitions[CurrentShowingPosition].Positions[0]) < 0.1f)
                {
                    if (CurrentShowingPosition >= EndFrame)
                    {
                        CurrentShowingPosition = EndFrame;
                    }
                    else
                    {
                        CurrentShowingPosition++;
                    }
                }
            }

            

        }
        void TriggerScrollPlayMode()
        {
            if (CamMarkers.Count > CurrentCamTargetPosition)
            {
                // position and rotation of cam relative to current showing frame and current markers


                // position
                Vector3 PosDir;
                Vector3 currentpos = CamMarkers[CurrentCamPosition].CamPos;
                Vector3 targetpos = CamMarkers[CurrentCamTargetPosition].CamPos;
                

               
                framestotal = CamMarkers[CurrentCamPosition].ReferenceFrame > CamMarkers[CurrentCamTargetPosition].ReferenceFrame ? CamMarkers[CurrentCamPosition].ReferenceFrame - CamMarkers[CurrentCamTargetPosition].ReferenceFrame : CamMarkers[CurrentCamTargetPosition].ReferenceFrame - CamMarkers[CurrentCamPosition].ReferenceFrame;
                PercentAong = (float)(CurrentShowingPosition - CamMarkers[CurrentCamPosition].ReferenceFrame)/framestotal;



                PosDir = (targetpos - currentpos).normalized;
                pos = PosDir * Vector3.Distance(currentpos,targetpos) * PercentAong;

                ReplayCam.transform.position = Vector3.Lerp(currentpos, currentpos + pos, 1);

                // rotation
                currentrot = CamMarkers[CurrentCamPosition].CamRot;
                Targetrot = CamMarkers[CurrentCamTargetPosition].CamRot;
                RotAxis =  Vector3.Cross(Targetrot,currentrot).normalized;
                angle = Vector3.SignedAngle(currentrot, Targetrot,RotAxis);
                Quaternion angleaxis = Quaternion.AngleAxis(angle, RotAxis);

                Vector3 Rot = RotAxis * (float)(angle * PercentAong);

                ReplayCam.transform.eulerAngles = Vector3.Lerp(currentrot,currentrot + Rot,1);


                if (MGInputManager.LTrigger() > 0.1f)
                {
                    if (Vector3.Distance(MyRidersTrans[0].position, MyPlayersPoisitions[CurrentShowingPosition].Positions[0]) < 0.1f)
                    {
                        if (CurrentShowingPosition > StartFrame)
                        {
                            CurrentShowingPosition--;
                        }
                    }
                    if (CurrentShowingPosition < CamMarkers[CurrentCamPosition].ReferenceFrame)
                    {
                        if (CurrentCamPosition > 0)
                        {
                            CurrentCamPosition--;
                            CurrentCamTargetPosition--;
                        }
                    }

                }




                if (MGInputManager.RTrigger() > 0.1f)
                {

                    if (Vector3.Distance(MyRidersTrans[0].position, MyPlayersPoisitions[CurrentShowingPosition].Positions[0]) < 0.1f)
                    {
                        if (CurrentShowingPosition < EndFrame)
                        {
                            CurrentShowingPosition++;
                        }
                    }

                    if (CurrentShowingPosition > CamMarkers[CurrentCamTargetPosition].ReferenceFrame)
                    {
                        if (CurrentCamTargetPosition < CamMarkers.Count-1)
                        {
                            CurrentCamPosition++;
                            CurrentCamTargetPosition++;

                        }
                    }

                }

                





            }
        }


        void MainWindow()
        {
            GUILayout.BeginArea(new Rect(new Vector2(10,10), new Vector2(Screen.width/1.2f,Screen.height/10)));
            GUILayout.BeginHorizontal();
            PlayEditToggle = GUILayout.Toggle(PlayEditToggle, ModeDisplay);
            if (!PlayEditToggle)
            {
                ModeDisplay = "Switch to Play Mode";
                GUILayout.Label($"{CamMarkers.Count} markers set");

            if (GUILayout.Button("Remove all markers"))
            {
                    CamMarkers.Clear();
            }
            if (GUILayout.Button("Remove marker"))
            {

            }
            if (GUILayout.Button("Cam Settings"))
            {
             OpenCamSettings = !OpenCamSettings;
                    
            }
            if (GUILayout.Button("something"))
            {

            }
            if (GUILayout.Button("Close"))
            {
                OpenCamSettings = false;
                Close();
            }

            }
            if (PlayEditToggle)
            {
                ModeDisplay = "Switch to Edit Mode";
                
                if (GUILayout.Button("Play"))
                {
                    PlayThrough = true;
                }

                GUILayout.Label(currentrot.ToString() + "<CRot");
                GUILayout.Label(Targetrot.ToString() + "<Trot");
                GUILayout.Label(RotAxis.ToString() + "<RotAxis");
                GUILayout.Label(angle.ToString() + "<angle");
                GUILayout.Label(Rot.ToString() + "<FinalRot");
                GUILayout.Label($"{PercentAong} percentalong");
                GUILayout.Label($"{pos.ToString()} pos");
                GUILayout.Label($"{framestotal} frame span");
                
                GUILayout.Label($"{CurrentShowingPosition} showing frame");


            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
           

            // bottom panel
            if (!PlayEditToggle)
            {
                BottomPanel();
            }


        }
        void BottomPanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(150, Screen.height - (Screen.height/20) - 50), new Vector2(Screen.width - 300, Screen.height / 20)),InGameUI.BoxStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("current cam pos:" + CurrentCamPosition.ToString());
            GUILayout.Label("Current Target: " + CurrentCamTargetPosition.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Start Frame: "))
            {
                StartFrame = CurrentShowingPosition;
            }
            GUILayout.Label(StartFrame.ToString());
            GUILayout.Space(5);
            if (GUILayout.Button("Reset Start"))
            {
                StartFrame = 0;
            }
            GUILayout.Space(20);
            GUILayout.Label(CurrentShowingPosition.ToString());
            GUILayout.Space(20);
            if (GUILayout.Button("Reset End"))
            {
                EndFrame = MyPlayersPoisitions.Count-1;
            }
            if (GUILayout.Button("End Frame: "))
            {
                EndFrame = CurrentShowingPosition;
            }
            GUILayout.Label(EndFrame.ToString());
            GUILayout.Space(5);


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("LB",BottomPanelStyle);
            
                CurrentShowingPosition = (int)GUILayout.HorizontalSlider(CurrentShowingPosition, StartFrame,EndFrame);
            
            GUILayout.Label("RB",BottomPanelStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }


        void AddCamMarker()
        {
            if (MGInputManager.A_Down())
            {
            ReplayPosition marker = new ReplayPosition();
            marker.CamPos = new Vector3(ReplayCam.transform.position.x, ReplayCam.transform.position.y, ReplayCam.transform.position.z);
            marker.CamRot = new Vector3(ReplayCam.transform.eulerAngles.x, ReplayCam.transform.eulerAngles.y, ReplayCam.transform.eulerAngles.z);
            marker.ReferenceFrame = CurrentShowingPosition;

            CamMarkers.Add(marker);

            }
        }

        GameObject GetPlayer()
        {
            LocalPlayer.instance.RiderTrackingSetup();
            Debug.Log("Making player");
            GameObject _RiderModel = null;
            if (LocalPlayer.instance.RiderModelname == "Daryien")
            {
             _RiderModel = Instantiate(LocalPlayer.instance.ActiveModel);

            }
            else
            {
               _RiderModel = GameManager.GetPlayerModel(LocalPlayer.instance.RiderModelname, LocalPlayer.instance.RiderModelBundleName);
            }




            if (_RiderModel.GetComponent<Animation>())
            {
                _RiderModel.GetComponent<Animation>().enabled = false;
            }
            if (_RiderModel.GetComponent<BMXLimbTargetAdjust>())
            {
                _RiderModel.GetComponent<BMXLimbTargetAdjust>().enabled = false;

            }
            if (_RiderModel.GetComponent<SkeletonReferenceValue>())
            {
                _RiderModel.GetComponent<SkeletonReferenceValue>().enabled = false;

            }
            // remove any triggers, Ontrigger events will cause local player to bail
            foreach (Transform t in _RiderModel.GetComponentsInChildren<Transform>())
            {
                if (t.name.Contains("Trigger"))
                {
                    Destroy(t.gameObject);
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
            if (_RiderModel.GetComponent<Rigidbody>())
            {
               _RiderModel.GetComponent<Rigidbody>().isKinematic = true;
            }


            // grab all transforms

            MyRidersTrans[0] = _RiderModel.transform;
            MyRidersTrans[1] = _RiderModel.transform.FindDeepChild("mixamorig:LeftUpLeg").transform;
            MyRidersTrans[2] = _RiderModel.transform.FindDeepChild("mixamorig:RightUpLeg").transform;
            MyRidersTrans[3] = _RiderModel.transform.FindDeepChild("mixamorig:LeftLeg").transform;
            MyRidersTrans[4] = _RiderModel.transform.FindDeepChild("mixamorig:RightLeg").transform;
            MyRidersTrans[5] = _RiderModel.transform.FindDeepChild("mixamorig:LeftFoot").transform;
            MyRidersTrans[6] = _RiderModel.transform.FindDeepChild("mixamorig:RightFoot").transform;
            MyRidersTrans[7] = _RiderModel.transform.FindDeepChild("mixamorig:Spine").transform;
            MyRidersTrans[8] = _RiderModel.transform.FindDeepChild("mixamorig:Spine1").transform;
            MyRidersTrans[9] = _RiderModel.transform.FindDeepChild("mixamorig:Spine2").transform;
            MyRidersTrans[10] = _RiderModel.transform.FindDeepChild("mixamorig:LeftShoulder").transform;
            MyRidersTrans[11] = _RiderModel.transform.FindDeepChild("mixamorig:RightShoulder").transform;
            MyRidersTrans[12] = _RiderModel.transform.FindDeepChild("mixamorig:LeftArm").transform;
            MyRidersTrans[13] = _RiderModel.transform.FindDeepChild("mixamorig:RightArm").transform;
            MyRidersTrans[14] = _RiderModel.transform.FindDeepChild("mixamorig:LeftForeArm").transform;
            MyRidersTrans[15] = _RiderModel.transform.FindDeepChild("mixamorig:RightForeArm").transform;
            MyRidersTrans[16] = _RiderModel.transform.FindDeepChild("mixamorig:LeftHand").transform;
            MyRidersTrans[17] = _RiderModel.transform.FindDeepChild("mixamorig:RightHand").transform;
            MyRidersTrans[18] = _RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex1").transform;
            MyRidersTrans[19] = _RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex1").transform;
            MyRidersTrans[20] = _RiderModel.transform.FindDeepChild("mixamorig:Hips").transform;
            MyRidersTrans[21] = _RiderModel.transform.FindDeepChild("mixamorig:Neck").transform;
            MyRidersTrans[22] = _RiderModel.transform.FindDeepChild("mixamorig:Head").transform;

            MyRidersTrans[32] = _RiderModel.transform.FindDeepChild("mixamorig:LeftHandIndex2").transform;
            MyRidersTrans[33] = _RiderModel.transform.FindDeepChild("mixamorig:RightHandIndex2").transform;



            return _RiderModel;
        }
        GameObject GetBmx()
        {
            Debug.Log("Making bike");
            GameObject Bmx_Root = Instantiate(LocalPlayer.instance.Bmx_Root);
            if (Bmx_Root.GetComponent<Rigidbody>())
            {
                Bmx_Root.GetComponent<Rigidbody>().isKinematic = true;
            }

            foreach(Transform t in Bmx_Root.GetComponentsInChildren<Transform>())
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

            MyRidersTrans[23] = Bmx_Root.transform;
            MyRidersTrans[24] = Bmx_Root.transform.FindDeepChild("BMX:Bike_Joint");
            MyRidersTrans[25] = Bmx_Root.transform.FindDeepChild("BMX:Bars_Joint");
            MyRidersTrans[26] = Bmx_Root.transform.FindDeepChild("BMX:DriveTrain_Joint");
            MyRidersTrans[27] = Bmx_Root.transform.FindDeepChild("BMX:Frame_Joint");
            MyRidersTrans[28] = Bmx_Root.transform.FindDeepChild("BMX:Wheel");
            MyRidersTrans[29] = Bmx_Root.transform.FindDeepChild("BMX:Wheel 1");
            MyRidersTrans[30] = Bmx_Root.transform.FindDeepChild("BMX:LeftPedal_Joint");
            MyRidersTrans[31] = Bmx_Root.transform.FindDeepChild("BMX:RightPedal_Joint");

            return Bmx_Root;
        }

        void Update()
        {
            if (ReplayOpen)
            {
                // in Edit mode
                if (!PlayEditToggle)
                {
                    PlayerFreeCam();
                    AddCamMarker();
                }

                if (InGameUI.instance.Connected)
                {
                    GameManager.KeepNetworkActive();
                }



            }
            else
            {
                
                try
                {

                if(_time < 0.020f)
                {
                    _time = _time + Time.deltaTime;
                }
                else
                {

                      

                    // Keep 30 seconds of my footage
                    if (MyPlayersPoisitions.Count > 1600)
                    {
                        MyPlayersPoisitions.RemoveRange(0, MyPlayersPoisitions.Count - 1600);
                    }
                    else
                    {
                        Currentpositions[0] = LocalPlayer.instance.Riders_Transforms[0].position;
                        Currentrotations[0] = LocalPlayer.instance.Riders_Transforms[0].eulerAngles;

                        for (int i = 1; i < 23; i++)
                        {
                            Currentpositions[i] = LocalPlayer.instance.Riders_Transforms[i].localPosition;
                            Currentrotations[i] = LocalPlayer.instance.Riders_Transforms[i].localEulerAngles;
                        }


                        Currentpositions[23] = LocalPlayer.instance.Riders_Transforms[23].position;
                        Currentrotations[23] = LocalPlayer.instance.Riders_Transforms[23].eulerAngles;

                        for (int i = 24; i < 32; i++)
                        {
                            Currentpositions[i] = LocalPlayer.instance.Riders_Transforms[i].localPosition;
                            Currentrotations[i] = LocalPlayer.instance.Riders_Transforms[i].localEulerAngles;
                        }

                        Currentrotations[32] = LocalPlayer.instance.Riders_Transforms[32].localEulerAngles;
                        Currentrotations[33] = LocalPlayer.instance.Riders_Transforms[33].localEulerAngles;


                        ReplayPosition pos = new ReplayPosition();
                        pos.Positions = new Vector3[32];
                        pos.Rotations = new Vector3[34];
                        Array.Copy(Currentpositions, pos.Positions, Currentpositions.Length);
                        Array.Copy(Currentrotations, pos.Rotations, Currentrotations.Length);
                        MyPlayersPoisitions.Add(pos);
                    }

                    // remote players footage track
                    if (InGameUI.instance.Connected && LocalPlayer.instance.ServerActive)
                    {
                        foreach (RemotePlayer player in GameManager.Players.Values)
                        {

                            if (player.MasterActive)
                            {

                            if (player.ReplayPostions.Count > 1600)
                            {
                                player.ReplayPostions.RemoveRange(0, player.ReplayPostions.Count + 1 - 1600);
                            }
                            else
                            {
                                Vector3[] pos = new Vector3[28];
                                Vector3[] rot = new Vector3[34];

                                pos[0] = player.Riders_Transforms[0].position;
                                rot[0] = player.Riders_Transforms[0].eulerAngles;

                                for (int i = 1; i < 23; i++)
                                {
                                    rot[i] = player.Riders_Transforms[i].localEulerAngles;
                                }

                                pos[20] = player.Riders_Transforms[20].localPosition;

                                pos[24] = player.Riders_Transforms[24].position;
                                rot[24] = player.Riders_Transforms[24].eulerAngles;

                                pos[25] = player.Riders_Transforms[25].position;
                                pos[27] = player.Riders_Transforms[27].position;

                                for (int i = 25; i < 34; i++)
                                {
                                 rot[i] = player.Riders_Transforms[i].localEulerAngles;
                                }
                                    
                               player.ReplayPostions.Add(new IncomingTransformUpdate(pos, rot));


                            }


                            }

                        }
                    }


                    _time = 0;
                }

                }
                catch (Exception x)
                {
                    Debug.Log($"Replay Update error: {x}");
                }
                
            }
        }
        void FixedUpdate()
        {
           
            // if not open, record data
            if (!ReplayOpen && Tracking)
            {

            }

            if (ReplayOpen)
            {

                Playspeed = (MGInputManager.LTrigger() > MGInputManager.RTrigger() ? MGInputManager.LTrigger() : MGInputManager.RTrigger()) * Time.fixedDeltaTime * 80;

                // edit mode
                if (!PlayEditToggle)
                {
                    TriggerScrollEditMode();
                }

                // in Play mode
                if (PlayEditToggle)
                {
                    TriggerScrollPlayMode();
                }



                // my position update
                if (MyPlayersPoisitions.Count > CurrentShowingPosition)
                {
                    // plug in Position Data to rider
                    MyRidersTrans[0].position = Vector3.Lerp(MyRidersTrans[0].position,MyPlayersPoisitions[CurrentShowingPosition].Positions[0], Playspeed);
                    MyRidersTrans[0].rotation = Quaternion.Lerp(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[0]),Playspeed);
                    for (int i = 1; i < 23; i++)
                    {
                        MyRidersTrans[i].localPosition = Vector3.Lerp(MyRidersTrans[i].localPosition ,MyPlayersPoisitions[CurrentShowingPosition].Positions[i],Playspeed);
                        MyRidersTrans[i].localRotation = Quaternion.Lerp(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[i]),Playspeed);
                    }
                    MyRidersTrans[23].position = Vector3.Lerp(MyRidersTrans[23].position, MyPlayersPoisitions[CurrentShowingPosition].Positions[23], Playspeed);
                    MyRidersTrans[23].rotation = Quaternion.Lerp(MyRidersTrans[23].rotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[23]), Playspeed);
                    for (int i = 24; i < 32; i++)
                    {
                        MyRidersTrans[i].localPosition = Vector3.Lerp(MyRidersTrans[i].localPosition, MyPlayersPoisitions[CurrentShowingPosition].Positions[i], Playspeed);
                        MyRidersTrans[i].localRotation = Quaternion.Lerp(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[i]), Playspeed);

                    }

                    MyRidersTrans[32].localRotation = Quaternion.Lerp(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[32]), Playspeed);
                    MyRidersTrans[33].localRotation = Quaternion.Lerp(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayersPoisitions[CurrentShowingPosition].Rotations[33]), Playspeed);



                }

                if (InGameUI.instance.Connected)
                {

                // online players position update
                foreach (RemotePlayer player in GameManager.Players.Values)
                {


                    if (player.ReplayPostions.Count > CurrentShowingPosition)
                    {
                        player.Riders_Transforms[0].position = Vector3.Lerp(player.Riders_Transforms[0].position,player.ReplayPostions[CurrentShowingPosition].Positions[0],Playspeed);
                        player.Riders_Transforms[0].rotation = Quaternion.Lerp(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPostions[CurrentShowingPosition].Rotations[0]), Playspeed);

                        for (int i = 1; i < 23; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.Lerp(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPostions[CurrentShowingPosition].Rotations[i]),Playspeed);
                        }

                        player.Riders_Transforms[20].localPosition = Vector3.Lerp(player.Riders_Transforms[20].localPosition, player.ReplayPostions[CurrentShowingPosition].Positions[20], Playspeed);

                        player.Riders_Transforms[24].position = Vector3.Lerp(player.Riders_Transforms[24].position, player.ReplayPostions[CurrentShowingPosition].Positions[24], Playspeed);
                        player.Riders_Transforms[24].rotation = Quaternion.Lerp(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPostions[CurrentShowingPosition].Rotations[24]), Playspeed);

                        player.Riders_Transforms[25].position = Vector3.Lerp(player.Riders_Transforms[25].position, player.ReplayPostions[CurrentShowingPosition].Positions[25], Playspeed);
                        player.Riders_Transforms[27].position = Vector3.Lerp(player.Riders_Transforms[27].position, player.ReplayPostions[CurrentShowingPosition].Positions[27], Playspeed);

                        for (int i = 25; i < 34; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.Lerp(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPostions[CurrentShowingPosition].Rotations[i]), Playspeed);
                        }

                        
                    }

                }



                }



            }



        }
       
        void Awake()
        {
            instance = this;
            MyRidersTrans = new Transform[34];
            Currentpositions = new Vector3[32];
            Currentrotations = new Vector3[34];
        }
        void Start()
        {

            BottomPanelStyle.fixedHeight = 50;
            BottomPanelStyle.fixedWidth = 50;
            BottomPanelStyle.fontStyle = FontStyle.Bold;


            ReplayCam = Instantiate(Camera.main.gameObject) as GameObject;
            ReplayCam.SetActive(false);
            DontDestroyOnLoad(ReplayCam);

            Tracking = true;

        }
        void OnGUI()
        {
            if (ReplayOpen)
            {
              GUI.skin = InGameUI.instance.skin;
                MainWindow();
            if (OpenCamSettings)
            {
                CameraSettings.instance.Show();
            }
               
            }



        }


    }

    /// <summary>
    /// Useable for storing every pos and rot of the rider and bmx in a packet, or for a camera marker with referenceframe etc
    /// </summary>
    public struct ReplayPosition
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public float Fov;
        public float FocusDistance;
        public float Aperture;
        public Vector3 CamPos;
        public Vector3 CamRot;
        public int ReferenceFrame;
        public bool IsFirst;
        public bool IsLast;
    }


}
