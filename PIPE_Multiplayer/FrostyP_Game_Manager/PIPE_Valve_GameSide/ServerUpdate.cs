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

            


            // Do any outgoing on Server thread
            SendToServerThread.UpdateMain();
           
        }


    }
}
