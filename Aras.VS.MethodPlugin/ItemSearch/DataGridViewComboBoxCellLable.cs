//------------------------------------------------------------------------------
// <copyright file="DataGridViewComboBoxCellLable.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Windows.Forms;

namespace OfficeConnector.PropertyDataGrid
{
	// TODO : should be removed
	public class DataGridViewComboBoxCellLable : DataGridViewComboBoxCell
	{
		public string Label { get; set; }

		protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			if (!string.IsNullOrEmpty(Label))
			{
				return this.Label;
			}

			return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
		}
	}
}
