using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{

    /// <summary>
    /// A collection of images, saved as byte[]'s with their image name and associated Gameobject name for reload
    /// </summary>
   [Serializable]
    public class RiderSaveData
    {
        public byte[] Shirtbytes;
        public string shirtimagename;
        public string shirtParentname;

        public byte[] bottomsbytes;
        public string bottomsimagename;
        public string bottomsParentname;

        public byte[] hatbytes;
        public string hatimagename;
        public string hatParentname;

        public byte[] shoesbytes;
        public string shoesimagename;
        public string shoesParentname;

        public byte[] headbytes;
        public string headimagename;
        public string headParentname;

        public byte[] bodybytes;
        public string bodyimagename;
        public string bodyParentname;

        public byte[] handsfeetbytes;
        public string handsfeetimagename;
        public string handsfeetParentname;
        public bool CapisForward;
    }
}
