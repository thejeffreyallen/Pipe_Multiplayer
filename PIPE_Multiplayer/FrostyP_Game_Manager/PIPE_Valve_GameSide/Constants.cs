﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Multiplayer
{
   public class Constants
    {
        public const int TicksPerSec = 60; // seconds
        public const int MSPerTick = 1000 / TicksPerSec;
        public const int ServerMessageTime = 40; // seconds
        public const int PlayerMessageTime = 60; // seconds
        public const int SystemMessageTime = 20; // seconds
    }
}