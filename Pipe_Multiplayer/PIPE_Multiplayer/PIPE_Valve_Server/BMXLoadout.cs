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
     


      
      
        public List<PlayerTextureInfo> BMXTextureInfos = new List<PlayerTextureInfo>();
        public List<PlayerTextureInfo> BMXNormalTexInfos = new List<PlayerTextureInfo>();
        public List<PlayerTextureInfo> RiderTextureInfos = new List<PlayerTextureInfo>();


      


    }
}
