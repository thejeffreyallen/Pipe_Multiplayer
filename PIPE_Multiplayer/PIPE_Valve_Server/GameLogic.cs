using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPE_Valve_Online_Server
{
    class GameLogic
    {

        public static void Update()
        {
            foreach (Player _client in Server.Players.Values)
            {
                if (_client != null)
                {
                    _client.Update();
                }
            }


            ThreadManager.UpdateMain();
        }



    }
}
