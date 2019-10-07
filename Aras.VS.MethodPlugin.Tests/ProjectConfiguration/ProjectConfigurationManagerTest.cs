using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.ProjectConfiguration
{
	[TestFixture]
	public class ProjectConfigurationManagerTest
	{
		private MessageManager messageManager;
		private ProjectConfigurationManager projectConfigurationManager;

		[SetUp]
		public void Init()
		{
			messageManager = new VisualStudioMessageManager();
			projectConfigurationManager = new ProjectConfigurationManager(messageManager);
		}

		[Test]
		public void Load_ShouldThrowException()
		{
			//Arrange
			var configPath = "";

			//Act
			var testDelegate = new TestDelegate(() => projectConfigurationManager.Load(configPath));

			//Assert
			Assert.Throws<Exception>(testDelegate);
		}

		[Test]
		public void Load_ShouldReturnCorrectEmptyConfiguration()
		{
			//Arrange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var configPath = Path.Combine(currentPath, @"ProjectConfiguration\TestData\EmptyProjectConfig.xml");

			//Act
			projectConfigurationManager.Load(configPath);
			var expected = projectConfigurationManager.CurrentProjectConfiguraiton;

			//Assert
			Assert.AreEqual(expected.Connections.Count, 0);
			Assert.AreEqual(expected.LastSavedSearch.Count, 0);
			Assert.AreEqual(expected.LastSelectedMfFile, string.Empty);
			Assert.AreEqual(expected.MethodInfos.Count, 0);
			Assert.AreEqual(expected.LastSelectedDir, string.Empty);
			Assert.AreEqual(expected.UseVSFormatting, false);
		}

		[Test]
		public void Load_ShouldReturnCorrectFilledConfiguration()
		{
			//Arrange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var configPath = Path.Combine(currentPath, @"ProjectConfiguration\TestData\FilledProjectConfig.xml");

			//Act
			projectConfigurationManager.Load(configPath);
			var expected = projectConfigurationManager.CurrentProjectConfiguraiton;

			//Assert
			Assert.AreEqual(expected.Connections.First().Database, "110SP15");
			Assert.AreEqual(expected.Connections.First().ServerUrl, "http://localhost/SP15/");
			Assert.AreEqual(expected.Connections.First().Login, "admin");
			Assert.AreEqual(expected.Connections.First().LastConnection, true);
			Assert.AreEqual(expected.LastSavedSearch.First().Key, "Method");
			Assert.AreEqual(expected.LastSavedSearch.First().Value.First().PropertyName, "name");
			Assert.AreEqual(expected.LastSavedSearch.First().Value.First().PropertyValue, "mso*");
			Assert.AreEqual(expected.LastSelectedMfFile, @"D:\file.mf");
			Assert.AreEqual(expected.MethodInfos.First().InnovatorMethodConfigId, "6D5D2A114135409D82561DC1C422C87F");
			Assert.AreEqual(expected.MethodInfos.First().InnovatorMethodId, "6B6E21E655CA4A1093FB9E970463F061");
			Assert.AreEqual(expected.MethodInfos.First().MethodName, "Method1");
			Assert.AreEqual(expected.MethodInfos.First().MethodType, "server");
			Assert.AreEqual(expected.MethodInfos.First().MethodComment, "");
			Assert.AreEqual(expected.MethodInfos.First().MethodLanguage, "C#");
			Assert.AreEqual(expected.MethodInfos.First().TemplateName, "CSharp");
			Assert.AreEqual(expected.MethodInfos.First().Package.Name, "MSO_Standard");
			Assert.AreEqual(expected.MethodInfos.First().ExecutionAllowedToId, "A73B655731924CD0B027E4F4D5FCC0A9");
			Assert.AreEqual(expected.MethodInfos.First().ExecutionAllowedToKeyedName, "World");
			Assert.AreEqual(expected.LastSelectedDir, @"C:\");
			Assert.AreEqual(expected.UseVSFormatting, true);
		}

		[Test]
		public void Save_SaveEmptyConfiguration()
		{
			//Arrange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var pathForConfig = Path.Combine(currentPath, "ProjectConfiguration\\TestData\\projectConfig.xml");
			File.Delete(pathForConfig);

			//Act
			projectConfigurationManager.Save(pathForConfig);

			//Assert
			Assert.That(File.ReadAllText(pathForConfig),
				Is.EqualTo(File.ReadAllText(Path.Combine(currentPath, "ProjectConfiguration\\TestData\\ExpectedEmptyConfig.txt"))).NoClip);
		}

		[Test]
		public void Save_SaveFilledConfiguration()
		{ 
			//Arrange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var pathForConfig = Path.Combine(currentPath, "ProjectConfiguration\\TestData\\projectConfig.xml");
			File.Delete(pathForConfig);
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch = new Dictionary<string, List<PropertyInfo>>
				{
					{ "Method", new List<PropertyInfo>
						{
							new PropertyInfo()
							{
								PropertyName = "name",
								PropertyValue = "mso*"
							}
						}
					}
				};
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedDir = @"C:\";
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedMfFile = @"C:\file.mf";
			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = true;
			projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos = new List<MethodInfo>
				{
					new MethodInfo
					{
						EventData = Aras.Method.Libs.Code.EventSpecificData.None,
						ExecutionAllowedToId = "A73B655731924CD0B027E4F4D5FCC0A9",
						ExecutionAllowedToKeyedName = "World",
						InnovatorMethodConfigId = "6D5D2A114135409D82561DC1C422C87F",
						InnovatorMethodId = "6B6E21E655CA4A1093FB9E970463F061",
						MethodComment = "",
						MethodLanguage = "C#",
						MethodName = "Method1",
						MethodType = "server",
						Package = new PackageInfo("MSO_Standard"),
						TemplateName = "CSharp"
					}
				};

			//Act
			projectConfigurationManager.Save(pathForConfig);

			//Assert
			Assert.That(File.ReadAllText(pathForConfig),
				Is.EqualTo(File.ReadAllText(Path.Combine(currentPath, "ProjectConfiguration\\TestData\\ExpectedFilledConfig.txt"))).NoClip);
		}
	}
}
