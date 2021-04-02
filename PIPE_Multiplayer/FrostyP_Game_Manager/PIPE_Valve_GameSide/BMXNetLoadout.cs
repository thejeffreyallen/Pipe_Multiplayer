using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
   
   public class BMXNetLoadout : MonoBehaviour
    {
        public static BMXNetLoadout instance;


        public Vector3 FrameColour = new Vector3(0,0,0);
        public float FrameSmooth = 0;
        public float FrameMetallic = 0;
        public byte[] FrameTex = new byte[0];
        public byte[] FrameNormal = new byte[0];
        public string FrameTexname = "e";
        public string FrameNormalName = "e";


        public Vector3 ForksColour = new Vector3(0, 0, 0);
        public float ForksSmooth = 0;
        public float ForksMetallic = 0;
        public byte[] ForksTex = new byte[0];
        public byte[] ForksNormal = new byte[0];
        public string ForkTexname = "e";
        public string ForksNormalName = "e";



        public Vector3 BarsColour = new Vector3(0, 0, 0);
        public float BarsSmooth = 0;
        public float BarsMetallic = 0; 
        public byte[] BarsTex = new byte[0];
        public byte[] BarsNormal = new byte[0];
        public string BarTexName = "e";
        public string BarsNormalName = "e";

        public Vector3 SeatColour = new Vector3(0, 0, 0);
        public float SeatSmooth = 0;
        public byte[] SeatTex = new byte[0];
        public byte[] SeatNormal = new byte[0];
        public string SeatTexname = "e";
        public string SeatNormalName = "e";



        public Vector3 FTireColour = new Vector3(0, 0, 0);
        public Vector3 RTireColour = new Vector3(0, 0, 0);
        public Vector3 FTireSideColour = new Vector3(0, 0, 0);
        public Vector3 RTireSideColour = new Vector3(0, 0, 0);
        public float FTireSmooth = 0;
        public float RTireSmooth = 0;
        public byte[] TiresTex = new byte[0];
        public byte[] TiresNormal = new byte[0];
        public string TireTexName = "e";
        public string TireNormalName = "e";
       


        public Vector3 FRimColour = new Vector3(0, 0, 0);
        public float FRimSmooth = 0;
        public float FRimMetallic = 0;
        public byte[] FRimTex = new byte[0];
        public byte[] FRimNormal = new byte[0];
        public string FRimTexName = "e";
        public string FRimNormalName = "e";


        public Vector3 RRimColour = new Vector3(0, 0, 0);
        public float RRimSmooth = 0;
        public float RRimMetallic = 0;
        public byte[] RRimTex = new byte[0];
        public byte[] RRimNormal = new byte[0];
        public string RRimTexName = "e";
        public string RRimNormalName = "e";

        public Vector3 StemColour = new Vector3(0, 0, 0);
        public float StemSmooth = 0;
        public float StemMetallic = 0;
        public byte[] StemTex = new byte[0];
        public byte[] StemNormal = new byte[0];
        public string StemTexName = "e";
        public string StemNormalName = "e";








        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Net loadout already exists, destroying old netloadout now");
                Destroy(this);
            }




    }



       public void GrabTextures()
        {
            try
            {
                
                if (CharacterModding.instance.FrameRen.material.mainTexture != null)
                {
           FrameTexname = CharacterModding.instance.FrameRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.ForksRen.material.mainTexture != null)
                {
                    ForkTexname = CharacterModding.instance.ForksRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.SeatRen.material.mainTexture != null)
                {
                    SeatTexname = CharacterModding.instance.SeatRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.BarsRen.material.mainTexture != null)
                {
                    BarTexName = CharacterModding.instance.BarsRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.FTireRen.material.mainTexture != null)
                {
                    TireTexName = CharacterModding.instance.FTireRen.material.mainTexture.name;  // needs normal too really
                }
            
            Debug.Log("Grabbed any Textures on bike");

            }
            catch (UnityException x)
            {
                Debug.Log(x);
            }
        }


        void OnGUI()
        {
           // GUILayout.Label(FrameColour.x.ToString());
        }

    }
}
