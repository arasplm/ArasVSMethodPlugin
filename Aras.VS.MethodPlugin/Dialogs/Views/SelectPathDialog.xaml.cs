//------------------------------------------------------------------------------
// <copyright file="SelectPathDialog.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for SelectPathDialog.xaml
	/// </summary>
	public partial class SelectPathDialog : Window
	{
		public SelectPathDialog()
		{
			InitializeComponent();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			SelectPathWindow.Close();
		}

		private void FolderView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			//TODO: bring into view does not work
			(sender as TreeView).BringIntoView();
		}

		private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
		{
			TreeViewItem item = sender as TreeViewItem;
			if (item != null)
			{
				item.Focus();
				e.Handled = true;
			}
		}

	}
}
