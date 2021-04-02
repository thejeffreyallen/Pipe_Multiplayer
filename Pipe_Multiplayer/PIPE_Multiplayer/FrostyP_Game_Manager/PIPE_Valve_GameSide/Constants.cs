using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
   public class Constants
    {
        public const int TicksPerSec = 50; // seconds
        public const int MSPerTick = 1000 / TicksPerSec;
        public const int ServerMessageTime = 15; // seconds
        public const int PlayerMessageTime = 30; // seconds
        public const int SystemMessageTime = 10; // seconds
    }
}
