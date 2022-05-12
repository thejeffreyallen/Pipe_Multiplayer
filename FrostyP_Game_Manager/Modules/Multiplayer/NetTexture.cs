using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
    public class NetTexture
    {
		public string Nameoftexture;
		public string NameofparentGameObject;
		public bool isNormal;
		public int Matnum;
		public string Directory;

		public NetTexture(string nameoftex, string nameofG_O, bool isnormal, int matnum, string dir)
		{
			Nameoftexture = nameoftex;
			NameofparentGameObject = nameofG_O;
			isNormal = isnormal;
			Matnum = matnum;
			Directory = dir;
		}

	}
}
