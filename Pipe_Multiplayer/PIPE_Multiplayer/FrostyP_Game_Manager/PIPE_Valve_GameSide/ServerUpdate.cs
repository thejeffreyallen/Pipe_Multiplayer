using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Updated by tick rate on server thread calculated by system time
    /// </summary>
    class ServerUpdate
    {
        
      
        /// <summary>
        /// fixed update function of sendtoserver thread, everything done by the secondary thread at tick rate is here
        /// </summary>
        public static void Update()
        {
           
            // Run Send
            SendToServerThread.UpdateMain();
            
            // run receive
            GameNetworking.instance.Run();




            /*
            // Send the command back to Unity thread to check movement threshold of our guy
            if (GameManager.instance._localplayer.ServerActive)
            {
           
            SendToUnityThread.instance.ExecuteOnMainThread(() =>
            {
            GameManager.instance._localplayer.CheckThreshold();

            });

            }
            */
            
        }


    }
}
