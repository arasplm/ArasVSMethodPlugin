namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromPackageTreeViewResult : ViewResult
	{
		public string SelectedPackageName { get; set; }
		public string SelectedPath { get; set; }
		public string SelectedSearchType { get; set; }
		public string SelectedMethodFullName { get; set; }
	}
}
