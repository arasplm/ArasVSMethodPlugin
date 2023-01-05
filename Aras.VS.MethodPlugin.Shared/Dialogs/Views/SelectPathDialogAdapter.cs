//------------------------------------------------------------------------------
// <copyright file="SelectPathDialogAdapter.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class SelectPathDialogAdapter : ViewAdaper<SelectPathDialog, SelectPathDialogResult>
	{
		public SelectPathDialogAdapter(SelectPathDialog view) : base(view)
		{
		}

		public override SelectPathDialogResult ShowDialog()
		{
			var viewModel = view.DataContext as SelectPathViewModel;
			var result = view.ShowDialog();

			return new SelectPathDialogResult()
			{
				DialogOperationResult = result,
				SelectedFullPath = viewModel.SelectedPath
			};
		}
	}
}
