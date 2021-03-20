using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
    class Constants
    {
        public const int TicksPerSec = 60; // seconds
        public const int MSPerTick = 1000 / TicksPerSec;
        public const int ServerMessage = 5; // seconds
        public const int PlayerMessage = 10; // seconds
        public const int SystemMessage = 2; // seconds
    }
}
