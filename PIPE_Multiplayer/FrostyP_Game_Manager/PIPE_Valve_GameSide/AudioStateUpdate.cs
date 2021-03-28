using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Class contains 2 floats, one int and a string, a package of data to be sent to Clientsend that contains enough info for receiver to sync audio of a riserbyvel on his game
    /// </summary>
   public  class AudioStateUpdate
    {
        public float Volume;
        public float pitch;
        public int playstate;
        public string nameofriser;
        public float Velocity;
        public string Path;
        public int code;

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

        enum Audiotype
        {
            Riser = 1,
            OneShot = 2,
        }

    }
}
