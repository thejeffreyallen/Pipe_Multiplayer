//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.Tools
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Extensions for List<T>
	/// </summary>
	static class ListExtensions
	{
		/// <summary>
		/// Equivalent to linq FindAll
		/// </summary>
		/// <typeparam name="T">type</typeparam>
		/// <param name="list">container</param>
		/// <param name="pred">predicate</param>
		/// <returns></returns>
		public static List<T> Subset<T>(this List<T> list, Predicate<T> pred)
		{
			var ret = new List<T>(list.Count);
			foreach (T t in list) {
				if (pred(t)) ret.Add(t);
			}

			return ret;
		}
	}
}
