//------------------------------------------------------------------------------
// <copyright file="CreateMethodView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for CreateMethodVew.xaml
	/// </summary>
	public partial class CreateMethodView : Window
	{
		public CreateMethodView()
		{
			InitializeComponent();
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void MethodName_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !Regex.IsMatch(e.Text, @"^\w$|^\w[\w, -]*\w$");
		}
	}
}
