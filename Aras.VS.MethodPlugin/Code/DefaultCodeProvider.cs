//------------------------------------------------------------------------------
// <copyright file="DefaultCodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Code
{
	public class DefaultCodeProvider
	{
		public DefaultCodeTemplate GetDefaultCodeTemplate(string defaultTempalteFilePath, string templateName, string eventName)
		{
		    DefaultCodeTemplate defaultTemlate = null;

			if (Directory.Exists(defaultTempalteFilePath))
			{
			    var mappedTemplateName = TemplateMapper.GetAliasTemplateName(templateName);
			    var file = Directory.GetFiles(defaultTempalteFilePath)
			        .FirstOrDefault(fl => fl.Contains(mappedTemplateName + eventName));
			    if (file == null)
			    {
			        return null;
			    }

			    XmlDocument doc = new XmlDocument();
				doc.Load(file);
				var node = doc.GetElementsByTagName("default_code_template")[0];


				defaultTemlate = new DefaultCodeTemplate();
				defaultTemlate.TempalteName = node.Attributes["templateName"]?.InnerText;
				defaultTemlate.EventDataType = EventSpecificData.None;
				EventSpecificData eventData;
				if (Enum.TryParse<EventSpecificData>(node.Attributes["eventData"]?.InnerText, out eventData))
				{
					defaultTemlate.EventDataType = eventData;
				}

				defaultTemlate.WrapperSourceCode = node.SelectSingleNode("wrapper_code")?.InnerText;
				defaultTemlate.SimpleSourceCode = node.SelectSingleNode("simple_code/source_code")?.InnerText;
				defaultTemlate.SimpleUnitTestsCode = node.SelectSingleNode("simple_code/test_code")?.InnerText;
				defaultTemlate.AdvancedSourceCode = node.SelectSingleNode("advanced_code/source_code")?.InnerText;
				defaultTemlate.AdvancedUnitTestsCode = node.SelectSingleNode("advanced_code/test_code")?.InnerText;
			}
			return defaultTemlate;
		}
	}
}
