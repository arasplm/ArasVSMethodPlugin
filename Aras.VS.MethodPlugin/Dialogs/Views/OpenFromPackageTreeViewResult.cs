using Aras.Method.Libs.Aras.Package;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromPackageTreeViewResult : ViewResult
	{
		public PackageInfo SelectedPackage { get; set; }
		public string SelectedPath { get; set; }
		public string SelectedSearchType { get; set; }
		public string SelectedMethodFullName { get; set; }
	}
}
