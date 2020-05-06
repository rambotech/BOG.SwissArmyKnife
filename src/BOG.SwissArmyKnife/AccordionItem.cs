using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife
{
	public enum AccordionItemState : int { Failed = 0, Succeeded = 1, InProgress = 2, Pending = 3}

	/// <summary>
	/// Defines a single item in the accordion.
	/// </summary>
	[JsonObject]
	public class AccordionItem
	{
		/// <summary>
		/// The unique index value of this item in the accordion.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Index")]
		public Int64 Index { get; set; }

		/// <summary>
		/// The current processing result of the item
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "State")]
		public AccordionItemState State { get; set; } = AccordionItemState.InProgress;

		/// <summary>
		/// The number of attempts made thus far (i.e. the number of issuances and re-issuances).
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Attempts")]
		public int Attempts { get; set; } = 0;

		/// <summary>
		/// The time after which the item can be issued for work.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Deadline")]
		public DateTime DeadLine { get; set; } = DateTime.MinValue;

		/// <summary>
		/// The number of timeouts which have occurred since this item was provided for processing.
		/// A timeout is defined as not receiving a RetryItem() or CompleteItem() method call since the 
		/// item was issued via a GetItem() or GetItems() method call.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Timeouts")]
		public int Timeouts { get; set; } = 0;
	}
}
