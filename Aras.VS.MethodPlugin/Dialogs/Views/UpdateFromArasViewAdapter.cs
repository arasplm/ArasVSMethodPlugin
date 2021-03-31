//------------------------------------------------------------------------------
// <copyright file="UpdateFromArasViewAdapter.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class UpdateFromArasViewAdapter : ViewAdaper<UpdateFromArasView, UpdateFromArasViewResult>
	{
		public UpdateFromArasViewAdapter(UpdateFromArasView view) : base(view)
		{
		}

		public override UpdateFromArasViewResult ShowDialog()
		{
			var viewModel = view.DataContext as UpdateFromArasViewModel;

			var result = view.ShowDialog();

			return new UpdateFromArasViewResult()
			{
				DialogOperationResult = result,
				SelectedTemplate = viewModel.SelectedTemplate,
				EventSpecificData = viewModel.EventSpecificData,
				ExecutionIdentityId = viewModel.ExecutionIdentityId,
				ExecutionIdentityKeyedName = viewModel.ExecutionIdentityKeyedName,
				MethodCode = viewModel.MethodCode,
				MethodConfigId = viewModel.MethodConfigId,
				MethodId = viewModel.MethodId,
				MethodLanguage = viewModel.MethodLanguage,
				MethodName = viewModel.MethodName,
				MethodType = viewModel.MethodType,
				MethodComment = viewModel.MethodComment,
				Package = viewModel.Package,
				IsUseVSFormattingCode = viewModel.IsUseVSFormattingCode
			};
		}
	}
}
