//------------------------------------------------------------------------------
// <copyright file="CreateMethodView.xaml.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
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

		private void MethodName_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !Regex.IsMatch(e.Text, @"^\w$|^\w[\w, -]*\w$");
		}
	}
}
