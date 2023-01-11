//------------------------------------------------------------------------------
// <copyright file="ItemSearchColumnProvider.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Forms;
using OfficeConnector.PropertyDataGrid;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemSearchColumnProvider
	{
		public ItemSearchColumnProvider() { }

		public DataGridViewColumn GetColumn(ItemSearchPropertyInfo property)
		{
			DataGridViewColumn column = null;

			if (property.PropertyName == "locked_by_id")
			{
				var colImage = new DataGridViewImageColumn()
				{
					ImageLayout = DataGridViewImageCellLayout.Normal,
					Image = null,
				};
				property.Label = string.Empty;
				colImage.DefaultCellStyle.NullValue = null;
				column = colImage;
			}
			else if (property.DataType == PropertyDataType.Boolean)
			{
				column = new DataGridViewCheckBoxColumn(true)
				{
					FalseValue = 0,
					TrueValue = 1
				};
			}
			else if (property.DataType == PropertyDataType.List)
			{
				var source = (property as ListPropertyInfo).ItemsSource;

				column = new DataGridViewComboBoxColumn()
				{
					DataSource = source,
					DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
					FlatStyle = FlatStyle.Flat
				};
			}
			else if (property.DataType == PropertyDataType.ColorList)
			{
				var source = (property as ColorListPropertyInfo).ColorSource;

				var template = new DataGridViewComboBoxCellLable()
				{
					DataSource = new BindingSource(source, null),
					DisplayMember = "Value",
					ValueMember = "Key",
					DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
					FlatStyle = FlatStyle.Flat,
				};

				column = new DataGridViewComboBoxColumn()
				{
					CellTemplate = template,
					Tag = "color list"
				};
			}
			else
			{
				column = new DataGridViewTextBoxColumn();

				if (property.DataType == PropertyDataType.Date && !string.IsNullOrEmpty(property.Pattern))
				{
					DataGridViewCalendarCell cell = new DataGridViewCalendarCell(Utilities.Utils.dateFormatDictionary[property.Pattern], false);
					column.CellTemplate = cell;
				}
			}

			if (property.DataType == PropertyDataType.Color)
			{
				column.Tag = "color";
			}

			column.HeaderText = property.Label;
			column.Name = property.PropertyName;

			column.Width = property.Width;

			//TODO: change to enum.
			if (!string.IsNullOrEmpty(property.Alignment))
			{
				switch (property.Alignment)
				{
					case "left":
						column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
						break;
					case "center":
						column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
						break;
					case "right":
						column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
						break;
				}
			}

			//TODO: check to get readonly from property.IsReadonly
			column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
			column.ReadOnly = true;
			column.SortMode = DataGridViewColumnSortMode.Programmatic;
			return column;
		}
	}
}
