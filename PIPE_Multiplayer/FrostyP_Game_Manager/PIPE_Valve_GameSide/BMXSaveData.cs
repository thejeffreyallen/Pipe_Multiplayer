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
        public float FrameMetallic;
        public float FrameSmooth;
        public byte[] FrameTex;
        public string FrameTexname;
        public byte[] FrameNormal;
        public string FrameNormalname;

        public float[] ForksColour;
        public float ForksSmooth;
        public float Forksmetallic;
        public byte[] ForksTex;
        public string ForksTexname;
        public byte[] ForksNormal;
        public string ForksNormalname;

        public float[] BarsColour;
        public float BarsSmooth;
        public float BarsMetallic;
        public byte[] BarsTex;
        public string BarsTexname;
        public byte[] BarsNormal;
        public string BarsNormalname;

        public float[] StemColour;
        public float StemSmooth;
        public float StemMetallic;
        public byte[] StemTex;
        public string StemTexName;
        public byte[] StemNormal;
        public string StemNormalName;


        public float[] FRimColour;
        public float FRimSmooth;
        public float FRimMetallic;
        public byte[] FRimTex;
        public string FRimTexName;
        public byte[] FRimNormal;
        public string FRimNormalName;

        public float[] RRimColour;
        public float RRimSmooth;
        public float RRimMetallic;
        public byte[] RRimTex;
        public string RRimTexName;
        public byte[] RRimNormal;
        public string RRimNormalName;


        public float[] SeatColour;
        public float SeatSmooth;
        public byte[] SeatTex;
        public string SeatTexname;
        public byte[] SeatNormal;
        public string SeatNormalname;


        public float[] FTireColour;
        public float[] RTireColour;
        public float[] FTireSideColour;
        public float[] RTireSideColour;
        public byte[] TiresTex;
        public string TiresTexname;
        public byte[] TiresNormal;
        public string TiresNormalname;


    }
}
