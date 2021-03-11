using UnityModManagerNet;
using UnityEngine;
using Harmony12;
using System.Reflection;
using System.IO;



 namespace Frosty_Online_GameSide

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


            string directory = Directory.GetCurrentDirectory();
            if (!File.Exists(directory + "\\GameNetworkingSockets.dll"))
            {
                try
                {
                    File.Copy(modEntry.Path + "GameNetworkingSockets.dll", directory + "\\GameNetworkingSockets.dll", true);
                    File.Copy(modEntry.Path + "libprotobuf.dll", directory + "\\libprotobuf.dll", true);
                    File.Copy(modEntry.Path + "libcrypto-1_1-x64.dll", directory + "\\libcrypto-1_1-x64.dll", true);



                }
                catch (UnityException x)
                {

                }
            }


            modobj = new GameObject("Fnet");
            modobj.AddComponent<ConsoleLog>().enabled = false;
            modobj.AddComponent<Client>();
            modobj.AddComponent<ThreadManager>();
            modobj.AddComponent<LocalPlayer>();
            modobj.AddComponent<GameManager>();
            modobj.AddComponent<Ingame_UI>();



                    return true;
        }
    }
}