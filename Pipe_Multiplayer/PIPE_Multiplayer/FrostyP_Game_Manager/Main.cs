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
        static GameObject FNetOBJ;
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
            GamemanOBJ.AddComponent<FrostyPGamemanager>().Onlineobj = FNetOBJ = new GameObject("FNET");
            GamemanOBJ.AddComponent<ParkBuilder>();
           


            
           
            FNetOBJ.AddComponent<GameManager>();
            FNetOBJ.AddComponent<InGameUI>();
            FNetOBJ.AddComponent<LocalPlayer>();
            FNetOBJ.AddComponent<SendToUnityThread>();
            FNetOBJ.AddComponent<BMXNetLoadout>();
            FNetOBJ.AddComponent<CharacterModding>();
         
            UnityEngine.Object.DontDestroyOnLoad(FNetOBJ);

            Network = new GameNetworking();
            Network.Start();



            UnityEngine.Object.DontDestroyOnLoad(GamemanOBJ);





            return true;
        }
























    }
}