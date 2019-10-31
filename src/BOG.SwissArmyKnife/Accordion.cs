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
        public Dictionary<Int64, AccordionItem> ItemsInProgress { get; set; } = new Dictionary<long, AccordionItem>();

        /// <summary>
        /// The first item number to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexStart")]
        public Int64 IndexStart { get; set; } = 0;

        /// <summary>
        /// The final item number to be worked
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "IndexEnd")]
        public Int64 IndexEnd { get; set; } = 0;

        /// <summary>
        /// The maximum number of items which can be actively tracked at any given time.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
        public int MaxInProgress { get; set; } = 0;

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
        /// <param name="count">the number of items to process.</param>
        /// <param name="maxInProgress">the maximum number of items which can actively tracked.</param>
        public Accordion(Int64 indexStart, Int64 count, int maxInProgress)
        {
            if (indexStart < 0) throw new ArgumentException("indexStart must be >= 0");
            if (count <= 0) throw new ArgumentException("count must be >= 0");
            if (maxInProgress < 10) throw new ArgumentException("maxInProgress must be >= 10");
            if (maxInProgress > 20000) throw new ArgumentException("maxInProgress must be <= 20,000");
            IndexStart = indexStart;
            IndexEnd = indexStart + (Int64)count;
            MaxInProgress = maxInProgress;
        }

        /// <summary>
        ///  Retrieves an AccordionItem requiring processing.
        /// </summary>
        /// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
        /// <param name="item">The Accordion Item object to be returned.</param>
        /// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
        public bool GetItem(int secondsTimeout, out AccordionItem item)
        {
            var result = false;
            item = null;
            lock (lockItemList)
            {
                Hydrate();
                var itemSelect = ItemsInProgress.Values
                    .Where(o => o.DeadLine < DateTime.Now)
                    .OrderBy(s => s.DeadLine)
                    .FirstOrDefault();
                result = itemSelect != null;
                if (result)
                {
                    ItemsInProgress[itemSelect.Index].Attempts++;
                    ItemsInProgress[itemSelect.Index].DeadLine = DateTime.Now.AddSeconds(secondsTimeout);

                    // create a new object for the return value
                    item = new AccordionItem
                    {
                        Index = itemSelect.Index,
                        Attempts = itemSelect.Attempts,
                        DeadLine = itemSelect.DeadLine
                    };
                }
            }
            return result;
        }

        /// <summary>
        ///  Retrieves a set of AccordionItems requiring processing.
        /// </summary>
        /// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
        /// <param name="maxItems">The maximum number of items to retrieve.</param>
        /// <param name="items">The list of Accordion Item objects to be returned.</param>
        /// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
        public bool GetItem(int secondsTimeout, int maxItems, out List<AccordionItem> items)
        {
            var result = false;
            items = new List<AccordionItem>();
            lock (lockItemList)
            {
                Hydrate();
                var itemsSelect = ItemsInProgress.Values
                    .Where(o => o.DeadLine < DateTime.Now)
                    .OrderBy(s => s.DeadLine)
                    .Take(maxItems < 1 ? 1 : maxItems);
                result = itemsSelect != null;
                if (result)
                {
                    foreach (var item in itemsSelect)
                    {
                        ItemsInProgress[item.Index].Attempts++;
                        ItemsInProgress[item.Index].DeadLine = DateTime.Now.AddSeconds(secondsTimeout);

                        // create a new object for the return value
                        items.Add(new AccordionItem
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
        /// Marks an item for retry (usually an item whose processing has failed).
        /// </summary>
        /// <param name="item">The Accordion Item object to be processed.</param>
        /// <returns>true if the item was found; false if no item was found to update.</returns>
        public bool RetryItem(AccordionItem item)
        {
            var result = false;
            lock (lockItemList)
            {
                result = ItemsInProgress.Keys.Contains(item.Index);
                if (result)
                {
                    ItemsInProgress[item.Index].Attempts++;
                    ItemsInProgress[item.Index].DeadLine = DateTime.MinValue;
                }
            }
            return result;
        }

        /// <summary>
        /// Resets the timeout period for an item.  Does not change the number of attempts.
        /// </summary>
        /// <param name="item">The Accordion Item object to be processed.</param>
        /// <param name="deadline">The new time when the item again becomes available via GetItem().  Note: the time
        /// can be set to the past, to force an item to become immediately available.</param>
        /// <returns>true if the item was found; false if no item was found to update.</returns>
        public bool ChangeItemTimeout(AccordionItem item, DateTime deadline)
        {
            var result = false;
            lock (lockItemList)
            {
                result = ItemsInProgress.Keys.Contains(item.Index);
                if (result)
                {
                    ItemsInProgress[item.Index].DeadLine = deadline;
                }
            }
            return result;
        }

        /// <summary>
        /// Resets the timeout period for all items.  Does not change the number of attempts.
        /// </summary>
        /// <param name="deadline">The new time when the item again becomes available via GetItem().  Note: the time
        /// can be set to the past, to force all items to become immediately available.</param>
        /// <returns>true if the item was found; false if no item was found to update.</returns>
        public bool ChangeAllItemTimeouts(DateTime deadline)
        {
            var result = false;
            lock (lockItemList)
            {
                foreach (var key in ItemsInProgress.Keys)
                {
                    ItemsInProgress[key].DeadLine = deadline;
                }
            }
            return result;
        }

        /// <summary>
        /// Marks all or a set of items for retry. (usually done when resetting all items in progress to allow immediate reissue for processing).
        /// </summary>
        /// <param name="item">The Accordion Item object to be processed.</param>
        /// <returns>true if the item was found; false if no item was found to update.</returns>
        public void RetryItems(bool resetAttempts)
        {
            lock (lockItemList)
            {
                foreach (var key in ItemsInProgress.Keys)
                {
                    if (resetAttempts)
                    {
                        ItemsInProgress[key].Attempts++;
                    }
                    ItemsInProgress[key].DeadLine = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Removes an item from the in-progress list (for an item whose processing is complete, or is being expired).
        /// </summary>
        /// <param name="item">The Accordion Item object to be processed.</param>
        /// <returns>true if the item was found; false if no item was found to update.</returns>
        public bool CompleteItem(AccordionItem item)
        {
            lock (lockItemList)
            {
                var result = ItemsInProgress.Keys.Contains(item.Index);
                if (result)
                {
                    ItemsInProgress.Remove(item.Index);
                }
                return result;
            }
        }

        /// <summary>
        /// Return the number of items remaining in the in-progress list.  If 0, then all items are completed.
        /// </summary>
        /// <returns>int: the number of items still in progress.</returns>
        public int GetInProgressCount()
        {
            lock (lockItemList)
            {
                Hydrate();
                return ItemsInProgress.Keys.Count;
            }
        }

        /// <summary>
        /// Return the number of items remaining in the in-progress list.  If 0, then all items are completed.
        /// </summary>
        /// <returns>int: the number of items still in progress.</returns>
        public bool IsFinished()
        {
            lock (lockItemList)
            {
                Hydrate();
                return (IndexStart + IndexOffset == IndexEnd) && ItemsInProgress.Keys.Count == 0;
            }
        }

        private void Hydrate()
        {
            if (ItemsInProgress.Count < MaxInProgress / 2)
            {
                // add items to mazimize buffer availability.
                while (IndexStart + IndexOffset < IndexEnd && ItemsInProgress.Count < MaxInProgress)
                {
                    ItemsInProgress.Add(IndexStart + IndexOffset, new AccordionItem
                    {
                        Index = IndexStart + IndexOffset
                    });
                    IndexOffset++;
                }
            }
        }
    }
}
