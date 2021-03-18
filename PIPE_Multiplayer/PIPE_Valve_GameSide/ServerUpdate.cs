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
        /// fixed update function of server thread
        /// </summary>
        public static void Update()
        {
            
            // run receive
            GameNetworking.instance.Run();
            if(InGameUI.instance.Messages.Count > 0)
            {
            InGameUI.instance.messagetimer++;
            if(InGameUI.instance.messagetimer >= 150)
            {
               
                InGameUI.instance.Messages.RemoveAt(0);
                    InGameUI.instance.messagetimer = 0;
                
            }
            }

           

                // run any commands from Unity's Thread
                SendToServerThread.UpdateMain();
           
        }


    }
}
