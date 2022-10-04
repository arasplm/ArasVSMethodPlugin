//------------------------------------------------------------------------------
// <copyright file="DataGridViewCalendarEditingControl.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace OfficeConnector.PropertyDataGrid
{
	public class DataGridViewCalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
	{
		public DataGridViewCalendarEditingControl()
		{
			EditingControlValueChanged = false;
		}

		public object EditingControlFormattedValue
		{
			get { return Value.ToString(CustomFormat); }
			set
			{
				var s = value as string;
				if (s != null)
				{
					Value = DateTime.Parse(s);
				}
			}
		}

		public object GetEditingControlFormattedValue(
			DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public void ApplyCellStyleToEditingControl(
			DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			CalendarForeColor = dataGridViewCellStyle.ForeColor;
			CalendarMonthBackground = dataGridViewCellStyle.BackColor;
		}

		public int EditingControlRowIndex { get; set; }

		public bool EditingControlWantsInputKey(
			Keys key, bool dataGridViewWantsInputKey)
		{
			switch (key & Keys.KeyCode)
			{
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Home:
				case Keys.End:
				case Keys.PageDown:
				case Keys.PageUp:
					return true;
				default:
					return false;
			}
		}

		public void PrepareEditingControlForEdit(bool selectAll)
		{
		}

		public bool RepositionEditingControlOnValueChange
		{
			get { return false; }
		}

		public DataGridView EditingControlDataGridView { get; set; }

		public bool EditingControlValueChanged { get; set; }

		public Cursor EditingPanelCursor
		{
			get { return Cursor; }
		}

		protected override void OnValueChanged(EventArgs eventargs)
		{
			EditingControlValueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(true);
			base.OnValueChanged(eventargs);
		}
	}
}
