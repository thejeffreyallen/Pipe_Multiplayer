﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{
    public class ReplayMode : MonoBehaviour
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
        public static List<ReplayPosition> MyPlayerPositions = new List<ReplayPosition>();
        public static List<ReplayPosition> CamMarkers = new List<ReplayPosition>();
        public bool ReplayOpen = false;
        public bool LookAtPlayer = true;
        public int CurrentShowingPosition;
        public int CurrentCamPosition;
        public int CurrentCamTargetPosition = 1;
        public bool PlayThrough;
        public int StartFrame;
        public int EndFrame;
        public bool PlayEditToggle;
        public string ModeDisplay = "Play Mode";
        public string LookatDisplay = "Look at Player";
        public bool OpenCamSettings;
        Vector2 MarkerScroll;
        GUIStyle Offstyle;
        GUIStyle OnStyle;
        GUIStyle LookatStyle;
        GUIStyle CamMarkerstyle;
        GUIStyle Playstyle;

        float movespeed;
        float rotatespeed;
        float TriggerPlaySpeed;
        float SetSpeed = 0.5f;
        float OriginalSpeed;

        bool moveforward;
        bool moveback;

        // Camera position based on frame and markers
        Quaternion currentrot;
        float Camspan = 0.001f;
        float RemovedCamSpan = 0.001f;
        bool CammarkersOpen = false;
        ReplayPosition activemarker;
        int BlendValue = 0;
        string blendval = "0";
        string speedframein = "5";
        string speedframeout = "5";
        string Speedatmarker = "0.2";

        Dictionary<ReplayPosition.SlowMoStyle,string> SlowMoLabels;





        bool HideGUI;
        System.Diagnostics.Stopwatch ReplayWatch = new System.Diagnostics.Stopwatch();
        float remaining = 0.001f;
        float removed = 0.01f;

        public void Open()
        {
            Debug.Log("Replay opening.");
            if (InGameUI.instance.Connected)
            {
                GameManager.instance.TurnOffNetUpdates();
            }
            if (MyMan)
            {
                Destroy(MyMan);
            }
            if (MyBmx)
            {
                Destroy(MyBmx);
            }

            StartFrame = 0;
            EndFrame = MyPlayerPositions.Count - 1;



            ReplayCam.SetActive(true);
            ReplayCam.transform.position = Camera.main.gameObject.transform.position;
            ReplayCam.transform.rotation = Camera.main.gameObject.transform.rotation;

            CurrentShowingPosition = MyPlayerPositions.Count - 1;
            MyMan = GetPlayer();
            MyBmx = GetBmx();
            MyMan.transform.position = LocalPlayer.instance.ActiveModel.transform.position;
            MyBmx.transform.position = LocalPlayer.instance.Bmx_Root.transform.position;

            Debug.Log("Player created");
            GameManager.TogglePlayerComponents(false);
            _MG = new MGInputManager();




            // if online, pause comms but send keepalive packets


            ReplayOpen = true;
            ReplayWatch.Stop();
            Debug.Log("open");
        }
        public void Close()
        {
            CamMarkers.Clear();
            PlayThrough = false;
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
            ReplayWatch.Start();
        }


        void PlayerFreeCam()
        {
            movespeed = 10;
            rotatespeed = 50;

            if (!MGInputManager.LB_Hold())
            {
              if (MGInputManager.LStickX() > 0.2f | MGInputManager.LStickX() < -0.2f | MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f | MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
              {
                ReplayCam.transform.Translate(MGInputManager.LStickX() * Time.deltaTime * movespeed, MGInputManager.RStickY() * Time.deltaTime * movespeed, MGInputManager.LStickY() * Time.deltaTime * movespeed);
              }

                if (LookAtPlayer && !PlayEditToggle)
                {
                    ReplayCam.transform.LookAt(MyMan.transform.FindDeepChild("mixamorig:Head"));
                }

            }
            else if (!LookAtPlayer && !PlayEditToggle)
            {
                ReplayCam.transform.Rotate(-MGInputManager.LStickY() * Time.deltaTime * rotatespeed, MGInputManager.LStickX() * Time.deltaTime * rotatespeed, -MGInputManager.RStickX() * Time.deltaTime * rotatespeed);
            }
            

        }

        void TriggerScrollEditMode()
        {


            if (MGInputManager.LTrigger() > 0.05f)
            {
                TriggerPlaySpeed = MGInputManager.LTrigger();


                if (CurrentShowingPosition - 1 != StartFrame)
                {

                    if (remaining < (Time.deltaTime * TriggerPlaySpeed / 2))
                    {

                        if (CurrentShowingPosition <= StartFrame)
                        {
                            CurrentShowingPosition = StartFrame;
                        }
                        else
                        {
                            CurrentShowingPosition--;
                            remaining = Mathf.Clamp(remaining + MyPlayerPositions[CurrentShowingPosition].Timspanfromlast, 0.00001f, 0.1f);

                        }


                    }


                }



            }
            else if (MGInputManager.RTrigger() > 0.05f)
            {
                TriggerPlaySpeed = MGInputManager.RTrigger();


                if (CurrentShowingPosition != EndFrame)
                {
                    if (remaining < (Time.deltaTime * TriggerPlaySpeed / 2))
                    {
                        if (CurrentShowingPosition >= EndFrame)
                        {
                            CurrentShowingPosition = EndFrame;
                        }
                        else
                        {
                            CurrentShowingPosition++;
                            remaining = Mathf.Clamp(remaining + MyPlayerPositions[CurrentShowingPosition].Timspanfromlast, 0.00001f, 0.1f);
                        }
                    }

                }


            }
            else
            {
                TriggerPlaySpeed = 0;


                if (MyPlayerPositions.Count > CurrentShowingPosition)
                {

                    // plug in Position Data to rider

                    MyRidersTrans[0].position = MyPlayerPositions[CurrentShowingPosition].Positions[0];
                    MyRidersTrans[0].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]);
                    for (int i = 1; i < 23; i++)
                    {
                        MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                    }

                    // hip joint
                    MyRidersTrans[20].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[20];



                    MyRidersTrans[24].position = MyPlayerPositions[CurrentShowingPosition].Positions[24];
                    MyRidersTrans[24].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]);

                    MyRidersTrans[25].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[25];
                    MyRidersTrans[27].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[27];


                    for (int i = 25; i < 32; i++)
                    {
                        MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);

                    }

                    MyRidersTrans[32].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]);
                    MyRidersTrans[33].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]);

                    remaining = Mathf.Clamp(remaining - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);

                }

                if (InGameUI.instance.Connected)
                {

                    // online players position update
                    foreach (RemotePlayer player in GameManager.Players.Values)
                    {
                        player.nameSign.transform.rotation = Camera.current.transform.rotation;

                        if (player.ReplayPositions.Count > CurrentShowingPosition)
                        {
                            player.Riders_Transforms[0].position = player.ReplayPositions[CurrentShowingPosition].Positions[0];
                            player.Riders_Transforms[0].rotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]);
                            for (int i = 1; i < 23; i++)
                            {
                                player.Riders_Transforms[i].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]);
                            }

                            // hip joint
                            player.Riders_Transforms[20].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[20];



                            player.Riders_Transforms[24].position = player.ReplayPositions[CurrentShowingPosition].Positions[24];
                            player.Riders_Transforms[24].rotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]);

                            player.Riders_Transforms[25].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[25];
                            player.Riders_Transforms[27].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[27];


                            for (int i = 25; i < 32; i++)
                            {
                                player.Riders_Transforms[i].localRotation =Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]);

                            }

                            player.Riders_Transforms[32].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]);
                            player.Riders_Transforms[33].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]);


                        }

                    }



                }




            }


            // my position update
            if (MyPlayerPositions.Count > CurrentShowingPosition)
            {

                // plug in Position Data to rider

                MyRidersTrans[0].position = Vector3.MoveTowards(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                MyRidersTrans[0].rotation = Quaternion.RotateTowards(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[0].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                for (int i = 1; i < 23; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                }

                // hip joint
                MyRidersTrans[20].localPosition = Vector3.MoveTowards(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20]) / remaining * (Time.deltaTime * TriggerPlaySpeed));



                MyRidersTrans[24].position = Vector3.MoveTowards(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                MyRidersTrans[24].rotation = Quaternion.RotateTowards(MyRidersTrans[24].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[24].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                MyRidersTrans[25].localPosition = Vector3.MoveTowards(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                MyRidersTrans[27].localPosition = Vector3.MoveTowards(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27]) / remaining * (Time.deltaTime * TriggerPlaySpeed));


                for (int i = 25; i < 32; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                }

                MyRidersTrans[32].localRotation = Quaternion.RotateTowards(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[32].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                MyRidersTrans[33].localRotation = Quaternion.RotateTowards(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[33].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                remaining = Mathf.Clamp(remaining - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);

            }


            if (InGameUI.instance.Connected)
            {

                // online players position update
                foreach (RemotePlayer player in GameManager.Players.Values)
                {
                    player.nameSign.transform.rotation = Camera.current.transform.rotation;

                    if (player.ReplayPositions.Count > CurrentShowingPosition)
                    {
                        player.Riders_Transforms[0].position = Vector3.MoveTowards(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        player.Riders_Transforms[0].rotation = Quaternion.RotateTowards(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[0].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        for (int i = 1; i < 23; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        }

                        // hip joint
                        player.Riders_Transforms[20].localPosition = Vector3.MoveTowards(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20]) / remaining * Time.deltaTime * TriggerPlaySpeed);



                        player.Riders_Transforms[24].position = Vector3.MoveTowards(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        player.Riders_Transforms[24].rotation = Quaternion.RotateTowards(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[24].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                        player.Riders_Transforms[25].localPosition = Vector3.MoveTowards(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        player.Riders_Transforms[27].localPosition = Vector3.MoveTowards(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27]) / remaining * Time.deltaTime * TriggerPlaySpeed);


                        for (int i = 25; i < 32; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                        }

                        player.Riders_Transforms[32].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[32].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[32].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                        player.Riders_Transforms[33].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[33].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[33].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33])) / remaining * Time.deltaTime * TriggerPlaySpeed);


                    }

                }



            }






        }
        void TriggerScrollPlayMode()
        {
            if (CamMarkers.Count >= CurrentCamTargetPosition)
            {
                // position and rotation of cam relative to current showing frame and current markers



                if (MGInputManager.LTrigger() > 0.1f)
                {
                    TriggerPlaySpeed = MGInputManager.LTrigger();
                    if (CurrentShowingPosition > StartFrame)
                    {
                        moveback = true;

                        if (removed < (Time.deltaTime * TriggerPlaySpeed / 1.2f))
                        {

                            if (CurrentShowingPosition <= StartFrame)
                            {
                                moveback = false;
                                CurrentShowingPosition = StartFrame;
                                removed = MyPlayerPositions[CurrentShowingPosition + 1].Timspanfromlast;
                            }
                            else
                            {
                                CurrentShowingPosition--;
                                remaining = removed;
                                removed = Mathf.Clamp(removed + MyPlayerPositions[CurrentShowingPosition + 1].Timspanfromlast, 0.001f, 0.9f);

                            }

                        }
                            
                            
                         if (CurrentShowingPosition <= CamMarkers[CurrentCamPosition].ReferenceFrame && CurrentShowingPosition> StartFrame)
                         {
                            CurrentCamPosition--;
                            CurrentCamTargetPosition--;
                            RemovedCamSpan = 0;
                            Camspan = RemovedCamSpan;
                            for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                            {
                             RemovedCamSpan = RemovedCamSpan + MyPlayerPositions[i].Timspanfromlast;
                            }
 
                         }


                        // position
                        Vector3 currentpos = CamMarkers[CurrentCamPosition].CamPos;
                        currentrot = CamMarkers[CurrentCamPosition].CamRot;
                        Vector3 Vel = -(ReplayCam.transform.position - currentpos).normalized;
                        ReplayCam.transform.position = Vector3.SmoothDamp(ReplayCam.transform.position, Vector3.MoveTowards(ReplayCam.transform.position, currentpos, Vector3.Distance(ReplayCam.transform.position, currentpos) / RemovedCamSpan * Time.deltaTime * TriggerPlaySpeed), ref Vel, BlendValue);

                        // rotation
                        ReplayCam.transform.rotation = Quaternion.RotateTowards(ReplayCam.transform.rotation, currentrot, Quaternion.Angle(ReplayCam.transform.rotation, currentrot) / RemovedCamSpan * Time.deltaTime * TriggerPlaySpeed);

                        RemovedCamSpan = RemovedCamSpan - (Time.deltaTime * TriggerPlaySpeed);
                        Camspan = Camspan + (Time.deltaTime * TriggerPlaySpeed);






                        if (moveback)
                        {

                            // my position update
                            if (MyPlayerPositions.Count > CurrentShowingPosition)
                            {
                                // plug in Position Data to rider
                                MyRidersTrans[0].position = Vector3.MoveTowards(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0]) / removed * Time.deltaTime * TriggerPlaySpeed);
                                MyRidersTrans[0].rotation = Quaternion.RotateTowards(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[0].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0])) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                for (int i = 1; i < 23; i++)
                                {
                                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                }
                                // hip joint
                                MyRidersTrans[20].localPosition = Vector3.MoveTowards(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20]) / removed * (Time.deltaTime * TriggerPlaySpeed));


                                MyRidersTrans[24].position = Vector3.MoveTowards(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24]) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                MyRidersTrans[24].rotation = Quaternion.RotateTowards(MyRidersTrans[24].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[24].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24])) / removed * (Time.deltaTime * TriggerPlaySpeed));

                                MyRidersTrans[25].localPosition = Vector3.MoveTowards(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25]) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                MyRidersTrans[27].localPosition = Vector3.MoveTowards(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27]) / removed * (Time.deltaTime * TriggerPlaySpeed));


                                for (int i = 25; i < 32; i++)
                                {
                                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                }
                                MyRidersTrans[32].localRotation = Quaternion.RotateTowards(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[32].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32])) / removed * (Time.deltaTime * TriggerPlaySpeed));
                                MyRidersTrans[33].localRotation = Quaternion.RotateTowards(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[33].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33])) / removed * (Time.deltaTime * TriggerPlaySpeed));

                                removed = Mathf.Clamp(removed - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);
                                remaining = Mathf.Clamp(remaining + (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);
                            }

                            // others position update
                            if (InGameUI.instance.Connected)
                            {

                                // online players position update
                                foreach (RemotePlayer player in GameManager.Players.Values)
                                {
                                    player.nameSign.transform.rotation = Camera.current.transform.rotation;

                                    if (player.ReplayPositions.Count > CurrentShowingPosition)
                                    {
                                        player.Riders_Transforms[0].position = Vector3.MoveTowards(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0]) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        player.Riders_Transforms[0].rotation = Quaternion.RotateTowards(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[0].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0])) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        for (int i = 1; i < 23; i++)
                                        {
                                            player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        }

                                        // hip joint
                                        player.Riders_Transforms[20].localPosition = Vector3.MoveTowards(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20]) / removed * Time.deltaTime * TriggerPlaySpeed);



                                        player.Riders_Transforms[24].position = Vector3.MoveTowards(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24]) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        player.Riders_Transforms[24].rotation = Quaternion.RotateTowards(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[24].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24])) / removed * Time.deltaTime * TriggerPlaySpeed);

                                        player.Riders_Transforms[25].localPosition = Vector3.MoveTowards(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25]) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        player.Riders_Transforms[27].localPosition = Vector3.MoveTowards(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27]) / removed * Time.deltaTime * TriggerPlaySpeed);


                                        for (int i = 25; i < 32; i++)
                                        {
                                            player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / removed * Time.deltaTime * TriggerPlaySpeed);

                                        }

                                        player.Riders_Transforms[32].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[32].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[32].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32])) / removed * Time.deltaTime * TriggerPlaySpeed);
                                        player.Riders_Transforms[33].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[33].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[33].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33])) / removed * Time.deltaTime * TriggerPlaySpeed);


                                    }

                                }



                            }


                        }








                    }
                    else
                    {
                        CurrentShowingPosition = StartFrame;
                        CurrentCamPosition = 0;
                        CurrentCamTargetPosition = 1;
                        RemovedCamSpan = 0;
                        Camspan = 0;
                        for (int i = StartFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                        {
                            Camspan = Camspan + MyPlayerPositions[i].Timspanfromlast;
                        }

                        remaining = MyPlayerPositions[StartFrame + 1].Timspanfromlast;
                        removed = 0;

                    }


                }
                else if (MGInputManager.RTrigger() > 0.1f)
                {
                    TriggerPlaySpeed = MGInputManager.RTrigger();
                    if (CurrentShowingPosition < EndFrame)
                    {

                        moveforward = true;
                        if (remaining < (Time.deltaTime * TriggerPlaySpeed/1.2f))
                        {
                            if (CurrentShowingPosition >= EndFrame)
                            {
                                moveforward = false;
                                CurrentShowingPosition = EndFrame;
                                remaining = MyPlayerPositions[CurrentShowingPosition].Timspanfromlast;
                            }
                            else
                            {
                               
                                CurrentShowingPosition++;
                                removed = remaining;
                                remaining = Mathf.Clamp(remaining + MyPlayerPositions[CurrentShowingPosition].Timspanfromlast, 0.0001f, 0.9f);
                            }
                        }
                        


                       
                        if (CurrentShowingPosition >= CamMarkers[CurrentCamTargetPosition].ReferenceFrame)
                        {
                          if(CurrentCamTargetPosition < CamMarkers.Count - 1)
                          {

                            CurrentCamPosition++;
                            CurrentCamTargetPosition++;
                            Camspan = 0;
                            RemovedCamSpan = Camspan;
                            for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                            {
                                Camspan = Camspan + MyPlayerPositions[i].Timspanfromlast;
                            }


                          }
                          else
                          {
                            RemovedCamSpan = 0.001f;
                                Camspan = 0.001f;
                          }

                        }

                        // position
                        Vector3 currentpos = CamMarkers[CurrentCamTargetPosition].CamPos;
                        currentrot = CamMarkers[CurrentCamTargetPosition].CamRot;
                        Vector3 Vel = -(ReplayCam.transform.position - currentpos).normalized;
                        ReplayCam.transform.position = Vector3.SmoothDamp(ReplayCam.transform.position,Vector3.MoveTowards(ReplayCam.transform.position, currentpos, Vector3.Distance(ReplayCam.transform.position, currentpos) / Camspan * Time.deltaTime * TriggerPlaySpeed),ref Vel , BlendValue);

                        // rotation
                        ReplayCam.transform.rotation = Quaternion.RotateTowards(ReplayCam.transform.rotation, currentrot, Quaternion.Angle(ReplayCam.transform.rotation, currentrot) / Camspan * Time.deltaTime * TriggerPlaySpeed);

                        Camspan = Camspan - (Time.deltaTime * TriggerPlaySpeed);
                        RemovedCamSpan = RemovedCamSpan + (Time.deltaTime * TriggerPlaySpeed);






                        if (moveforward)
                        {

                        // my position update
                        if (MyPlayerPositions.Count > CurrentShowingPosition)
                        {
                            // plug in Position Data to rider
                            MyRidersTrans[0].position = Vector3.MoveTowards(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            MyRidersTrans[0].rotation = Quaternion.RotateTowards(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[0].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            for (int i = 1; i < 23; i++)
                            {
                                MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            }
                            // hip joint
                            MyRidersTrans[20].localPosition = Vector3.MoveTowards(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20]) / remaining * (Time.deltaTime * TriggerPlaySpeed));


                            MyRidersTrans[24].position = Vector3.MoveTowards(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            MyRidersTrans[24].rotation = Quaternion.RotateTowards(MyRidersTrans[24].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[24].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                            MyRidersTrans[25].localPosition = Vector3.MoveTowards(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            MyRidersTrans[27].localPosition = Vector3.MoveTowards(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27]) / remaining * (Time.deltaTime * TriggerPlaySpeed));


                            for (int i = 25; i < 32; i++)
                            {
                                MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            }
                            MyRidersTrans[32].localRotation = Quaternion.RotateTowards(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[32].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                            MyRidersTrans[33].localRotation = Quaternion.RotateTowards(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[33].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                            remaining = Mathf.Clamp(remaining - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);
                            removed = Mathf.Clamp(removed + (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);
                        }

                        // others position update
                        if (InGameUI.instance.Connected)
                        {

                            // online players position update
                            foreach (RemotePlayer player in GameManager.Players.Values)
                            {
                                player.nameSign.transform.rotation = Camera.current.transform.rotation;

                                if (player.ReplayPositions.Count > CurrentShowingPosition)
                                {
                                    player.Riders_Transforms[0].position = Vector3.MoveTowards(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    player.Riders_Transforms[0].rotation = Quaternion.RotateTowards(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[0].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    for (int i = 1; i < 23; i++)
                                    {
                                        player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    }

                                    // hip joint
                                    player.Riders_Transforms[20].localPosition = Vector3.MoveTowards(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20]) / remaining * Time.deltaTime * TriggerPlaySpeed);



                                    player.Riders_Transforms[24].position = Vector3.MoveTowards(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    player.Riders_Transforms[24].rotation = Quaternion.RotateTowards(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[24].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                                    player.Riders_Transforms[25].localPosition = Vector3.MoveTowards(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    player.Riders_Transforms[27].localPosition = Vector3.MoveTowards(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27]) / remaining * Time.deltaTime * TriggerPlaySpeed);


                                    for (int i = 25; i < 32; i++)
                                    {
                                        player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                                    }

                                    player.Riders_Transforms[32].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[32].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[32].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                                    player.Riders_Transforms[33].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[33].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[33].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33])) / remaining * Time.deltaTime * TriggerPlaySpeed);


                                }

                            }



                        }


                        }










                    }
                    else
                    {
                        Camspan = 0;
                        RemovedCamSpan = 0;
                        for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= EndFrame; i++)
                        {
                            RemovedCamSpan = RemovedCamSpan + MyPlayerPositions[i].Timspanfromlast;
                        }

                        removed = MyPlayerPositions[EndFrame].Timspanfromlast;
                    }



                }
                else
                {
                    TriggerPlaySpeed = 0;
                }







              




            }
        }




        void SpeedChange()
        {
            if(CamMarkers[CurrentCamTargetPosition].ReferenceFrame - CamMarkers[CurrentCamTargetPosition].SpeedFramein <= CurrentShowingPosition && CamMarkers[CurrentCamTargetPosition].AlterSpeed && CurrentShowingPosition <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame)
            {
                // going in

                ReplayPosition rp = CamMarkers[CurrentCamTargetPosition];
                if (rp.StyleIn == ReplayPosition.SlowMoStyle.Linear)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame - rp.SpeedFramein + 1; i <= rp.ReferenceFrame; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Timspanfromlast;
                    }


                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    SetSpeed = Mathf.MoveTowards(SetSpeed, rp.Speedatmarker, diff / deltas * Time.deltaTime * SetSpeed);
                }
                else if(rp.StyleIn == ReplayPosition.SlowMoStyle.SlowIn)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame - rp.SpeedFramein + 1; i <= rp.ReferenceFrame; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Timspanfromlast;
                    }


                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    SetSpeed = Mathf.MoveTowards(SetSpeed, rp.Speedatmarker, diff / deltas * Time.deltaTime * SetSpeed);


                }
                else if (rp.StyleIn == ReplayPosition.SlowMoStyle.Slowout)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = CurrentShowingPosition; i < rp.ReferenceFrame; i++)
                    {
                        deltas = Mathf.Clamp(deltas + MyPlayerPositions[i].Timspanfromlast,0.001f,100);
                    }


                    float diff = Mathf.Clamp(SetSpeed >= rp.Speedatmarker ? SetSpeed - rp.Speedatmarker : rp.Speedatmarker - SetSpeed,0.001f,100);
                    SetSpeed = Mathf.Clamp(Mathf.MoveTowards(SetSpeed, rp.Speedatmarker, diff / 4 * Time.deltaTime * SetSpeed),0.001f,100);

                }



            }
            else if(CamMarkers[CurrentCamPosition].ReferenceFrame + CamMarkers[CurrentCamPosition].SpeedFrameout >= CurrentShowingPosition && CamMarkers[CurrentCamPosition].AlterSpeed)
            {
                // going out

                ReplayPosition rp = CamMarkers[CurrentCamPosition];
                if (rp.StyleOut == ReplayPosition.SlowMoStyle.Linear)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame + 1; i <= rp.ReferenceFrame + rp.SpeedFrameout; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Timspanfromlast;
                    }



                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    SetSpeed = Mathf.MoveTowards(SetSpeed, OriginalSpeed, diff / deltas * Time.deltaTime * SetSpeed);
                }
                else if (rp.StyleOut == ReplayPosition.SlowMoStyle.SlowIn)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame + 1; i <= rp.ReferenceFrame + rp.SpeedFrameout; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Timspanfromlast;
                    }



                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    SetSpeed = Mathf.MoveTowards(SetSpeed, OriginalSpeed, diff / deltas * Time.deltaTime * SetSpeed);

                }
                else if (rp.StyleOut == ReplayPosition.SlowMoStyle.Slowout)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = CurrentShowingPosition; i < rp.ReferenceFrame + rp.SpeedFrameout; i++)
                    {
                        deltas = Mathf.Clamp(deltas + MyPlayerPositions[i].Timspanfromlast,0.001f,100);
                    }

                    float diff = Mathf.Clamp(SetSpeed >= rp.Speedatmarker ? SetSpeed - rp.Speedatmarker : rp.Speedatmarker - SetSpeed,0.001f,100);
                    SetSpeed = Mathf.Clamp(Mathf.MoveTowards(SetSpeed, OriginalSpeed, diff / 4 * Time.deltaTime * SetSpeed),0.001f,100);

                }
            }
        }

        void TopPanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(10, 10), new Vector2(Screen.width / 1.2f, Screen.height / 10)));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(ModeDisplay))
            {
                SwitchMode(PlayEditToggle);
            }
            if (!PlayEditToggle)
            {
                ModeDisplay = "Switch to Play Mode";
                GUILayout.Space(5);
                GUILayout.Label($"{CamMarkers.Count} markers set");
                GUILayout.Space(5);
                if (GUILayout.Button("Remove all markers"))
                {
                    CamMarkers.Clear();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Cam Markers",CamMarkerstyle))
                {
                    if (OpenCamSettings)
                    {
                        OpenCamSettings = false;
                        
                    }
                    CammarkersOpen = !CammarkersOpen;

                }
                CamMarkerstyle = CammarkersOpen ? OnStyle : Offstyle;
                GUILayout.Space(5);
                if (GUILayout.Button("Cam Settings"))
                {
                    if (CammarkersOpen)
                    {
                        CammarkersOpen = false;
                    }
                    OpenCamSettings = !OpenCamSettings;

                }
                GUILayout.Space(5);
                if (LookAtPlayer)
                {
                    LookatDisplay = "Look At Player on";
                    LookatStyle = OnStyle;
                }
                else
                {
                    LookatDisplay = "look at Player off";
                    LookatStyle = Offstyle;
                }
                LookAtPlayer = GUILayout.Toggle(LookAtPlayer, LookatDisplay,LookatStyle);

                GUILayout.Space(5);
                if (GUILayout.Button("Close"))
                {
                    OpenCamSettings = false;
                    Close();
                }

            }
            if (PlayEditToggle)
            {
                ModeDisplay = "Switch to Edit Mode";
                GUILayout.Space(5);
                Playstyle = PlayThrough ? Offstyle : OnStyle;
                if (GUILayout.Button("Play", Playstyle))
                {
                    PlaySetup();
                }
                GUILayout.Space(5);
                GUILayout.Label($"PlaySpeed: {SetSpeed}");
                GUILayout.Space(5);
                SetSpeed = GUILayout.HorizontalSlider(SetSpeed,0f, 1f);
                GUILayout.Space(5);
                if (GUILayout.Button("Cam Markers"))
                {
                    if (OpenCamSettings)
                    {
                        OpenCamSettings = false;
                    }
                    CammarkersOpen = !CammarkersOpen;
                }
                GUILayout.Space(5);
               // GUILayout.Label("Blend:",GUILayout.MaxWidth(100));
                //blendval = GUILayout.TextField(blendval,GUILayout.MaxWidth(30));
                //if(int.TryParse(blendval,out int res))
               // {
                   // BlendValue = res;
               // }

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
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width > 1500 ? 150 : 75, Screen.height > 900 ? Screen.height - (Screen.height / 20) - 50 : Screen.height - (Screen.height / 13) - 50), new Vector2(Screen.width > 1500 ? Screen.width - 300 : Screen.width - 150,Screen.height > 900 ? Screen.height / 20 : Screen.height / 13)), InGameUI.BoxStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("H to Hide : ");
            GUILayout.Label("LB hold for Cam rotation");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(StartFrame.ToString());
            if (GUILayout.Button("Start Frame"))
            {
                StartFrame = CurrentShowingPosition;
            }
            GUILayout.Space(2);
            if (GUILayout.Button("Reset"))
            {
                StartFrame = 0;
            }
            GUILayout.Space(20);
            GUILayout.Label("X to Place at:");
            GUILayout.Label(CurrentShowingPosition.ToString());
            GUILayout.Space(20);
            if (GUILayout.Button("Reset"))
            {
                EndFrame = MyPlayerPositions.Count - 1;
            }
            GUILayout.Space(2);
            if (GUILayout.Button("End Frame"))
            {
                EndFrame = CurrentShowingPosition;
            }
            GUILayout.Label(EndFrame.ToString());
            GUILayout.Space(5);


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("LT", GUILayout.MaxWidth(50));

            CurrentShowingPosition = (int)GUILayout.HorizontalSlider(CurrentShowingPosition, StartFrame, EndFrame);

            GUILayout.Label("RT",GUILayout.MaxWidth(50));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void MarkersPanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(10, 50), new Vector2(Screen.width / 4, Screen.height / 3f)), InGameUI.BoxStyle);
            GUILayout.Label("Cam Markers");
            MarkerScroll = GUILayout.BeginScrollView(MarkerScroll);
            if (CamMarkers.Count > 0)
            {
                if(activemarker == null)
                {
                   


                    for (int i = 0; i < CamMarkers.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                    if(GUILayout.Button($"Marker {i}"))
                    {
                        if(activemarker != null)
                        {
                          activemarker = null;
                        }
                        else
                        {
                                activemarker = CamMarkers[i];
                               
                                ReplayCam.transform.position = activemarker.CamPos;
                                ReplayCam.transform.rotation = activemarker.CamRot;
                        }
                    }
                        if (GUILayout.Button("Remove",Offstyle))
                        {
                            CamMarkers.RemoveAt(i);
                        }

                        GUILayout.EndHorizontal();
                      GUILayout.Space(5);
                    }

                }
                else
                {
                 MarkerEditBox(activemarker);
                }


            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void MarkerEditBox(ReplayPosition pos)
        {
            if (GUILayout.Button("Return"))
            {
                activemarker = null;
            }
            GUILayout.Space(20);
            GUILayout.Label($"Edit Marker");
            GUILayout.Space(20);
            GUILayout.Label("Position");
            GUILayout.Label($"{pos.CamPos.ToString()}");

            GUILayout.Space(20);
            GUILayout.Label("Rotation");
            GUILayout.Label($"{pos.CamRot.ToString()}");
            GUILayout.Space(20);


            
            pos.AlterSpeed = GUILayout.Toggle(pos.AlterSpeed, "Speed Controlled");


            GUILayout.Label("Speed at marker");
            Speedatmarker = GUILayout.TextField(Speedatmarker);
            if (float.TryParse(Speedatmarker, out float speedatmark))
            {
                pos.Speedatmarker = speedatmark;
            }
            GUILayout.Space(10);


            GUILayout.BeginHorizontal();
            GUILayout.Label("Frames in");
            speedframein = GUILayout.TextField(speedframein);
            if(int.TryParse(speedframein,out int speedfrres))
            {
                pos.SpeedFramein = speedfrres;
            }

            GUILayout.Label("Frames out");
            speedframeout = GUILayout.TextField(speedframeout);
            if (int.TryParse(speedframeout, out int speedfrresout))
            {
                pos.SpeedFrameout = speedfrresout;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Style in:");
            if (GUILayout.Button(SlowMoLabels[pos.StyleIn]))
            {
                if(pos.StyleIn == ReplayPosition.SlowMoStyle.Slowout)
                {
                    pos.StyleIn = ReplayPosition.SlowMoStyle.Linear;
                }
                else
                {
                    pos.StyleIn++;
                }
            }


            GUILayout.Space(5);
            GUILayout.Label("Style out:");
            if (GUILayout.Button(SlowMoLabels[pos.StyleOut]))
            {
                if (pos.StyleOut == ReplayPosition.SlowMoStyle.Slowout)
                {
                    pos.StyleOut = ReplayPosition.SlowMoStyle.Linear;
                }
                else
                {
                    pos.StyleOut++;
                }
            }
            GUILayout.EndHorizontal();


            


            if (MyPlayerPositions.Count > CurrentShowingPosition)
            {

                // plug in Position Data to rider

                MyRidersTrans[0].position = MyPlayerPositions[CurrentShowingPosition].Positions[0];
                MyRidersTrans[0].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]);
                for (int i = 1; i < 23; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                }

                // hip joint
                MyRidersTrans[20].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[20];



                MyRidersTrans[24].position = MyPlayerPositions[CurrentShowingPosition].Positions[24];
                MyRidersTrans[24].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]);

                MyRidersTrans[25].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[25];
                MyRidersTrans[27].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[27];


                for (int i = 25; i < 32; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);

                }

                MyRidersTrans[32].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]);
                MyRidersTrans[33].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]);

                remaining = Mathf.Clamp(remaining - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);

            }

            if (InGameUI.instance.Connected)
            {

                // online players position update
                foreach (RemotePlayer player in GameManager.Players.Values)
                {
                    player.nameSign.transform.rotation = Camera.current.transform.rotation;

                    if (player.ReplayPositions.Count > CurrentShowingPosition)
                    {
                        player.Riders_Transforms[0].position = player.ReplayPositions[CurrentShowingPosition].Positions[0];
                        player.Riders_Transforms[0].rotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]);
                        for (int i = 1; i < 23; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]);
                        }

                        // hip joint
                        player.Riders_Transforms[20].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[20];



                        player.Riders_Transforms[24].position = player.ReplayPositions[CurrentShowingPosition].Positions[24];
                        player.Riders_Transforms[24].rotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]);

                        player.Riders_Transforms[25].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[25];
                        player.Riders_Transforms[27].localPosition = player.ReplayPositions[CurrentShowingPosition].Positions[27];


                        for (int i = 25; i < 32; i++)
                        {
                            player.Riders_Transforms[i].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]);

                        }

                        player.Riders_Transforms[32].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]);
                        player.Riders_Transforms[33].localRotation = Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]);


                    }

                }



            }




            pos.CamPos = ReplayCam.transform.position;
            pos.CamRot = ReplayCam.transform.rotation;
           


        }

        void PlaySetup()
        {
            if (PlayThrough)
            {
                PlayThrough = false;
            }
            else
            {
                OriginalSpeed = SetSpeed;

                if (CurrentCamTargetPosition >= CamMarkers.Count-1 | CurrentShowingPosition >= EndFrame)
                {
                    CurrentCamPosition = 0;
                    CurrentCamTargetPosition = 1;
                    CurrentShowingPosition = StartFrame;
                    ReplayCam.transform.position = CamMarkers[CurrentCamPosition].CamPos;
                    ReplayCam.transform.rotation = CamMarkers[CurrentCamPosition].CamRot;


                    MyRidersTrans[0].position = MyPlayerPositions[CurrentShowingPosition].Positions[0];
                    MyRidersTrans[0].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]);
                    for (int i = 1; i < 23; i++)
                    {
                        MyRidersTrans[i].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[i];
                        MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                    }
                    // hip joint
                    MyRidersTrans[20].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[20];


                    MyRidersTrans[24].position = MyPlayerPositions[CurrentShowingPosition].Positions[24];
                    MyRidersTrans[24].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]);

                    MyRidersTrans[25].localPosition =  MyPlayerPositions[CurrentShowingPosition].Positions[25];
                    MyRidersTrans[27].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[27];


                    for (int i = 25; i < 32; i++)
                    {
                        MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                    }
                    MyRidersTrans[32].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]);
                    MyRidersTrans[33].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]);



                    Camspan = 0;
                    RemovedCamSpan = 0.0001f;
                    remaining = MyPlayerPositions[CurrentShowingPosition + 1].Timspanfromlast;
                    removed = 0;
                    for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                    {
                        Camspan = Camspan + MyPlayerPositions[i].Timspanfromlast;
                    }


                }
                TriggerPlaySpeed = SetSpeed;
                PlayThrough = true;
            }

        }
        void Play()
        {
            if (CamMarkers.Count - 1 >= CurrentCamTargetPosition)
            {
                // position and rotation of cam relative to current showing frame and current markers



                    SpeedChange();
                    TriggerPlaySpeed = SetSpeed;
                    if (CurrentShowingPosition != EndFrame)
                    {


                        if (remaining < (Time.deltaTime * TriggerPlaySpeed / 2))
                        {
                            if (CurrentShowingPosition >= EndFrame)
                            {
                                CurrentShowingPosition = EndFrame;
                            }
                            else
                            {
                                CurrentShowingPosition++;
                                removed = remaining;
                                remaining = Mathf.Clamp(remaining + MyPlayerPositions[CurrentShowingPosition].Timspanfromlast, 0.00001f, 0.1f);
                            }
                        }

                        if (CurrentShowingPosition > CamMarkers[CurrentCamTargetPosition].ReferenceFrame)
                        {
                           if(CurrentCamTargetPosition < CamMarkers.Count - 1)
                           {
                            CurrentCamPosition++;
                            CurrentCamTargetPosition++;
                            RemovedCamSpan = Camspan;
                            for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                            {
                                Camspan = Camspan + MyPlayerPositions[i].Timspanfromlast;
                            }

                           }
                        }



                    }
                    else
                    {
                      PlayThrough = false;
                    RemovedCamSpan = 0;


                    for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                    {
                        RemovedCamSpan = RemovedCamSpan + MyPlayerPositions[i].Timspanfromlast;
                    }




                    }




                if (PlayThrough)
                {


                    // position to go to
                    Vector3 currentpos = CamMarkers[CurrentCamTargetPosition].CamPos;
                    currentrot = CamMarkers[CurrentCamTargetPosition].CamRot;
                    Vector3 Vel = -(ReplayCam.transform.position - currentpos).normalized;
                    ReplayCam.transform.position = Vector3.SmoothDamp(ReplayCam.transform.position, Vector3.MoveTowards(ReplayCam.transform.position, currentpos, Vector3.Distance(ReplayCam.transform.position, currentpos) / Camspan * Time.deltaTime * TriggerPlaySpeed), ref Vel, BlendValue);

                    // rotation to go to
                    ReplayCam.transform.rotation = Quaternion.RotateTowards(ReplayCam.transform.rotation, currentrot, Quaternion.Angle(ReplayCam.transform.rotation, currentrot) / Camspan * Time.deltaTime * TriggerPlaySpeed);

                // remove time
                Camspan = Mathf.Clamp(Camspan - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 1000);
                RemovedCamSpan = Mathf.Clamp(RemovedCamSpan + (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 1000);



                // my position update
                if (MyPlayerPositions.Count > CurrentShowingPosition)
                {
                    // plug in Position Data to rider
                    MyRidersTrans[0].position = Vector3.MoveTowards(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                    MyRidersTrans[0].rotation = Quaternion.RotateTowards(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[0].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    for (int i = 1; i < 23; i++)
                    {
                        MyRidersTrans[i].localPosition = Vector3.MoveTowards(MyRidersTrans[i].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[i], Vector3.Distance(MyRidersTrans[i].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[i]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                        MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    }
                    // hip joint
                    MyRidersTrans[20].localPosition = Vector3.MoveTowards(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20]) / remaining * (Time.deltaTime * TriggerPlaySpeed));


                    MyRidersTrans[24].position = Vector3.MoveTowards(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    MyRidersTrans[24].rotation = Quaternion.RotateTowards(MyRidersTrans[24].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[24].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                    MyRidersTrans[25].localPosition = Vector3.MoveTowards(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25]) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    MyRidersTrans[27].localPosition = Vector3.MoveTowards(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27]) / remaining * (Time.deltaTime * TriggerPlaySpeed));


                    for (int i = 25; i < 32; i++)
                    {
                        MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    }
                    MyRidersTrans[32].localRotation = Quaternion.RotateTowards(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[32].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32])) / remaining * (Time.deltaTime * TriggerPlaySpeed));
                    MyRidersTrans[33].localRotation = Quaternion.RotateTowards(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[33].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33])) / remaining * (Time.deltaTime * TriggerPlaySpeed));

                    remaining = Mathf.Clamp(remaining - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);
                }

                // others position update
                if (InGameUI.instance.Connected)
                {

                    // online players position update
                    foreach (RemotePlayer player in GameManager.Players.Values)
                    {
                        player.nameSign.transform.rotation = Camera.current.transform.rotation;

                        if (player.ReplayPositions.Count > CurrentShowingPosition)
                        {
                            player.Riders_Transforms[0].position = Vector3.MoveTowards(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            player.Riders_Transforms[0].rotation = Quaternion.RotateTowards(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[0].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            for (int i = 1; i < 23; i++)
                            {
                                player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            }

                            // hip joint
                            player.Riders_Transforms[20].localPosition = Vector3.MoveTowards(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20]) / remaining * Time.deltaTime * TriggerPlaySpeed);



                            player.Riders_Transforms[24].position = Vector3.MoveTowards(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            player.Riders_Transforms[24].rotation = Quaternion.RotateTowards(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[24].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                            player.Riders_Transforms[25].localPosition = Vector3.MoveTowards(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25]) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            player.Riders_Transforms[27].localPosition = Vector3.MoveTowards(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27]) / remaining * Time.deltaTime * TriggerPlaySpeed);


                            for (int i = 25; i < 32; i++)
                            {
                                player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / remaining * Time.deltaTime * TriggerPlaySpeed);

                            }

                            player.Riders_Transforms[32].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[32].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[32].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32])) / remaining * Time.deltaTime * TriggerPlaySpeed);
                            player.Riders_Transforms[33].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[33].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[33].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33])) / remaining * Time.deltaTime * TriggerPlaySpeed);


                        }

                    }



                }





                }





            }
            else
            {
                PlayThrough = false;
            }
        }

        void SwitchMode(bool editmode)
        {
            if (!editmode)
            {
                CurrentShowingPosition = StartFrame;
                EndFrame = CamMarkers[CamMarkers.Count - 1].ReferenceFrame;
                CurrentCamPosition = 0;
                CurrentCamTargetPosition = 1;

                CurrentShowingPosition = StartFrame;
                ReplayCam.transform.position = CamMarkers[CurrentCamPosition].CamPos;
                ReplayCam.transform.rotation = CamMarkers[CurrentCamPosition].CamRot;


                MyRidersTrans[0].position = MyPlayerPositions[CurrentShowingPosition].Positions[0];
                MyRidersTrans[0].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]);
                for (int i = 1; i < 23; i++)
                {
                    MyRidersTrans[i].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[i];
                    MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                }
                // hip joint
                MyRidersTrans[20].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[20];


                MyRidersTrans[24].position = MyPlayerPositions[CurrentShowingPosition].Positions[24];
                MyRidersTrans[24].rotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]);

                MyRidersTrans[25].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[25];
                MyRidersTrans[27].localPosition = MyPlayerPositions[CurrentShowingPosition].Positions[27];


                for (int i = 25; i < 32; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]);
                }
                MyRidersTrans[32].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]);
                MyRidersTrans[33].localRotation = Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]);



                Camspan = 0;
                RemovedCamSpan = 0.0001f;
                remaining = MyPlayerPositions[CurrentShowingPosition + 1].Timspanfromlast;
                removed = 0;
                for (int i = CamMarkers[CurrentCamPosition].ReferenceFrame + 1; i <= CamMarkers[CurrentCamTargetPosition].ReferenceFrame; i++)
                {
                    Camspan = Camspan + MyPlayerPositions[i].Timspanfromlast;
                }



            }

            PlayEditToggle = !PlayEditToggle;
        }

        void AddCamMarker()
        {
            if (MGInputManager.X_Down())
            {
                if (CamMarkers.Count == 0)
                {
                    StartFrame = CurrentShowingPosition;
                }


                ReplayPosition marker = new ReplayPosition();
                marker.CamPos = new Vector3(ReplayCam.transform.position.x, ReplayCam.transform.position.y, ReplayCam.transform.position.z);
                marker.CamRot = new Quaternion(ReplayCam.transform.rotation.x, ReplayCam.transform.rotation.y, ReplayCam.transform.rotation.z, ReplayCam.transform.rotation.w);
                marker.ReferenceFrame = CurrentShowingPosition;
                marker.StyleIn = ReplayPosition.SlowMoStyle.SlowIn;
                marker.StyleOut = ReplayPosition.SlowMoStyle.Slowout;

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
                    TriggerScrollEditMode();
                }

                if (InGameUI.instance.Connected)
                {
                    GameManager.KeepNetworkActive();
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    HideGUI = !HideGUI;
                }


                // in Play mode
                if (PlayEditToggle)
                {
                    if (!PlayThrough)
                    {
                    TriggerScrollPlayMode();
                    }
                    else
                    {
                        Play();
                    }
                }


            }
            else
            {

                try
                {

                    if (ReplayWatch.ElapsedMilliseconds > 16f)
                    {

                        // Keep 30 seconds of my footage
                        if (MyPlayerPositions.Count > 1600)
                        {
                            MyPlayerPositions.RemoveRange(0, MyPlayerPositions.Count - 1600);
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

                            Currentpositions[24] = LocalPlayer.instance.Riders_Transforms[24].position;
                            Currentrotations[24] = LocalPlayer.instance.Riders_Transforms[24].eulerAngles;

                            Currentpositions[25] = LocalPlayer.instance.Riders_Transforms[25].localPosition;
                            Currentpositions[27] = LocalPlayer.instance.Riders_Transforms[27].localPosition;


                            for (int i = 25; i < 32; i++)
                            {
                                Currentrotations[i] = LocalPlayer.instance.Riders_Transforms[i].localEulerAngles;
                            }

                            Currentrotations[32] = LocalPlayer.instance.Riders_Transforms[32].localEulerAngles;
                            Currentrotations[33] = LocalPlayer.instance.Riders_Transforms[33].localEulerAngles;


                            ReplayPosition newreplaymarker = new ReplayPosition();
                            newreplaymarker.Positions = new Vector3[32];
                            newreplaymarker.Rotations = new Vector3[34];
                            newreplaymarker.Timspanfromlast = (float)ReplayWatch.Elapsed.TotalSeconds;
                            Array.Copy(Currentpositions, newreplaymarker.Positions, Currentpositions.Length);
                            Array.Copy(Currentrotations, newreplaymarker.Rotations, Currentrotations.Length);
                            MyPlayerPositions.Add(newreplaymarker);

                        }

                        // remote players footage track
                        if (InGameUI.instance.Connected && LocalPlayer.instance.ServerActive)
                        {
                            foreach (RemotePlayer player in GameManager.Players.Values)
                            {

                                if (player.MasterActive)
                                {

                                    if (player.ReplayPositions.Count > 1600)
                                    {
                                        player.ReplayPositions.RemoveRange(0, player.ReplayPositions.Count - 1600);
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

                                        pos[25] = player.Riders_Transforms[25].localPosition;
                                        pos[27] = player.Riders_Transforms[27].localPosition;

                                        for (int i = 25; i < 34; i++)
                                        {
                                            rot[i] = player.Riders_Transforms[i].localEulerAngles;
                                        }

                                        player.ReplayPositions.Add(new IncomingTransformUpdate(pos, rot, (float)ReplayWatch.Elapsed.TotalSeconds));


                                    }


                                }

                            }
                        }

                        ReplayWatch.Reset();
                        ReplayWatch.Start();

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
          
        }
        void Awake()
        {
            instance = this;
            MyRidersTrans = new Transform[34];
            Currentpositions = new Vector3[32];
            Currentrotations = new Vector3[34];


            Offstyle = new GUIStyle();
            Offstyle.normal.background = InGameUI.instance.RedTex;
            Offstyle.hover.background = InGameUI.instance.GreyTex;
            Offstyle.alignment = TextAnchor.MiddleCenter;
            Offstyle.normal.textColor = Color.white;
            Offstyle.hover.textColor = Color.white;

            OnStyle = new GUIStyle();
            OnStyle.normal.background = InGameUI.instance.GreenTex;
            OnStyle.hover.background = InGameUI.instance.GreyTex;
            OnStyle.onNormal.background = InGameUI.instance.GreenTex;
            OnStyle.onHover.background = InGameUI.instance.GreyTex;
            OnStyle.alignment = TextAnchor.MiddleCenter;
            OnStyle.onNormal.textColor = Color.white;
            OnStyle.onHover.textColor = Color.white;
            OnStyle.normal.textColor = Color.white;
            OnStyle.hover.textColor = Color.white;

            CamMarkerstyle = OnStyle;


            SlowMoLabels = new Dictionary<ReplayPosition.SlowMoStyle,string>
            {
                {ReplayPosition.SlowMoStyle.Linear,"Linear" },
                {ReplayPosition.SlowMoStyle.SlowIn,"Slow Start" },
                {ReplayPosition.SlowMoStyle.Slowout, "Slow End" },

            };

        }
        void Start()
        {

            BottomPanelStyle.fixedHeight = 50;
            BottomPanelStyle.fixedWidth = 50;
            BottomPanelStyle.fontStyle = FontStyle.Bold;


            ReplayCam = Instantiate(Camera.main.gameObject) as GameObject;
            ReplayCam.SetActive(false);
            DontDestroyOnLoad(ReplayCam);

            ReplayWatch.Start();

        }
        void OnGUI()
        {
            if (ReplayOpen)
            {
                GUI.skin = InGameUI.instance.skin;

                if (!HideGUI)
                {
                    TopPanel();
                    if (CammarkersOpen)
                    {
                     MarkersPanel();
                    }
                    if (OpenCamSettings)
                    {
                        CameraSettings.instance.Show();
                    }


                }

            }

        }


    }

    /// <summary>
    /// Useable for storing every pos and rot of the rider and bmx in a packet, or for a camera marker with referenceframe etc
    /// </summary>
    public class ReplayPosition
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public float Fov;
        public float FocusDistance;
        public float Aperture;
        public Vector3 CamPos;
        public Quaternion CamRot;
        public int ReferenceFrame;
        public float Timspanfromlast;
        public float Speedatmarker;
        public int SpeedFramein;
        public int SpeedFrameout;
        public SlowMoStyle StyleIn;
        public SlowMoStyle StyleOut;
        public bool AlterSpeed;




        public enum SlowMoStyle
        {
          Linear = 1,
          SlowIn = 2,
          Slowout = 3,

        }


    }


}
