//------------------------------------------------------------------------------
// <copyright file="OpenFromArasViewResult.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class OpenFromArasViewResult : ViewResult
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
		public PackageInfo Package { get; set; }
		public string SelectedIdentityId { get; set; }
		public string SelectedIdentityKeyedName { get; set; }
		public bool IsUseVSFormattingCode { get; set; }
	}
}
