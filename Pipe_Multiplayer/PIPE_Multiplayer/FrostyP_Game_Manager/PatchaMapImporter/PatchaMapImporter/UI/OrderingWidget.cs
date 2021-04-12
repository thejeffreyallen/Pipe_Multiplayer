//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Models;
	using Tools;
	using UnityEngine;

	/// <summary>
	/// Represent Ordering
	/// </summary>
	class OrderingWidget
	{
		/// <summary>
		/// Construct with existing order
		/// </summary>
		/// <param name="order"></param>
		public OrderingWidget(Ordering order)
		{
			Value = order;

			using (new GUILayout.HorizontalScope(_containerStyle)) {
				GUILayout.FlexibleSpace();
				GUILayout.Label("Order by : ", GUILayout.ExpandWidth(false));

				var options = new string[3] { "By name", "By authors", "By rating" };
				Value = (Ordering)GUILayout.Toolbar((int)order, options);
			}

			if (order != Value) {
				Log.Write("OrderingWidget: has changed ! [" + order + "] => [" + Value + "]");
			}
		}

		/// <summary>
		/// Return current value
		/// </summary>
		public Ordering Value { get; private set; }

		private static readonly GUIStyle _containerStyle = new GUIStyle {
			fontSize = 30,
			normal = new GUIStyleState {
				textColor = Color.white
			}
		};
	}
}
