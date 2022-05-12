using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace FrostyP_Game_Manager
{

    internal static class Main
    {

        public static string modId;
        static GameObject GamemanOBJ;
        




        static bool Load(UnityModManager.ModEntry modEntry)
        {


            Main.modId = modEntry.Info.Id;
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            GamemanOBJ = new GameObject("FrostyGameManager");
            GamemanOBJ.AddComponent<ConfigMenu>();
            GamemanOBJ.AddComponent<Consolelog>().enabled = false;
            GamemanOBJ.AddComponent<InGameUI>();
            GamemanOBJ.AddComponent<ParkBuilder>();
            GamemanOBJ.AddComponent<ReplayMode>();
            GamemanOBJ.AddComponent<CameraModding>();
            GamemanOBJ.AddComponent<RiderPhysics>();
            GamemanOBJ.AddComponent<FrostyPGamemanager>();
            GamemanOBJ.AddComponent<Teleport>();
            GamemanOBJ.AddComponent<CharacterModding>();

            GamemanOBJ.AddComponent<SendToUnityThread>();
            GamemanOBJ.AddComponent<MultiplayerManager>();
            GamemanOBJ.AddComponent<LocalPlayer>();
            GamemanOBJ.AddComponent<RemoteLoadManager>();
         
           Object.DontDestroyOnLoad(GamemanOBJ);





            return true;
        }
























    }
}