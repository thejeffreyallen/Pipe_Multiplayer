using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
   public class TextMessage
    {
        /// <summary>
        /// The incoming message to be displayed
        /// </summary>
        public string Message;
        /// <summary>
        /// Colour code
        /// </summary>
        public FrostyUIColor uicolour;
        public Textmessagemode Mode;
        public ushort Playerid;

        public TextMessage(string _message, FrostyUIColor messagecolourclass, Textmessagemode mode, ushort playerid = 0)
        {
            Message = _message;
            uicolour = messagecolourclass;
            Mode = mode;
            Playerid = playerid;
        }


        public enum Textmessagemode : ushort
        {
            server,
            me,
            remoteplayer,
            system,
        }
    }

    public enum FrostyUIColor : ushort
    {
        Server,
        Me,
        Player,
        System,
    }
}
