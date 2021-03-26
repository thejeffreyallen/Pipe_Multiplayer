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
        public byte[] FrameTex = new byte[0];

        public Vector3 ForksColour = new Vector3(0, 0, 0);
        public float ForksSmooth = 0;
        public byte[] ForksTex = new byte[0];

        public Vector3 BarsColour = new Vector3(0, 0, 0);
        public float BarsSmooth = 0;
        public byte[] BarsTex = new byte[0];

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
        public string ForkTexname = "";
        public string SeatTexname = "";
        public string BarTexName = "";
        public string TireTexName = "";
        public string TireNormalName = "";

       public List<string> Texturenames = new List<string>();
       public List<Vector3> Colours = new List<Vector3>();
       public List<float> Smooths = new List<float>();
        public List<TextureInfo> TexInfos = new List<TextureInfo>();
       

        public void Setup()
        {
            Texturenames = new List<string>();
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

            Texturenames.Add(FrameTexname);
            Texturenames.Add(ForkTexname);
            Texturenames.Add(BarTexName);
            Texturenames.Add(TireTexName);
            Texturenames.Add(TireNormalName);
            Texturenames.Add(SeatTexname);
        }


    }
}
