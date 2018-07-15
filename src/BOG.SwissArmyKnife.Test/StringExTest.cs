﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BOG.SwissArmyKnife;
using BOG.SwissArmyKnife.Extensions;
using NUnit.Framework;

namespace BOG.SwissArmyKnife.Test
{
	[TestFixture]
	public class StringExTest
	{
		[Test, Description("QuotedTrim_UnquotedEmptyString(): check for a quoted empty string")]
		public void StringEx_QuotedTrim_UnquotedEmptyString()
		{
			string Test = string.Empty;
			string Result = Test.QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedEmptyString(): check for a quoted empty string")]
		public void StringEx_QuotedTrim_QuotedEmptyString()
		{
			string Result = "\"\"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedSingleSpace(): check for a quoted string of single space")]
		public void StringEx_QuotedTrim_QuotedSingleSpace()
		{
			string Result = "\" \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedMultipleSpaces(): check for a quoted string of multiple spaces")]
		public void StringEx_QuotedTrim_QuotedMultipleSpaces()
		{
			string Result = "\"      \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedRJString(): check for a quoted string of right justified values")]
		public void StringEx_QuotedTrim_QuotedRJString()
		{
			string Result = "\"     X\"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare("X", Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedLJString(): check for a quoted string of left justified values")]
		public void StringEx_QuotedTrim_QuotedLJString()
		{
			string Result = "\"X     \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare("X", Result) == 0);
		}

		[Test, Description("QuotedTrim_QuotedCJString(): check for a quoted string of center justified values")]
		public void StringEx_QuotedTrim_QuotedCJString()
		{
			string Result = "\"  X  \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare("X", Result) == 0);
		}

		[Test, Description("QuotedTrim_EmbeddedQuotedString(): check for a quoted string embedded in quotes")]
		public void StringEx_QuotedTrim_EmbeddedQuotedString()
		{
			string Result = "\" \"  X  \" \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
			Assert.IsTrue(string.Compare("\"  X  \"", Result) == 0);
		}

		[Test, Description("Filter(): case-insensitive")]
		public void StringEx_Filter_CaseInsensitive()
		{
			string Filter = "0123456789.c";
			string Result = "76.34F.25.41C".Filter(Filter, true);
			Assert.IsTrue(string.Compare("76.34.25.41C", Result) == 0);
		}

		[Test, Description("Filter(): case-sensitive")]
		public void StringEx_Filter_CaseSensitive()
		{
			string Filter = "0123456789.c";
			string Result = "76.34F.25.41C".Filter(Filter, false);
			Assert.IsTrue(string.Compare("76.34.25.41", Result) == 0);
		}

		[Test, Description("FilterOut(): case-insensitive")]
		public void StringEx_FilterOut_CaseInsensitive()
		{
			string Filter = "0123456789.c";
			string Result = "76.34F.25.41C".FilterOut(Filter, true);
			Assert.IsTrue(string.Compare("F", Result) == 0);
		}

		[Test, Description("FilterOut(): case-sensitive")]
		public void StringEx_FilterOut_CaseSensitive()
		{
			string Filter = "0123456789.c";
			string Result = "76.34F.25.41C".FilterOut(Filter, false);
			Assert.IsTrue(string.Compare("FC", Result) == 0);
		}
    }
}
