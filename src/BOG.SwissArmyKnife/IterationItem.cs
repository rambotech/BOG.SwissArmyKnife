using Newtonsoft.Json;
using System.Collections.Generic;

namespace BOG.SwissArmyKnife
{
    /// <summary>
    /// Represents the values for a single iteration, used in a set of iterations.
    /// </summary>
    [JsonObject]
    public class IterationItem
    {
        /// <summary>
        /// Describes the internal handling of the item ordinal.
        /// </summary>
        public enum Handling : int
        {
            /// <summary>
            /// It is an ordinal type.  The start and step values are used to calculate the value.
            /// </summary>
            OrdinalNumber = 0,
            /// <summary>
            /// It is a literal string.  The dictionary is used to resolve the value.
            /// </summary>
            Literal = 1
        }

        /// <summary>
        /// The name of the iteration item.  Must be unique.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether the item represents a list of strings, or a calculated numeric sequence.
        /// </summary>
        [JsonProperty]
        public Handling HandleAs { get; set; } = Handling.OrdinalNumber;

        /// <summary>
        /// The index 0 value for the numeri sequence
        /// </summary>
        [JsonProperty]
        public decimal NumericStartValue { get; set; } = 0.0M;

        /// <summary>
        /// The step value to get the next ordinal value.
        /// </summary>
        [JsonProperty]
        public decimal NumericStepValue { get; set; } = 0.0M;

        /// <summary>
        /// The number of indexed values (equivalent to count() on a list).
        /// </summary>
        [JsonProperty]
        public long NumericValueCount { get; set; } = 0;

        /// <summary>
        /// The literal values which result from this item.
        /// </summary>
        [JsonProperty]
        public Dictionary<int, string> LiteralValues { get; set; } = new Dictionary<int, string>();
    }
}