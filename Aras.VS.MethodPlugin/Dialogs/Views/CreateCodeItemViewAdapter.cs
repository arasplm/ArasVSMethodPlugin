//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemViewAdapter.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreateCodeItemViewAdapter : ViewAdaper<CreateCodeItemView, CreateCodeItemViewResult>
	{
		public CreateCodeItemViewAdapter(CreateCodeItemView view) : base(view)
		{
		}

		public override CreateCodeItemViewResult ShowDialog()
		{
			var viewModel = view.DataContext as CreateCodeItemViewModel;

			var result = view.ShowDialog();
			return new CreateCodeItemViewResult()
			{
				DialogOperationResult = result,
				FileName = viewModel.FileName,
				SelectedCodeType = viewModel.SelectedCodeType,
				SelectedElementType = viewModel.SelectedElementType,
				IsUseVSFormattingCode = viewModel.IsUseVSFormattingCode
			};
		}
	}
}
