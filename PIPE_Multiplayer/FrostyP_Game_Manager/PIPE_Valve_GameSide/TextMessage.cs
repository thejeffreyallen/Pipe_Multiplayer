using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Multiplayer
{
   public class TextMessage
    {
        /// <summary>
        /// The incoming message to be displayed
        /// </summary>
        public string Message;
        /// <summary>
        /// Colour code (int), use (int)MessageColour.
        /// </summary>
        public int FromCode;
        /// <summary>
        /// not used, Server adds an int to tell me whether its a server, player or my message
        /// </summary>
        public uint FromConnection;

        public TextMessage(string _message, int messagecolourclass, uint _fromconnection)
        {
            Message = _message;
            FromCode = messagecolourclass;
            FromConnection = _fromconnection;
        }



    }
}
