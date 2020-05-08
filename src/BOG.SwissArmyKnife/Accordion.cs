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
		public List<AccordionItem<T>> ItemsInProgress { get; set; } = new List<AccordionItem<T>>();

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
		/// <param name="favorNewItems">When true, higher attempts </param>
		/// <returns>true if an item for processing exists; false if no items are not available for processing.</returns>
		public List<AccordionItem<T>> GetItems(int secondsTimeout, int maxItems, bool favorNewItems)
		{
			var result = new List<AccordionItem<T>>();
			lock (lockItemList)
			{
				Hydrate();
				IEnumerable<AccordionItem<T>> itemsSelect;
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
						if (item.DeadLine != DateTime.MinValue)
						{
							item.Timeouts++;
						}
						else
						{
							item.Attempts++;
						}
						item.DeadLine = DateTime.Now.AddSeconds(secondsTimeout);
						// create a new object for the return value
						result.Add(new AccordionItem<T>
						{
							Index = item.Index,
							Attempts = item.Attempts,
							Timeouts = item.Timeouts,
							DeadLine = item.DeadLine,
							State = item.State,
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
		/// <param name="favorNewItems">When true, higher attempts </param>
		/// <returns>The AccordionItem found for processing; false if no items are not available for processing.</returns>
		public AccordionItem<T> GetItem(int secondsTimeout, bool favorNewItems)
		{
			AccordionItem<T> result = null;
			lock (lockItemList)
			{
				var items = GetItems(secondsTimeout, 1, favorNewItems);
				if (items.Count > 0)
				{
					result = items[0];
				}
			}
			return result;
		}

		/// <summary>
		/// Sets the state for an item.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion itemd.</param>
		/// <param name="state">The AccordionItemState enum value to assign to the Accordion item.</param>
		/// <remarks>The item reference by itemIdex must be in the in-processing list, or an exception is thrown.</remarks>
		public void SetItemState(Int64 itemIndex, AccordionItemState state)
		{
			lock (lockItemList)
			{
				ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault().State = state;
			}
		}

		/// <summary>
		/// Sets the state for an item.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Accordion item to be processed.</param>
		public void SetItemPayload(Int64 itemIndex, T payload)
		{
			lock (lockItemList)
			{
				var thisItem = ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault();
				if (thisItem != null)
				{
					thisItem.Payload = payload;
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
				var thisItem = ItemsInProgress.Where(o => o.Index == itemIndex).FirstOrDefault();
				if (thisItem != null)
				{
					thisItem.DeadLine = DateTime.MinValue;
					thisItem.Timeouts = 0;
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
		/// This is regardless of the items processing result.
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
				return (int)((Int64)100 * (IndexEnd - (IndexStart + IndexOffset - ItemsInProgress.Count)) / (IndexStart + IndexEnd));
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
			var query = ItemsInProgress.GroupBy(
					o => (int)Math.Floor((float)o.Timeouts),
					o => o.Index,
					(baseTimeout, indexes) => new
					{
						key = baseTimeout,
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

		/// <summary>
		/// Are all items in progress beyond their allowed retry count?
		/// Deadlock can be a fatal condition, or just be used to generate warnings.  This is just to detect state.
		/// </summary>
		/// <returns>Return true if so.</returns>
		public bool IsDeadlocked(int maxAttemptsAllowed)
		{
			return ItemsInProgress
				.Where(o => o.Attempts >= maxAttemptsAllowed)
				.Count() == ItemsInProgress.Count;
		}

		private void Hydrate()
		{
			// add items to maximize buffer availability.
			while (IndexStart + IndexOffset < IndexEnd && ItemsInProgress.Count < MaxInProgress)
			{
				ItemsInProgress.Add(new AccordionItem<T>
				{
					Index = IndexStart + IndexOffset
				});
				IndexOffset++;
			}
		}

		#endregion
	}
}
