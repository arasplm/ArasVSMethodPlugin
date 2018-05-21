//------------------------------------------------------------------------------
// <copyright file="SaveMethodViewAdapter.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class SaveMethodViewAdapter : ViewAdaper<SaveMethodView, SaveMethodViewResult>
	{
		public SaveMethodViewAdapter(SaveMethodView view) : base(view)
		{
		}

		public override SaveMethodViewResult ShowDialog()
		{
			var viewModel = view.DataContext as SaveMethodViewModel;

			var result = view.ShowDialog();

			return new SaveMethodViewResult()
			{
				DialogOperationResult = result,
				MethodCode = viewModel.MethodCode,
				MethodItem = viewModel.MethodItem,
				MethodLanguage = viewModel.MethodLanguage,
				MethodName = viewModel.MethodName,
				SelectedIdentityId = viewModel.SelectedIdentityId,
				SelectedIdentityKeyedName = viewModel.SelectedIdentityKeyedName,
				SelectedPackage = viewModel.SelectedPackage,
				TemplateName = viewModel.TemplateName
			};
		}
	}
}
