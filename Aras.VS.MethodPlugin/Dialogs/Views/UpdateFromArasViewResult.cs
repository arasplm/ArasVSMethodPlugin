//------------------------------------------------------------------------------
// <copyright file="UpdateFromArasViewResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class UpdateFromArasViewResult : ViewResult
	{ 
		public TemplateInfo SelectedTemplate { get; set; }
		public EventSpecificData EventSpecificData { get; set; }
		public string MethodLanguage { get; set; }
		public string MethodCode { get; set; }
		public string MethodType { get; set; }
		public string MethodName { get; set; }
		public string MethodConfigId { get; set; }
		public string MethodId { get; set; }
		public string MethodComment { get; set; }
		public string PackageName { get; set; }
		public string ExecutionIdentityId { get; set; }
		public string ExecutionIdentityKeyedName { get; set; }
		public bool IsUseVSFormattingCode { get; set; }
	}
}
