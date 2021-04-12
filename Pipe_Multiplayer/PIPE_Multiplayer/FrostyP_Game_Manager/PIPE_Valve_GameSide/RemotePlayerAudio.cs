using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;

namespace PIPE_Valve_Console_Client
{
    public class RemotePlayerAudio : MonoBehaviour
    {
        // Daryien object, needed to determine 3d attributes of sounds
        public GameObject Rider;

        // list of updates that come in through Clienthandle.ReceiveAudioForAPlayer
        public List<AudioStateUpdate> IncomingRiserUpdates;
        public List<AudioStateUpdate> IncomingOneShotUpdates;

        string sliderpath;
        string railsinglepath;
        string ledgesinglepath;
        string raildoublepath;
        string tirespath;
        string footbrakepath;
        string singletirepath;
        string ledgedoublepath;
        string cassettepath;



        FMOD.Studio.EventInstance Slider;
        FMOD.Studio.EventInstance Tires;
        FMOD.Studio.EventInstance RailSingle;
        FMOD.Studio.EventInstance RailDouble;
        FMOD.Studio.EventInstance LedgeSingle;
        FMOD.Studio.EventInstance LedgeDouble;
        FMOD.Studio.EventInstance singleTire;
        FMOD.Studio.EventInstance FootBrake;
        FMOD.Studio.EventInstance Cassette;


        int[] laststates;




        delegate void Handler(AudioStateUpdate update);
        private Dictionary<string, Handler> StateHandlers;



        // Use this for initialization
        void Start()
        {

           
            IncomingRiserUpdates = new List<AudioStateUpdate>();
            IncomingOneShotUpdates = new List<AudioStateUpdate>();

            // setup state handlers to fire approriate function for each state
            StateHandlers = new Dictionary<string, Handler>()
            {
                {"TiresRolling",TiresRoll},
                {"SingleTireRolling",SingleTire},
                {"Rail_Single_Slide",Railsingle},
                {"Rail_Double_Slide",Raildouble},
                {"Cassette",Hubsound},
                {"Ledge_Single_Slide",Ledgesingle},
                {"Ledge_Double_Slide",Ledgedouble},
                {"FootBraking",FootBraking},
                {"Slider Sound", SliderSound},
                
            };


            sliderpath = "event:/Character Sounds/Cloth Sliding";
            railsinglepath = "event:/Grind/rail_slide_single";
            raildoublepath = "event:/Grind/rail_slide_double";
            tirespath = "event:/Tires/tire_roll";
            ledgesinglepath = "event:/Grind/cement_slide_single";
            ledgedoublepath = "event:/Grind/cement_slide_double";
            singletirepath = "event:/Tires/tire_roll";
            footbrakepath = "event:/Tires/foot_braking";
            cassettepath = "event:/Tires/cassette";

            RailSingle = FMODUnity.RuntimeManager.CreateInstance(railsinglepath);
            Tires = FMODUnity.RuntimeManager.CreateInstance(tirespath);
            RailDouble = FMODUnity.RuntimeManager.CreateInstance(raildoublepath);
            LedgeSingle = FMODUnity.RuntimeManager.CreateInstance(ledgesinglepath);
            LedgeDouble = FMODUnity.RuntimeManager.CreateInstance(ledgedoublepath);
            FootBrake = FMODUnity.RuntimeManager.CreateInstance(footbrakepath);
            Cassette = FMODUnity.RuntimeManager.CreateInstance(cassettepath);
            singleTire = FMODUnity.RuntimeManager.CreateInstance(singletirepath);
            Slider = FMODUnity.RuntimeManager.CreateInstance(sliderpath);


        }

       
        void Update()
        {
            if(Rider == null)
            {
                if (gameObject.GetComponent<RemotePlayer>().RiderModel)
                {
                Rider = gameObject.GetComponent<RemotePlayer>().RiderModel;
                }
                else
                {
                UnityEngine.Debug.Log("remoteaudio's ridermodel lost!");
                }
            }

           


              // Clienthandle adds updates to this list on receive 
            if(IncomingRiserUpdates.Count > 0)
            {
                try
                {
                   foreach(AudioStateUpdate update in IncomingRiserUpdates)
                   {
                       // UnityEngine.Debug.Log($"Incoming audio state: {update.playstate}");
                 StateHandlers[update.nameofriser]?.Invoke(update);
                   }

                   
                        IncomingRiserUpdates.Clear();

                }
                catch(UnityException x)
                {
                    UnityEngine.Debug.Log("Audio remote rider!!");
                    UnityEngine.Debug.Log(x);

                }


            }
           
           
             if(IncomingOneShotUpdates.Count > 0)
            {
                for (int i = 0; i < IncomingOneShotUpdates.Count; i++)
                {
                    FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(IncomingOneShotUpdates[i].Path);
                    instance.setVolume(IncomingOneShotUpdates[i].Volume);
                    instance.set3DAttributes(Rider.transform.position.To3DAttributes());
                    instance.start();
                    instance.release();
                    instance.clearHandle();
                    
                }
                IncomingOneShotUpdates.Clear();
            }  

            
                
        }



        #region Audio Controllers
        void TiresRoll(AudioStateUpdate update)
        {
            Tires.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
           
            Tires.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (Tires.isValid())
                    {
                        Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                       
                    }
                   // Tires = FMODUnity.RuntimeManager.CreateInstance(tirespath);
                    Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Tires.setVolume(update.Volume);
                    Tires.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Tires.start();
                }

                Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                Tires.setVolume(update.Volume);
                Tires.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                  //  Tires = FMODUnity.RuntimeManager.CreateInstance(tirespath);
                    Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Tires.setVolume(update.Volume);
                    Tires.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Tires.start();

                }
                Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                Tires.setVolume(update.Volume);
                Tires.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                Tires.setVolume(update.Volume);
                Tires.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Tires.setVolume(update.Volume);
                    Tires.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    
                }
                Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }


        }
        void Railsingle(AudioStateUpdate update)
        {
            RailSingle.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            
            RailSingle.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (RailSingle.isValid())
                    {
                        RailSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        
                    }
                   // RailSingle = FMODUnity.RuntimeManager.CreateInstance(railsinglepath);
                    RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailSingle.setVolume(update.Volume);
                    RailSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailSingle.start();
                }

                RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailSingle.setVolume(update.Volume);
                RailSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if(_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
               // RailSingle = FMODUnity.RuntimeManager.CreateInstance(railsinglepath);
                RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailSingle.setVolume(update.Volume);
                RailSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailSingle.start();

                }
                RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailSingle.setVolume(update.Volume);
                RailSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailSingle.setVolume(update.Volume);
                RailSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                RailSingle.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailSingle.setVolume(update.Volume);
                    RailSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    
                }
                RailSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                RailSingle.release();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {
                
            }

           
        }
        void Raildouble(AudioStateUpdate update)
        {
            RailDouble.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
           
            RailDouble.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (RailDouble.isValid())
                    {
                        RailDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        
                    }
                   // RailDouble = FMODUnity.RuntimeManager.CreateInstance(raildoublepath);
                    RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailDouble.setVolume(update.Volume);
                    RailDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailDouble.start();
                }

                RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailDouble.setVolume(update.Volume);
                RailDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // RailDouble = FMODUnity.RuntimeManager.CreateInstance(raildoublepath);
                    RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailDouble.setVolume(update.Volume);
                    RailDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailDouble.start();

                }
                RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailDouble.setVolume(update.Volume);
                RailDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailDouble.setVolume(update.Volume);
                RailDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                RailDouble.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailDouble.setVolume(update.Volume);
                    RailDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    RailDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    
                }
                RailDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }

        }
        void Ledgesingle(AudioStateUpdate update)
        {
            LedgeSingle.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
          
            LedgeSingle.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (LedgeSingle.isValid())
                    {
                        LedgeSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        
                    }
                   // LedgeSingle = FMODUnity.RuntimeManager.CreateInstance(ledgesinglepath);
                    LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeSingle.setVolume(update.Volume);
                    LedgeSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeSingle.start();
                }

                LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeSingle.setVolume(update.Volume);
                LedgeSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // LedgeSingle = FMODUnity.RuntimeManager.CreateInstance(ledgesinglepath);
                    LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeSingle.setVolume(update.Volume);
                    LedgeSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeSingle.start();

                }
                LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeSingle.setVolume(update.Volume);
                LedgeSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeSingle.setVolume(update.Volume);
                LedgeSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                LedgeSingle.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeSingle.setVolume(update.Volume);
                    LedgeSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    
                }
                LedgeSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
               
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }

        }
        void Ledgedouble(AudioStateUpdate update)
        {
            LedgeDouble.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            LedgeDouble.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (LedgeDouble.isValid())
                    {
                        LedgeDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                       
                    }
                   // LedgeDouble = FMODUnity.RuntimeManager.CreateInstance(ledgedoublepath);
                    LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeDouble.setVolume(update.Volume);
                    LedgeDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeDouble.start();
                }

                LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeDouble.setVolume(update.Volume);
                LedgeDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // LedgeDouble = FMODUnity.RuntimeManager.CreateInstance(ledgedoublepath);
                    LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeDouble.setVolume(update.Volume);
                    LedgeDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeDouble.start();

                }
                LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeDouble.setVolume(update.Volume);
                LedgeDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeDouble.setVolume(update.Volume);
                LedgeDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                LedgeDouble.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeDouble.setVolume(update.Volume);
                    LedgeDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    LedgeDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }

                LedgeDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }

        }
        void FootBraking(AudioStateUpdate update)
        {
            FootBrake.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            FootBrake.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (FootBrake.isValid())
                    {
                        FootBrake.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        //FootBrake.release();
                    }
                   // FootBrake = FMODUnity.RuntimeManager.CreateInstance(footbrakepath);
                    FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                    FootBrake.setVolume(update.Volume);
                    FootBrake.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    FootBrake.start();
                }

                FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                FootBrake.setVolume(update.Volume);
                FootBrake.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                  //  FootBrake = FMODUnity.RuntimeManager.CreateInstance(footbrakepath);
                    FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                    FootBrake.setVolume(update.Volume);
                    FootBrake.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    FootBrake.start();

                }
                FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                FootBrake.setVolume(update.Volume);
                FootBrake.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                FootBrake.setVolume(update.Volume);
                FootBrake.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                FootBrake.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                    FootBrake.setVolume(update.Volume);
                    FootBrake.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    FootBrake.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                   // FootBrake.release();
                }
                FootBrake.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
               // FootBrake.release();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }
        }
        void Hubsound(AudioStateUpdate update)
        {
            Cassette.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            Cassette.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (Cassette.isValid())
                    {
                        Cassette.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                       // Cassette.release();
                    }
                   // Cassette = FMODUnity.RuntimeManager.CreateInstance(cassettepath);
                    Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Cassette.setVolume(update.Volume);
                    Cassette.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Cassette.start();
                }

                Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                Cassette.setVolume(update.Volume);
                Cassette.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // Cassette = FMODUnity.RuntimeManager.CreateInstance(cassettepath);
                    Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Cassette.setVolume(update.Volume);
                    Cassette.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Cassette.start();

                }
                Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                Cassette.setVolume(update.Volume);
                Cassette.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                Cassette.setVolume(update.Volume);
                Cassette.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Cassette.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Cassette.setVolume(update.Volume);
                    Cassette.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Cassette.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                   
                }
                Cassette.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
               // Cassette.release();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }

        }
        void SingleTire(AudioStateUpdate update)
        {
            singleTire.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            singleTire.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (singleTire.isValid())
                    {
                        singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                       // singleTire.release();
                    }
                   // singleTire = FMODUnity.RuntimeManager.CreateInstance(singletirepath);
                    singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                    singleTire.setVolume(update.Volume);
                    singleTire.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    singleTire.start();
                }

                singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                singleTire.setVolume(update.Volume);
                singleTire.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // singleTire = FMODUnity.RuntimeManager.CreateInstance(singletirepath);
                    singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                    singleTire.setVolume(update.Volume);
                    singleTire.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    singleTire.start();

                }
                singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                singleTire.setVolume(update.Volume);
                singleTire.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                singleTire.setVolume(update.Volume);
                singleTire.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                    singleTire.setVolume(update.Volume);
                    singleTire.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                   
                }
                singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
              //  singleTire.release();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }

        }
        void SliderSound(AudioStateUpdate update)
        {
            Slider.getParameter("Velocity", out FMOD.Studio.ParameterInstance Vel);
            Slider.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE _currentstate);

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    if (Slider.isValid())
                    {
                        Slider.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                       // Slider.release();
                    }
                   // Slider = FMODUnity.RuntimeManager.CreateInstance(sliderpath);
                    Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Slider.setVolume(update.Volume);
                    Slider.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Slider.start();
                }

                Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                Slider.setVolume(update.Volume);
                Slider.setPitch(update.pitch);
                Vel.setValue(update.Velocity);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                if (_currentstate != (int)FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                   // Slider = FMODUnity.RuntimeManager.CreateInstance(sliderpath);
                    Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Slider.setVolume(update.Volume);
                    Slider.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Slider.start();

                }
                Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                Slider.setVolume(update.Volume);
                Slider.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                Slider.setVolume(update.Volume);
                Slider.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Slider.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Slider.setVolume(update.Volume);
                    Slider.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    Slider.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                   // Slider.release();
                }
                Slider.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
              //  Slider.release();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }
        }



        void PlayOneShot(string bankPath, float volume)
        {
            FMOD.Studio.EventInstance eventInstance = RuntimeManager.CreateInstance(bankPath);
            eventInstance.set3DAttributes(Rider.transform.position.To3DAttributes());
            eventInstance.setVolume(volume);
           
            eventInstance.start();
            eventInstance.release();
            eventInstance.clearHandle();
        }


        #endregion
    }
}