//------------------------------------------------------------------------------
// <copyright file="CreateMethodViewAdapter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreateMethodViewAdapter : ViewAdaper<CreateMethodView, CreateMethodViewResult>
	{
		public CreateMethodViewAdapter(CreateMethodView view) : base(view)
		{
		}

		public override CreateMethodViewResult ShowDialog()
		{
			var viewModel = view.DataContext as CreateMethodViewModel;

			var result = view.ShowDialog();
			return new CreateMethodViewResult()
			{
				DialogOperationResult = result,
				MethodName = viewModel.MethodName,
				MethodComment = viewModel.MethodComment,
				SelectedActionLocation = viewModel.SelectedActionLocation,
				SelectedEventSpecificData = viewModel.SelectedEventSpecificData,
				SelectedIdentityId = viewModel.SelectedIdentityId,
				SelectedIdentityKeyedName = viewModel.SelectedIdentityKeyedName,
				SelectedLanguage = viewModel.SelectedLanguage,
				SelectedPackage = viewModel.SelectedPackageInfo,
				SelectedTemplate = viewModel.SelectedTemplate,
				UseRecommendedDefaultCode = viewModel.UseRecommendedDefaultCode,
				IsUseVSFormattingCode = viewModel.IsUseVSFormattingCode,
				SelectedUserCodeTemplate = viewModel.SelectedUserCodeTemplate.Value
			};
		}
	}
}
