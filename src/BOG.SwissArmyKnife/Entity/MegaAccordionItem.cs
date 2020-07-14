using Newtonsoft.Json;
using System.Collections.Generic;

namespace BOG.SwissArmyKnife.Entity
{
	/// <summary>
	/// Defines a single item in the Metaset.  Note: Value is normally only used to persist an item in progress if it requires a retry from its current state.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[JsonObject]
	public class MegaAccordionItem<T>
	{
		/// <summary>
		/// A big-endian style hexadecimal notation of each iteration's current offset, e.g. 32:27A:45:B:7
		/// Each position represents the 0-based index of the iteration item being processed.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Index")]
		public string Key { get; set; }

		[JsonProperty(Required = Required.Always, PropertyName = "Arguments")]
		public Dictionary<string, string> Arguments { get; set; }
		/// <summary>
		/// The time when the item is available for processing.  This is usually the time when the item can be re-released,
		/// since the previous item did not complete on time.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "DateAvailableTicks")]
		public long DateAvailableTicks { get; set; }

		/// <summary>
		/// The object represented by this index.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Value")]
		public T Value { get; set; }
	}

}
