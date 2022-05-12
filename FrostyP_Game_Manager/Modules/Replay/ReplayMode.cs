using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    public class ReplayMode : FrostyModule
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
        Vector2 MarkerScroll;
        GUIStyle Offstyle;
        GUIStyle OnStyle;
        GUIStyle LookatStyle;
        GUIStyle CamMarkerstyle;
        GUIStyle Playstyle;


        public static List<ReplayMark> MyPlayerPositions = new List<ReplayMark>();
        public static List<ReplayMark> CamMarkers = new List<ReplayMark>();
        public bool ReplayOpen = false;
        public bool LookAtPlayer = true;
        public int CurrentShowingPosition;
        public int lastcampos;
        public int camtargetpos = 1;
        public bool PlayThrough;
        public int StartFrame;
        public int EndFrame;
        public bool PlayEditToggle;
        public string ModeDisplay = "Play Mode";
        public string LookatDisplay = "Look at Player";
        public bool OpenCamSettings;

        float Free_cam_moveSpeed;
        float Free_cam_rotSpeed;
        float TriggerPlaySpeed;
        float PlayingSpeed = 0.5f;
        float OriginalSpeed;

        bool TriggerPressed;
        bool moveforward;
        bool moveback;

        // Camera position based on frame and markers
        float Camspan = 0.001f;
        float RemovedCamSpan = 0.001f;
        bool CammarkersOpen = false;
        ReplayMark activemarker;
        int SmoothLineVal = 1;
        string speedframein = "5";
        string speedframeout = "5";
        string Speedatmarker = "0.2";

        List<string> SlowMoLabels;

        /// <summary>
        /// Object sticks to path exactly as determined, cam follows smoothly
        /// </summary>
        GameObject FollowObject;
        Transform[] Camobjs;
        GameObject baselinerenderer;
        GameObject smoothlinerenderer;
        LineRenderer baseline;
        List<GameObject> baselinechildren;
        LineRenderer smoothline;
        bool LineActive = true;
        bool CamVisible = true;
        /// <summary>
        /// Cammarkers after being subdivided
        /// </summary>
        Vector3[] smoothedpoints;
        int smoothpostarget;
        Vector3[] originalpoints;
        float FullclipTime;
        float timeelapsed = 0;
        /// <summary>
        /// used to determine linerenderer colours, to viualise the camera's speed between markers and specifically the even-ness of the speed along the line
        /// </summary>
        float Average_time_span_between_cam_markers;
        float Average_distance_between_cam_markers;

        bool HideGUI;

        /// <summary>
        /// When replay is closed watch is running, a replayposition is recorded for every rider at the same with this same clock, passing in the elapsed seconds since the last position was recorded, used to keep syncronization of all riders and cam
        /// </summary>
        System.Diagnostics.Stopwatch Recording_watch = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// when we reach a target position, we reset to new target and load up this float with the full time span over which to reach the new target
        /// </summary>
        float Interp_time_to_next = 0.001f;
        /// <summary>
        /// how much time it should take to go backwards to the marker we were just at, as we reach a marker, this value ends up being the full time_remaining
        /// </summary>
        float Interp_time_to_last = 0.001f;
       

        public override void Open()
        {
            if (FollowObject == null && MultiplayerManager.SLRcam != null) FollowObject = Instantiate(MultiplayerManager.SLRcam);


            Debug.Log("Replay opening.");
            if (MultiplayerManager.isConnected())
            {
                MultiplayerManager.instance.TurnOffNetUpdates();
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
            CurrentShowingPosition = EndFrame;
            timeelapsed = 0;


            ReplayCam.SetActive(true);
            ReplayCam.transform.position = Camera.main.gameObject.transform.position;
            ReplayCam.transform.rotation = Camera.main.gameObject.transform.rotation;

            MyMan = GetPlayer();
            MyBmx = GetBmx();
            MyMan.transform.position = LocalPlayer.instance.ActiveModel.transform.position;
            MyBmx.transform.position = LocalPlayer.instance.Bmx_Root.transform.position;

            Debug.Log("Player created");
            MultiplayerManager.TogglePlayerComponents(false);
            _MG = new MGInputManager();


            Interp_time_to_last = MyPlayerPositions[EndFrame].Time_span_seconds;
            Interp_time_to_next = 0;
            // if online, pause comms but send keepalive packets


            ReplayOpen = true;
            FrostyPGamemanager.instance.OpenMenu = false;
            Recording_watch.Stop();
            base.Open();
            Debug.Log("open");
        }
        public override void Close()
        {
            CamMarkers.Clear();
            PlayThrough = false;
            if (MultiplayerManager.isConnected())
            {
                MultiplayerManager.instance.TurnOnNetUpdates();
            }
            Destroy(MyMan);
            Destroy(MyBmx);
            MultiplayerManager.TogglePlayerComponents(true);
            ReplayCam.SetActive(false);
            ReplayOpen = false;
            _MG = null;
            FrostyPGamemanager.instance.MenuShowing = 0;
            FrostyPGamemanager.instance.OpenMenu = true;
            Recording_watch.Start();
        }


        void PlayerFreeCam()
        {
            Free_cam_moveSpeed = 10;
            Free_cam_rotSpeed = 50;

            if (!MGInputManager.LB_Hold())
            {
              if (MGInputManager.LStickX() > 0.2f | MGInputManager.LStickX() < -0.2f | MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f | MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
              {
                ReplayCam.transform.Translate(MGInputManager.LStickX() * Time.deltaTime * Free_cam_moveSpeed, MGInputManager.RStickY() * Time.deltaTime * Free_cam_moveSpeed, MGInputManager.LStickY() * Time.deltaTime * Free_cam_moveSpeed);
              }

                if (LookAtPlayer && !PlayEditToggle)
                {
                    ReplayCam.transform.LookAt(MyMan.transform.FindDeepChild("mixamorig:Head"));
                }

            }
            else if (!LookAtPlayer && !PlayEditToggle)
            {
                ReplayCam.transform.Rotate(-MGInputManager.LStickY() * Time.deltaTime * Free_cam_rotSpeed, MGInputManager.LStickX() * Time.deltaTime * Free_cam_rotSpeed, -MGInputManager.RStickX() * Time.deltaTime * Free_cam_rotSpeed);
            }
            

        }

        void TriggerScrollEditMode()
        {
            if (moveback)
            {
                TriggerPlaySpeed = MGInputManager.LTrigger();
                MoveCamFollowerThroughMarkers(false,false);
                InterpolateRiders(false, false, TriggerPlaySpeed, Interp_time_to_last);
                timeelapsed = timeelapsed - (Time.deltaTime * TriggerPlaySpeed);
                if (timeelapsed < 0) timeelapsed = 0;
            }
            else if (moveforward)
            {
                TriggerPlaySpeed = MGInputManager.RTrigger();
                MoveCamFollowerThroughMarkers(true, false);
                InterpolateRiders(true, false, TriggerPlaySpeed, Interp_time_to_next);
                timeelapsed = timeelapsed + (Time.deltaTime * TriggerPlaySpeed);
            }
            else
            {
                TriggerPlaySpeed = 0;
            }

        }

        void TriggerScrollPlayMode()
        {
            if (moveback)
            {
                TriggerPlaySpeed = MGInputManager.LTrigger();
                MoveCamFollowerThroughMarkers(false, false);
                InterpolateRiders(false, false, TriggerPlaySpeed, Interp_time_to_last);
                timeelapsed = timeelapsed - (Time.deltaTime * TriggerPlaySpeed);
                if (timeelapsed < 0) timeelapsed = 0;
            }
            else if (moveforward)
            {
                TriggerPlaySpeed = MGInputManager.RTrigger();
                MoveCamFollowerThroughMarkers(true, false);
                InterpolateRiders(true, false, TriggerPlaySpeed, Interp_time_to_next);
                timeelapsed = timeelapsed + (Time.deltaTime * TriggerPlaySpeed);
                if (timeelapsed >= FullclipTime) timeelapsed = FullclipTime;
            }
            else
            {
                TriggerPlaySpeed = 0;
            }
        }

        void InterpolateRiders(bool forward, bool playing, float playspeed, float span)
        {
            #region Calculate this movement

            // less than a deltatime's worth of movement left
            if (span < Time.deltaTime)
            {
                // on start frame moving back, higher than start moving back, less than end moving forward, higher than end moving forward
                if (CurrentShowingPosition <= StartFrame && !forward)
                {
                    // lerp to start and reset
                    ReplayCam.transform.position = Vector3.Lerp(ReplayCam.transform.position, CamMarkers[0].CamPos, 1);
                    ReplayCam.transform.rotation = Quaternion.Lerp(ReplayCam.transform.rotation, CamMarkers[0].CamRot, 1);
                    FollowObject.transform.position = Vector3.Lerp(FollowObject.transform.position, CamMarkers[0].CamPos, 1);
                    FollowObject.transform.rotation = Quaternion.Lerp(FollowObject.transform.rotation, CamMarkers[0].CamRot, 1);
                    CurrentShowingPosition = StartFrame;
                    Interp_time_to_last = 0.001f;
                    Interp_time_to_next = MyPlayerPositions[StartFrame + 1].Time_span_seconds;
                    span = 0;
                }
                else if(CurrentShowingPosition > StartFrame && !forward)
                {
                    // set new backwards movement
                    CurrentShowingPosition--;
                    Interp_time_to_next = Interp_time_to_last;
                    Interp_time_to_last = Interp_time_to_last + MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                    span = Interp_time_to_last = Interp_time_to_last + MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                }
                else if (CurrentShowingPosition < EndFrame && forward)
                {
                    CurrentShowingPosition++;
                    Interp_time_to_last = Interp_time_to_next;
                    Interp_time_to_next = Interp_time_to_next + MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                    span = Interp_time_to_next + MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                }
                else if (CurrentShowingPosition >= EndFrame && forward)
                {
                    CurrentShowingPosition = EndFrame;
                    ReplayCam.transform.position = Vector3.Lerp(ReplayCam.transform.position, CamMarkers[camtargetpos].CamPos, 1);
                    ReplayCam.transform.rotation = Quaternion.Lerp(ReplayCam.transform.rotation, CamMarkers[camtargetpos].CamRot, 1);
                    FollowObject.transform.position = Vector3.Lerp(FollowObject.transform.position, CamMarkers[camtargetpos].CamPos, 1);
                    FollowObject.transform.rotation = Quaternion.Lerp(FollowObject.transform.rotation, CamMarkers[camtargetpos].CamRot, 1);

                    Interp_time_to_last = MyPlayerPositions[EndFrame].Time_span_seconds;
                    Interp_time_to_next = 0.0001f;
                    span = 0;
                    if (PlayThrough) PlayThrough = false;
                }
            }

            #endregion

            #region Actual movement of all riders

            // my position update
            if (MyPlayerPositions.Count > CurrentShowingPosition)
            {
                // plug in Position Data to rider
                MyRidersTrans[0].position = Vector3.MoveTowards(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(MyRidersTrans[0].position, MyPlayerPositions[CurrentShowingPosition].Positions[0]) / span * Time.deltaTime * playspeed);
                MyRidersTrans[0].rotation = Quaternion.RotateTowards(MyRidersTrans[0].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[0].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[0])) / span * (Time.deltaTime * playspeed));
                for (int i = 1; i < 23; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / span * (Time.deltaTime * playspeed));
                }
                // hip joint
                MyRidersTrans[20].localPosition = Vector3.MoveTowards(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(MyRidersTrans[20].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[20]) / span * (Time.deltaTime * playspeed));


                MyRidersTrans[24].position = Vector3.MoveTowards(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(MyRidersTrans[24].position, MyPlayerPositions[CurrentShowingPosition].Positions[24]) / span * (Time.deltaTime * playspeed));
                MyRidersTrans[24].rotation = Quaternion.RotateTowards(MyRidersTrans[24].rotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[24].eulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[24])) / span * (Time.deltaTime * playspeed));

                MyRidersTrans[25].localPosition = Vector3.MoveTowards(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(MyRidersTrans[25].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[25]) / span * (Time.deltaTime * playspeed));
                MyRidersTrans[27].localPosition = Vector3.MoveTowards(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(MyRidersTrans[27].localPosition, MyPlayerPositions[CurrentShowingPosition].Positions[27]) / span * (Time.deltaTime * playspeed));


                for (int i = 25; i < 32; i++)
                {
                    MyRidersTrans[i].localRotation = Quaternion.RotateTowards(MyRidersTrans[i].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[i].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[i])) / span * (Time.deltaTime * playspeed));
                }
                MyRidersTrans[32].localRotation = Quaternion.RotateTowards(MyRidersTrans[32].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[32].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[32])) / span * (Time.deltaTime * playspeed));
                MyRidersTrans[33].localRotation = Quaternion.RotateTowards(MyRidersTrans[33].localRotation, Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(MyRidersTrans[33].localEulerAngles), Quaternion.Euler(MyPlayerPositions[CurrentShowingPosition].Rotations[33])) / span * (Time.deltaTime * playspeed));

            }

                // others position update
                if (MultiplayerManager.isConnected())
                {

                    // online players position update
                    foreach (RemotePlayer player in MultiplayerManager.Players.Values)
                    {
                        player.nameSign.transform.rotation = Camera.current.transform.rotation;

                        if (player.ReplayPositions.Count > CurrentShowingPosition)
                        {
                            player.Riders_Transforms[0].position = Vector3.MoveTowards(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0], Vector3.Distance(player.Riders_Transforms[0].position, player.ReplayPositions[CurrentShowingPosition].Positions[0]) / span * Time.deltaTime * playspeed);
                            player.Riders_Transforms[0].rotation = Quaternion.RotateTowards(player.Riders_Transforms[0].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[0].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[0])) / span * Time.deltaTime * playspeed);
                            for (int i = 1; i < 23; i++)
                            {
                                player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / span * Time.deltaTime * playspeed);
                            }

                            // hip joint
                            player.Riders_Transforms[20].localPosition = Vector3.MoveTowards(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20], Vector3.Distance(player.Riders_Transforms[20].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[20]) / span * Time.deltaTime * playspeed);



                            player.Riders_Transforms[24].position = Vector3.MoveTowards(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24], Vector3.Distance(player.Riders_Transforms[24].position, player.ReplayPositions[CurrentShowingPosition].Positions[24]) / span * Time.deltaTime * playspeed);
                            player.Riders_Transforms[24].rotation = Quaternion.RotateTowards(player.Riders_Transforms[24].rotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[24].eulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[24])) / span * Time.deltaTime * playspeed);

                            player.Riders_Transforms[25].localPosition = Vector3.MoveTowards(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25], Vector3.Distance(player.Riders_Transforms[25].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[25]) / span * Time.deltaTime * playspeed);
                            player.Riders_Transforms[27].localPosition = Vector3.MoveTowards(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27], Vector3.Distance(player.Riders_Transforms[27].localPosition, player.ReplayPositions[CurrentShowingPosition].Positions[27]) / span * Time.deltaTime * playspeed);


                            for (int i = 25; i < 32; i++)
                            {
                                player.Riders_Transforms[i].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[i].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[i].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[i])) / span * Time.deltaTime * playspeed);

                            }

                            player.Riders_Transforms[32].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[32].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[32].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[32])) / span * Time.deltaTime * playspeed);
                            player.Riders_Transforms[33].localRotation = Quaternion.RotateTowards(player.Riders_Transforms[33].localRotation, Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33]), Quaternion.Angle(Quaternion.Euler(player.Riders_Transforms[33].localEulerAngles), Quaternion.Euler(player.ReplayPositions[CurrentShowingPosition].Rotations[33])) / span * Time.deltaTime * playspeed);


                        }

                    }



                }

            // update time
            if (forward)
            {
                Interp_time_to_last = Interp_time_to_last + (Time.deltaTime * playspeed);
                Interp_time_to_next = Interp_time_to_next - (Time.deltaTime * playspeed);
            }
            else
            {
                Interp_time_to_last = Interp_time_to_last - (Time.deltaTime * playspeed);
                Interp_time_to_next = Interp_time_to_next + (Time.deltaTime * playspeed);
            }


            #endregion

        }

        void MoveCamFollowerThroughMarkers(bool forward, bool playing)
        {
            if (CamMarkers.Count < 2) return;

            if(TriggerPressed | PlayThrough)
            {

                if (timeelapsed > CamMarkers[camtargetpos].cliptimestamp && CamMarkers.Count > camtargetpos && forward)
                {
                  camtargetpos++;
                }
                else if (timeelapsed < CamMarkers[camtargetpos].cliptimestamp && camtargetpos != 0 && !forward)
                {
                  camtargetpos--; 
                }
               


                // follow object moving
                Vector3 currentpos;
                Quaternion currentrot;
                if (forward)
                {

                    // position
                    currentpos = CamMarkers[camtargetpos].CamPos;
                    currentrot = CamMarkers[camtargetpos].CamRot;
                    FollowObject.transform.position = Vector3.MoveTowards(FollowObject.transform.position, currentpos, Vector3.Distance(FollowObject.transform.position, currentpos) / (CamMarkers[camtargetpos].cliptimestamp - timeelapsed) * Time.deltaTime * TriggerPlaySpeed);

                    // rotation
                    FollowObject.transform.rotation = Quaternion.RotateTowards(FollowObject.transform.rotation, currentrot, Quaternion.Angle(FollowObject.transform.rotation, currentrot) / (CamMarkers[camtargetpos].cliptimestamp - timeelapsed) * Time.deltaTime * TriggerPlaySpeed);


                }
                else
                {

                    // position
                    currentpos = CamMarkers[camtargetpos].CamPos;
                    currentrot = CamMarkers[camtargetpos].CamRot;
                    FollowObject.transform.position = Vector3.MoveTowards(FollowObject.transform.position, currentpos, Vector3.Distance(FollowObject.transform.position, currentpos) / (CamMarkers[camtargetpos + 1].cliptimestamp - timeelapsed) * Time.deltaTime * TriggerPlaySpeed);

                    // rotation
                    FollowObject.transform.rotation = Quaternion.RotateTowards(FollowObject.transform.rotation, currentrot, Quaternion.Angle(FollowObject.transform.rotation, currentrot) / (CamMarkers[camtargetpos + 1].cliptimestamp - timeelapsed) * Time.deltaTime * TriggerPlaySpeed);



                }


            }
        }

        void CamMoveToFollower(bool playing)
        {
            // position to go to
            Vector3 currentpos = FollowObject.transform.position;
            Quaternion currentrot = FollowObject.transform.rotation;
            ReplayCam.transform.position = Vector3.Lerp(ReplayCam.transform.position, currentpos, SmoothLineVal);

            // rotation to go to
            ReplayCam.transform.rotation = Quaternion.RotateTowards(ReplayCam.transform.rotation, currentrot, Quaternion.Angle(ReplayCam.transform.rotation, currentrot) * SmoothLineVal);

        }

        void SpeedChangeRun()
        {
            if(CamMarkers[camtargetpos].ReferenceFrame - CamMarkers[camtargetpos].Speed_change_start_frame <= CurrentShowingPosition && CamMarkers[camtargetpos].AlterSpeed && CurrentShowingPosition <= CamMarkers[camtargetpos].ReferenceFrame)
            {
                ReplayMark rp = CamMarkers[camtargetpos];
                

                // going in

                if (rp.StyleIn == ReplayMark.SlowMoStyle.Linear)
                {

                    float full_speed_difference = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    float speed_to_remove_each_frame;
                    float Frames_required;

                    // take off an equal fraction each time till we get there
                    float Deltas_of_each_frame = 0;
                    for (int i = rp.ReferenceFrame - rp.Speed_change_start_frame; i <= rp.ReferenceFrame; i++)
                    {
                        Deltas_of_each_frame = Deltas_of_each_frame + MyPlayerPositions[i].Time_span_seconds;
                    }
                    Frames_required = Deltas_of_each_frame / Time.deltaTime * PlayingSpeed;
                    speed_to_remove_each_frame = full_speed_difference / Frames_required;

                    //float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    // PlayingSpeed = Mathf.MoveTowards(PlayingSpeed, rp.Speedatmarker, diff / deltas * Time.deltaTime * PlayingSpeed);
                    if (PlayingSpeed > rp.Speedatmarker)
                    {
                     PlayingSpeed = PlayingSpeed - speed_to_remove_each_frame;
                    }
                    else
                    {
                        PlayingSpeed = rp.Speedatmarker;
                    }

                }
                else if(rp.StyleIn == ReplayMark.SlowMoStyle.SlowIn)
                {
                    // slow lerp at start then speed up
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame - rp.Speed_change_start_frame + 1; i <= rp.ReferenceFrame; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Time_span_seconds;
                    }


                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    PlayingSpeed = Mathf.MoveTowards(PlayingSpeed, rp.Speedatmarker, diff / deltas * Time.deltaTime * PlayingSpeed);


                }
                else if (rp.StyleIn == ReplayMark.SlowMoStyle.Slowout)
                {
                    // fast lerp at start then slow down
                    float deltas = 0;
                    for (int i = CurrentShowingPosition; i < rp.ReferenceFrame; i++)
                    {
                        deltas = Mathf.Clamp(deltas + MyPlayerPositions[i].Time_span_seconds,0.001f,100);
                    }


                    float diff = Mathf.Clamp(PlayingSpeed >= rp.Speedatmarker ? PlayingSpeed - rp.Speedatmarker : rp.Speedatmarker - PlayingSpeed,0.001f,100);
                    PlayingSpeed = Mathf.Clamp(Mathf.MoveTowards(PlayingSpeed, rp.Speedatmarker, diff / 4 * Time.deltaTime * PlayingSpeed),0.001f,100);

                }



            }
            else if(CamMarkers[lastcampos].ReferenceFrame + CamMarkers[lastcampos].Speed_change_end_frame >= CurrentShowingPosition && CamMarkers[lastcampos].AlterSpeed)
            {
                // going out

                ReplayMark rp = CamMarkers[lastcampos];
                if (rp.StyleOut == ReplayMark.SlowMoStyle.Linear)
                {
                    float full_speed_difference = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    float speed_to_add_each_frame;
                    float Frames_required;

                    // take off an equal fraction each time till we get there
                    float deltas_of_each_frame = 0;
                    for (int i = rp.ReferenceFrame; i <= rp.ReferenceFrame + rp.Speed_change_end_frame; i++)
                    {
                        deltas_of_each_frame = deltas_of_each_frame + MyPlayerPositions[i].Time_span_seconds;
                    }
                    Frames_required = deltas_of_each_frame / Time.deltaTime * PlayingSpeed;
                    speed_to_add_each_frame = full_speed_difference / Frames_required;


                    if (PlayingSpeed < OriginalSpeed) 
                    { 
                        PlayingSpeed = PlayingSpeed + speed_to_add_each_frame;
                    }
                    else
                    {
                        PlayingSpeed = OriginalSpeed;
                    }
                    
                }
                else if (rp.StyleOut == ReplayMark.SlowMoStyle.SlowIn)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = rp.ReferenceFrame + 1; i <= rp.ReferenceFrame + rp.Speed_change_end_frame; i++)
                    {
                        deltas = deltas + MyPlayerPositions[i].Time_span_seconds;
                    }



                    float diff = OriginalSpeed > rp.Speedatmarker ? OriginalSpeed - rp.Speedatmarker : rp.Speedatmarker - OriginalSpeed;
                    PlayingSpeed = Mathf.MoveTowards(PlayingSpeed, OriginalSpeed, diff / deltas * Time.deltaTime * PlayingSpeed);

                }
                else if (rp.StyleOut == ReplayMark.SlowMoStyle.Slowout)
                {
                    // take off an equal fraction each time till we get there
                    float deltas = 0;
                    for (int i = CurrentShowingPosition; i < rp.ReferenceFrame + rp.Speed_change_end_frame; i++)
                    {
                        deltas = Mathf.Clamp(deltas + MyPlayerPositions[i].Time_span_seconds,0.001f,100);
                    }

                    float diff = Mathf.Clamp(PlayingSpeed >= rp.Speedatmarker ? PlayingSpeed - rp.Speedatmarker : rp.Speedatmarker - PlayingSpeed,0.001f,100);
                    PlayingSpeed = Mathf.Clamp(Mathf.MoveTowards(PlayingSpeed, OriginalSpeed, diff / 4 * Time.deltaTime * PlayingSpeed),0.001f,100);

                }
            }
        }
        void FovChangeRun()
        {

        }

        void TopPanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(10, 10), new Vector2(Screen.width -20, Screen.height / 10)));
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
                if (GUILayout.Button(baselinerenderer !=null && smoothlinerenderer != null ? "Show Lines:" + baselinerenderer.activeInHierarchy.ToString() : "line error", LineActive?OnStyle:Offstyle))
                {
                    baselinerenderer.SetActive(!baselinerenderer.activeInHierarchy);
                    smoothlinerenderer.SetActive(!smoothlinerenderer.activeInHierarchy);
                }
                GUILayout.Space(5);
                if (GUILayout.Button(FollowObject != null ? "Show Cam:" + CamVisible : "Cam error",CamVisible?OnStyle:Offstyle))
                {
                    ToggleCamVisible(!CamVisible);
                }
                GUILayout.Space(5);
                GUILayout.Label("Smooth:");
                SmoothLineVal = Mathf.RoundToInt(GUILayout.HorizontalSlider(SmoothLineVal, 1, 4,GUILayout.MaxWidth(100)));
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
                GUILayout.Label($"PlaySpeed: {PlayingSpeed}");
                GUILayout.Space(5);
                PlayingSpeed = GUILayout.HorizontalSlider(PlayingSpeed,0f, 1f);
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
              

            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.Space(50);
            if(CamMarkers.Count>camtargetpos) GUILayout.Label($"Averg distance: {Average_distance_between_cam_markers} | averg time: {Average_time_span_between_cam_markers} | averg vel: {Average_distance_between_cam_markers/Average_time_span_between_cam_markers}, elapsed: {timeelapsed}, Full:{FullclipTime}, curCamTarget{camtargetpos}, smoothtarg{smoothpostarget}");

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
                                LookAtPlayer = false;
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

        void MarkerEditBox(ReplayMark pos)
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


            
            pos.AlterSpeed = GUILayout.Toggle(pos.AlterSpeed, "Speed Control on/off");


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
                pos.Speed_change_start_frame = speedfrres;
            }

            GUILayout.Label("Frames out");
            speedframeout = GUILayout.TextField(speedframeout);
            if (int.TryParse(speedframeout, out int speedfrresout))
            {
                pos.Speed_change_end_frame = speedfrresout;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Style in:");
            if (GUILayout.Button(SlowMoLabels[(int)pos.StyleIn]))
            {
                if(pos.StyleIn == ReplayMark.SlowMoStyle.Slowout)
                {
                    pos.StyleIn = ReplayMark.SlowMoStyle.Linear;
                }
                else
                {
                    pos.StyleIn++;
                }
            }


            GUILayout.Space(5);
            GUILayout.Label("Style out:");
            if (GUILayout.Button(SlowMoLabels[(int)pos.StyleOut]))
            {
                if (pos.StyleOut == ReplayMark.SlowMoStyle.Slowout)
                {
                    pos.StyleOut = ReplayMark.SlowMoStyle.Linear;
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

                Interp_time_to_next = Mathf.Clamp(Interp_time_to_next - (Time.deltaTime * TriggerPlaySpeed), 0.00001f, 0.1f);

            }

            if (MultiplayerManager.isConnected())
            {

                // online players position update
                foreach (RemotePlayer player in MultiplayerManager.Players.Values)
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

                ToggleCamVisible(false);
                OriginalSpeed = PlayingSpeed;

                if (camtargetpos >= CamMarkers.Count-1 | CurrentShowingPosition >= EndFrame)
                {
                    lastcampos = 0;
                    camtargetpos = 1;
                    CurrentShowingPosition = StartFrame;
                    ReplayCam.transform.position = CamMarkers[lastcampos].CamPos;
                    ReplayCam.transform.rotation = CamMarkers[lastcampos].CamRot;
                    FollowObject.transform.position = CamMarkers[lastcampos].CamPos;
                    FollowObject.transform.rotation = CamMarkers[lastcampos].CamRot;


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
                    Interp_time_to_next = MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                    Interp_time_to_last = 0;
                    for (int i = CamMarkers[lastcampos].ReferenceFrame + 1; i <= CamMarkers[camtargetpos].ReferenceFrame; i++)
                    {
                        Camspan = Camspan + MyPlayerPositions[i].Time_span_seconds;
                    }


                }
                TriggerPlaySpeed = PlayingSpeed;
                PlayThrough = true;
            }

        }
        void Play()
        {
            if (CamMarkers.Count - 1 >= camtargetpos)
            {
               
                #region Renew Data for this movement
                TriggerPlaySpeed = PlayingSpeed;
                SpeedChangeRun();

                #endregion


                #region Actual movement of objects 

                    // camera being updated -----------------
                    MoveCamFollowerThroughMarkers(true,true);
                    CamMoveToFollower(true);
                    InterpolateRiders(true, true, PlayingSpeed, Interp_time_to_next);

                #endregion


            }
            else
            {
                PlayThrough = false;
            }
        }

        void ToggleCamVisible(bool value)
        {
            foreach (Transform t in Camobjs)
            {
                t.gameObject.SetActive(value);
            }
            CamVisible = value;
        }

        void SwitchMode(bool editmode)
        {
            FullclipTime = GetFullClipTimeSecs();
            if (!editmode)
            {
                timeelapsed = 0;
                CurrentShowingPosition = StartFrame;
                EndFrame = CamMarkers[CamMarkers.Count - 1].ReferenceFrame;
                lastcampos = 0;
                camtargetpos = 1;

                CurrentShowingPosition = StartFrame;
                ReplayCam.transform.position = CamMarkers[lastcampos].CamPos;
                ReplayCam.transform.rotation = CamMarkers[lastcampos].CamRot;
                FollowObject.transform.position = CamMarkers[lastcampos].CamPos;
                FollowObject.transform.rotation = CamMarkers[lastcampos].CamRot;

                

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
                Interp_time_to_next = MyPlayerPositions[CurrentShowingPosition + 1].Time_span_seconds;
                Interp_time_to_last = 0;
                for (int i = CamMarkers[lastcampos].ReferenceFrame + 1; i <= CamMarkers[camtargetpos].ReferenceFrame; i++)
                {
                    Camspan = Camspan + MyPlayerPositions[i].Time_span_seconds;
                }

                ToggleCamVisible(false);

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


                ReplayMark marker = new ReplayMark();
                marker.CamPos = new Vector3(ReplayCam.transform.position.x, ReplayCam.transform.position.y, ReplayCam.transform.position.z);
                marker.CamRot = new Quaternion(ReplayCam.transform.rotation.x, ReplayCam.transform.rotation.y, ReplayCam.transform.rotation.z, ReplayCam.transform.rotation.w);
                marker.ReferenceFrame = CurrentShowingPosition;
                marker.Fov = Camera.current.fieldOfView;
                marker.cliptimestamp = timeelapsed;
                CamMarkers.Add(marker);

                FullclipTime = GetFullClipTimeSecs();
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
                _RiderModel = MultiplayerManager.GetPlayerModel(LocalPlayer.instance.RiderModelname, LocalPlayer.instance.RiderModelBundleName);
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

        void ShowBaseLine()
        {
            try
            {
                if (CamMarkers.Count > 1)
                {
                    if (LineActive)
                    {
                        // assign colours of each segment

                        // parents line
                        baseline.positionCount = 2;
                        for (int i = 0; i < 2; i++)
                        {
                            baseline.SetPosition(i, CamMarkers[i].CamPos + Vector3.up);
                        }
                        float basespan = 0;
                        for (int times = CamMarkers[0].ReferenceFrame + 1; times < CamMarkers[1].ReferenceFrame + 1; times++)
                        {
                            basespan = basespan + MyPlayerPositions[times].Time_span_seconds;
                        }


                        float basered = Mathf.Clamp((Average_distance_between_cam_markers / Average_time_span_between_cam_markers) - (Vector3.Distance(CamMarkers[0].CamPos, CamMarkers[1].CamPos) / basespan) / 100, 0, 1);
                        float basegreen = Mathf.Clamp(1 - basered, 0, 1);
                        baseline.material.color = new Color(basered, basegreen, 0, 0.8f);

                        if (baselinechildren == null | baselinechildren.Count < CamMarkers.Count - 1) return;
                        // all children lines
                        if (CamMarkers.Count > 2)
                        {
                            int child = 0;
                            // for each cam marker in list
                            for (int markerno = 1; markerno < CamMarkers.Count - 1; markerno++)
                            {
                              LineRenderer line = baselinechildren[child].GetComponent<LineRenderer>();
                                line.positionCount = 2;
                                line.SetPosition(0, CamMarkers[markerno].CamPos + Vector3.up);
                                line.SetPosition(1, CamMarkers[markerno + 1].CamPos + Vector3.up);

                                // get timespan of this cam movement
                                float timespan = 0;
                                for (int times = CamMarkers[markerno].ReferenceFrame + 1; times < CamMarkers[markerno + 1].ReferenceFrame + 1; times++)
                                {
                                    timespan = timespan + MyPlayerPositions[times].Time_span_seconds;
                                }


                                float red = Mathf.Clamp((Average_distance_between_cam_markers / Average_time_span_between_cam_markers) - (Vector3.Distance(CamMarkers[markerno].CamPos,CamMarkers[markerno + 1].CamPos) / timespan) / 100, 0, 1);
                                float green = Mathf.Clamp(1 - red,0,1);
                                line.material.color = new Color(red, green, 0,0.8f);
                                child++;
                                
                            }

                        }


                    }

                }

            }
            catch (Exception x )
            {
                Debug.Log("ShowBaseLine() : " + x);
            }

            
        }
        void ShowSmoothLine()
        {
            if (CamMarkers.Count > 1)
            {
                Vector3[] originalarray = new Vector3[CamMarkers.Count];
                for (int i = 0; i < CamMarkers.Count; i++)
                {
                    originalarray[i] = CamMarkers[i].CamPos;
                }
                originalpoints = originalarray;
                smoothedpoints = SubdivideBaseLine(originalarray.ToList(),SmoothLineVal);

                if (LineActive && smoothedpoints != null)
                {
                    smoothline.positionCount = smoothedpoints.Length;
                    smoothline.SetPositions(smoothedpoints);

                }

            }
        }
        void ShowMarkerRotations()
        {

        }
        void CalculateCamAverageTimespanAndDistance()
        {
            if (CamMarkers.Count > 1)
            {
                List<float> timespans = new List<float>();
                List<float> distances = new List<float>();
                // for each cam marker
                for (int currentmarker = 0; currentmarker < CamMarkers.Count - 1; currentmarker++)
                {
                    // grab all timespans and distance between this marker and next
                    float distance = Vector3.Distance(CamMarkers[currentmarker].CamPos, CamMarkers[currentmarker + 1].CamPos);
                    float time = 0;
                    for (int currentframe = CamMarkers[currentmarker].ReferenceFrame + 1; currentframe < CamMarkers[currentmarker + 1].ReferenceFrame; currentframe++)
                    {
                        time = time + MyPlayerPositions[currentframe].Time_span_seconds;    
                    }
                    timespans.Add(time);
                    distances.Add(distance);
                }

                // get averages and store
                Average_time_span_between_cam_markers = 0;
                Average_distance_between_cam_markers = 0;
                for (int times = 0; times < timespans.Count; times++)
                {
                    Average_time_span_between_cam_markers = Average_time_span_between_cam_markers + timespans[times];
                }
                Average_time_span_between_cam_markers = Average_time_span_between_cam_markers / timespans.Count;

                for (int currentdistance = 0; currentdistance < distances.Count; currentdistance++)
                {
                    Average_distance_between_cam_markers = Average_distance_between_cam_markers + distances[currentdistance];
                }
                Average_distance_between_cam_markers = Average_distance_between_cam_markers / distances.Count;

            }
        }
        void ResetLineSegments()
        {
            try
            {
                List<GameObject> todelete = new List<GameObject>();
                if (CamMarkers.Count + 1 >= baselinechildren.Count)
                {
                    for (int i = 0; i < CamMarkers.Count - baselinerenderer.GetComponentsInChildren<Transform>(true).Length; i++)
                    {
                        GameObject child = new GameObject();
                        DontDestroyOnLoad(child);
                        LineRenderer rend = child.AddComponent<LineRenderer>();
                        rend.material = new Material(Shader.Find("Sprites/Default"));
                        rend.widthMultiplier = 0.2f;
                        child.transform.parent = baselinerenderer.transform;
                        baselinechildren.Add(child);
                    }



                }
                else if(CamMarkers.Count < baselinechildren.Count + 1 && baselinechildren.Count > 10)
                {
                    Transform[] transformsofchildren = baselinerenderer.GetComponentsInChildren<Transform>(true);
                    for (int tdel = 0; tdel < transformsofchildren.Length; tdel++)
                    {
                        todelete.Add(transformsofchildren[tdel].gameObject);
                    }
                    for (int tdel = 10; tdel < transformsofchildren.Length; tdel++)
                    {
                        todelete.Add(transformsofchildren[tdel].gameObject);
                        baselinechildren.RemoveAt(tdel-1);
                    }

                   
                }

                if (todelete.Count > 0)
                {
                    for (int i = 0; i < todelete.Count; i++)
                    {
                        Destroy(todelete[i]);
                    }
                }

            }
            catch (Exception x)
            { 
                Debug.Log("ResetLineSegments() : " + x);
            }

        }
       
        Vector3[] SubdivideBaseLine(List<Vector3> points,int iterations, float tightness = 0.3f)
        {
           
            for (int iteration = 0; iteration < iterations; iteration++)
            {
               
                List<Vector3> newpoints = new List<Vector3>();
                newpoints.Add(points[0]);
                // for each point in the newpoints list excluding start and end point, newpoints is given back the new list of points each iteration
                for (int current_point = 1; current_point < points.Count - 1; current_point++)
                {
                    Vector3 b_a_dir = -(points[current_point] - points[current_point - 1]).normalized;
                    Vector3 b_c_dir = -(points[current_point] - points[current_point + 1]).normalized;
                    float b_a_dist = Vector3.Distance(points[current_point], points[current_point - 1]);
                    float b_c_dist = Vector3.Distance(points[current_point], points[current_point + 1]);

                    Vector3 x1 = points[current_point] + b_a_dir * (b_a_dist * tightness);
                    Vector3 x2 = points[current_point] + b_c_dir * (b_c_dist * tightness);

                    float x1_x2_dist = Vector3.Distance(x1, x2);
                    Vector3 X1_X2_dir = -(x1 - x2).normalized;

                    Vector3 x3 = x1 + (X1_X2_dir * (x1_x2_dist * 0.5f));
                    Vector3 b_x3_dir = -(points[current_point] - x3).normalized;
                    float b_x3_dist = Vector3.Distance(points[current_point], x3);
                    Vector3 b = points[current_point] + (b_x3_dir * (b_x3_dist * tightness));

                    newpoints.Add(x1);
                    newpoints.Add(b);
                    newpoints.Add(x2);

                   
                }
                newpoints.Add(points[points.Count - 1]);
                points = newpoints;


            }

            return points.ToArray();

        }

        float GetFullClipTimeSecs()
        {
            float time = 0;
            if (CamMarkers.Count < 2) return 0;
            for (int mark = StartFrame + 1; mark < CamMarkers[CamMarkers.Count-1].ReferenceFrame; mark++)
            {
              time = time + MyPlayerPositions[mark].Time_span_seconds;
            }
            return time;
        }

        void Update()
        {
            if (ReplayOpen)
            {
                // to do no matter what
                if (Camobjs == null) Camobjs = FollowObject.GetComponentsInChildren<Transform>(true);
                moveforward = MGInputManager.RTrigger() > 0.15f;
                moveback = MGInputManager.LTrigger() > 0.15f;
                TriggerPressed = MGInputManager.LTrigger() > 0.15f | MGInputManager.RTrigger() > 0.15f ? true : false;
                LineActive = baselinerenderer.activeInHierarchy;
                

               
                if (MultiplayerManager.isConnected())
                {
                    MultiplayerManager.KeepNetworkActive();
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    HideGUI = !HideGUI;
                    foreach(Transform t in Camobjs)
                    {
                        t.gameObject.SetActive(!HideGUI);
                    }
                    baselinerenderer.SetActive(!HideGUI);
                    smoothlinerenderer.SetActive(!HideGUI);
                }



                // in Edit mode stuff
                if (!PlayEditToggle)
                {
                    ResetLineSegments();
                    CalculateCamAverageTimespanAndDistance();
                    ShowBaseLine();
                    ShowSmoothLine();

                    PlayerFreeCam();
                    AddCamMarker();
                    TriggerScrollEditMode();
                }

                // in Play mode stuff
                if (PlayEditToggle)
                {
                    if (!PlayThrough)
                    {
                    TriggerScrollPlayMode();
                    CamMoveToFollower(false);
                    }
                    else
                    {
                        Play();
                    }
                }


            }
            else
            {
                // replay closed, continue taking in data

                try
                {

                    if (Recording_watch.ElapsedMilliseconds > 16f)
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


                            ReplayMark newreplaymarker = new ReplayMark();
                            newreplaymarker.Positions = new Vector3[32];
                            newreplaymarker.Rotations = new Vector3[34];
                            newreplaymarker.Time_span_seconds = (float)Recording_watch.Elapsed.TotalSeconds;
                            Array.Copy(Currentpositions, newreplaymarker.Positions, Currentpositions.Length);
                            Array.Copy(Currentrotations, newreplaymarker.Rotations, Currentrotations.Length);
                            MyPlayerPositions.Add(newreplaymarker);

                        }

                        // remote players footage track
                        if (MultiplayerManager.isConnected() && LocalPlayer.instance.SendStream)
                        {
                            foreach (RemotePlayer player in MultiplayerManager.Players.Values)
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

                                        player.ReplayPositions.Add(new RemoteTransform(pos, rot, (float)Recording_watch.Elapsed.TotalSeconds));


                                    }


                                }

                            }
                        }

                        Recording_watch.Reset();
                        Recording_watch.Start();

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
            Buttontext = "Replay";
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


            SlowMoLabels = new List<string>
            {
                {"Linear"},
                {"Slow start"},
                {"Slow end"},

            };

            baselinerenderer = new GameObject();
            DontDestroyOnLoad(baselinerenderer);
            baseline = baselinerenderer.AddComponent<LineRenderer>();
            baseline.material = new Material(Shader.Find("Sprites/Default"));
            baseline.widthMultiplier = 0.2f;
            baseline.material.color = Color.green;
            // create pool of line segments
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < 10; i++)
            {
                GameObject child = new GameObject();
                DontDestroyOnLoad(child);
                LineRenderer rend = child.AddComponent<LineRenderer>();
                rend.material = new Material(Shader.Find("Sprites/Default"));
                rend.widthMultiplier = 0.2f;
                child.transform.parent = baselinerenderer.transform;
                children.Add(child);
            }
            baselinechildren = children;
            smoothlinerenderer = new GameObject();
            DontDestroyOnLoad(smoothlinerenderer);
            smoothline = smoothlinerenderer.AddComponent<LineRenderer>();
            smoothline.material = new Material(Shader.Find("Sprites/Default"));
            smoothline.material.color = new Color(0.4f,0.1f,0.8f,0.85f); // purple line shows interpolated points
            smoothline.widthMultiplier = 0.125f;



        }
        void Start()
        {

            BottomPanelStyle.fixedHeight = 50;
            BottomPanelStyle.fixedWidth = 50;
            BottomPanelStyle.fontStyle = FontStyle.Bold;


            ReplayCam = GameObject.Instantiate(Camera.main.gameObject) as GameObject;
            ReplayCam.SetActive(false);
            DontDestroyOnLoad(ReplayCam);

            Recording_watch.Start();
            StartCoroutine(LateLoad());
            DontDestroyOnLoad(FollowObject);

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
                        CameraModding.instance.Show();
                    }
                   
                }

            }

        }


        IEnumerator LateLoad()
        {
            while(MultiplayerManager.SLRcam == null)
            {
                yield return new WaitForEndOfFrame();
            }
            FollowObject = Instantiate(MultiplayerManager.SLRcam);
            while (FollowObject == null)
            {
                Debug.Log("Waiting for replay followobject");
                yield return new WaitForEndOfFrame();
            }
            FollowObject.transform.localScale = new Vector3(100, 100, 100);
            Camobjs = FollowObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in Camobjs)
            {
                t.localEulerAngles = new Vector3(0, 180, 0);
                t.localScale = new Vector3(20, 20, 20);
            }
            Debug.Log("Done follow object");
        }

    }

    /// <summary>
    /// Useable for storing every pos and rot of the rider and bmx in a packet, or for a camera marker with referenceframe etc
    /// </summary>
    public class ReplayMark
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public float Fov;
        public float FocusDistance;
        public float Aperture;
        public Vector3 CamPos;
        public Quaternion CamRot;
        /// <summary>
        /// position of this replayposition in the Myplayerpositions list
        /// </summary>
        public int ReferenceFrame;
        /// <summary>
        /// Timespan in seconds since last Replayposition was captured
        /// </summary>
        public float Time_span_seconds;
        public float Speedatmarker;
        /// <summary>
        /// how many frames behind marker frame to start speed change
        /// </summary>
        public int Speed_change_start_frame;
        /// <summary>
        /// how many frames after marker frame to end speed change
        /// </summary>
        public int Speed_change_end_frame;
        public SlowMoStyle StyleIn = SlowMoStyle.Linear;
        public SlowMoStyle StyleOut = SlowMoStyle.Linear;
        public bool AlterSpeed;
        public float cliptimestamp;




        public enum SlowMoStyle
        {
          Linear = 0,
          SlowIn = 1,
          Slowout = 2,

        }


    }


}
