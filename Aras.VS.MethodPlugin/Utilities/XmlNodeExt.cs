//------------------------------------------------------------------------------
// <copyright file="XmlNodeExt.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Xml;

namespace Aras.VS.MethodPlugin.Utilities
{
	internal static class XmlNodeExt
	{
		internal static string GetXmlNodePropertyValue(this XmlNode node, string childPropertyName, string defaultValue = "")
		{
			string value = node.SelectSingleNode(childPropertyName).InnerText ?? defaultValue;
			return value;
		}

		internal static string GetXmlNodePropertyAttribute(this XmlNode node, string childPropertyName, string attributeName, string defaultValue = "")
		{
			var selectSingleNode = node.SelectSingleNode(childPropertyName);

			if (selectSingleNode?.Attributes == null)
			{
				return defaultValue;
			}

			var value = selectSingleNode.Attributes[attributeName].Value ?? defaultValue;
			return value;
		}
	}
}
