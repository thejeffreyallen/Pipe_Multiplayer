using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
	public class PlacedObject
	{
		public GameObject Object;
		public BundleData BundleData;
		public int ObjectId = 0;
		public ushort OwnerID = 0;

		public PlacedObject(GameObject GO, BundleData Bundata)
		{
			Object = GO;
			BundleData = Bundata;
		}

	}
}
