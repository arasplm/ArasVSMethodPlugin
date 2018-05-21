//------------------------------------------------------------------------------
// <copyright file="OpenFromArasView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for OpenFromPackageView.xaml
	/// </summary>
	public partial class OpenFromArasView : Window
	{
		public OpenFromArasView()
		{
			InitializeComponent();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
