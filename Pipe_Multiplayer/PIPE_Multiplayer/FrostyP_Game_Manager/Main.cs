using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using System.Reflection;
using System.IO;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{

    internal static class Main
    {

        public static string modId;
        static GameObject GamemanOBJ;
        public static GameNetworking Network;






        static bool Load(UnityModManager.ModEntry modEntry)
        {

            Main.modId = modEntry.Info.Id;
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


            // checks for files and copies from the this mod's folder if needed
            string directory = Directory.GetCurrentDirectory();
            if (!File.Exists(directory + "\\GameNetworkingSockets.dll"))
            {
                try
                {
                    File.Copy(modEntry.Path + "GameNetworkingSockets.dll", directory + "\\GameNetworkingSockets.dll", true);

                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }
            if (!File.Exists(directory + "\\libprotobuf.dll"))
            {
                try
                {

                    File.Copy(modEntry.Path + "libprotobuf.dll", directory + "\\libprotobuf.dll", true);
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }
            if (!File.Exists(directory + "\\libcrypto-1_1.dll"))
            {
                try
                {

                    File.Copy(modEntry.Path + "libcrypto-1_1.dll", directory + "\\libcrypto-1_1.dll", true);
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }
            if (!File.Exists(directory + "\\libssl-1_1.dll"))
            {
                try
                {

                    File.Copy(modEntry.Path + "libssl-1_1.dll", directory + "\\libssl-1_1.dll", true);
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }
            if (!File.Exists(directory + "\\steamwebrtc.dll"))
            {
                try
                {

                    File.Copy(modEntry.Path + "steamwebrtc.dll", directory + "\\steamwebrtc.dll", true);
                }
                catch (UnityException x)
                {
                    Debug.Log(x);
                }
            }




            GamemanOBJ = new GameObject("FrostyGameManager");
            GamemanOBJ.AddComponent<Consolelog>().enabled = false;
            GamemanOBJ.AddComponent<InGameUI>();
            GamemanOBJ.AddComponent<ParkBuilder>();
            GamemanOBJ.AddComponent<ReplayMode>();
            GamemanOBJ.AddComponent<CameraSettings>();
            GamemanOBJ.AddComponent<RiderPhysics>();
            GamemanOBJ.AddComponent<FrostyPGamemanager>();
            GamemanOBJ.AddComponent<Teleport>();

            GamemanOBJ.AddComponent<GameManager>();
            GamemanOBJ.AddComponent<LocalPlayer>();
            GamemanOBJ.AddComponent<SendToUnityThread>();
            GamemanOBJ.AddComponent<CharacterModding>();
            GamemanOBJ.AddComponent<RemoteLoadManager>();
         
            Network = new GameNetworking();
            Network.Start();



           Object.DontDestroyOnLoad(GamemanOBJ);





            return true;
        }
























    }
}