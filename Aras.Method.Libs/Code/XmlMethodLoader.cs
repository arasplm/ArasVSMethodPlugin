//------------------------------------------------------------------------------
// <copyright file="XmlMethodLoader.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Aras.Method.Libs.Code
{
	public class XmlMethodLoader
	{
		public List<XmlMethodInfo> LoadMethods(List<string> paths)
		{
			List<XmlMethodInfo> methodInfos = new List<XmlMethodInfo>();

			foreach(string path in paths)
			{
				XmlMethodInfo methodInfo = LoadMethod(path);
				if (methodInfo != null)
				{
					methodInfos.Add(methodInfo);
				}
			}

			return methodInfos;
		}

		public XmlMethodInfo LoadMethod(string path)
		{
			XmlMethodInfo methodInfo = null;

			if (File.Exists(path))
			{
				try
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(path);
					XmlNode itemXmlNode = xmlDocument.SelectSingleNode("AML/Item");
					XmlNode nameTypeXmlNode = itemXmlNode.SelectSingleNode("name");
					XmlNode methodTypeXmlNode = itemXmlNode.SelectSingleNode("method_type");
					XmlNode methodCodeXmlNode = itemXmlNode.SelectSingleNode("method_code");

					methodInfo = new XmlMethodInfo()
					{
						Path = path,
						MethodName = nameTypeXmlNode.InnerText,
						MethodType = methodTypeXmlNode.InnerText,
						Code = methodCodeXmlNode.InnerText
					};
				}
				catch
				{

				}
			}

			return methodInfo;
		}
	}
}
