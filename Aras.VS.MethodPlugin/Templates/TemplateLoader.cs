//------------------------------------------------------------------------------
// <copyright file="TemplateLoader.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace Aras.VS.MethodPlugin.Templates
{
	public class TemplateLoader
	{
		private readonly IDialogFactory dialogFactory;
		private readonly IVsUIShell vsUIShell;

		private List<string> supportedTemplates = new List<string>();
		List<TemplateInfo> templates = new List<TemplateInfo>();

		public TemplateLoader(IDialogFactory dialogFactory, IVsUIShell vsUIShell)
		{
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (vsUIShell == null) throw new ArgumentNullException(nameof(vsUIShell));

			this.dialogFactory = dialogFactory;
			this.vsUIShell = vsUIShell;
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

		public TemplateInfo GetTemplateFromCodeString(string methodCode, string methodLanguage, string operationName)
		{
			TemplateInfo template = null;
			string methodTemplatePattern = @"//MethodTemplateName\s*=\s*(?<templatename>[^\W]*:?[\w.]*\(?[\w.:]*\)?)\s*";
			Match methodTemplateNameMatch = Regex.Match(methodCode, methodTemplatePattern);

			if (methodTemplateNameMatch.Success)
			{
				string templateName = methodTemplateNameMatch.Groups["templatename"].Value;
				template = Templates.Where(t => t.TemplateLanguage == methodLanguage && t.TemplateName == templateName).FirstOrDefault();

				if (template == null)
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow(this.vsUIShell);
					messageWindow.ShowDialog($"The template {templateName} from selected method not found. Default template will be used.",
						operationName,
						MessageButtons.OK,
						MessageIcon.Information);
				}
			}

			if (template == null)
			{
				template = Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
			}

			return template;
		}

		public List<TemplateInfo> Templates { get { return templates; } }
	}
}
