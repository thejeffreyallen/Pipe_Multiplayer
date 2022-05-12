using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
	public class BundleData
	{

		public AssetBundle Bundle;
		/// <summary>
		/// file no with no path
		/// </summary>
		public string FileName;
		/// <summary>
		/// Full path to file
		/// </summary>
		public string FullDir;


		public BundleData(AssetBundle _bundle, string _filename, string fulldir)
		{
			Bundle = _bundle;
			FileName = _filename;
			FullDir = fulldir;


		}



	}

}
