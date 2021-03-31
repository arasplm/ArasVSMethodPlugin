//------------------------------------------------------------------------------
// <copyright file="ItemGridLayout.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aras.VS.MethodPlugin.ItemSearch.Preferences
{
	public class ItemGridLayout
	{
		private const string defaultPageSize = "25";
		private const string defaultColumnWidth = "100";

		private dynamic layoutItem;

		public ItemGridLayout(dynamic layoutItem)
		{
			if (layoutItem == null) throw new ArgumentNullException(nameof(layoutItem));

			this.layoutItem = layoutItem.getItemByIndex(0);

			LoadItemGridLayoutData();
		}

		#region Properties

		public string ItemTypeId
		{
			get
			{
				return this.layoutItem.getProperty("item_type_id", string.Empty);
			}
		}

		public string PageSize
		{
			get
			{
				return this.layoutItem.getProperty("page_size", defaultPageSize);
			}
		}

		public List<string> ColumnWidthsList { get; private set; }

		public List<string> ColumnOrderList { get; private set; }

		#endregion

		public void UpdateLayout(dynamic itemType)
		{
			StringBuilder colOrder = new StringBuilder();
			StringBuilder colWidths = new StringBuilder();
			colOrder.AppendFormat("{0};", "L");
			colWidths.AppendFormat("{0};", "24");

			StringBuilder endColOrder = new StringBuilder();
			StringBuilder endColWidths = new StringBuilder();

			dynamic properties = itemType.getRelationships("Property");
			dynamic property;

			for (int i = 0; i < properties.getItemCount(); i++)
			{
				property = properties.getItemByIndex(i);
				if (!string.Equals("0", property.getProperty("is_hidden")))
				{
					continue;
				}

				string name = property.getProperty("name");
				string columnWidth = property.getProperty("column_width", defaultColumnWidth);
				string sortOrder = property.getProperty("sort_order", string.Empty);

				if (string.IsNullOrEmpty(sortOrder))
				{
					endColOrder.AppendFormat("{0}_D;", name);
					endColWidths.AppendFormat("{0};", columnWidth);
				}
				else
				{
					colOrder.AppendFormat("{0}_D;", name);
					colWidths.AppendFormat("{0};", columnWidth);
				}
			}

			colOrder.Append(endColOrder.ToString());
			colWidths.Append(endColWidths.ToString());

			string pageSize = itemType.getProperty("default_page_size", "0");

			int tmp;
			if (!int.TryParse(pageSize, out tmp) || tmp <= 0)
			{
				pageSize = defaultPageSize;
			}

			this.layoutItem.setProperty("col_order", colOrder.ToString(0, colOrder.Length - 1));
			this.layoutItem.setProperty("col_widths", colWidths.ToString(0, colWidths.Length - 1));
			this.layoutItem.setProperty("page_size", pageSize);

			LoadItemGridLayoutData();
		}

		private void LoadItemGridLayoutData()
		{
			string columnWidths = this.layoutItem.getProperty("col_widths", string.Empty);
			string columnOrders = this.layoutItem.getProperty("col_order", string.Empty);

			if (!string.IsNullOrEmpty(columnOrders))
			{
				columnOrders += ";";
			}

			this.ColumnWidthsList = columnWidths
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();

			this.ColumnOrderList = columnOrders
				.Replace("_D;", ";")
				.Replace("L;", "locked_by_id;")
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();
		}
	}
}