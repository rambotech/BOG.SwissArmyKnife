using Newtonsoft.Json;
using System;

namespace BOG.SwissArmyKnife.Entity
{
	/// <summary>
	/// The result portion of successfully answered question, which is tracked in the forecast rankings.
	/// </summary>
	[JsonObject]
	public class Forecast
	{
		/// <summary>
		/// The index is a string to locate the specific collection of arguments which generated the result.
		/// </summary>
		[JsonProperty("Key")]
		[JsonRequired]
		public string Key { get; set; }

		/// <summary>
		/// The forecast value
		/// </summary>
		[JsonProperty("Outcome")]
		[JsonRequired]
		public string Outcome { get; set; } = string.Empty;

		/// <summary>
		/// The forecast time for occurence
		/// </summary>
		[JsonProperty("Timestamp")]
		[JsonRequired]
		public DateTime Timestamp { get; set; } = DateTime.MinValue;

		/// <summary>
		/// The ranking value for this forecast: this determines its position of appearance (or absence) in the final summary.
		/// It is a simple string, intended to be sorted in descending order with others, where the top value represents
		/// the best ranked answer.
		/// </summary>
		[JsonProperty("Ranking")]
		[JsonRequired]
		public string Ranking { get; set; } = string.Empty;
	}
}