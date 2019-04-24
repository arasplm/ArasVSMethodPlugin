//------------------------------------------------------------------------------
// <copyright file="SaveToPackageViewResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class SaveToPackageViewResult : ViewResult
	{
		public string PackagePath { get; set; }
		public string SelectedPackage { get; set; }
		public MethodInfo MethodInformation { get; set; }
		public string SelectedIdentityKeyedName { get; set; }
		public string SelectedIdentityId { get; set; }
		public string MethodCode { get; set; }
		public string MethodName { get; set; }
		public string MethodComment { get; set; }
	}
}
