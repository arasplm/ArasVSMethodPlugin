using Aras.Method.Libs.Aras.Package;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters
{
	class OpenFromPackageTreeViewAdapterTest : IViewAdaper<OpenFromPackageTreeView, OpenFromPackageTreeViewResult>
	{
		private bool dialogResult;
		private PackageInfo selectedPackage;
		private string selectedPath;
		private string selectedSearchType;
		private string selectedMethodFullName;

		public OpenFromPackageTreeViewAdapterTest(bool dialogResult, PackageInfo selectedPackage, string selectedPath, string selectedSearchType, string selectedMethodFullName)
		{
			this.dialogResult = dialogResult;
			this.selectedPackage = selectedPackage;
			this.selectedPath = selectedPath;
			this.selectedSearchType = selectedSearchType;
			this.selectedMethodFullName = selectedMethodFullName;
		}

		public OpenFromPackageTreeViewResult ShowDialog()
		{
			return new OpenFromPackageTreeViewResult()
			{
				DialogOperationResult = this.dialogResult,
				SelectedPackage = this.selectedPackage,
				SelectedPath = this.selectedPath,
				SelectedSearchType = this.selectedSearchType,
				SelectedMethodFullName = this.selectedMethodFullName
			};
		}
	}
}
