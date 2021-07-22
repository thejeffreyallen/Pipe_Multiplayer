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
        public string FrameTexName;
        public byte[] FrameNormal;
        public string FrameNormalName;

        public float[] ForksColour;
        public float ForksSmooth;
        public float ForksMetallic;
        public byte[] ForksTex;
        public string ForksTexName;
        public byte[] ForksNormal;
        public string ForksNormalName;

        public float[] BarsColour;
        public float BarsSmooth;
        public float BarsMetallic;
        public byte[] BarsTex;
        public string BarsTexName;
        public byte[] BarsNormal;
        public string BarsNormalName;

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
        public string SeatTexName;
        public byte[] SeatNormal;
        public string SeatNormalName;


        // F Tire
        public float[] FTireColour;
        public float[] FTireSideColour;
        public float FTireSmooth;
        public float FTireSideSmooth;
        public byte[] FTireTex;
        public byte[] FTireNormal;
        public string FTireTexName;
        public string FTireNormalName;
        public string FTireMeshFileName;

        // R Tire
        public float[] RTireColour;
        public float[] RTireSideColour;
        public float RTireSmooth;
        public float RTireSideSmooth;
        public byte[] RTireTex;
        public byte[] RTireNormal;
        public string RTireTexName;
        public string RTireNormalName;
        public string RTireMeshFileName;



    }
}
