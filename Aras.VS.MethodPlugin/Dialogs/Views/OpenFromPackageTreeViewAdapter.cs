using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	class OpenFromPackageTreeViewAdapter : ViewAdaper<OpenFromPackageTreeView, OpenFromPackageTreeViewResult>
	{
		public OpenFromPackageTreeViewAdapter(OpenFromPackageTreeView view) : base(view)
		{
		}

		public override OpenFromPackageTreeViewResult ShowDialog()
		{
			var viewModel = view.DataContext as OpenFromPackageTreeViewModel;

			bool? result = this.view.ShowDialog();
			return new OpenFromPackageTreeViewResult()
			{
				DialogOperationResult = result,
				SelectedPackage = viewModel.SelectedPakckageInfo,
				SelectedPath = viewModel.SelectPathViewModel.SelectedPath,
				SelectedSearchType = viewModel.SelectedSearchType,
				SelectedMethodFullName = viewModel.SelectedMethod?.FullName
			};
		}
	}
}
