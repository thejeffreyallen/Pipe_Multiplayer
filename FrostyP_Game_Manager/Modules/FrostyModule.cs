using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    // a component viable for frostypmanager to include
    public class FrostyModule : MonoBehaviour
    {
        public string Buttontext = "Module";
        public bool isOpen;
        public int MenuID;

        public virtual void Show()
        {

        }
        public virtual void Open()
        {
            FrostyPGamemanager.instance.MenuShowing = MenuID;
            isOpen = true;
        }
        public virtual void Close()
        {
            isOpen = false;
            FrostyPGamemanager.instance.MenuShowing = 0;
            FrostyPGamemanager.instance.OpenMenu = true;
        }

    }
}
