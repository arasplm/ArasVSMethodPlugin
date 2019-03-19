using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters
{
	class OpenFromPackageTreeViewAdapterTest : IViewAdaper<OpenFromPackageTreeView, OpenFromPackageTreeViewResult>
	{
		private bool dialogResult;
		private string selectedPackageName;
		private string selectedPath;
		private string selectedSearchType;
		private string selectedMethodFullName;

		public OpenFromPackageTreeViewAdapterTest(bool dialogResult, string selectedPackageName, string selectedPath, string selectedSearchType, string selectedMethodFullName)
		{
			this.dialogResult = dialogResult;
			this.selectedPackageName = selectedPackageName;
			this.selectedPath = selectedPath;
			this.selectedSearchType = selectedSearchType;
			this.selectedMethodFullName = selectedMethodFullName;
		}

		public OpenFromPackageTreeViewResult ShowDialog()
		{
			return new OpenFromPackageTreeViewResult()
			{
				DialogOperationResult = this.dialogResult,
				SelectedPackageName = this.selectedPackageName,
				SelectedPath = this.selectedPath,
				SelectedSearchType = this.selectedSearchType,
				SelectedMethodFullName = this.selectedMethodFullName
			};
		}
	}
}
