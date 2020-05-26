using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife
{
	/// <summary>
	/// Defines a single item in the accordion.
	/// </summary>
	[JsonObject]
	public class AccordionItem<T>
	{
		/// <summary>
		/// The unique index value of this item in the accordion.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Index")]
		public Int64 Index { get; set; }

		/// <summary>
		/// The time after which the item can be issued for work.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "AvailableOn")]
		public DateTime AvailableOn { get; set; } = DateTime.MinValue;

		/// <summary>
		/// The times when the item was issued / re-issued for work.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "IssueHistory")]
		public List<DateTime> IssueHistory { get; set; } = new List<DateTime>();

		/// <summary>
		/// The payload for the object.  No payload is required
		/// </summary>
		[JsonProperty(Required = Required.AllowNull, PropertyName = "Payload")]
		public T Payload { get; set; } = default;
	}
}
