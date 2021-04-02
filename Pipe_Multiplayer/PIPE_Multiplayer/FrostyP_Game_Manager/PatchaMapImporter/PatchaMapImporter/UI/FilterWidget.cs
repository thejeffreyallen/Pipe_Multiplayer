//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Models;

	/// <summary>
	/// All filters widget
	/// </summary>
	class FilterWidget
	{
		/// <summary>
		/// Construct with current value
		/// </summary>
		/// <param name="filter"></param>
		public FilterWidget(Filter filter)
		{
			Value = new Filter();
			Value.Flags = new FlagsFilterWidget("Filter by types : ", filter.Flags).Value;
			Value.Search = new StringFilterWidget(filter.Search).Value;
			Value.Ordering = new OrderingWidget(filter.Ordering).Value;
		}

		/// <summary>
		/// user selected filter
		/// </summary>
		public Filter Value { get; private set; }


	}
}
