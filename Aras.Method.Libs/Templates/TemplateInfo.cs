//------------------------------------------------------------------------------
// <copyright file="TemplateInfo.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Templates
{
	public class TemplateInfo
	{
		public string TemplateLanguage { get; set; }

		public bool IsSupported { get; set; }

		public string TemplateName { get; set; }

		public string TemplateCode { get; set; }

		public string TemplateLabel { get; set; }

		public bool IsSuccessfullySupported { get; set; }

		public string Message { get; set; }

		public override string ToString()
		{
			return TemplateName;
		}
	}
}
