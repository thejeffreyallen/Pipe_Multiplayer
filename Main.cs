using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using System.Reflection;


namespace FrostyP_PIPE_MultiPlayer
{

    internal static class Main
    {




        public static string modId;
        static GameObject modobj;













        static bool Load(UnityModManager.ModEntry modEntry)
        {

            Main.modId = modEntry.Info.Id;
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            modobj = new GameObject("Multiplayer_insert_object");
            modobj.AddComponent<LobbyEntry>();
           // modobj.AddComponent<Consolelog>();
            LobbyEntry.DontDestroyOnLoad(modobj);
            





            return true;
        }
    }
}
