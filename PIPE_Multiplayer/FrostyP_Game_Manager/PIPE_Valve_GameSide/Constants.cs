using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
   public class Constants
    {
        public const int TicksPerSec = 30; // seconds
        public const int MSPerTick = 1000 / TicksPerSec;
        public const int ServerMessageTime = 12; // seconds
        public const int PlayerMessageTime = 15; // seconds
        public const int SystemMessageTime = 5; // seconds
    }
}
