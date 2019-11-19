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
        /// <summary>
        /// The unique index value of this item in the accordion.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "Index")]
        public Int64 Index { get; set; }

        /// <summary>
        /// The number of attempts made thus far (i.e. the number of issuances and re-issuances).
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "Attempts")]
        public int Attempts { get; set; } = 0;

        /// <summary>
        /// The time after which the item can be issued for work.
        /// </summary>
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
        public List<AccordionItem> ItemsInProgress { get; set; } = new List<AccordionItem>();

        /// <summary>
        /// The first item number to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexStart")]
        public Int64 IndexStart { get; set; } = 0;

        /// <summary>
        /// The final item number (inclusive) to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexEnd")]
        public Int64 IndexEnd { get; set; } = 0;

        /// <summary>
        /// The maximum number of items which can be actively tracked at any given time.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
        public int MaxInProgress { get; set; } = 0;

        // Instance variable serialized to track progression through the list.
        [JsonProperty(Required = Required.Always, PropertyName = "IndexOffset")]
        public Int64 IndexOffset { get; private set; } = 0;

        // local
        private readonly object lockItemList = new object();

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
        /// <param name="count">the number of items to process.</param>
        /// <param name="maxInProgress">the maximum number of items which can actively tracked.</param>
        public Accordion(Int64 indexStart, Int64 count, int maxInProgress)
        {
            if (indexStart < 0) throw new ArgumentException("indexStart must be >= 0");
            if (count <= 0) throw new ArgumentException("count must be > 0");
            if (maxInProgress < 10) throw new ArgumentException("maxInProgress must be >= 10");
            IndexStart = indexStart;
            IndexEnd = indexStart + count;
            MaxInProgress = maxInProgress;
        }

        /// <summary>
        ///  Retrieves a set of AccordionItems requiring processing, up to the MaxItemsInProgress limit.
        /// </summary>
        /// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
        /// <param name="maxItems">The maximum number of items to retrieve (at least 1, and capped by the value of maximum in progress)</param>
        /// <param name="items">The list of Accordion Item objects to be returned.</param>
        /// <param name="favorNewItems">When true, higher attempts </param>
        /// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
        public List<AccordionItem> GetItems(int secondsTimeout, int maxItems, bool favorNewItems)
        {
            var result = new List<AccordionItem>();
            lock (lockItemList)
            {
                Hydrate();
                IEnumerable<AccordionItem> itemsSelect;
                if (favorNewItems)
                {
                    itemsSelect = ItemsInProgress
                        .Where(o => o.DeadLine < DateTime.Now)
                        .OrderBy(s => s.Attempts)
                        .Take(maxItems < 1 ? 1 : maxItems);
                }
                else
                {
                    itemsSelect = ItemsInProgress
                        .Where(o => o.DeadLine < DateTime.Now)
                        .OrderByDescending(s => s.Attempts)
                        .Take(maxItems < 1 ? 1 : maxItems);
                }
                if (itemsSelect != null)
                {
                    foreach (var item in itemsSelect)
                    {
                        item.Attempts++;
                        item.DeadLine = DateTime.Now.AddSeconds(secondsTimeout);

                        // create a new object for the return value
                        result.Add(new AccordionItem
                        {
                            Index = item.Index,
                            Attempts = item.Attempts,
                            DeadLine = item.DeadLine
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///  Retrieves a single AccordionItem requiring processing.  Content is null if none are available.
        /// </summary>
        /// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
        /// <param name="maxItems">The maximum number of items to retrieve (at least 1, and capped by the value of maximum in progress)</param>
        /// <param name="items">The list of Accordion Item objects to be returned.</param>
        /// <param name="favorNewItems">When true, higher attempts </param>
        /// <returns>The AccordionItem found for processing; false if no items are not available for processing.</returns>
        public AccordionItem GetItem(int secondsTimeout, bool favorNewItem)
        {
            AccordionItem result = null;
            lock (lockItemList)
            {
                var items = GetItems(secondsTimeout, 1, favorNewItem);
                if (items.Count > 0)
                {
                    result = items[0];
                }
            }
            return result;
        }

        /// <summary>
        /// Marks an item for retry (usually an item whose processing has failed).  Increments the attempts and resets the item as available.
        /// </summary>
        /// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
        public void RetryItem(Int64 itemIndex)
        {
            lock (lockItemList)
            {
                var thisItem = ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault();
                if (thisItem != null)
                {
                    thisItem.DeadLine = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Resets the deadlne for an item.  Does not change the number of attempts.
        /// </summary>
        /// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
        /// <param name="deadline">The new time when the item again becomes available via GetItem().  Note: the time
        /// must be in the future, or no action is taken.</param>
        public void ExtendItemTimeout(Int64 itemIndex, DateTime deadline)
        {
            if (deadline <= DateTime.Now) return;
            lock (lockItemList)
            {
                var thisItem = ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault();
                if (thisItem != null)
                {
                    thisItem.DeadLine = deadline;
                }
            }
        }

        /// <summary>
        /// Removes an item from the in-progress list (for an item whose need for processing is no longer required).
        /// </summary>
        /// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
        public void CompleteItem(Int64 itemIndex)
        {
            lock (lockItemList)
            {
                var thisItem = ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault();
                if (thisItem != null)
                {
                    ItemsInProgress.Remove(thisItem);
                }
                Hydrate();
            }
        }

        /// <summary>
        /// Return the number of items remaining in the in-progress list.
        /// If 0, then all items are completed.
        /// If less than the max in progress, no new items are left to add to the in-progresss list.
        /// Otherwise, the value will match MaxInProgress.
        /// </summary>
        /// <returns>int: the number of items still in progress.</returns>
        public int GetInProgressCount()
        {
            lock (lockItemList)
            { 
                Hydrate();
                return ItemsInProgress.Count;
            }
        }

        /// <summary>
        /// Returns the percentage of items which have completed processing.  The value is %###.##, represented as in integer.
        /// E.g. 5001 represents 50.01%
        /// </summary>
        /// <returns>int: the percentage of total items no longer in progress.</returns>
        public int GetPercentageComplete()
        {
            lock (lockItemList)
            {
                Hydrate();
                return (int) ((Int64) 100 * (IndexEnd - (IndexStart + IndexOffset - ItemsInProgress.Count)) / (IndexStart + IndexEnd));
            }
        }

        /// <summary>
        /// Return the number of items remaining in the in-progress list.  If 0, then all items are completed.
        /// </summary>
        /// <returns>int: the number of items still in progress.</returns>
        public bool IsFinished()
        {
            return (GetInProgressCount() == 0);
        }

        private void Hydrate()
        {
            // add items to maximize buffer availability.
            while (IndexStart + IndexOffset < IndexEnd && ItemsInProgress.Count < MaxInProgress)
            {
                ItemsInProgress.Add(new AccordionItem
                {
                    Index = IndexStart + IndexOffset
                });
                IndexOffset++;
            }
        }
    }
}
