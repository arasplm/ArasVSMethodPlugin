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
		    this.Loaded += TreeViewItem_BringIntoView;
        }

		private void TreeViewItem_BringIntoView(object sender, RoutedEventArgs e)
		{
		    TreeViewItem item = sender as TreeViewItem;
		    if (item != null)
		    {
		        item.BringIntoView();
		        e.Handled = true;
		    }
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
