using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    static class Pooler
    {
        static Queue<RemoteTransform> transformUpdatePool = new Queue<RemoteTransform>();
        static Queue<AudioStateUpdate> audioUpdatePool = new Queue<AudioStateUpdate>();
        static int maxtransformsize;
        static int maxaudiosize;
        
        public static void Setup(MultiplayerConfig config)
        {
            maxtransformsize = config.TransformPool;
            maxaudiosize = config.AudioPool;

            while(transformUpdatePool.Count < maxtransformsize)
            {
                transformUpdatePool.Enqueue(new RemoteTransform());
            }
            while (audioUpdatePool.Count < maxaudiosize)
            {
                audioUpdatePool.Enqueue(new AudioStateUpdate());
            }
           
        }
       

        #region add to pool
        public static void AddRemoteTransform(RemoteTransform trans)
        {
            if (transformUpdatePool.Count >= maxtransformsize)
            {
                MultiplayerManager.TransformDump++;
                return;
            }
                transformUpdatePool.Enqueue(trans);
        }
        public static void AddAudioUpdate(AudioStateUpdate audio)
        {
            if (audioUpdatePool.Count >= maxaudiosize)
            {
                MultiplayerManager.AudioDump++;
                return;
            }
            audioUpdatePool.Enqueue(audio);
        }
        #endregion

        #region Get from pool
        public static RemoteTransform GetRemoteTransform()
        {
            if(transformUpdatePool.Count <= 0)
            {
                return new RemoteTransform();
            }
            return transformUpdatePool.Dequeue();
        }
        public static AudioStateUpdate GetAudio()
        {
            if(audioUpdatePool.Count <= 0)
            {
                return new AudioStateUpdate();
            }
            return audioUpdatePool.Dequeue();
        }
       
        #endregion


    }


    /// <summary>
    /// Incoming list of received transform updates, with or without timestamp
    /// </summary>
    public class RemoteTransform
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public int Ping;
        public long ServerTimeStamp;
        public DateTime MyTimeAtReceive;
        public float timespanFromlastReplaymarker;
        public double PlayerMsFromLast;
        public bool LowFPS;
        public int MoveCount;

        public RemoteTransform()
        {

        }

        /// <summary>
        /// Adding new replay position
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_rot"></param>
        /// <param name="replaymakertime"></param>
        public RemoteTransform(Vector3[] _pos, Vector3[] _rot, float replaymakertime)
        {
            Positions = _pos;
            Rotations = _rot;
            timespanFromlastReplaymarker = replaymakertime;


        }


        public static RemoteTransform Create(Vector3[] _pos, Vector3[] _rot, int _ping, long _serverstamp, double elapsed)
        {
            RemoteTransform trans = Pooler.GetRemoteTransform();
            trans.Positions = _pos;
            trans.Rotations = _rot;
            trans.Ping = _ping;
            trans.ServerTimeStamp = _serverstamp;
            trans.PlayerMsFromLast = elapsed;
            return trans;
        }
        public void Release()
        {
            Pooler.AddRemoteTransform(this);
        }

    }
    public class AudioStateUpdate
    {
        public float Volume;
        public float pitch;
        public int playstate;
        public string nameofriser;
        public float Velocity;
        public string Path;
        public int code;
        public RiptideNetworking.MessageSendMode Sendflag;

        public AudioStateUpdate()
        {

        }

        /// <summary>
        /// Outgoing Riser Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="_pitch"></param>
        /// <param name="_playstate"></param>
        /// <param name="_nameofrisersound"></param>
        /// <param name="_Velocity"></param>
        /// <param name="flag"></param>
        public AudioStateUpdate(float _volume, float _pitch, int _playstate, string _nameofrisersound, float _Velocity, RiptideNetworking.MessageSendMode flag)
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
        /// Outgoing One Shot Update
        /// </summary>
        /// <param name="_volume"></param>
        /// <param name="Pathofsound"></param>
        /// <param name="_flag"></param>
        public AudioStateUpdate(float _volume, string Pathofsound, RiptideNetworking.MessageSendMode _flag)
        {
            Volume = _volume;
            Path = Pathofsound;
            code = 2;
            Sendflag = _flag;
        }


        public static AudioStateUpdate Create(float volume, string Pathofsound)
        {
            AudioStateUpdate update = Pooler.GetAudio();
            update.Volume = volume;
            update.Path = Pathofsound;
            update.code = 2;
            return update;
        }
        public static AudioStateUpdate Create(float volume, float pitch, int playstate, string nameofrisersound, float Velocity)
        {
            AudioStateUpdate update = Pooler.GetAudio();
            update.Volume = volume;
            update.pitch = pitch;
            update.playstate = playstate;
            update.nameofriser = nameofrisersound;
            update.Velocity = Velocity;
            update.code = 1;
            return update;
        }

        public void Release()
        {
            Pooler.AddAudioUpdate(this);
        }

    }

}
