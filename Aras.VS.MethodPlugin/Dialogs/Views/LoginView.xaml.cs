//------------------------------------------------------------------------------
// <copyright file="LoginView.xaml.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for LoginView.xaml
	/// </summary>
	public partial class LoginView : Window
	{
		public LoginView()
		{
			InitializeComponent();
		}

		private void ServerUrl_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.PasswordPB.Password = string.Empty;
		}

		private void Database_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.PasswordPB.Password = string.Empty;
		}
	}
}
