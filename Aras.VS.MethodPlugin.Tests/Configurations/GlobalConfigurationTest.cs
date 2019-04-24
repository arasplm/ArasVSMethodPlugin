using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Aras.Method.Libs;
using Aras.VS.MethodPlugin.Configurations;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Configurations
{
	[TestFixture]
	public class GlobalConfigurationTest
	{
		private IIOWrapper iOWrapper;

		[SetUp]
		public void SetUp()
		{
			this.iOWrapper = Substitute.For<IIOWrapper>();
		}

		[Test]
		public void Ctor_ShouldThrowException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(null);
			});
		}

		[Test]
		public void Ctor_ShouldInitXmlDocument()
		{
			//Arange
			string configFilePath = @"UserDocuments\Aras Innovator Method Plugin\configFile.xml";
			MockConfigFilePath(configFilePath);
			this.iOWrapper.FileExists(configFilePath).Returns(false);

			//Act
			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Assert
			Assert.AreEqual("<configuration><userCodeTemplates /></configuration>", globalConfigurationTest.ConfigXmlDocument.InnerXml);
		}

		[Test]
		public void Ctor_ShouldLoadExistedXmlDocument()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string configFilePath = Path.Combine(currentPath, @"Configurations\TestData\config.xml");
			MockConfigFilePath(configFilePath);
			this.iOWrapper.FileExists(configFilePath).Returns(File.Exists(configFilePath));

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(configFilePath);
			this.iOWrapper.XmlDocumentLoad(configFilePath).Returns(xmlDocument);

			//Act
			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Assert
			Assert.AreEqual(xmlDocument.InnerXml, globalConfigurationTest.ConfigXmlDocument.InnerXml);
		}

		[Test]
		public void Save_ShouldReceivedIOWrapperSave()
		{
			//Arange
			string configFilePath = @"UserDocuments\Aras Innovator Method Plugin\configFile.xml";
			this.iOWrapper.FileExists(configFilePath).Returns(false);
			MockConfigFilePath(configFilePath);

			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Act
			globalConfigurationTest.Save();

			//Assert
			this.iOWrapper.Received().XmlDocumentSave(Arg.Is<XmlDocument>(x => x.InnerXml == "<configuration><userCodeTemplates /></configuration>"), configFilePath);
		}

		[Test]
		public void GetUserCodeTemplatesPaths_Should()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string configFilePath = Path.Combine(currentPath, @"Configurations\TestData\config.xml");
			MockConfigFilePath(configFilePath);
			this.iOWrapper.FileExists(configFilePath).Returns(File.Exists(configFilePath));

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(configFilePath);
			this.iOWrapper.XmlDocumentLoad(configFilePath).Returns(xmlDocument);
			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Act
			List<string> actualUserCodeTemlates = globalConfigurationTest.GetUserCodeTemplatesPaths();

			//Assert
			Assert.AreEqual(2, actualUserCodeTemlates.Count);
			foreach (var actualUserCodeTemlate in actualUserCodeTemlates)
			{
				XmlNode userCodeTemlate = xmlDocument.SelectSingleNode($"//userCodeTemplate[text()=\"{actualUserCodeTemlate}\"]");
				Assert.IsNotNull(userCodeTemlate);
			}
		}

		[Test]
		public void AddUserCodeTemplatePath_Should()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string configFilePath = Path.Combine(currentPath, @"Configurations\TestData\config.xml");
			MockConfigFilePath(configFilePath);
			this.iOWrapper.FileExists(configFilePath).Returns(File.Exists(configFilePath));

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(configFilePath);
			this.iOWrapper.XmlDocumentLoad(configFilePath).Returns(xmlDocument);

			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Act
			globalConfigurationTest.AddUserCodeTemplatePath("testUserCodeTemplatePath3");

			//Assert
			Assert.NotNull(globalConfigurationTest.ConfigXmlDocument.SelectSingleNode("//userCodeTemplate[text()=\"testUserCodeTemplatePath3\"]"));
		}

		[Test]
		public void RemoveUserCodeTemplatePath_Should()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string configFilePath = Path.Combine(currentPath, @"Configurations\TestData\config.xml");
			MockConfigFilePath(configFilePath);
			this.iOWrapper.FileExists(configFilePath).Returns(File.Exists(configFilePath));

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(configFilePath);
			this.iOWrapper.XmlDocumentLoad(configFilePath).Returns(xmlDocument);

			GlobalConfigurationTestProxy globalConfigurationTest = new GlobalConfigurationTestProxy(this.iOWrapper);

			//Act
			globalConfigurationTest.RemoveUserCodeTemplatePath("testUserCodeTemplatePath2");

			//Assert
			Assert.AreEqual(1, globalConfigurationTest.ConfigXmlDocument.SelectNodes("//userCodeTemplate").Count);
		}

		private void MockConfigFilePath(string filePath)
		{
			string userDocumentFolder = "UserDocuments";
			string projectFolderName = "Aras Innovator Method Plugin";
			string userDocumentFolderPath = @"UserDocuments\Aras Innovator Method Plugin";

			this.iOWrapper.EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments).Returns(userDocumentFolder);
			this.iOWrapper.PathCombine(userDocumentFolder, projectFolderName).Returns(userDocumentFolderPath);
			this.iOWrapper.DirectoryExists(userDocumentFolderPath).Returns(true);
			this.iOWrapper.PathCombine(userDocumentFolderPath, "config.xml").Returns(filePath);
		}
	}

	internal class GlobalConfigurationTestProxy : GlobalConfiguration
	{
		public GlobalConfigurationTestProxy(IIOWrapper iIOWrapper) : base(iIOWrapper)
		{
		}

		public XmlDocument ConfigXmlDocument => base.configXmlDocument;
	}
}
