using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BOG.SwissArmyKnife
{
	/// <summary>
	/// Allows Parameter sets to be added for loops, and allows the loop enumerations 
	/// to be represented by an index, and vice-versa.  Intended to support large and 
	/// deep looping with millions to even trillions of combinations.
	/// </summary>
	[JsonObject]
	public class Iteration
	{
		/// <summary>
		/// EndValueEval is for numeric sequences, and specifies how the range limit value is handled.
		/// </summary>
		public enum EndValueEval : int
		{
			/// <summary>
			/// The range limit should be used      ( 1 &lt;= x &lt;= Value )
			/// </summary>
			Inclusive = 0,
			/// <summary>
			/// The range limit should not be used  ( 1 &lt;= x &lt; Value )
			/// </summary>
			Exclusive = 1
		}

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty]
		public Dictionary<int, IterationItem> IterationItems { get; set; } = new Dictionary<int, IterationItem>();

		public Int64 TotalIterationCount
		{
			get
			{
				if (_TotalIterationCount == -1L)
				{
					RecalculateTotalItems();
				}
				return _TotalIterationCount;
			}
		}

		private Int64 _TotalIterationCount = -1L;

		/// <summary>
		/// Creates a default instantiation.
		/// </summary>
		public Iteration()
		{

		}

		private void RecalculateTotalItems()
		{
			Int64 tally = 0L;
			foreach (var item in IterationItems.Values)
			{
				switch (item.HandleAs)
				{
					case IterationItem.Handling.OrdinalNumber:
						tally = tally == 0 ? item.NumericValueCount : tally * item.NumericValueCount;
						break;

					case IterationItem.Handling.Literal:
						tally = tally == 0 ? item.LiteralValues.Count : tally * item.LiteralValues.Count;
						break;
				}
			}
			_TotalIterationCount = tally;
		}

		#region Helper methods

		/// <summary>
		/// Gets whether an iteration with a specific name already exists in the iteration items.
		/// </summary>
		/// <param name="name"></param>
		/// <returns>false if not found</returns>
		private bool CheckIterationItemNameExists(string name)
		{
			return IterationItems.Values.Where(o => string.Compare(o.Name, name, false) == 0).Count() > 0;
		}

		/// <summary>
		/// Gets a list of the items associated with an iteration name.
		/// </summary>
		/// <param name="forName">the iteration name.</param>
		/// <returns>A dictionary of the items, with the key being the ordinal sequence (0...N)</returns>
		public Dictionary<int, string> GetIterationItemsForName(string name)
		{
			var result = new Dictionary<int, string>();
			if (!CheckIterationItemNameExists(name))
			{
				throw new ArgumentException($"{name} does not exist in the list of iteration items defined.");
			}

			foreach (int key in IterationItems.Keys)
			{
				if (IterationItems[key].Name != name)
				{
					continue;
				}
				switch (IterationItems[key].HandleAs)
				{
					case IterationItem.Handling.Literal:
						foreach (var i in IterationItems[key].LiteralValues.Keys)
						{
							result.Add(i, IterationItems[key].LiteralValues[i]);
						}
						break;

					case IterationItem.Handling.OrdinalNumber:
						var value = IterationItems[key].NumericStartValue;
						while (result.Values.Count < IterationItems[key].NumericValueCount)
						{
							result.Add(result.Count, value.ToString());
							value += IterationItems[key].NumericStepValue;
						}
						break;
				}
				break;
			}
			return result;
		}

		/// <summary>
		/// Add numbers by range and increment as an iteration item.  This is equivalent to:
		/// for (decimal value = initialValue, value &lt; limitValue, value += incrementValue)  // when increment is positive
		/// for (decimal value = initialValue, value &gt; limitValue, value -= incrementValue)  // when increment is negative
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="initialValue">the starting value</param>
		/// <param name="incrementValue">the step or increment to the next value</param>
		/// <param name="limitValue">the end value</param>
		/// <param name="endValueEval">whether the end value itself can be included in the list of items.</param>
		/// <returns>The number of items created for the iteration item.</returns>
		public int AddNumberRange(string name, decimal initialValue, decimal incrementValue, decimal limitValue, EndValueEval endValueEval)
		{
			if (CheckIterationItemNameExists(name))
			{
				throw new ArgumentException($"{name} already exists in the list of iteration items defined.");
			}

			if (Math.Sign(incrementValue) == 0)
			{
				throw new ArgumentException($"The increment value for \"{name}\" can not be zero.");
			}

			if (Math.Sign(incrementValue) == 1 && initialValue > limitValue)
			{
				throw new ArgumentException($"The increment value for \"{name}\" is invalid: it should be negative, but is positive.");
			}

			if (Math.Sign(incrementValue) == -1 && initialValue < limitValue)
			{
				throw new ArgumentException($"The increment value for \"{name}\" is invalid: it should be positive, but is negative.");
			}

			// decimal initialValue, decimal incrementValue, decimal limitValue, EndValueEval endValueEval
			var count = Math.Max(1, (int)(Math.Abs(limitValue - initialValue) / Math.Abs(incrementValue)));
			var modulo = Math.Abs(limitValue - initialValue) - ((decimal)count * Math.Abs(incrementValue));
			if (endValueEval == Iteration.EndValueEval.Inclusive && (limitValue != initialValue) && modulo == 0.0M) count++;

			int result = count;
			IterationItem item = new IterationItem
			{
				Name = name,
				HandleAs = IterationItem.Handling.OrdinalNumber,
				NumericStartValue = initialValue,
				NumericStepValue = incrementValue,
				NumericValueCount = (long)count,
				LiteralValues = null
			};
			IterationItems.Add(IterationItems.Count, item);
			RecalculateTotalItems();
			return result;
		}

		/// <summary>
		/// Add numbers by start and loop count.
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="initialValue">the starting value</param>
		/// <param name="incrementValue">the step or increment to the next value</param>
		/// <param name="iterationCount">the number of times to increment the value and add the new value to list.</param>
		/// <returns>The number of items created for the iteration item.</returns>
		public int AddNumberSequence(string name, decimal initialValue, decimal incrementValue, int iterationCount)
		{
			if (CheckIterationItemNameExists(name))
			{
				throw new ArgumentException($"{name} already exists in the list of iteration items defined.");
			}

			if (iterationCount <= 0)
			{
				throw new ArgumentException($"The iteration count for \"{name}\" can not be zero or negative.");
			}

			if (Math.Sign(incrementValue) == 0)
			{
				throw new ArgumentException($"The increment value for \"{name}\" can not be zero.");
			}

			int result = iterationCount;
			IterationItem item = new IterationItem
			{
				Name = name,
				HandleAs = IterationItem.Handling.OrdinalNumber,
				NumericStartValue = initialValue,
				NumericStepValue = incrementValue,
				NumericValueCount = iterationCount,
				LiteralValues = null
			};
			IterationItems.Add(IterationItems.Count, item);
			RecalculateTotalItems();
			return result;
		}

		/// <summary>
		/// Adds items from a list of items.  This allows non-sequence values to be added. Eg.
		/// { "91352", 66202", "34761" }   // zip codes
		/// { "Fred", "Harry", "Sally" }
		/// 
		/// </summary>
		/// <param name="name">The iteration item name</param>
		/// <param name="itemValues"></param>
		/// <returns></returns>
		public int AddListItems(string name, List<string> itemValues)
		{
			if (CheckIterationItemNameExists(name))
			{
				throw new ArgumentException($"{name} already exists in the list of iteration items defined.");
			}

			if (itemValues == null || itemValues.Count == 0)
			{
				throw new ArgumentException(string.Format("The list of items to add can not be empty.", name));
			}
			IterationItem item = new IterationItem
			{
				Name = name,
				HandleAs = IterationItem.Handling.Literal,
				LiteralValues = new Dictionary<int, string>()
			};
			foreach (string value in itemValues)
			{
				item.LiteralValues.Add(item.LiteralValues.Count, value);
			}
			int result = item.LiteralValues.Count;
			IterationItems.Add(IterationItems.Count, item);
			RecalculateTotalItems();
			return result;
		}

		/// <summary>
		/// Returns the items and their values for a particular index.
		/// </summary>
		/// <param name="indexSpecific">The zero-based index of the sequence.</param>
		/// <returns>A dictionary of strings where the key is the IterationItem name, and the value is its value for this index.</returns>
		public Dictionary<string, string> GetIterationValueSet(Int64 indexSpecific)
		{
			if (indexSpecific < 0)
			{
				throw new ArgumentException("The requested index can not be negative.");
			}
			if (indexSpecific >= TotalIterationCount)
			{
				throw new ArgumentException($"The requested index ({indexSpecific}) is beyond the maximum of {TotalIterationCount - 1}");
			}

			long index = indexSpecific;
			Dictionary<string, string> result = new Dictionary<string, string>();
			for (int ItemInSetIndex = IterationItems.Count - 1; ItemInSetIndex >= 0; ItemInSetIndex--)
			{
				Int64 whole = 0;
				int remainder;
				switch (IterationItems[ItemInSetIndex].HandleAs)
				{
					case IterationItem.Handling.Literal:
						whole = index / (Int64)IterationItems[ItemInSetIndex].LiteralValues.Count;
						remainder = (int)(index % (Int64)IterationItems[ItemInSetIndex].LiteralValues.Count);
						result.Add(IterationItems[ItemInSetIndex].Name, IterationItems[ItemInSetIndex].LiteralValues[remainder]);
						break;
					case IterationItem.Handling.OrdinalNumber:
						whole = index / (Int64)IterationItems[ItemInSetIndex].NumericValueCount;
						remainder = (int)(index % (Int64)IterationItems[ItemInSetIndex].NumericValueCount);
						result.Add(
							IterationItems[ItemInSetIndex].Name,
							(IterationItems[ItemInSetIndex].NumericStartValue +
							(IterationItems[ItemInSetIndex].NumericStepValue * (decimal)remainder)).ToString()
						);
						break;
				}
				index = whole;
			}

			return result;
		}
		#endregion
	}
}
