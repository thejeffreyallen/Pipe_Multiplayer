using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Multiplayer
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

        /// <summary>
        /// Outgoing Riser Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="_pitch"></param>
        /// <param name="_playstate"></param>
        /// <param name="_nameofrisersound"></param>
        /// <param name="_Velocity"></param>
        /// <param name="flag"></param>
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

        /// <summary>
        /// incoming Riser Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="_pitch"></param>
        /// <param name="_playstate"></param>
        /// <param name="_nameofrisersound"></param>
        /// <param name="_Velocity"></param>
        public AudioStateUpdate(float _volume, float _pitch, int _playstate, string _nameofrisersound, float _Velocity)
        {
            Volume = _volume;
            pitch = _pitch;
            playstate = _playstate;
            nameofriser = _nameofrisersound;
            Velocity = _Velocity;
            code = 1;

        }


        /// <summary>
        /// Incoming One Shot Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="Pathofsound"></param>
        /// <param name="_flag"></param>
        public AudioStateUpdate(float _volume, string Pathofsound)
        {
            Volume = _volume;
            Path = Pathofsound;
            code = 2;
        }

        /// <summary>
        /// Outgoing One Shot Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="Pathofsound"></param>
        /// <param name="_flag"></param>
        public AudioStateUpdate(float _volume, string Pathofsound, Valve.Sockets.SendFlags _flag)
        {
            Volume = _volume;
            Path = Pathofsound;
            code = 2;
            Sendflag = _flag;
        }



       
    }
}
