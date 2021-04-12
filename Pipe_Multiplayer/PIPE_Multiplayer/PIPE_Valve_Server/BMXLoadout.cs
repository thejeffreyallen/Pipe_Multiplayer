using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PIPE_Valve_Online_Server
{
    class BMXLoadout
    {
        public Vector3 FrameColour = new Vector3(1, 0, 0);
        public float FrameSmooth = 0;
        public float FrameMetallic = 0;
        public byte[] FrameTex = new byte[0];

        public Vector3 ForksColour = new Vector3(0, 0, 0);
        public float ForksSmooth = 0;
        public float ForksMetallic = 0;
        public byte[] ForksTex = new byte[0];

        public Vector3 BarsColour = new Vector3(0, 0, 0);
        public float BarsSmooth = 0;
        public float BarsMetallic = 0;
        public byte[] BarsTex = new byte[0];

        public Vector3 StemColour = new Vector3(0, 0, 0);
        public float StemSmooth = 0;
        public float StemMetallic = 0;
        public byte[] StemTex = new byte[0];


        public Vector3 FRimColour = new Vector3(0, 0, 0);
        public float FRimSmooth = 0;
        public float FrimMetallic = 0;
        public byte[] FrimTex = new byte[0];

        public Vector3 RRimColour = new Vector3(0, 0, 0);
        public float RRimSmooth = 0;
        public float RrimMetallic = 0;
        public byte[] RrimTex = new byte[0];



        public Vector3 SeatColour = new Vector3(0, 0, 0);
        public float SeatSmooth = 0;
        public byte[] SeatTex = new byte[0];

        public Vector3 FTireColour = new Vector3(0, 0, 0);
        public Vector3 RTireColour = new Vector3(0, 0, 0);
        public Vector3 FTireSideColour = new Vector3(0, 0, 0);
        public Vector3 RTireSideColour = new Vector3(0, 0, 0);
        public byte[] TiresTex = new byte[0];
        public byte[] TiresNormal = new byte[0];

        public string FrameTexname = "";
        public string Framenormalname = "";

        public string ForkTexname = "";
        public string Forknormalname = "";

        public string StemTexName = "";
        public string Stemnormalname = "";

        public string SeatTexname = "";
        public string Seatnormalname = "";

        public string BarTexName = "";
        public string Barnormalname = "";

        public string TireTexName = "";
        public string Tirenormalname = "";

        public string FRimTexName = "";
        public string FRimnormalname = "";

        public string RRimTexName = "";
        public string RRimnormalname = "";
     


       public List<string> Textureinfos = new List<string>();
       public List<Vector3> Colours = new List<Vector3>();
       public List<float> Smooths = new List<float>();
       public List<TextureInfo> bikeTexnames = new List<TextureInfo>();
        public List<TextureInfo> Bikenormalnames = new List<TextureInfo>();
        public List<TextureInfo> RiderTexnames = new List<TextureInfo>();


        public void Setup()
        {
            Textureinfos = new List<string>();
            Colours = new List<Vector3>();
           Smooths = new List<float>();
            Colours.Add(FrameColour);
            Colours.Add(ForksColour);
            Colours.Add(BarsColour);
            Colours.Add(SeatColour);
            Colours.Add(FTireColour);
            Colours.Add(FTireSideColour);
            Colours.Add(RTireColour);
            Colours.Add(RTireSideColour);

            Smooths.Add(FrameSmooth);
            Smooths.Add(ForksSmooth);
            Smooths.Add(BarsSmooth);
            Smooths.Add(SeatSmooth);

            Textureinfos.Add(FrameTexname);
            Textureinfos.Add(ForkTexname);
            Textureinfos.Add(BarTexName);
            Textureinfos.Add(TireTexName);
            Textureinfos.Add(Tirenormalname);
            Textureinfos.Add(SeatTexname);
        }


    }
}
