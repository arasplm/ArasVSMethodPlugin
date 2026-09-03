using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.Views.Styles
{
	[TestFixture]
	public class ComboBoxControlTests
	{
		[Test]
		public void LocalDynamicResources_ShouldBeDefinedInResourceDictionary()
		{
			string xamlPath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				@"Dialogs\Views\Styles\ComboBoxControl.xaml");
			XmlDocument document = new XmlDocument();
			document.Load(xamlPath);

			const string xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
			HashSet<string> resourceKeys = new HashSet<string>(StringComparer.Ordinal);
			foreach (XmlElement element in document.SelectNodes("//*"))
			{
				string resourceKey = element.GetAttribute("Key", xamlNamespace);
				if (!string.IsNullOrEmpty(resourceKey))
				{
					resourceKeys.Add(resourceKey);
				}
			}

			const string dynamicResourcePrefix = "{DynamicResource ";
			List<string> missingResourceKeys = new List<string>();
			foreach (XmlAttribute attribute in document.SelectNodes("//@*"))
			{
				if (!attribute.Value.StartsWith(dynamicResourcePrefix, StringComparison.Ordinal)
					|| attribute.Value.Contains("{x:Static"))
				{
					continue;
				}

				string resourceKey = attribute.Value
					.Substring(dynamicResourcePrefix.Length)
					.TrimEnd('}');
				if (!resourceKeys.Contains(resourceKey))
				{
					missingResourceKeys.Add(resourceKey);
				}
			}

			Assert.That(missingResourceKeys, Is.Empty,
				"The resource dictionary contains references to undefined local resources.");
		}
	}
}