using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
    [Serializable]
    class TempFile
    {
        public List<int> PacketNumbersStored = new List<int>();
        public long ByteLengthOfFile;
        public int FileType;
    }
}
