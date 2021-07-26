using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
   
   public class AudioStateUpdate
    {
        public float Volume;
        public float pitch;
        public int playstate;
        public string nameofriser;
        public float Velocity;
        public string Path;
        public int code;
        public Valve.Sockets.SendFlags Sendflag;

        public AudioStateUpdate(float _volume, float _pitch, int _playstate, string _nameofrisersound, float _Velocity,Valve.Sockets.SendFlags flag)
        {
            Volume = _volume;
            pitch = _pitch;
            playstate = _playstate;
            nameofriser = _nameofrisersound;
            Velocity = _Velocity;
            code = 1;
            Sendflag = flag;

        }
        public AudioStateUpdate(float _volume, float _pitch, int _playstate, string _nameofrisersound, float _Velocity)
        {
            Volume = _volume;
            pitch = _pitch;
            playstate = _playstate;
            nameofriser = _nameofrisersound;
            Velocity = _Velocity;
            code = 1;

        }



        public AudioStateUpdate(float _volume, string Pathofsound)
        {
            Volume = _volume;
            Path = Pathofsound;
            code = 2;
        }

       
    }
}
