using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    
		/// <summary>
		/// Structure for sending data to net
		/// </summary>
		public class NetGameObject
		{
			public string NameofObject;
			public string NameofAssetBundle;
			public string NameOfFile;
			public Vector3 Rotation;
			public Vector3 Position;
			public Vector3 Scale;
			public bool IsPhysics;
			public GameObject _Gameobject = null;
			public int ObjectID;
			public AssetBundle AssetBundle;
			public ushort OwnerID;
			public string Directory;

			public NetGameObject(string _nameofobject, string _nameoffile, string _nameofassetbundle, Vector3 _rotation, Vector3 _position, Vector3 _scale, bool _IsPhysicsenabled, int objectid, GameObject GO, string dir)
			{
				NameofObject = _nameofobject;
				NameOfFile = _nameoffile;
				NameofAssetBundle = _nameofassetbundle;
				Rotation = _rotation;
				Position = _position;
				Scale = _scale;
				IsPhysics = _IsPhysicsenabled;
				_Gameobject = GO;
				ObjectID = objectid;
				Directory = dir;

			}




		}
	
}
