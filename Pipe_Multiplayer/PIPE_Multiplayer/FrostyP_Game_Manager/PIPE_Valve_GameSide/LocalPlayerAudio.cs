using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Track the states of local players FMODRiserbyVel's and begins all network sends
    /// </summary>
    public class LocalPlayerAudio : MonoBehaviour
    {

        public LocalPlayer Mylocalplayer;

        public bool active = false;
        // list of Risers with play state attached

        delegate void Handler(int state, FmodRiserByVel riser);
        private Dictionary<int, Handler> StateHandlers;

        // list of final info sent to server every frame
        List<AudioStateUpdate> RisersStateUpdate = new List<AudioStateUpdate>();

        // daryiens object regardless
        public GameObject Riderroot;

        // just used to flick through them
        private FmodRiserByVel[] Risers;

        private FmodSoundLauncher OneShotter;
        private FMOD.Studio.EventDescription Description;
        private string LastSoundPath = "";
        private string ThisSoundpath;
        private FMOD.Studio.EventInstance LastOneShotEvent;
        private List<AudioStateUpdate> OneShotUpdates = new List<AudioStateUpdate>();


        /// <summary>
        /// last recorded state, only send if state has changed
        /// </summary>
        private int[] laststates;
        private Dictionary<string, float> PlayThresholds = new Dictionary<string, float>();



        // Use this for initialization
        void Start()
        {

            Mylocalplayer = gameObject.GetComponent<LocalPlayer>();
            // setup state handlers to fire approriate function for each state
            StateHandlers = new Dictionary<int, Handler>()
            {
                {0,Playing},
                {3,Starting},
                {2,Stopped},
                {4,Stopping},
                {1,Sustaining},
            };


            // grab risers and add to list
            Risers = Riderroot.transform.parent.parent.parent.gameObject.GetComponentsInChildren<FmodRiserByVel>();
            OneShotter = Riderroot.transform.parent.parent.parent.gameObject.GetComponentInChildren<FmodSoundLauncher>();
            laststates = new int[Risers.Length];

            foreach(FmodRiserByVel v in Risers)
            {
                PlayThresholds.Add(v.name, 0);
            }




        }







        void Update()
        {





            if (InGameUI.instance.Connected && Mylocalplayer.ServerActive)
            {
                // take care of all FMODbyRiserVel's in Bmx
                for (int i = 0; i < Risers.Length; i++)
                {
                    Risers[i].sound.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE st);

                    // if both the current state and last sent state are not STOPPED, carry on
                    if (((int)st != 2 && laststates[i] != 2))
                    {
                        // send update
                        if (st == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                        {
                            // Debug.Log("Playing" + FindRisers[i].gameObject.name);
                            StateHandlers[0](0, Risers[i]);
                        }
                        //send update and tell to start
                        if (st == FMOD.Studio.PLAYBACK_STATE.STARTING)
                        {
                            //  Debug.Log("starting" + FindRisers[i].gameObject.name);
                            StateHandlers[3](3, Risers[i]);
                        }
                        // send just this info
                        if (st == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                        {
                            // Debug.Log("stopped" + FindRisers[i].gameObject.name);
                            StateHandlers[2](2, Risers[i]);
                        }
                        // send update and tell to stop
                        if (st == FMOD.Studio.PLAYBACK_STATE.STOPPING)
                        {
                            // Debug.Log("stopping" + FindRisers[i].gameObject.name);
                            StateHandlers[4](4, Risers[i]);
                        }
                        // paused? dont think that used, if so paused = sustaining, stopping and playing at once so will need more checks on all above
                        if (st == FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
                        {
                            // Debug.Log("sustaining" + FindRisers[i].gameObject.name);
                            StateHandlers[1](1, Risers[i]);
                        }

                    }



                    laststates[i] = (int)st;

                }




                // take care of the FMODSoundLauncher
                if (OneShotter != null)
                {
                    if (OneShotter.CurrentEvent.isValid())
                    {
                        OneShotter.CurrentEvent.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
                        OneShotter.CurrentEvent.getDescription(out Description);
                        Description.getPath(out ThisSoundpath);

                        if (!ThisSoundpath.Contains("UI"))
                        {
                        if (state == FMOD.Studio.PLAYBACK_STATE.STARTING | state == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                        {
                            // process this event
                            OneShotter.CurrentEvent.getVolume(out float volume, out float finalvol);

                                if(ThisSoundpath != LastSoundPath)
                                {
                                 OneShotUpdates.Add(new AudioStateUpdate(finalvol, ThisSoundpath));

                                }



                            LastOneShotEvent = OneShotter.CurrentEvent;
                            LastSoundPath = ThisSoundpath;
                        }

                        }
                    }





                }





                if (RisersStateUpdate.Count > 0)
                {
                    ClientSend.SendAudioUpdate(RisersStateUpdate, 1);
                    RisersStateUpdate.Clear();

                }
                if (OneShotUpdates.Count > 0)
                {
                    ClientSend.SendAudioUpdate(OneShotUpdates, 2);
                    OneShotUpdates.Clear();

                }



            }
        }



           private bool PlayThreshold(string name, float finalvol)
           {
              if(PlayThresholds[name] - finalvol > 0.1f | (PlayThresholds[name] - finalvol < -0.1f))
              {
                return true;

              }
              else
              {
                return false;
              }

           }
          


            // all fired by the state of a riserbyVel above and add their data to StatesUpdate list for sending
            void Playing(int state, FmodRiserByVel riser)
            {
                riser.sound.getVolume(out float volume, out float finalvol);
                riser.sound.getPitch(out float pitch, out float finalpitch);
                riser.sound.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
                Vel.getValue(out float _Vel);
                string soundname = riser.gameObject.name;
                AudioStateUpdate update = new AudioStateUpdate(finalvol, finalpitch, state, soundname, _Vel);

                 if (PlayThreshold(riser.name,finalvol))
                 {
                    RisersStateUpdate.Add(update);

                 }
            

            }

            void Stopping(int state, FmodRiserByVel riser)
            {
                riser.sound.getVolume(out float volume, out float finalvol);
                riser.sound.getPitch(out float pitch, out float finalpitch);
                riser.sound.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
                Vel.getValue(out float _Vel);
                string soundname = riser.gameObject.name;
                AudioStateUpdate update = new AudioStateUpdate(finalvol, finalpitch, state, soundname, _Vel);
                RisersStateUpdate.Add(update);

            }

            void Stopped(int state, FmodRiserByVel riser)
            {

                AudioStateUpdate update = new AudioStateUpdate(0, 0, state, riser.gameObject.name, 0);
                RisersStateUpdate.Add(update);
            }

            void Sustaining(int state, FmodRiserByVel riser)
            {
                AudioStateUpdate update = new AudioStateUpdate(0, 0, state, riser.gameObject.name, 0);
                RisersStateUpdate.Add(update);

            }

            void Starting(int state, FmodRiserByVel riser)
            {
                riser.sound.getVolume(out float volume, out float finalvol);
                riser.sound.getPitch(out float pitch, out float finalpitch);
                riser.sound.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
                Vel.getValue(out float _Vel);
                string soundname = riser.gameObject.name;
                AudioStateUpdate update = new AudioStateUpdate(finalvol, finalpitch, state, soundname, _Vel);
                RisersStateUpdate.Add(update);

            }





        
    }
}


/*
                                  Quick References;

   playback state enums
playing = 0
starting = 3
stopped = 2
stopping = 4


*/