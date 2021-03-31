//------------------------------------------------------------------------------
// <copyright file="MoveToViewAdapter.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class MoveToViewAdapter : ViewAdaper<MoveToView, MoveToViewResult>
	{
		public MoveToViewAdapter(MoveToView view) : base(view)
		{
		}

		public override MoveToViewResult ShowDialog()
		{
			var viewModel = view.DataContext as MoveToViewModel;
			var result = view.ShowDialog();

			return new MoveToViewResult()
			{
				DialogOperationResult = result,
				FileName = viewModel.FileName,
				SelectedCodeType = viewModel.SelectedCodeType,
				SelectedFullPath = viewModel.SelectedFullPath
			};
		}
	}
}
