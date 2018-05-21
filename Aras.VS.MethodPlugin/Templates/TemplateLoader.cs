//------------------------------------------------------------------------------
// <copyright file="TemplateLoader.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Aras.VS.MethodPlugin.Templates
{
	public class TemplateLoader
	{
		private List<string> references = new List<string>();
		private List<string> supportedTemplates = new List<string>();
		List<TemplateInfo> templates = new List<TemplateInfo>();

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
					IsSupported = templateIsSupported,
					TemplateCode = templateCode,
					TemplateLanguage = templateLanguage
				};

				templates.Add(template);
			}
			var referenceNodes = doc.SelectNodes("MethodConfig/ReferencedAssemblies/name");
			foreach (XmlNode item in referenceNodes)
			{
				references.Add(item.InnerText);
			}
		}

		public TemplateInfo GetTemplateFromCodeString(string code, string language)
		{
			TemplateInfo template = null;
			string methodTemplatePattern = @"//MethodTemplateName\s*=\s*(?<templatename>[^\W]*)\s*";
			Match methodTemplateNameMatch = Regex.Match(code, methodTemplatePattern);
			if (methodTemplateNameMatch.Success)
			{
				string value = methodTemplateNameMatch.Groups["templatename"].Value;
				template = Templates.Where(t => t.TemplateLanguage == language && t.TemplateName == value).FirstOrDefault();
			}
			if (template == null)
			{
				template = Templates.Where(t => t.TemplateLanguage == language && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception(string.Empty);
			}

			return template;
		}

		public List<TemplateInfo> Templates { get { return templates; } }
	}
}
