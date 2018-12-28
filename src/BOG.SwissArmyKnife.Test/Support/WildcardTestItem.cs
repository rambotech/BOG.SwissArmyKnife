using Newtonsoft.Json;

namespace BOG.SwissArmyKnife.Test.Support
{
    [JsonObject]
    public class WildcardTestItem
    {
        [JsonRequired]
        public string DataRow { get; set; }
        [JsonRequired]
        public string Value { get; set; }
        [JsonRequired]
        public string WildcardPattern { get; set; }
        [JsonRequired]
        public string CaseSensitive { get; set; }
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
