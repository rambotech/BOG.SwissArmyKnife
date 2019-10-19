using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife
{
    /// <summary>
    /// Defines a single item in the accordion.
    /// </summary>
    [JsonObject]
    public class AccordionItem
    {
        [JsonProperty(Required = Required.Always, PropertyName = "Index")]
        public Int64 Index { get; set; }

        [JsonProperty(Required = Required.Always, PropertyName = "Attempts")]
        public int Attempts { get; set; } = 0;

        [JsonProperty(Required = Required.Always, PropertyName = "Deadline")]
        public DateTime DeadLine { get; set; } = DateTime.MinValue;
    }

    /// <summary>
    /// Defines the Accordion for tracking the collection of AccordionItems, and the associated methods.
    /// </summary>
    [JsonObject]
    public class Accordion
    {
        // Metrics
        /// <summary>
        /// The list of items currently available for work.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "Items")]
        public Dictionary<Int64,AccordionItem> ItemsInProgress { get; set; } = new Dictionary<long, AccordionItem>();

        /// <summary>
        /// The first item number to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexStart")]
        public Int64 IndexStart { get; set; }  = 0;

        /// <summary>
        /// The final item number to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexEnd")]
        public Int64 IndexEnd { get; set; } = 0;

        /// <summary>
        /// The maximum number of items which can be actively tracked at any given time.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
        public Int64 MaxInProgress { get; set; } = 0;

        // Instance variables 
        [JsonProperty(Required = Required.Always, PropertyName = "IndexOffset")]
        public Int64 IndexOffset { get; private set; } = 0;

        // local
        private object lockItemList = new object();

        /// <summary>
        /// Deserialization instantiator
        /// </summary>
        public Accordion()
        {

        }

        /// <summary>
        /// Standard instantiator for a new instance
        /// </summary>
        /// <param name="indexStart">the low value to begin processing.</param>
        /// <param name="indexEnd">the high value representing the final number to process.</param>
        /// <param name="maxInProgress">the maximum number of items which can actively tracked.</param>
        public Accordion(Int64 indexStart, Int64 indexEnd, Int64 maxInProgress)
        {
            if (indexStart < 0) throw new ArgumentException("indexStart must be >= 0");
            if (indexEnd < 0) throw new ArgumentException("indexEnd must be >= 0");
            if (indexEnd < indexStart) throw new ArgumentException("indexStart must be <= indexEnd");
            if (maxInProgress > 20000) throw new ArgumentException("maxInProgress must be <= 20,000");
            if (maxInProgress < (indexEnd - indexStart + 1)) throw new ArgumentException("maxInProgress must be lower than the total number of items to process");
            IndexStart = indexStart;
            IndexEnd = indexEnd;
            MaxInProgress = maxInProgress;
        }

        /// <summary>
        ///  Retrieves an AccordionItem requiring processing.
        /// </summary>
        /// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
        /// <param name="item">The Accordion Item object to be processed.</param>
        /// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
        public bool GetItem(int secondsTimeout, out AccordionItem item)
        {
            var result = false;
            item = null;
            lock (lockItemList)
            {
                // add items to mazimize buffer availability.
                while (IndexStart + IndexOffset <= IndexEnd && ItemsInProgress.Count < MaxInProgress)
                {
                    ItemsInProgress.Add(IndexStart + IndexOffset, new AccordionItem
                    {
                        Index = IndexStart + IndexOffset
                    });
                    IndexOffset++;
                }
                var itemSelect = ItemsInProgress.Where(o => o.DeadLine < DateTime.Now).FirstOrDefault();
                if (itemSelect != null)
                {
                    itemSelect.Attempts++;
                    itemSelect.DeadLine = DateTime.Now.AddSeconds(secondsTimeout);

                    item = new AccordionItem
                    {
                        Index = itemSelect.Index,
                        Attempts= itemSelect.Attempts,
                        DeadLine= itemSelect.DeadLine
                    };
                }
            }
            return result;
        }

        public bool RetryItem(AccordionItem item)
        {
            var result = false;
            lock(lockItemList)
            {
                var itemSelect = ItemsInProgress.Where(o => o.Index == item.Index).FirstOrDefault();
                result = itemSelect != null;
                if (result)
                {
                    itemSelect.Attempts++;
                    itemSelect.DeadLine = DateTime.MinValue;
                }
            }
            return result;
        }

        public void CompleteItem(AccordionItem item)
        {
            lock (lockItemList)
            {
                var selectedItem = ItemsInProgress.Where(o => o.Index == item.Index).FirstOrDefault() ;
                if (selectedItem != null)
                {
                    ItemsInProgress.Remove(selectedItem);
                }
            }
        }
    }
}
