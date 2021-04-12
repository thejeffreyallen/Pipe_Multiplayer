using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPE_Valve_Online_Server
{
    class GameLogic
    {
        /// <summary>
        /// Add any functions here which need to fire at tick rate
        /// </summary>
        public static void Update()
        {
            

            // run any actions added by other threads, listen socket automatically adds incoming messages to be
            // processed at tick rate
            ThreadManager.UpdateMain();
        }



    }
}
