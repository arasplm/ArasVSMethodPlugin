using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.OpenMethodInVS
{
	[TestFixture]
	public class ImportOpenInVSActionAmlTests
	{
		[Test]
		public void ImportedMethods_ShouldUseSupportedVersionVariablesAndJavaScriptType()
		{
			string amlPath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				@"OpenMethodInVS\Import\ImportOpenInVSActionAML.xml");
			XmlDocument document = new XmlDocument();
			document.Load(amlPath);

			XmlElement canExecuteMethod = GetMethod(document, "VSP_CanExecuteOpenInVS");
			XmlElement openInVsMethod = GetMethod(document, "VSP_OpenInVS");

			Assert.That(canExecuteMethod["method_type"].InnerText, Is.EqualTo("JavaScript"));
			Assert.That(openInVsMethod["method_type"].InnerText, Is.EqualTo("JavaScript"));

			string methodCode = openInVsMethod["method_code"].InnerText;
			Assert.That(methodCode, Does.Contain("VersionMajor"));
			Assert.That(methodCode, Does.Contain("VersionMinor"));
			Assert.That(methodCode, Does.Not.Contain("VersionServicePack"));
			Assert.That(methodCode, Does.Not.Contain("servicePack"));
		}

		private static XmlElement GetMethod(XmlDocument document, string methodName)
		{
			XmlElement method = document.SelectSingleNode(
				string.Format("/AML/Item[@type='Method'][name='{0}']", methodName)) as XmlElement;
			Assert.That(method, Is.Not.Null, "Method {0} is missing from the import AML.", methodName);
			return method;
		}
	}
}