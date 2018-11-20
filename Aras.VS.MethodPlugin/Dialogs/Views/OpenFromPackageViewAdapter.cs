//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageViewAdapter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromPackageViewAdapter : ViewAdaper<OpenFromPackageView, OpenFromPackageViewResult>
	{
        public OpenFromPackageViewAdapter(OpenFromPackageView view) : base(view)
		{
		}

		public override OpenFromPackageViewResult ShowDialog()
		{
			var viewModel = view.DataContext as OpenFromPackageViewModel;

			var result = view.ShowDialog();

			return new OpenFromPackageViewResult()
			{
				DialogOperationResult = result,
				IdentityId = viewModel.IdentityId,
				IdentityKeyedName = viewModel.IdentityKeyedName,
				MethodCode = viewModel.MethodCode,
				MethodConfigId = viewModel.MethodConfigId,
				MethodId = viewModel.MethodId,
				MethodLanguage = viewModel.MethodLanguage,
				MethodName = viewModel.MethodName,
				MethodType = viewModel.MethodType,
				MethodComment = viewModel.MethodComment,
				Package = viewModel.Package,
				SelectedTemplate = viewModel.SelectedTemplate,
				SelectedEventSpecificData = viewModel.SelectedEventSpecificData,
				SelectedFolderPath = Path.GetDirectoryName(viewModel.SelectedManifestFilePath),
				SelectedManifestFileName = Path.GetFileName(viewModel.SelectedManifestFilePath),
                SelectedManifestFullPath = viewModel.SelectedManifestFilePath,
                IsUseVSFormattingCode = viewModel.IsUseVSFormattingCode
			};
		}
	}
}
