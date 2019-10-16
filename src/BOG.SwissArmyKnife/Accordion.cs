using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife
{
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

    [JsonObject]
    public class Accordion
    {
        [JsonProperty(Required = Required.Always, PropertyName = "Items")]
        public List<AccordionItem> ItemsInProgress { get; set; } = new List<AccordionItem>();

        [JsonProperty(Required = Required.Always, PropertyName = "IndexStart")]
        public Int64 IndexStart { get; set; }  = 0;

        [JsonProperty(Required = Required.Always, PropertyName = "IndexEnd")]
        public Int64 IndexEnd { get; set; } = 0;

        [JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
        public Int64 MaxInProgress { get; set; } = 0;

        // Instance variables        
        [JsonProperty(Required = Required.Always, PropertyName = "IndexOffset")]
        public Int64 IndexOffset { get; private set; } = 0;

        private object lockItemList = new object();

        public Accordion()
        {

        }

        public bool GetItem(int secondsTimeout, out AccordionItem item)
        {
            var result = false;
            item = null;
            lock (lockItemList)
            {
                while (IndexStart + IndexOffset <= IndexEnd && ItemsInProgress.Count < MaxInProgress)
                {
                    ItemsInProgress.Add(new AccordionItem
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

        public bool RecycleItem(AccordionItem item)
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
