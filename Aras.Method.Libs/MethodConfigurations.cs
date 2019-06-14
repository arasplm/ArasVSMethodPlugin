//------------------------------------------------------------------------------
// <copyright file="MethodConfig.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Aras.Method.Libs.Templates;

namespace Aras.Method.Libs
{
	public class MethodConfig
	{
		private List<string> referencedAssemblies = new List<string>();
		private TemplateLoader templateLoader = new TemplateLoader();

		public MethodConfig()
		{

		}

		public List<string> ReferencedAssemblies { get { return referencedAssemblies; } }
		public TemplateLoader TemplateLoader { get { return templateLoader; } }

		public void Load(string fullPath)
		{
			LoadReferencedAssemblies(fullPath);
			templateLoader.Load(fullPath);
		}

		private void LoadReferencedAssemblies(string fullPath)
		{
			XDocument methodConfig = XDocument.Load(fullPath);
			XElement assembliesElements = methodConfig.Descendants("ReferencedAssemblies").FirstOrDefault();
			if (assembliesElements != null)
			{
				foreach (XElement descendant in assembliesElements.Descendants("name"))
				{
					referencedAssemblies.Add(descendant.Value);
				}
			}
		}
	}
}
