//------------------------------------------------------------------------------
// <copyright file="ItemGridLayout.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch.Preferences
{
	public class ItemGridLayout
	{
		private dynamic layout;

		public ItemGridLayout(dynamic layout)
		{
			if (layout == null) throw new ArgumentNullException(nameof(layout));

			this.layout = layout.getItemByIndex(0);

			var colWidth = layout.getProperty("col_widths", string.Empty)
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			ColumnWidthsList = new List<string>(colWidth);

			var columnOrder = layout.getProperty("col_order", string.Empty)
				.Replace("_D;", ";")
				.Replace("L;", "locked_by_id;")
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			ColumnOrderList = new List<string>(columnOrder);
		}

		public string ItemTypeId { get { return layout.getProperty("item_type_id"); } }

		public List<string> ColumnWidthsList { get; private set; }

		public List<string> ColumnOrderList { get; private set; }

		public void UpdateLayout(string columnOrder, string columnWidth, string pageSize)
		{
			var cOrder = columnOrder.Replace("_D;", ";")
				.Replace("L;", "locked_by_id;")
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			ColumnOrderList = new List<string>(cOrder);

			var colWidth = columnWidth.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			ColumnWidthsList = new List<string>(colWidth);
			PageSize = pageSize;
		}

		public string PageSize
		{
			get { return layout.getProperty("page_size", "0"); }
			set { layout.setProperty("page_size", value); }
		}
	}
}