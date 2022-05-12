using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace FrostyP_Game_Manager
{
    public class SendToServerThread
    {
        private static readonly List<UnityAction> executeOnMainThread = new List<UnityAction>();
        private static readonly List<UnityAction> executeCopiedOnMainThread = new List<UnityAction>();
        private static bool actionToExecuteOnMainThread = false;


        


        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="_action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(UnityAction _action)
        {
            if (_action == null)
            {
                Debug.Log("No action to execute on main thread!");
                return;
            }


            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;

        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>

        public static void UpdateMain()
        {
            if (actionToExecuteOnMainThread)
            {
                executeCopiedOnMainThread.Clear();

                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;


                for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                {
                    executeCopiedOnMainThread[i]();
                }
            }
        }
    }
}