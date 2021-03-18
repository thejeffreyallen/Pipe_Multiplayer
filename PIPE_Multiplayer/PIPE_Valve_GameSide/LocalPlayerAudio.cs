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

        public bool active = false;
        // list of Risers with play state attached
      
        delegate void Handler(int state, FmodRiserByVel riser);
        private Dictionary<int, Handler> StateHandlers;

        // list of final info sent to server every frame
        List<AudioStateUpdate> statesUpdate = new List<AudioStateUpdate>();

        // daryiens object regardless
        public GameObject Riderroot;

        // just used to flick through them
        private FmodRiserByVel[] FindRisers;
        /// <summary>
        /// last recorded state, only send if state has changed
        /// </summary>
        private int[] laststates;
       
       

        // Use this for initialization
        void Start()
        {

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
            FindRisers = Riderroot.transform.parent.parent.parent.gameObject.GetComponentsInChildren<FmodRiserByVel>();
            laststates = new int[FindRisers.Length];
            



           
            
        }





        // Update is called once per frame
        void FixedUpdate()
        {

            
            if (InGameUI.instance.Connected && FindRisers != null)
            {
                for (int i = 0; i < FindRisers.Length; i++)
                {
                    FindRisers[i].sound.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE st);

                    // if both the current state and last sent state are not STOPPED, carry on
                    if(((int)st != 2 && laststates[i] != 2))
                    {
                    // send update
                    if (st == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                    {
                        // Debug.Log("Playing" + FindRisers[i].gameObject.name);
                        StateHandlers[0](0, FindRisers[i]);
                    }
                    //send update and tell to start
                    if (st == FMOD.Studio.PLAYBACK_STATE.STARTING)
                    {
                        //  Debug.Log("starting" + FindRisers[i].gameObject.name);
                        StateHandlers[3](3, FindRisers[i]);
                    }
                    // send just this info
                    if (st == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                    {
                        // Debug.Log("stopped" + FindRisers[i].gameObject.name);
                        StateHandlers[2](2, FindRisers[i]);
                    }
                    // send update and tell to stop
                    if (st == FMOD.Studio.PLAYBACK_STATE.STOPPING)
                    {
                        // Debug.Log("stopping" + FindRisers[i].gameObject.name);
                        StateHandlers[4](4, FindRisers[i]);
                    }
                    // paused? dont think that used, if so paused = sustaining, stopping and playing at once so will need more checks on all above
                    if (st == FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
                    {
                        // Debug.Log("sustaining" + FindRisers[i].gameObject.name);
                        StateHandlers[1](1, FindRisers[i]);
                    }

                    }
                    laststates[i] = (int)st;


                }

            }

            if(statesUpdate.Count > 0 && InGameUI.instance.Connected)
            {
            ClientSend.SendAudioUpdate(statesUpdate);
            statesUpdate.Clear();
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
            statesUpdate.Add(update);
            
        }

        void Stopping(int state, FmodRiserByVel riser)
        {
            riser.sound.getVolume(out float volume, out float finalvol);
            riser.sound.getPitch(out float pitch, out float finalpitch);
            riser.sound.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            Vel.getValue(out float _Vel);
            string soundname = riser.gameObject.name;
            AudioStateUpdate update = new AudioStateUpdate(finalvol, finalpitch, state, soundname,_Vel);
            statesUpdate.Add(update);

        }

        void Stopped(int state, FmodRiserByVel riser)
        {
            
            AudioStateUpdate update = new AudioStateUpdate(0, 0, state, riser.gameObject.name,0);
            statesUpdate.Add(update);
        }

        void Sustaining(int state, FmodRiserByVel riser)
        {
            AudioStateUpdate update = new AudioStateUpdate(0, 0, state, riser.gameObject.name,0);
            statesUpdate.Add(update);

        }

        void Starting(int state, FmodRiserByVel riser)
        {
            riser.sound.getVolume(out float volume, out float finalvol);
            riser.sound.getPitch(out float pitch, out float finalpitch);
            riser.sound.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            Vel.getValue(out float _Vel);
            string soundname = riser.gameObject.name;
            AudioStateUpdate update = new AudioStateUpdate(finalvol,finalpitch, state, soundname,_Vel);
            statesUpdate.Add(update);

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