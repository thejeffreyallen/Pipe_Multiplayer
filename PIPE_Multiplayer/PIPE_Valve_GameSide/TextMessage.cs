using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
   public class TextMessage
    {
        /// <summary>
        /// The incoming message to be displayed
        /// </summary>
        public string Message;
        /// <summary>
        /// The int representing whether this is a server or player message
        /// </summary>
        public int FromCode;

        public uint FromConnection;

        public TextMessage(string _message, int _fromcode, uint _fromconnection)
        {
            Message = _message;
            FromCode = _fromcode;
            FromConnection = _fromconnection;
        }








        // other possibilities?
        /*
        colour for each player? could be aLOT of players



        */

    }
}
