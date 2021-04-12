//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Disposable pattern for Disabling events on UI
	/// </summary>
	class DisableAllButtons : IDisposable
	{
		private bool _disable;

		public DisableAllButtons(bool disable)
		{
			_disable = disable;
			if(_disable) GUI.enabled = false;
		}

		public void Dispose()
		{
			if (_disable) GUI.enabled = true;
		}
	}
}
