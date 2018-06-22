//------------------------------------------------------------------------------
// <copyright file="CreatePartialElementView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for CreatePartialElementView.xaml
	/// </summary>
	public partial class CreatePartialElementView : Window
	{
		public CreatePartialElementView()
		{
			InitializeComponent();
		}

		private void CreatePartialElementView_SourceInitialized(object sender, EventArgs e)
		{
			this.FileNameTextBlock.Focus();
			this.FileNameTextBlock.SelectAll();
		}
	}
}
