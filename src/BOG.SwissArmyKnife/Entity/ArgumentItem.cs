using Newtonsoft.Json;
using BOG.SwissArmyKnife.Enums;

namespace BOG.SwissArmyKnife.Entity
{
	/// <summary>
	/// Defines a single item in the Metaset.  Note: Value is normally only used to persist an item in progress if it requires a retry from its current state.
	/// </summary>
	[JsonObject]
	public class ArgumentItem
	{
		/// <summary>
		/// The (unique) name of the iteration.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// The items for each iteration.  e.g. ["1.20", "1.25", "1.30"], ["A","a","B","Z"], etc.
		/// The client will convert from a string to the desired type.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Items")]
		public string[] Items { get; set; }
	}
}
