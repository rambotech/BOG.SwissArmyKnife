using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BOG.SwissArmyKnife.Test.Support
{
    [JsonObject]
    public class ResolvePlaceholderTestItem
    {
        public enum TestType : int
        {
            Undefined = 0,
            ResolvePlaceHolder = 1,
            ResolvePathPlaceHolder = 2,
            ResolveLocalSpec = 3
        }

        [JsonRequired]
        public string DataRow { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public TestType TestToExecute { get; set; }

        [JsonRequired]
        public string Value { get; set; }

        [JsonRequired]
        public Dictionary<string, string> LookupDictionary { get; set; }

        [JsonRequired]
        public string StartDelimiter { get; set; }

        [JsonRequired]
        public string EndDelimiter { get; set; }

        [JsonRequired]
        public string ExpectedResult { get; set; }

        [JsonRequired]
        public string ThrowsException { get; set; }

        [JsonRequired]
        public string ExceptionContains { get; set; }

        [JsonRequired]
        public string Note { get; set; }
    }
}
