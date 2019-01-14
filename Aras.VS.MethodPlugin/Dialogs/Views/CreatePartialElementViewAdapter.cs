//------------------------------------------------------------------------------
// <copyright file="CreatePartialElementViewAdapter.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreatePartialElementViewAdapter : ViewAdaper<CreatePartialElementView, CreatePartialElementViewResult>
	{
		public CreatePartialElementViewAdapter(CreatePartialElementView view) : base(view)
		{
		}

		public override CreatePartialElementViewResult ShowDialog()
		{
			var viewModel = view.DataContext as CreatePartialElementViewModel;

			var result = view.ShowDialog();
			return new CreatePartialElementViewResult()
			{
				DialogOperationResult = result,
				FileName = viewModel.FileName,
				IsUseVSFormattingCode = viewModel.IsUseVSFormattingCode
			};
		}
	}
}
