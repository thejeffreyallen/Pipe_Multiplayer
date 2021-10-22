using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
using System.Diagnostics;

namespace PIPE_Multiplayer
{
    public class RemotePlayerAudio : MonoBehaviour
    {
        // Daryien object, needed to determine 3d attributes of sounds
        public GameObject Rider;
        public RemotePlayer player;

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

        // riser timeouts
        Stopwatch sliderwatch = new Stopwatch();
        Stopwatch tireswatch = new Stopwatch();
        Stopwatch railsinglewatch = new Stopwatch();
        Stopwatch raildoublewatch = new Stopwatch();
        Stopwatch ledgesinglewatch = new Stopwatch();
        Stopwatch ledgedoublewatch = new Stopwatch();
        Stopwatch singletirewatch = new Stopwatch();
        Stopwatch footbrakewatch = new Stopwatch();
        Stopwatch cassettewatch = new Stopwatch();
        Dictionary<Stopwatch, FMOD.Studio.EventInstance> listofwatches = new Dictionary<Stopwatch, FMOD.Studio.EventInstance>();
       
        delegate void Handler(AudioStateUpdate update);
        private Dictionary<string, Handler> StateHandlers;


        void Awake()
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

            listofwatches.Add(sliderwatch,Slider);
            listofwatches.Add(tireswatch,Tires);
            listofwatches.Add(railsinglewatch,RailSingle);
            listofwatches.Add(raildoublewatch,RailDouble);
            listofwatches.Add(ledgesinglewatch,LedgeSingle);
            listofwatches.Add(ledgedoublewatch,LedgeDouble);
            listofwatches.Add(singletirewatch,singleTire);
            listofwatches.Add(footbrakewatch,FootBrake);
            listofwatches.Add(cassettewatch,Cassette);
            


        }


      
       
        void Update()
        {

            if (player.MasterActive)
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

                        for (int i = 0; i < IncomingRiserUpdates.Count; i++)
                        {
                          StateHandlers[IncomingRiserUpdates[i].nameofriser]?.Invoke(IncomingRiserUpdates[i]);
                            IncomingRiserUpdates.RemoveAt(i);
                        }
                     

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
                    instance.setVolume(Mathf.Clamp(IncomingOneShotUpdates[i].Volume / (Vector3.Distance(Rider.transform.position, Camera.current.transform.position) * 0.5f),0,10));
                    instance.set3DAttributes(Rider.transform.position.To3DAttributes());
                    instance.start();
                    instance.release();
                    instance.clearHandle();
                    
                    IncomingOneShotUpdates.RemoveAt(i);
                }
             }


                foreach(Stopwatch s in listofwatches.Keys)
                {
                    if (s.IsRunning)
                    {
                        if (s.Elapsed.TotalSeconds > 1)
                        {
                            listofwatches[s].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                            s.Reset();
                        }
                    }
                }
            

            }
          

                
        }



      

        public void ShutdownAllSounds()
        {
            // make sure sounds are stopped
            RailSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            RailDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            LedgeSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            LedgeDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            FootBrake.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Cassette.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Slider.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);


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
                tireswatch.Reset();
                tireswatch.Start();

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
                tireswatch.Reset();
                tireswatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                Tires.setVolume(update.Volume);
                Tires.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                tireswatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Tires.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Tires.setVolume(update.Volume);
                    Tires.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    
                }
                Tires.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                if (tireswatch.IsRunning)
                {
                    tireswatch.Reset();
                }
                
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
                railsinglewatch.Reset();
                railsinglewatch.Start();
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
                railsinglewatch.Reset();
                railsinglewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailSingle.setVolume(update.Volume);
                RailSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                RailSingle.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                railsinglewatch.Reset();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    RailSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailSingle.setVolume(update.Volume);
                    RailSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    
                }
                RailSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                RailSingle.release();
                railsinglewatch.Reset();
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
                raildoublewatch.Reset();
                raildoublewatch.Start();

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
                raildoublewatch.Reset();
                raildoublewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                RailDouble.setVolume(update.Volume);
                RailDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                RailDouble.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                raildoublewatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    RailDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    RailDouble.setVolume(update.Volume);
                    RailDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    
                }
                RailDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                raildoublewatch.Reset();

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
                ledgesinglewatch.Reset();
                ledgesinglewatch.Start();

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
                ledgesinglewatch.Reset();
                ledgesinglewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeSingle.setVolume(update.Volume);
                LedgeSingle.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                LedgeSingle.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                ledgesinglewatch.Reset();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    LedgeSingle.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeSingle.setVolume(update.Volume);
                    LedgeSingle.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                    
                    
                }
                LedgeSingle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                ledgesinglewatch.Reset();

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
                ledgedoublewatch.Reset();
                ledgedoublewatch.Start();
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
                ledgedoublewatch.Reset();
                ledgedoublewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                LedgeDouble.setVolume(update.Volume);
                LedgeDouble.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                LedgeDouble.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                ledgedoublewatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    LedgeDouble.set3DAttributes(Rider.transform.position.To3DAttributes());
                    LedgeDouble.setVolume(update.Volume);
                    LedgeDouble.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                }

                LedgeDouble.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                ledgedoublewatch.Reset();

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
                footbrakewatch.Reset();
                footbrakewatch.Start();

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
                footbrakewatch.Reset();
                footbrakewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                FootBrake.setVolume(update.Volume);
                FootBrake.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                FootBrake.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                footbrakewatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    FootBrake.set3DAttributes(Rider.transform.position.To3DAttributes());
                    FootBrake.setVolume(update.Volume);
                    FootBrake.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                }
                FootBrake.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                footbrakewatch.Reset();
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
                cassettewatch.Reset();
                cassettewatch.Start();

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
                cassettewatch.Reset();
                cassettewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                Cassette.setVolume(update.Volume);
                Cassette.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Cassette.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                cassettewatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Cassette.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Cassette.setVolume(update.Volume);
                    Cassette.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                   
                }
                Cassette.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                cassettewatch.Reset();
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
                singletirewatch.Reset();
                singletirewatch.Start();

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
                singletirewatch.Reset();
                singletirewatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                singleTire.setVolume(update.Volume);
                singleTire.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

                singletirewatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    singleTire.set3DAttributes(Rider.transform.position.To3DAttributes());
                    singleTire.setVolume(update.Volume);
                    singleTire.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                   
                }
                singleTire.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

                singletirewatch.Reset();
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
                sliderwatch.Reset();
                sliderwatch.Start();

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
                sliderwatch.Reset();
                sliderwatch.Start();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                Slider.setVolume(update.Volume);
                Slider.setPitch(update.pitch);
                Vel.setValue(update.Velocity);
                Slider.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                sliderwatch.Reset();

            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                if (_currentstate != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    Slider.set3DAttributes(Rider.transform.position.To3DAttributes());
                    Slider.setVolume(update.Volume);
                    Slider.setPitch(update.pitch);
                    Vel.setValue(update.Velocity);
                }
                Slider.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                sliderwatch.Reset();
            }

            if (update.playstate == (int)FMOD.Studio.PLAYBACK_STATE.SUSTAINING)
            {

            }
        }



        #endregion
    }
}