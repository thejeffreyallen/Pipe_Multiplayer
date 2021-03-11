
using Harmony;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;

namespace PIPE_Valve_Console_Client
{



    internal static class Main
    {



        static GameObject modobj;
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





           
            modobj = new GameObject("Fnet");
            modobj.AddComponent<ConsoleLog>().enabled = false;
            modobj.AddComponent<GameManager>();
            modobj.AddComponent<GameNetworking>();
            modobj.AddComponent<ThreadManager>();
            modobj.AddComponent<InGameUI>();
            modobj.AddComponent<LocalPlayer>();








            return true;
        }
    }
}  

