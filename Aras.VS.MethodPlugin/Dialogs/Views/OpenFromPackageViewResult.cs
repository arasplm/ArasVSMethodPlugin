//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageViewResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromPackageViewResult : ViewResult
	{
		public string MethodLanguage { get; set; }
		public string MethodCode { get; set; }
		public string MethodType { get; set; }
		public string MethodName { get; set; }
		public string MethodConfigId { get; set; }
		public string MethodId { get; set; }
		public string MethodComment { get; set; }
		public TemplateInfo SelectedTemplate { get; set; }
		public EventSpecificDataType SelectedEventSpecificData { get; set; }
		public string Package { get; set; }
		public string IdentityId { get; set; }
		public string IdentityKeyedName { get; set; }
		public string SelectedFolderPath { get; set; }
		public string SelectedManifestFileName { get; set; }
        public string SelectedManifestFullPath { get; set; }
        public bool IsUseVSFormattingCode{ get; set; }
	}
}
