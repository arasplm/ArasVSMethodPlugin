//------------------------------------------------------------------------------
// <copyright file="DataGridViewCalendarCell.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using OfficeConnector.Properties;

namespace OfficeConnector.PropertyDataGrid
{
	public class DataGridViewCalendarCell : DataGridViewTextBoxCell, IUpdatedCell
	{
		private Button clearButton;
		private bool isFileToAras;
		DataGridViewCalendarEditingControl ctl;
		public event Action<DataGridViewCellEventArgs> ClearButtonClick;

		public DataGridViewCalendarCell()
			: base()
		{
			CreateButton();
		}

		public DataGridViewCalendarCell(string format, bool isFileToAras)
			: base()
		{
			Style.Format = format;
			this.isFileToAras = isFileToAras;
			CreateButton();
		}

		private void CreateButton()
		{
			clearButton = new Button() { Width = 20, Height = 20 };
			clearButton.BackgroundImage = Resources.addLinkDialogDeleteItemLinkImg;
			clearButton.BackgroundImageLayout = ImageLayout.Stretch;
			clearButton.Click += ClearButton_Click;
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			var clearEvent = ClearButtonClick;
			if (clearEvent != null)
			{
				clearEvent(new DataGridViewCellEventArgs(this.ColumnIndex, this.RowIndex));
			}

			this.Value = string.Empty;
		}

		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
			ctl = DataGridView.EditingControl as DataGridViewCalendarEditingControl;
			ctl.Format = DateTimePickerFormat.Custom;
			ctl.CustomFormat = Style.Format;
			DateTime dt;

			if (Value == null || !DateTime.TryParse(Value.ToString(), out dt))
			{
				dt = DateTime.Now;
			}
			ctl.Value = dt;
		}

		public void Update(Rectangle cellBounds, bool displayed, bool isHeaderCell)
		{
			clearButton.Visible = displayed;
			if (!displayed)
			{
				return;
			}
			if (!this.IsInEditMode && this.Value != null && !string.IsNullOrEmpty(this.Value.ToString()) && (isFileToAras || isHeaderCell))
			{
				clearButton.Visible = true;
				clearButton.Parent = this.DataGridView;
				clearButton.Top = cellBounds.Top;
				clearButton.Left = cellBounds.Right - clearButton.Width;
				clearButton.Height = Math.Min(cellBounds.Height, 20);
			}
			else
			{
				clearButton.Visible = false;
			}

			clearButton.Invalidate();
			clearButton.Refresh();

			if (ctl != null)
			{
				ctl.Invalidate();
				ctl.Refresh();
			}
		}

		public override Type EditType
		{
			get { return typeof(DataGridViewCalendarEditingControl); }
		}

		public override Type ValueType
		{
			get { return typeof(string); }
		}

		public override object DefaultNewRowValue
		{
			get { return string.Empty; }
		}
	}
}
