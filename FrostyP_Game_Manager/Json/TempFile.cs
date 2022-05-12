using System;
using System.Collections.Generic;

namespace FrostyP_Game_Manager
{
    [Serializable]
    class TempFile
    {
        public List<int> PacketNumbersStored = new List<int>();
        public long ByteLengthOfFile;
    }
}
