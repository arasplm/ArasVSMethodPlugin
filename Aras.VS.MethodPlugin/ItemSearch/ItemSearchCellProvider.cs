//------------------------------------------------------------------------------
// <copyright file="ItemSearchCellProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using OfficeConnector.Properties;
using OfficeConnector.PropertyDataGrid;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemSearchCellProvider
	{
		public DataGridViewCell GetCell(ItemSearchPropertyInfo property)
		{
			
			var lockProperInfo = property as LockedByPropertyInfo;
			if (lockProperInfo != null)
			{
				var cell = new DataGridViewImageCell();

				if (lockProperInfo.IsLocked)
				{
					cell.Value = Resources.img_locked;
				}
				if (!lockProperInfo.IsLockedByMe)
				{
					cell.Value = Resources.img_locked_else;
				}
				if (!lockProperInfo.IsLocked)
				{
					cell.Value = null;
				}

				return cell;
			}

			if (property.DataType == PropertyDataType.Boolean)
			{
				var cell = new DataGridViewCheckBoxCell();
				cell.Value = property.PropertyValue=="1";
				return cell;
			}

			if (property.DataType == PropertyDataType.Color)
			{
				var cell = new DataGridViewTextBoxCell();
				var color = string.IsNullOrEmpty(property.PropertyValue) ? Color.White : System.Drawing.ColorTranslator.FromHtml(property.PropertyValue);
				cell.Style.BackColor = color;
				return cell;
			}

			if (property.DataType == PropertyDataType.ColorList)
			{
				var colors = (property as ColorListPropertyInfo).ColorSource;
				var cell = new DataGridViewTextBoxCell();
				
				string value = property.PropertyValue ?? string.Empty;
				string label;

				if (!colors.TryGetValue(value, out label))
				{
					label = value;
				}

				var color = string.IsNullOrEmpty(value) ? Color.White : ColorTranslator.FromHtml(value);

				cell.Value = label;
				cell.Style.BackColor = color;
				return cell;
			}

			if (property.DataType == PropertyDataType.Date)
			{
				var cell = new DataGridViewCalendarCell();

				DateTime dt;
				if (DateTime.TryParse(property.PropertyValue, out dt))
				{
					cell.Value = dt;
				}
				else
				{
					cell.Value = property.PropertyValue;
				}

				return cell;
			}

			if (property.DataType == PropertyDataType.Item)
			{
				var cell = new DataGridViewTextBoxCell();
				cell.Value = property.Label;

				return cell;
			}

			var defaultCell = new DataGridViewTextBoxCell();
			defaultCell.Value = property.PropertyValue;
		
			return defaultCell;
		}
	}
}
