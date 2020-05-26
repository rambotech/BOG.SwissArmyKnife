using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BOG.SwissArmyKnife
{
	/// <summary>
	/// Defines the Accordion for tracking the collection of AccordionItems, and the associated methods.
	/// </summary>
	[JsonObject]
	public class Accordion<T>
	{
		/// <summary>
		/// Deserialization instantiator
		/// </summary>
		public Accordion()
		{
		}

		/// <summary>
		/// Explicit instantiator
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

		// Metrics
		/// <summary>
		/// The list of items currently available for work.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Items")]
		public Dictionary<Int64, AccordionItem<T>> ItemsInProgress { get; set; } = new Dictionary<long, AccordionItem<T>>();

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

		private readonly object lockItemList = new object();

		#region Helper methods

		/// <summary>
		///  Retrieves a set of AccordionItems requiring processing, up to the MaxItemsInProgress limit.
		/// </summary>
		/// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
		/// <param name="maxItems">The maximum number of items to retrieve (at least 1, and capped by the value of maximum in progress)</param>
		/// <param name="favorNew">True to return any new items, ahead of retry items.</param>
		/// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
		public List<AccordionItem<T>> GetItems(int secondsTimeout, int maxItems, bool favorNew)
		{
			var result = new List<AccordionItem<T>>();
			lock (lockItemList)
			{
				Hydrate();
				IEnumerable<AccordionItem<T>> itemsSelect;
				if (favorNew)
				{
					itemsSelect = ItemsInProgress.Values
							.Where(o => o.AvailableOn <= DateTime.Now)
							.OrderBy(s => s.IssueHistory.Count)
							.Take(maxItems < 1 ? 1 : maxItems);
				}
				else
				{
					itemsSelect = ItemsInProgress.Values
							.Where(o => o.AvailableOn <= DateTime.Now)
							.OrderByDescending(s => s.IssueHistory.Count)
							.Take(maxItems < 1 ? 1 : maxItems);
				}
				if (itemsSelect != null)
				{
					foreach (var item in itemsSelect)
					{
						item.AvailableOn = DateTime.Now.AddSeconds(secondsTimeout);
						item.IssueHistory.Add(DateTime.Now);
						// create a new object for the return value
						result.Add(new AccordionItem<T>
						{
							Index = item.Index,
							AvailableOn = item.AvailableOn,
							IssueHistory = new List<DateTime>(item.IssueHistory),
							Payload = item.Payload
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
		/// <param name="favorNew">True to return any new items, ahead of retry items.</param>
		/// <returns>The AccordionItem found for processing; false if no items are not available for processing.</returns>
		public AccordionItem<T> GetItem(int secondsTimeout, bool favorNew)
		{
			AccordionItem<T> result = null;
			lock (lockItemList)
			{
				var items = GetItems(secondsTimeout, 1, favorNew);
				if (items.Count > 0)
				{
					result = items[0];
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the payload for an item.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		public void GetItemPayload(Int64 itemIndex, T payload)
		{
			lock (lockItemList)
			{
				if (ItemsInProgress.Keys.Contains(itemIndex))
				{
					ItemsInProgress[itemIndex].Payload = payload;
				}
			}
		}

		/// <summary>
		/// Sets the payload for an item.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		public void SetItemPayload(Int64 itemIndex, T payload)
		{
			lock (lockItemList)
			{
				if (ItemsInProgress.Keys.Contains(itemIndex))
				{
					ItemsInProgress[itemIndex].Payload = payload;
				}
			}
		}

		/// <summary>
		/// Marks an item for retry (usually an item whose processing has failed).
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		public void RetryItem(Int64 itemIndex)
		{
			lock (lockItemList)
			{
				if (ItemsInProgress.Keys.Contains(itemIndex))
				{
					ItemsInProgress[itemIndex].AvailableOn = DateTime.MinValue;
				}
			}
		}

		/// <summary>
		/// Resets the deadlne for an item.  Does not change the number of attempts.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		/// <param name="availableOn">The new time when the item again becomes available via GetItem().  Note: the time
		/// must be in the future, or no action is taken.</param>
		public void ExtendItemTimeout(Int64 itemIndex, DateTime availableOn)
		{
			if (availableOn <= DateTime.Now) return;
			lock (lockItemList)
			{
				if (ItemsInProgress.Keys.Contains(itemIndex))
				{
					ItemsInProgress[itemIndex].AvailableOn = availableOn;
				}
			}
		}

		/// <summary>
		/// Removes an item from the in-progress list (for an item whose need for processing is no longer required).
		/// This is regardless of the items processing result.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		public void CompleteItem(Int64 itemIndex)
		{
			lock (lockItemList)
			{
				if (ItemsInProgress.Keys.Contains(itemIndex))
				{
					ItemsInProgress.Remove(itemIndex);
				}
			}
		}

		/// <summary>
		/// Return the number of items remaining in the in-progress list.
		/// If 0, then all items are completed.
		/// If less than the max in progress, no new items are left to add to the in-progresss list (i.e. Sunsetting).
		/// Otherwise, the value will match MaxInProgress.
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
		/// Returns the percentage of items which have completed processing.  The value is %###.##, represented as in integer.
		/// E.g. 5001 represents 50.01%
		/// </summary>
		/// <returns>decimal: the percentage of total items no longer in progress.  Divide by 100 for a percentage</returns>
		public decimal GetPercentageComplete()
		{
			lock (lockItemList)
			{
				Hydrate();
				return (decimal)((int)((Int64)100 * (IndexEnd - (IndexStart + IndexOffset - ItemsInProgress.Keys.Count)) / (IndexStart + IndexEnd)) / 100.0);
			}
		}

		/// <summary>
		/// Returns a dictionary where the key is the number of timeouts, and the value is the count of items with that number of timeouts.
		/// This answer can help the caller determine if a processing bottleneck is occurring.
		/// </summary>
		/// <returns>Dictionary&lt;int,int&gt;: key == timeout occurrences, value == count of items with it.</returns>
		public Dictionary<int, int> GetTimeoutCountSummary()
		{
			var result = new Dictionary<int, int>();
			var query = ItemsInProgress.Values.GroupBy(
					o => (int)Math.Floor((float)o.IssueHistory.Count),
					o => o.Index,
					(timeoutCount, indexes) => new
					{
						key = timeoutCount,
						count = indexes.Count()
					});

			foreach (var timeoutCount in query)
			{
				result.Add(timeoutCount.key, timeoutCount.count);
			}
			return result;
		}

		/// <summary>
		/// Are all items have been processed?
		/// </summary>
		/// <returns>Return true if so.</returns>
		public bool IsFinished()
		{
			return (GetInProgressCount() == 0);
		}

		private void Hydrate()
		{
			// add items to maximize buffer availability.
			while (IndexStart + IndexOffset < IndexEnd && ItemsInProgress.Count < MaxInProgress)
			{
				ItemsInProgress.Add(
					IndexStart + IndexOffset,
					new AccordionItem<T>
					{
						Index = IndexStart + IndexOffset
					});
				IndexOffset++;
			}
		}
		#endregion
	}
}
