using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BOG.SwissArmyKnife.Test.Support
{
    [JsonObject]
    public class IterationTestItem
    {
        public enum OrdinalNumberMethod : int { Range, Count }

        [JsonRequired]
        public string DataRow { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public IterationItem.Handling HandleAs { get; set; } = IterationItem.Handling.OrdinalNumber;

        public List<string> LiteralValues { get; set; }
        
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrdinalNumberMethod Method { get; set; } = OrdinalNumberMethod.Count;

        [JsonRequired]
        public decimal StartValue { get; set; }

        [JsonRequired]
        public decimal StepValue { get; set; }

        [JsonRequired]
        public int CountValue { get; set; }

        [JsonRequired]
        public decimal EndValue { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public Iteration.EndValueEval EndEval { get; set; }

        [JsonRequired]
        public string ThrowsException { get; set; }

        [JsonRequired]
        public string ExceptionContains { get; set; }

        [JsonRequired]
        public long CountTestValue { get; set; }
    }
}
