using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOG.SwissArmyKnife.Entity
{
	/// <summary>
	/// MegaAccordion is a simplified version of Accordion from BOG.SwissArmyKnife, customized for use here.
	/// It keeps a window list of active items, in a much bigger ordinal list of nearly unlimited items.
	/// Don't use this object directly: use the static methods in MegaAccordionHelper.
	/// </summary>
	[JsonObject]
	public class MegaAccordion<T>
	{
		/// <summary>
		/// The signature for the work 
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Signature")]
		public string Signature { get; set; } = string.Empty;

		/// <summary>
		/// The level for the work. And index to Levels: determines the size of the static and mutuable levels.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Level")]
		public int Level { get; set; } = 0;

		[JsonProperty(Required = Required.Always, PropertyName = "State")]
		public ProcessState State { get; set; } = ProcessState.Active;

		/// <summary>
		/// The levels for which the work is defined.  Entry, and entry before it (if any), define the number of static indexes, then the number of mutable indexes.
		/// Example:  The Argumentitems.Count is 22, and the Levels are defined as: int[] { 5, 10, 7 };
		///   at Level 0, the first five indexes are mutable
		///   at Level 1, the first five indexes are static, then the next 10 are mutable
		///   at Level 2, the first fifteen indexes are static, then the final seven are mutable.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Level")]
		public int[] Levels { get; set; } = new int[] { 5, 10, 7 };

		/// <summary>
		/// The indexes are the offsets of the iteration items.  Note: only the indexes which are mutable (determined by level) will be iterated.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Indexes")]
		public long[] Indexes { get; set; } = default;

		/// <summary>
		/// The Iteration object for the parameter combinations. NOTE: StaticOffset.Count + StaticOffset.Count must be <= ArgumentItems.Count 
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Iterations")]
		public Dictionary<int, ArgumentItem> ArgumentItems { get; set; } = null;

		/// <summary>
		/// The maximum number of items which can be actively tracked at any given time.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
		public int MaxInProgress { get; set; } = 0;

		/// <summary>
		/// A timeout period in ticks for the MagAccordionItem to be completed, before it is allowed to be reissued.
		/// This time includes not only processing time itself, but lag time in queues or other delays.
		/// Default is five minutes.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "MaxInProgress")]
		public long TimeoutTicks { get; set; } = TimeSpan.FromMinutes(5).Ticks;

		/// <summary>
		/// The list of items currently available for work.  The key is the item's index for all iteration items.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "ItemsInProgress")]
		public Dictionary<string, MegaAccordionItem<T>> ItemsInProgress { get; set; } = new Dictionary<string, MegaAccordionItem<T>>();

		/// <summary>
		/// The list of forecasts.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Forecasts")]
		public List<Forecast> Forecasts { get; set; } = new List<Forecast>();

		/// <summary>
		/// The list of forecasts.
		/// </summary>
		[JsonProperty(Required = Required.Always, PropertyName = "Forecasts")]
		public List<ItemError> ItemErrors { get; set; } = new List<ItemError>();

		/// <summary>
		/// Deserialization instantiator
		/// </summary>
		public MegaAccordion()
		{
		}

		#region Support Methods

		private readonly object lockItemList = new object();

		private bool isValidated = false;
		private int staticLength = 0;
		private int mutableStart = 0;
		private int mutableLength = 0;

		/// <summary>
		/// Called at any time to ensure the object is valid: must call immediately after deserialization.
		/// Message is hydrated if the object is not valid, and will list only the first problem detected.
		/// </summary>
		/// <returns></returns>
		public void Validate()
		{
			if (State != ProcessState.Active)
			{
				throw new InvalidOperationException($"The state must be set to Active or Sunsetting, but is set to {State}");
			}
			if (ItemsInProgress.Keys.Count > MaxInProgress)
			{
				throw new InvalidOperationException($"ItemsInProgress count {ItemsInProgress.Keys.Count} is greater than allowed MaxInProgress of {MaxInProgress}");
			}
			else if (Indexes.Length != ArgumentItems.Count)
			{
				throw new InvalidOperationException($"Offset count of {Indexes.Length} does not equal the ArgumentItems count of {ArgumentItems.Keys.Count}");
			}
			else if (Level < 0 || Level >= Levels.Length)
			{
				throw new InvalidOperationException($"Level value of {Level} must be less than the number of levels: {Levels.Length}");
			}
			// Are the indexes within the boundries.
			for (int index = 0; index <= Indexes.Length; index++)
			{
				if (Indexes[index] < 0)
				{
					throw new InvalidOperationException($"The value {Indexes[index]} in Indexes[{index}] can not be negative.");
				}
				if (Indexes[index] >= ArgumentItems[index].Items.Length)
				{
					throw new InvalidOperationException($"The value {Indexes[index]} in Indexes[{index}] is out of bounds for {ArgumentItems[index].Name} ({ArgumentItems[index].Items.Length} items)");
				}
			}
			for (var index = 0; index < Level; index++)
			{
				staticLength += Levels[index];
			}
			mutableStart = staticLength;
			mutableLength = Levels.Length == 0 ? ArgumentItems.Count : Levels[Level];

			isValidated = true;
		}

		/// <summary>
		/// Resets the ItemsInProgress indexes for the mutuable indexes only.
		/// </summary>
		/// <param name="accordion"></param>
		public void ResetMegaAccordion(MegaAccordion<T> accordion)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				ItemsInProgress = new Dictionary<string, MegaAccordionItem<T>>();
				Forecasts = new List<Forecast>();
				Indexes = new long[ArgumentItems.Count];
				for (var index = staticLength; index < mutableLength; index++)
				{
					Indexes[index] = 0L;
				}
				State = ProcessState.Active;
			}
		}

		/// <summary>
		/// Retrieves a Metasetitem requiring processing.
		/// </summary>
		/// <param name="secondsTimeout">The number of seconds to allow for completion before the item can be reissued.</param>
		/// <param name="favorNew">True to return any new items, ahead of retry items.</param>
		/// <param name="availableItem" >(OUT) The object found for the item, or null (default).  An item first issued is always default/null.</param>
		/// <returns>The ordinal index (zero-based) of the item to be processed, or -1 if nothing is available.</returns>
		/// <remarks>item will only reflect the last value when UpdateItem() was called.</remarks>
		public bool TryGetWorkItem(int secondsTimeout, bool favorNew, out MegaAccordionItem<T> availableItem)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var result = false;
				Hydrate();
				if (favorNew)
				{
					availableItem = ItemsInProgress.Values
							.Where(o => o.DateAvailableTicks <= DateTime.Now.Ticks)
							.OrderByDescending(s => s.DateAvailableTicks)
							.FirstOrDefault();
				}
				else
				{
					availableItem = ItemsInProgress.Values
							.Where(o => o.DateAvailableTicks <= DateTime.Now.Ticks)
							.OrderBy(s => s.DateAvailableTicks)
							.FirstOrDefault();
				}
				if (availableItem != null)
				{
					var index = ItemsInProgress[availableItem.Key].Key;
					availableItem = ItemsInProgress[availableItem.Key];
					ItemsInProgress[availableItem.Key].DateAvailableTicks = DateTime.Now.AddSeconds(secondsTimeout).Ticks;
					result = true;
				}
				return result;
			}
		}

		/// <summary>
		/// Sets the payload for an item.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Metaset item to be processed.</param>
		public void UpdateItem(string key, T payload)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var thisItem = ItemsInProgress.Values.Where(o => o.Key == key).FirstOrDefault();
				if (thisItem != null)
				{
					ItemsInProgress[thisItem.Key].Value = payload;
				}
			}
		}

		/// <summary>
		/// Marks an item for retry (usually an item whose processing has failed or timed-out).
		/// </summary>
		/// <param name="itemIndex">The index property value of the Metaset item to be processed.</param>
		public void RetryItem(string key)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var thisItem = ItemsInProgress.Values.Where(o => o.Key == key).FirstOrDefault();
				if (thisItem != null)
				{
					ItemsInProgress[thisItem.Key].DateAvailableTicks = DateTime.MinValue.Ticks;
				}
			}
		}

		/// <summary>
		/// Resets the deadlne for an item.  Does not change the number of attempts.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Metaset item to be processed.</param>
		/// <param name="availableOn">The new time when the item again becomes available via GetItem().  Note: the time
		/// must be in the future, or no action is taken.</param>
		public void ExtendItemDeadline(string key, DateTime availableOn)
		{
			if (availableOn <= DateTime.Now) return;
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var thisItem = ItemsInProgress.Values.Where(o => o.Key == key).FirstOrDefault();
				if (thisItem != null)
				{
					ItemsInProgress[thisItem.Key].DateAvailableTicks = availableOn.Ticks;
				}
			}
		}

		/// <summary>
		/// Removes an item from the in-progress list (for an item whose need for processing is no longer required).
		/// This is regardless of the items processing result.
		/// </summary>
		/// <param name="itemIndex">The index property value of the Metaset item to be processed.</param>
		public void CompleteItem(string key)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var thisItem = ItemsInProgress.Values.Where(o => o.Key == key).FirstOrDefault();
				if (thisItem != null)
				{
					ItemsInProgress.Remove(thisItem.Key);
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
				if (!isValidated) Validate();
				Hydrate();
				return ItemsInProgress.Keys.Count;
			}
		}

		/// <summary>
		/// Have all items have been processed?
		/// </summary>
		/// <returns>Return true if so.</returns>
		public bool ProcessingCompleted()
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var result = true;
				switch (State)
				{
					case ProcessState.Active:
					case ProcessState.Sunsetting:
						result = false;
						break;
				}
				return result;
			}
		}

		/// <summary>
		/// Adds a new list of forecasts to the existing set of forecasts, and prunes the list for the TOP (prunedSize)
		/// items when the maxSize is exceeded.
		/// </summary>
		/// <param name="forecasts"></param>
		/// <param name="maxSize"></param>
		/// <param name="prunedSize"></param>
		public void AddForecasts(List<Forecast> forecasts, int maxSize, int prunedSize)
		{
			if (maxSize <= 0)
			{
				throw new ArgumentException($"The maxSize argument must be greater then zero.");
			}
			if (prunedSize <= 0)
			{
				throw new ArgumentException($"The prunedSize argument must be greater then zero.");
			}
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				foreach (var item in forecasts)
				{
					Forecasts.Add(item);
				}
			}
			if (Forecasts.Count >= maxSize)
			{
				PruneForecasts(prunedSize);
			}
		}

		/// <summary>
		/// Adds a new list of forecasts to the existing set of forecasts, and prunes the list for the TOP (prunedSize)
		/// items when the maxSize is exceeded.
		/// </summary>
		/// <param name="prunedSize"></param>
		public void PruneForecasts(int prunedSize)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				Forecasts = Forecasts
					.OrderByDescending(x => x.Ranking)
					.Take(prunedSize)
					.Select(y => y)
					.ToList();
			}
		}

		public void Save(string fileName, int prunedSize)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				PruneForecasts(prunedSize);
				BOG.ResearchRunner.Common.Helper.Converter<MegaAccordion<T>>.ToJsonFile(this, fileName);
			}
		}

		/// <summary>
		/// Adds a new list of errors to the existing set of ItemErrors, up to the maximum allowed.
		/// If maximum allowed is exceeded, the state is set to MaxErrorsExceeded, and the remaining errors beyond the max are discarded.
		/// </summary>
		/// <param name="itemErrors"></param>
		/// <param name="failErrorLimit"></param>
		public void AddItemErrors(List<ItemError> itemErrors, int failErrorLimit)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				foreach (var item in itemErrors)
				{
					if (ItemErrors.Count >= failErrorLimit)
						break;
					ItemErrors.Add(item);
				}
				if (ItemErrors.Count >= failErrorLimit)
				{
					State = ProcessState.MaxErrorsExceeded;
				}
			}
		}

		private void Hydrate()
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				// add items to maximize buffer availability.
				while (ItemsInProgress.Count < MaxInProgress && State == ProcessState.Active)
				{
					var thisKey = BuildKeyFromIndexes();
					ItemsInProgress.Add(thisKey, new MegaAccordionItem<T>
					{
						DateAvailableTicks = DateTime.Now.Ticks + TimeoutTicks,
						Key = thisKey
					});
					Increment();
				}
			}
		}

		private void Increment()
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();

				for (var index = mutableStart + mutableLength - 1; State == ProcessState.Active && index >= mutableStart; index--)
				{
					if (Indexes[index] == ArgumentItems.Count - 1)
					{
						Indexes[index] = 0;
						if (index > mutableStart) continue;
						State = ProcessState.Sunsetting;
						break;
					}
					Indexes[index]++;
					break;
				}
			}
		}

		private decimal GetPercentageComplete(bool mutableOnly)
		{
			string result;

			int offset = mutableOnly ? mutableStart : 0;
			int offsetEnd = mutableLength;

			if (offsetEnd - offset == 1)
			{
				result = string.Format("{0:m}", 100.0m * (decimal)Indexes[offset] / (decimal)ArgumentItems[offset].Items.Length);
			}
			else
			{
				result = ".";
				while (offset < offsetEnd)
				{
					var digit = (int)((Indexes[offset]) / (ArgumentItems[offset].Items.Length / 10));
					if (digit > 9) digit = 9;
					result += $"{digit}";
				}
				result = string.Format("{0:m}", (decimal)((long)(decimal.Parse(result) * 10000m)/100m));
			}

			return decimal.Parse(result);
		}

		private string BuildKeyFromIndexes()
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var result = new StringBuilder();
				for (var index = 0; index <= staticLength + mutableLength - 1; index++)
				{
					if (result.Length > 0) result.Append(":");
					result.Append(string.Format("{0:x", Indexes[index]));
				}
				return result.ToString();
			}
		}

		public long[] BuildIndexesFromKey(string key)
		{
			lock (lockItemList)
			{
				if (!isValidated) Validate();
				var result = new long[Indexes.Length];
				var parts = key.Split(new char[] { ':' });
				if (parts.Length != Indexes.Length)
				{
					throw new InvalidOperationException($"Invalid key: requires {Indexes.Length} index values, but only has {parts.Length}");
				}
				for (var index = mutableStart + mutableLength - 1; State == ProcessState.Active && index >= mutableStart; index--)
				{
					result[index] = long.Parse("0x" + parts[index]);
				}
				return result;
			}
		}

		#endregion
	}
}
