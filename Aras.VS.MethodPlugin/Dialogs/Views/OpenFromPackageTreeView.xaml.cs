//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageTreeView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
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
			this.Loaded += OpenFromPackageView_Loaded;
		}

		private void OpenFromPackageView_Loaded(object sender, RoutedEventArgs e)
		{
			//TODO: bring into view does not work
			FolderView.BringIntoView();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
