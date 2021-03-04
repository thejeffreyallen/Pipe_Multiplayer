using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frosty_Pipe_Server
{
    class GameLogic
    {


        public static void Update()
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                }
            }


            ThreadManager.UpdateMain();
        }
    }
}
