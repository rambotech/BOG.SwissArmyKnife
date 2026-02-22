using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife.Test.Support
{
    [JsonObject]
    public class DateTimeExTestItem
    {
        public enum DateTimeExTestType
        {
            Indeterminate,
            Earliest,
            Latest
        }
        [JsonRequired]
        public string DataRow { get; set; }
        [JsonRequired]
        public DateTime OriginalDateTime { get; set; } = DateTime.Now;
        [JsonRequired]
        public DateTime[] DateTimeValuesToCompare { get; set; } = new DateTime[]
            {
                DateTime.MinValue,
                DateTime.MaxValue,
                DateTime.Now
            };
        [JsonRequired]
        public DateTimeExTestType TestAs { get; set; } = DateTimeExTestType.Indeterminate;
        [JsonRequired]
        public string ThrowsException { get; set; }
        [JsonRequired]
        public string ExceptionContains { get; set; }
        [JsonRequired]
        public DateTime ExpectedResultValue { get; set; } = DateTime.MinValue;
        [JsonRequired]
        public string Note { get; set; }
    }
}
