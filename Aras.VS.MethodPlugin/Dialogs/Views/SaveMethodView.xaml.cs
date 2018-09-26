//------------------------------------------------------------------------------
// <copyright file="SaveMethodView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for SaveMethodView.xaml
	/// </summary>
	public partial class SaveMethodView : Window
	{
		public SaveMethodView()
		{
			InitializeComponent();
		}

		private void MethodName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !Regex.IsMatch(e.Text, @"^\w$|^\w[\w, -]*\w$");
		}
	}
}
