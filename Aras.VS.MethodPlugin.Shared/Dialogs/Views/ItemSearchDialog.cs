//------------------------------------------------------------------------------
// <copyright file="ItemSearchDialog.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Aras.Method.Libs.Code;
using Aras.VS.MethodPlugin.ItemSearch;
using OfficeConnector.Dialogs;
using OfficeConnector.Properties;
using OfficeConnector.PropertyDataGrid;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	[ExcludeFromCodeCoverage]
	public partial class ItemSearchDialog : Form, IItemSearchView
	{
		private readonly ItemSearchColumnProvider columnProvider;
		private readonly ItemSearchCellProvider cellProvider;

		private Bitmap bmpLocked = Resources.img_locked;
		private Bitmap bmpLockedElse = Resources.img_locked_else;

		public ItemSearchDialog(ItemSearchColumnProvider columnProvider, ItemSearchCellProvider cellProvider)
		{
			this.InitializeComponent();
			this.InitializeDialogStyle();

			this.columnProvider = columnProvider;
			this.cellProvider = cellProvider;

			runSearchBtn.Click += (sender, args) => InvokeCustom(RunSearch);
			newSearchBtn.Click += (sender, args) => InvokeCustom(ClearSearch);
			getPreviousPage.Click += (sender, args) => InvokeCustom(PreviousPage);
			getNextPage.Click += (sender, args) => InvokeCustom(NextPage);
			cancelButton.Click += (sender, args) => InvokeCustom(Cancel);
			okButton.Click += (sender, args) => InvokeCustom(Ok);
			itemTypeBox.SelectedIndexChanged += (sender, args) => InvokeCustom<ItemTypeItem>(ItemTypeChanged, (ItemTypeItem)itemTypeBox.SelectedItem);
			savedSearchesBox.SelectedIndexChanged += (sender, args) => InvokeCustom<SavedSearch>(SavedSearchChanged, (SavedSearch)savedSearchesBox.SelectedItem);
			tbPageSize.TextChanged += TbPageSize_TextChanged;

			partsGrid.parentDialog = this;
			partsGrid.CellValueChanged += (sender, args) => this.InvokeCustom<int, string>(SearchValueChanged, args.ColumnIndex, partsGrid[args.ColumnIndex, args.RowIndex].Value?.ToString());
			partsGrid.CellMouseDoubleClick += (sender, args) =>
			{
				if (args.RowIndex > 0)
				{
					InvokeCustom(Ok);
				}
			};
			partsGrid.CurrentCellDirtyStateChanged += (sender, args) =>
			{
				if (partsGrid.IsCurrentCellDirty)
				{
					partsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
				}
			};

			partsGrid.ColumnHeaderMouseClick += PartsGrid_ColumnHeaderMouseClick;
			partsGrid.SelectionChanged += (sender, args) =>
			{
				if (partsGrid.SelectedRows.Count > 0)
				{
					var idList = new List<string>();
					foreach (DataGridViewRow item in partsGrid.SelectedRows)
					{
						idList.Add(item.Cells["id"].Value?.ToString());
					}
					InvokeCustom(SelectedItemChanged, idList);
				}

				if (partsGrid.CurrentRow != null && partsGrid.CurrentRow.Index > 0)
				{
					okButton.Enabled = true;
				}
				else
				{
					okButton.Enabled = false;
				}
			};
		}

		public event Action Cancel;
		public event Action Ok;
		public event Action<ItemTypeItem> ItemTypeChanged;
		public event Action NextPage;
		public event Action<string> PageSizeChanged;
		public event Action PreviousPage;
		public event Action<SavedSearch> SavedSearchChanged;
		public event Action<int, string> SearchValueChanged;
		public event Action<EventSpecificDataType> EventDateChanged;
		public event Action RunSearch;
		public event Action ClearSearch;
		public event Action<List<string>> SelectedItemChanged;

		public void SetSearchColumns(List<ItemSearchPropertyInfo> searchColumnList)
		{
			partsGrid.Rows.Clear();
			partsGrid.Columns.Clear();

			foreach (var propInfo in searchColumnList)
			{
				var column = columnProvider.GetColumn(propInfo);
				//add column styles
				partsGrid.Columns.Add(column);

				column.Visible = !propInfo.IsHidden;
			}

			partsGrid.Rows.Add();
			for (int i = 0; i < searchColumnList.Count; i++)
			{
				partsGrid.Rows[0].Cells[i].Value = searchColumnList[i].PropertyValue;
				partsGrid.Rows[0].Cells[i].ReadOnly = searchColumnList[i].IsReadonly;
			}

			partsGrid.Rows[0].Frozen = true;
			partsGrid.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
			partsGrid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 238, 192);

			partsGrid.Rows[0].DefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 231, 245);
			partsGrid.Rows[0].DefaultCellStyle.BackColor = Color.FromArgb(221, 231, 245);
			partsGrid.SortCompare += PartsGrid_SortCompare;
			//partsGrid.CellPainting += PartsGrid_CellPainting;
		}

		public void SetItemTypes(List<ItemTypeItem> itemTypes, ItemTypeItem selectedItem)
		{
			itemTypeBox.Items.Clear();
			itemTypeBox.Items.AddRange(itemTypes.ToArray());
			itemTypeBox.SelectedItem = selectedItem;

			itemTypeLabel.Visible = true;
			itemTypeBox.Visible = true;

			if (itemTypes.Count == 1)
			{
				itemTypeBox.Enabled = false;
			}
		}

		public void SetSearchInfo(string pageSize, int page, int pageMax)
		{
			this.tbPageSize.Text = pageSize;

			this.getPreviousPage.Enabled = page > 1;
			this.getNextPage.Enabled = (pageMax - page > 0);
		}

		public void SetSavedSearch(List<SavedSearch> savedSearches)
		{
			savedSearchesBox.Items.Clear();
			savedSearchesBox.Items.AddRange(savedSearches.ToArray());
			if (savedSearches.Count > 0)
			{
				savedSearchesLabel.Visible = true;
				savedSearchesBox.Visible = true;
			}
			else
			{
				savedSearchesLabel.Visible = false;
				savedSearchesBox.Visible = false;
			}
		}

		public void SetSearchResult(List<ItemSearchResult> searchResults)
		{
			while (partsGrid.RowCount > 1)
			{
				partsGrid.Rows.RemoveAt(1);
			}

			foreach (var searchResult in searchResults)
			{
				int index = partsGrid.Rows.Add();
				foreach (var item in searchResult.FoundedItems)
				{
					var cell = cellProvider.GetCell(item);
					partsGrid[item.PropertyName, index] = cell;
					cell.ReadOnly = true;
				}
			}
		}

		public void SetViewInfo(string title)
		{
			this.Text = title;
		}

		public void RunSearchMethod()
		{
			InvokeCustom(RunSearch);
		}

		public void ShowAsDialog(IWin32Window parentWindow = null)
		{
			this.ShowDialog();
		}

		private void TbPageSize_TextChanged(object sender, EventArgs e)
		{
			var cellValue = tbPageSize.Text;
			InvokeCustom(PageSizeChanged, cellValue);
		}

		private void PartsGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var currentColumn = partsGrid.Columns[e.ColumnIndex];
			var newDirection = currentColumn.HeaderCell.SortGlyphDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			partsGrid.Sort(currentColumn, newDirection == SortOrder.Ascending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending);
			currentColumn.HeaderCell.SortGlyphDirection = newDirection;
		}

		private void PartsGrid_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
		{
			if (e.RowIndex1 == 0)
			{
				e.SortResult = 0;
				e.Handled = true;
			}
		}

		private void PartsGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex != 0 || e.ColumnIndex < 1)
			{
				return;
			}

			var dataGrid = (sender as DataGridView);
			var cell = dataGrid[e.ColumnIndex, e.RowIndex];

			if (cell is IUpdatedCell)
			{
				(cell as IUpdatedCell).Update(e.CellBounds, cell.Displayed, true);
			}
		}

		private void InvokeCustom(Action action)
		{
			if (action != null)
			{
				action();
			}
		}

		private void InvokeCustom<T>(Action<T> action, T arg1)
		{
			if (action != null)
			{
				action(arg1);
			}
		}

		private void InvokeCustom<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
		{
			if (action != null)
			{
				action(arg1, arg2);
			}
		}

		
	}

	public class KeyDownEnabledGrid : DataGridView
	{
		public ItemSearchDialog parentDialog = null;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (Keys.Enter == keyData && null != CurrentCell && CurrentCell.RowIndex == 0)
			{
				parentDialog.RunSearchMethod();
			}

			if(CurrentCell.EditType == typeof(DataGridViewTextBoxEditingControl) && (Keys.Home == keyData || Keys.End == keyData))
			{
				((TextBox)EditingControl).SelectionStart = ((TextBox)EditingControl).Text.Length;

				if (Keys.Home == keyData)
				{
					((TextBox)EditingControl).SelectionStart = 0;
				}

				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}