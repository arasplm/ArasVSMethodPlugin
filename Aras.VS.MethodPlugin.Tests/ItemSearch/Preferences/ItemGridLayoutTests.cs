using System;
using System.Collections.Generic;
using System.Linq;
using Aras.IOM;
using Aras.VS.MethodPlugin.ItemSearch.Preferences;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.ItemSearch.Preferences
{
	[TestFixture]
	public class ItemGridLayoutTests
	{
		private IServerConnection serverConnection;
		private Innovator innovator;

		[SetUp]
		public void Setup()
		{
			serverConnection = Substitute.For<IServerConnection>();
			innovator = new Innovator(this.serverConnection);
		}

		[Test]
		public void Ctro_LayoutIsNull_ShouldThrowArgumentNullException()
		{
			//Arrange
			dynamic layoutItem = null;

			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				//Act
				var itemGridLayout = new ItemGridLayout(layoutItem);
			}), nameof(layoutItem));
		}

		[Test]
		public void Ctro_ShouldDoesNotThrow()
		{
			//Arrange
			dynamic layoutItem = innovator.newItem();

			//Assert
			Assert.DoesNotThrow(new TestDelegate(() =>
			{
				//Act
				var itemGridLayout = new ItemGridLayout(layoutItem);
			}));
		}

		[TestCase("value_id")]
		public void ItemTypeId_ShouldReturn_ExpectedId(string expected)
		{
			// Arrange
			dynamic layoutItem = innovator.newItem();
			layoutItem.setProperty("item_type_id", expected);
			var itemGridLayout = new ItemGridLayout(layoutItem);

			// Act
			string actual = itemGridLayout.ItemTypeId;

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestCase("101")]
		public void PageSize_ShouldReturnExpectedValue(string expected)
		{
			//Arrange
			dynamic layoutItem = innovator.newItem();
			layoutItem.setProperty("page_size", expected);

			var itemGridLayout = new ItemGridLayout(layoutItem);

			//Act
			string actual = itemGridLayout.PageSize;

			//Assert
			Assert.AreEqual(expected, actual);
		}

		[TestCase("24;120;50;200;120;80;150;60;60;60;150;100;100;100;100;100;100")]
		public void ColumnWidthsList_ShouldReturnExpectedValues(string colWidthsValues)
		{
			//Arrange
			dynamic layoutItem = innovator.newItem();
			layoutItem.setProperty("col_widths", colWidthsValues);
			var itemGridLayout = new ItemGridLayout(layoutItem);

			List<string> expected = colWidthsValues
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();

			//Act
			List<string> actual = itemGridLayout.ColumnWidthsList;

			//Assert
			Assert.AreEqual(expected, actual);
		}

		[TestCase("item_number_D;major_rev_D;name_D;classification_D;state_D;authoring_tool_D;has_change_pending_D;has_files_D;is_template_D;from_template_D;linked_part_number_D;linked_project_number_D;linked_part_D;linked_project_D;test_string_o_a_D;test_string_a_o_D")]
		public void ColumnOrderList_ShouldReturnExpectedValues(string colOrderValues)
		{
			//Arrange
			dynamic layoutItem = innovator.newItem();
			layoutItem.setProperty("col_order", colOrderValues);
			var itemGridLayout = new ItemGridLayout(layoutItem);

			if (!string.IsNullOrEmpty(colOrderValues))
			{
				colOrderValues += ";";
			}

			List<string> expected = colOrderValues
				.Replace("_D;", ";")
				.Replace("L;", "locked_by_id;")
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();

			//Act
			List<string> actual = itemGridLayout.ColumnOrderList;

			//Assert
			Assert.AreEqual(expected, actual);
		}
	}
}
