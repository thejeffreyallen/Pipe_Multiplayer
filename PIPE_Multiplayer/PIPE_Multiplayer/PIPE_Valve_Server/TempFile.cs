using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPE_Valve_Online_Server
{
    [Serializable]
    class TempFile
    {
        public List<int> PacketNumbersStored = new List<int>();
        public long ByteLengthOfFile;
        public int FileType;

    }
}
