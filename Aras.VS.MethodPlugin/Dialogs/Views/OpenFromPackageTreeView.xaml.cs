//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageTreeView.xaml.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for OpenFromPackageView.xaml
	/// </summary>
	public partial class OpenFromPackageTreeView : Window
	{
		public OpenFromPackageTreeView()
		{
			InitializeComponent();
			this.Loaded += OpenFromPackageView_BringIntoView;
		}

		private void OpenFromPackageView_BringIntoView(object sender, RoutedEventArgs e)
		{
		    TreeViewItem item = sender as TreeViewItem;
		    if (item != null)
		    {
		        item.BringIntoView();
		        e.Handled = true;
		    }
        }

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox listBox = sender as ListBox;
			if (listBox != null)
			{
				listBox.ScrollIntoView(listBox.SelectedItem);
			}
		}
	}
}
