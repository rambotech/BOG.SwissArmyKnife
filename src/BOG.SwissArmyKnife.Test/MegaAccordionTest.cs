using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using BOG.SwissArmyKnife.Entity;
using BOG.SwissArmyKnife.Enums;
using NUnit.Framework.Constraints;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BOG.SwissArmyKnife.Test
{
	class MyMegaObject
	{
		public long Index { get; set; }
		public string Message { get; set; }
		public bool Succeeded { get; set; } = false;
	}

	[TestFixture]
	public class MegaAccordionTest
	{
		#region Initialization
		/// <summary>
		/// Validate that no arguments are rejected.
		/// </summary>
		[Test]
		public void Accordion_Init_NoArguments()
		{
			try
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "ArgumentItems must have at least one item in the list.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that all states other than Active and Sunsetting are invalid.
		/// </summary>
		[Test]
		public void Accordion_Init_InvalidState()
		{
			var invalidStates = new MegaAccordionState[] {
				MegaAccordionState.CompletedSuccessfully,
				MegaAccordionState.Deadlocked,
				MegaAccordionState.MaxErrorsExceeded,
				MegaAccordionState.UnexpectedError };
			var acc = MakeMegaAccordionTest_OneArgument_Valid();
			foreach (var state in invalidStates)
			{
				try
				{
					acc.State = state;
					acc.ResetMegaAccordion();
					Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
				}
				catch (ArgumentException ex)
				{
					Assert.That(ex.Message == $"The state must be set to Active or Sunsetting, but is set to {state}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
				}
				catch (Exception gex)
				{
					Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
				}
			}

			acc.State = MegaAccordionState.Active;
			acc.ResetMegaAccordion();
			acc.State = MegaAccordionState.Sunsetting;
			acc.ResetMegaAccordion();
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Level_Count()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevel();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "One or more values in Levels is less than the minumum of 1.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Levels_Count_External()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevels_External();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "There are 1 arguments, but there are 2 arguments indicated in the Levels.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Levels_Count_Internal()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevels_Internal();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "Offset count of 1 does not equal the ArgumentItems count of 2.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Dupe_Arg_Names()
		{
			try
			{
				var acc = MakeMegaAccordionTest_DupeArgName();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"ArgumentItems has one or more items with duplicated names: each item must have a unique name.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_TooManyInProgress()
		{
			var acc = MakeMegaAccordionTest_TooManyInProgress();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"ItemsInProgress count {acc.ItemsInProgress.Keys.Count} is greater than allowed MaxInProgress of {acc.MaxInProgress}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_LevelTooHigh()
		{
			var acc = MakeMegaAccordionTest_LevelTooHigh();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"Level value of {acc.Level} must be less than the number of levels: {acc.Levels.Length}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_IndexesNegVal()
		{
			var acc = MakeMegaAccordionTest_IndexesNegVal();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				var r = new Regex($"The\\svalue\\s-\\d+\\sin\\sIndexes\\[\\d+\\]\\scan\\snot\\sbe\\snegative.");
				Assert.That(r.IsMatch(ex.Message), $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_IndexesOutOfBounds()
		{
			var acc = MakeMegaAccordionTest_IndexesOutOfBounds();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				var r = new Regex($"The\\svalue\\s\\d+\\sin\\sIndexes\\[\\d+\\]\\sis\\sout\\sof\\sbounds\\sfor\\s.+\\sitems\\).");
				Assert.That(r.IsMatch(ex.Message), $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_Init_Valid()
		{
			var acc = MakeMegaAccordionTest_OneArgument_Valid();

			acc.ResetMegaAccordion();
			Assert.That(acc.Level == 0, $"acc.Level: expected 0, but got {acc.Level}");
			Assert.That(acc.Levels.Length == 1, $"acc.IndexEnd: expected 0, but got {acc.Levels.Length}");
			Assert.That(acc.ArgumentItems.Count == 1, $"acc.IndexOffset: expected 0, but got {acc.ArgumentItems.Count}");
			Assert.That(acc.MaxInProgress == 100, $"acc.MaxInProgress: expected 0, but got {acc.MaxInProgress}");
			Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got {acc.ItemsInProgress.Count}");
		}

		/// <summary>
		/// Validate single-level, single index behavior.
		/// </summary>
		[Test]
		public void Accordion_Validate_Set1()
		{
			var acc = MakeMegaAccordionTest_Set1();
			acc.ResetMegaAccordion();
			var key = acc.BuildKeyFromIndexes();
			Assert.IsTrue(key == "0", "Expected BuildKeyFromIndexes to be \"0\", but was \"{key}\".");
			var index = 0;
			for (; index < 5; index++)
			{
				Assert.IsTrue(acc.State == MegaAccordionState.Active, $"For index {index}, expected state to be Active but was {acc.State}.");
				key = acc.BuildKeyFromIndexes();
				Assert.IsTrue(key == $"{index}", $"Expected BuildKeyFromIndexes to be \"{index}\", but was \"{key}\".");
				MegaAccordionItem<MyMegaObject> o;
				Assert.IsTrue(acc.TryGetWorkItem(5, true, out o), "Expected TryGetWorkItem to be true.");
				key = acc.BuildKeyFromIndexes();
				Assert.IsTrue(key == $"{index + 1}", $"Expected BuildKeyFromIndexes to be \"{index + 1}\", but was \"{key}\".");
				Assert.IsTrue(acc.ItemsInProgress.Count == 1, $"{index + 1} ", $"Expected BuildKeyFromIndexes to be \"{index + 1}\", but was \"{key}\".");
				acc.CompleteItem(o.Key);
				Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"{index + 1} ", $"Expected BuildKeyFromIndexes to be \"{index + 1}\", but was \"{key}\".");
			}
			Assert.IsTrue(key == $"{index}", $"Expected BuildKeyFromIndexes to be \"{index + 1}\", but was \"{key}\".");
			Assert.IsTrue(acc.State == MegaAccordionState.CompletedSuccessfully, $"Expected final state to be CompletedSuccessfully but was {acc.State}.");
		}
		#endregion

		#region Private Helpers
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_NoArguments()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>()
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevel()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevels_External()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1, 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevels_Internal()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1, 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgument()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgument_Valid()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_DupeArgName()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_TooManyInProgress()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 2,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				},
				ItemsInProgress = new Dictionary<string, MegaAccordionItem<MyMegaObject>>
				{
					{"0:0:0", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:0",
							Value=new MyMegaObject
							{
								Index = 0,
								Message = "Hello",
								Succeeded=false
							}
						}
					},
					{"0:0:1", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:1",
							Value=new MyMegaObject
							{
								Index = 1,
								Message = "World",
								Succeeded=false
							}
						}
					},
					{"0:0:2", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:2",
							Value=new MyMegaObject
							{
								Index = 2,
								Message = "Piece",
								Succeeded=false
							}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_LevelTooHigh()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 3,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_IndexesNegVal()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 2,
				Levels = new int[] { 1, 1, 1 },
				Indexes = new long[] { -1, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_IndexesOutOfBounds()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 2,
				Levels = new int[] { 1, 1, 1 },
				Indexes = new long[] { 0, 2, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_ArgsIndexes()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// Single Argument, single level
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set1()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2", "4", "5"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set2()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}


		#endregion
	}
}