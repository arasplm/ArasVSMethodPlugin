//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemView.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for CreateCodeItemView.xaml
	/// </summary>
	public partial class CreateCodeItemView : Window
	{
		public CreateCodeItemView()
		{
			InitializeComponent();
		}

		private void CreateCodeItemView_SourceInitialized(object sender, EventArgs e)
		{
			this.FileNameTextBlock.Focus();
			this.FileNameTextBlock.SelectAll();
		}
	}
}
