using UnityModManagerNet;
using UnityEngine;
using Harmony12;
using System.Reflection;



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