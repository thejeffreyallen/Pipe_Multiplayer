
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
    // Unneeded, brought in by Frosty manager, this will fire up just mutltiplayer stuff


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
                    Debug.Log(x);
                }
            }
            if (!File.Exists(directory + "\\libprotobufd.dll"))
            {
                try
                {

                    File.Copy(modEntry.Path + "libprotobufd.dll", directory + "\\libprotobufd.dll", true);
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


            
            
            FNetOBJ = new GameObject("Fnet");
            FNetOBJ.AddComponent<ConsoleLog>();
            FNetOBJ.AddComponent<GameManager>();
            FNetOBJ.AddComponent<InGameUI>();
            FNetOBJ.AddComponent<LocalPlayer>();
            FNetOBJ.AddComponent<SendToUnityThread>();
            FNetOBJ.AddComponent<CharacterModding>();
            UnityEngine.Object.DontDestroyOnLoad(FNetOBJ);

            Network = new GameNetworking();
            Network.Start();

           

           



            return true;
        }


       
       








    }
}  

