//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using System.IO;
	using UnityEngine;

	class TextureHelpers
	{
		/// <summary>
		/// create texture with unique color
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="col"></param>
		/// <returns></returns>
		public static Texture2D MakeTexture(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++) pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}

		/// <summary>
		/// create texture from image path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Texture2D MakeTextureFromFile(string path, int width, int height)
		{
			return MakeTextureFromData(File.ReadAllBytes(path), width, height);
		}

		/// <summary>
		/// create texture from bytearray
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Texture2D MakeTextureFromData(byte[] data, int width, int height)
		{
			var texture = new Texture2D(128, 128);
			ImageConversion.LoadImage(texture, data);
			return texture;
		}
	}


}
