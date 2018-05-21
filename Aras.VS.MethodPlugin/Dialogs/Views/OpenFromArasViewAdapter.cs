//------------------------------------------------------------------------------
// <copyright file="OpenFromArasViewAdapter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromArasViewAdapter : ViewAdaper<OpenFromArasView, OpenFromArasViewResult>
	{
		public OpenFromArasViewAdapter(OpenFromArasView view) : base(view)
		{
		}

		public override OpenFromArasViewResult ShowDialog()
		{
			var viewModel = view.DataContext as OpenFromArasViewModel;

			var result = view.ShowDialog();

			return new OpenFromArasViewResult()
			{
				DialogOperationResult = result,
				SelectedIdentityId = viewModel.IdentityId,
				SelectedIdentityKeyedName = viewModel.IdentityKeyedName,
				MethodCode = viewModel.MethodCode,
				MethodConfigId = viewModel.MethodConfigId,
				MethodId = viewModel.MethodId,
				MethodLanguage = viewModel.MethodLanguage,
				MethodName = viewModel.MethodName,
				MethodType = viewModel.MethodType,
				Package = viewModel.Package,
				SelectedTemplate = viewModel.SelectedTemplate,
				SelectedEventSpecificData = viewModel.SelectedEventSpecificData
			};
		}
	}
}
