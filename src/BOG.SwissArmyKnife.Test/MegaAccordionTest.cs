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
		/// <summary>
		/// Validate that no arguments are rejected.
		/// </summary>
		[Test]
		public void Accordion_Init_NoArguments()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.ResetMegaAccordion();
			});
		}

		/// <summary>
		/// Validate that all states other than Active and Sunsetting are invalid.
		/// </summary>
		[Test]
		public void Accordion_Init_InvalidState()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.State = MegaAccordionState.CompletedSuccessfully;
				acc.ResetMegaAccordion();
			});
			Assert.Throws<ArgumentException>(() =>
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.State = MegaAccordionState.Deadlocked;
				acc.ResetMegaAccordion();
			});
			Assert.Throws<ArgumentException>(() =>
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.State = MegaAccordionState.MaxErrorsExceeded;
				acc.ResetMegaAccordion();
			});
			Assert.Throws<ArgumentException>(() =>
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.State = MegaAccordionState.UnexpectedError;
				acc.ResetMegaAccordion();
			});
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_Init_Valid()
		{
			var acc = MakeMegaAccordionTest_Bad();

			acc.ResetMegaAccordion();
			Assert.That(acc.Level == 0, $"acc.Level: expected 0, but got {acc.Level}");
			Assert.That(acc.Levels.Length == 3, $"acc.IndexEnd: expected 0, but got {acc.Levels.Length}");
			Assert.That(acc.ArgumentItems.Count == 3, $"acc.IndexOffset: expected 0, but got {acc.ArgumentItems.Count}");
			Assert.That(acc.MaxInProgress == 100, $"acc.MaxInProgress: expected 0, but got {acc.MaxInProgress}");
			Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got {acc.ItemsInProgress.Count}");
		}

		#region Private Helpers
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_NoArguments()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[1] {0},
				Indexes = new long[] {0},
				ArgumentItems = new Dictionary<int, ArgumentItem>()
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Bad()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[3] { 0, 1, 2 },
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