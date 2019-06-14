//------------------------------------------------------------------------------
// <copyright file="SaveToPackageViewAdapter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class SaveToPackageViewAdapter : ViewAdaper<SaveToPackageView, SaveToPackageViewResult>
	{
		public SaveToPackageViewAdapter(SaveToPackageView view) : base(view)
		{
		}

		public override SaveToPackageViewResult ShowDialog()
		{
			var viewModel = view.DataContext as SaveToPackageViewModel;

			var result = view.ShowDialog();

			return new SaveToPackageViewResult()
			{
				DialogOperationResult = result,
				MethodCode = viewModel.MethodCode,
				MethodInformation = viewModel.MethodInformation,
				MethodName = viewModel.MethodName,
				MethodComment = viewModel.MethodComment,
				PackagePath = viewModel.PackagePath,
				SelectedIdentityId = viewModel.SelectedIdentityId,
				SelectedIdentityKeyedName = viewModel.SelectedIdentityKeyedName,
				SelectedPackage = viewModel.SelectedPackageInfo
			};
		}
	}
}
