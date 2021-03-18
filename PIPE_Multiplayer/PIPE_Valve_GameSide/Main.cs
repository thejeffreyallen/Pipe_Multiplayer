
using Harmony;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

namespace PIPE_Valve_Console_Client
{



    static class Main
    {

       public static GameNetworking Network;


        static GameObject FNetOBJ;
        public static string modId;

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

                }
            }


            
            
            FNetOBJ = new GameObject("Fnet");
            FNetOBJ.AddComponent<ConsoleLog>().enabled = false;
            FNetOBJ.AddComponent<GameManager>();
            FNetOBJ.AddComponent<InGameUI>().enabled = false;
            FNetOBJ.AddComponent<LocalPlayer>().enabled = false;
            FNetOBJ.AddComponent<SendToUnityThread>();
            UnityEngine.Object.DontDestroyOnLoad(FNetOBJ);

            Network = new GameNetworking();
            Network.Start();

           

           



            return true;
        }


       
       








    }
}  

