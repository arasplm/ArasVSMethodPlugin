//------------------------------------------------------------------------------
// <copyright file="CreateMethodViewResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreateMethodViewResult : ViewResult
	{
		public TemplateInfo SelectedTemplate { get; set; }

		public EventSpecificDataType SelectedEventSpecificData { get; set; }

		public string MethodName { get; set; }

		public string MethodComment { get; set; }

		public bool UseRecommendedDefaultCode { get; set; }

		public ListInfo SelectedActionLocation { get; set; }

		public string SelectedPackage { get; set; }

		public string SelectedIdentityId { get; set; }

		public FilteredListInfo SelectedLanguage { get; set; }

		public string SelectedIdentityKeyedName { get; set; }

		public bool IsUseVSFormattingCode { get; set; }

		public XmlMethodInfo SelectedUserCodeTemplate { get; set; }
	}
}
