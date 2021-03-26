using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
    [Serializable]
    public class BMXSaveData
    {
        public float[] FrameColour;
        public float FrameSmooth;
        public byte[] FrameTex;

        public float[] ForksColour;
        public float ForksSmooth;
        public byte[] ForksTex;

        public float[] BarsColour;
        public float BarsSmooth;
        public byte[] BarsTex;

        public float[] SeatColour;
        public float SeatSmooth;
        public byte[] SeatTex;

        public float[] FTireColour;
        public float[] RTireColour;
        public float[] FTireSideColour;
        public float[] RTireSideColour;
        public byte[] TiresTex;
        public byte[] TiresNormal;

        public string FrameTexname;
        public string ForksTexname;
        public string BarsTexname;
        public string SeatTexname;
        public string TiresTexname;
        public string TiresNormalname;

    }
}
