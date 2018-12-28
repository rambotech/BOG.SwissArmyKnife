using Newtonsoft.Json;

namespace BOG.SwissArmyKnife.Test.Support
{
    [JsonObject]
    public class UrlTestItem
    {
        [JsonRequired]
        public string DataRow { get; set; }
        [JsonRequired]
        public string OriginalUrl { get; set; }
        [JsonRequired]
        public string ThrowsException { get; set; }
        [JsonRequired]
        public string ExceptionContains { get; set; }
        [JsonRequired]
        public string Scheme { get; set; }
        [JsonRequired]
        public string User { get; set; }
        [JsonRequired]
        public string Password { get; set; }
        [JsonRequired]
        public string Host { get; set; }
        [JsonRequired]
        public string Port { get; set; }
        [JsonRequired]
        public string Path { get; set; }
        [JsonRequired]
        public string Query { get; set; }
        [JsonRequired]
        public string Fragment { get; set; }
        [JsonRequired]
        public string UrlDecodedScheme { get; set; }
        [JsonRequired]
        public string UrlDecodedUser { get; set; }
        [JsonRequired]
        public string UrlDecodedPassword { get; set; }
        [JsonRequired]
        public string UrlDecodedHost { get; set; }
        [JsonRequired]
        public string UrlDecodedPort { get; set; }
        [JsonRequired]
        public string UrlDecodedPath { get; set; }
        [JsonRequired]
        public string UrlDecodedQuery { get; set; }
        [JsonRequired]
        public string UrlDecodedFragment { get; set; }
        [JsonRequired]
        public string AsString { get; set; }
        [JsonRequired]
        public string Note { get; set; }
    }
}
