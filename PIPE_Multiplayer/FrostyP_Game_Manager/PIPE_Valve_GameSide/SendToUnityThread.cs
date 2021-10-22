using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PIPE_Multiplayer
{

    /// <summary>
    /// Runs any commands left by the Server Thread at fixed update
    /// </summary>
    public class SendToUnityThread : MonoBehaviour
    {
        public static SendToUnityThread instance;

        private static readonly List<UnityAction> executeOnMainThread = new List<UnityAction>();
        private static readonly List<UnityAction> executeCopiedOnMainThread = new List<UnityAction>();
        private static bool actionToExecuteOnMainThread = false;


        private void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }
        }


        /// <summary>
        /// this will run any actions copied from servers thread on Unity's thread
        /// </summary>
        void LateUpdate()
        {
            UpdateMain();
        }


        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="_action">The action to be executed on the main thread.</param>
        public void ExecuteOnMainThread(UnityAction _action)
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

        public void UpdateMain()
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