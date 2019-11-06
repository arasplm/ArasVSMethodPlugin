//------------------------------------------------------------------------------
// <copyright file="SaveMethodViewResult.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Aras.Package;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class SaveMethodViewResult : ViewResult
	{
		public string MethodCode { get; set; }
		public string TemplateName { get; set; }
		public string MethodName { get; set; }
		public string MethodLanguage { get; set; }
		public string SelectedIdentityId { get; set; }
		public string SelectedIdentityKeyedName { get; set; }
		public PackageInfo SelectedPackageInfo { get; set; }
		public string CurrentMethodPackage { get; set; }
		public dynamic MethodItem { get; set; }
		public string MethodComment { get; set; }
	}
}
