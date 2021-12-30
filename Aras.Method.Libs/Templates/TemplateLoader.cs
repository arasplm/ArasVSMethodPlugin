//------------------------------------------------------------------------------
// <copyright file="TemplateLoader.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Aras.Method.Libs.Templates
{
	public class TemplateLoader
	{
		private List<string> supportedTemplates = new List<string>();
		private List<TemplateInfo> templates = new List<TemplateInfo>();

		public TemplateLoader()
		{

		}

		public void Load(string templatesFileText)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(templatesFileText);

			var supportedTemlates = doc.GetElementsByTagName("Support");
			foreach (XmlNode item in supportedTemlates)
			{
				supportedTemplates.Add(item.Attributes["template"].InnerText);
			}
			supportedTemplates = supportedTemplates.Distinct().ToList();
			var templateElements = doc.GetElementsByTagName("Template");
			foreach (XmlNode item in templateElements)
			{
				var templateName = item.Attributes["name"].InnerText;

				bool isSuccessfullySupported = true;
				bool value;
				if (bool.TryParse(item.Attributes["isSupported"]?.InnerText, out value))
				{
					isSuccessfullySupported = value;
				}

				string message = item.Attributes["message"]?.InnerText;
				var templateIsSupported = supportedTemplates.Contains(templateName);
				var templateCode = item.InnerText;
				string templateLanguage = string.Empty;
				if (templateCode.Contains("Imports Microsoft.VisualBasic"))
				{
					templateLanguage = "VB";
				}
				if (templateCode.Contains("using System;"))
				{
					templateLanguage = "C#";
				}
				var template = new TemplateInfo()
				{
					TemplateName = templateName,
					IsSuccessfullySupported = isSuccessfullySupported,
					Message = message,
					IsSupported = templateIsSupported,
					TemplateCode = templateCode,
					TemplateLanguage = templateLanguage
				};

				templates.Add(template);
			}
		}

		public string GetMethodTemplateName(string methodCode)
		{
			const string methodTemplatePattern = @"//MethodTemplateName\s*=\s*(?<templatename>[^\W]*:?[\w.]*\(?[\w.:]*\)?)\s*";

			string templateName = string.Empty;
			Match methodTemplateNameMatch = Regex.Match(methodCode, methodTemplatePattern);
			if (methodTemplateNameMatch.Success)
			{
				templateName = methodTemplateNameMatch.Groups["templatename"].Value;
			}

			return templateName;
		}

		public TemplateInfo GetTemplateFromCodeString(string templateName, string methodLanguage)
		{
			return Templates.Where(t => t.TemplateLanguage == methodLanguage && t.TemplateName == templateName).FirstOrDefault();
		}

		public TemplateInfo GetDefaultTemplate(string methodLanguage)
		{
			return  Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
		}

		public List<TemplateInfo> Templates { get { return templates; } }
	}
}
