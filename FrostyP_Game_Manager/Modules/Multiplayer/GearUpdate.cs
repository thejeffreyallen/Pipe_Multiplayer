using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
	/// <summary>
	/// Built when sending or receiving an Update to net
	/// </summary>
	public class GearUpdate
	{
		public bool isRiderUpdate;
		public List<NetTexture> RiderTextures;
		public bool Capisforward;
		public string GarageSaveXML;
		public string Presetname;



		/// <summary>
		/// to Send Just Riders gear
		/// </summary>
		/// <param name="ridertextures"></param>
		public GearUpdate(List<NetTexture> ridertextures)
		{
			isRiderUpdate = true;
			RiderTextures = ridertextures;

		}

		public GearUpdate()
		{
		}

	}
}
