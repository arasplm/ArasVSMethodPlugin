using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.ProjectTemplates
{
	[TestFixture]
	public class ProjectTemplatesTests
	{
		string pathToZipFolder;
		readonly string[] expectedTemplateFiles = {
			"Aras12SP18MethodProject.zip",
			"Aras14010MethodProject.zip",
			"Aras14011MethodProject.zip",
			"Aras14012MethodProject.zip",
			"Aras14015MethodProject.zip",
			"Aras14018MethodProject.zip",
			"Aras14020MethodProject.zip",
			"Aras14022MethodProject.zip",
			"Aras14025MethodProject.zip",
			"Aras14028MethodProject.zip",
			"Aras14030MethodProject.zip",
			"Aras14034MethodProject.zip",
			"Aras14035MethodProject.zip",
			"Aras14036MethodProject.zip",
			"Aras14037MethodProject.zip",
			"Aras14038MethodProject.zip",
			"Aras14039MethodProject.zip",
			"Aras14040MethodProject.zip",
		};
		readonly List<string> listOfCommonFiles = new List<string> {
			"projectConfig.xml",
			"Attributes/PartialPathAttribute.cs",
			"Attributes/ExternalPathAttribute.cs",
			"MyTemplate.vstemplate",
			"method-config.xml",
			"GlobalSuppressions.cs",
			"__TemplateIcon.ico",
			"__PreviewImage.png",
			"Properties/AssemblyInfo.cs",

		};

		readonly List<string> listOfCommonDlls = new List<string> {
			"ArasLibs/Aras.Server.Core.dll",
			"ArasLibs/Aras.TDF.Base.dll",
			"ArasLibs/Aras.TDF.Base.Extensions.dll",
			"ArasLibs/Conversion.Base.dll",
			"ArasLibs/ConversionManager.dll",
			"ArasLibs/FileExchangeService.dll",
			"ArasLibs/IOM.dll",
			"ArasLibs/Newtonsoft.Json.dll",
		};

		readonly List<string> listOfEvents = new List<string> {
			"None",
			"FailedLogin",
			"SuccessfulLogin",
			"Logout",
			"OnVote",
			"OnRefuse",
			"OnDue",
			"OnAssign",
			"OnClose",
			"OnActivate",
			"OnRemind",
			"OnEscalate",
			"OnDelegate",
			"OnGet",
			"OnAfterAdd",
			"OnBeforeUpdate",
			"OnAfterUpdate",
			"OnAfterVersion",
			"OnBeforeDelete",
			"OnAfterDelete",
			"OnAfterCopy",
			"OnUpdate",
			"OnDelete",
			"OnBeforePromote",
			"OnPromote",
			"OnAfterPromote",
			"OnAfterResetLifecycle",
		};

		[SetUp]
		public void Init()
		{
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			pathToZipFolder = Path.Combine(currentPath, @"ProjectTemplates\CSharp\Aras Innovator\Methods");
		}


		[Test]
		public void TemplateSet_ShouldMatchSupportedVersions()
		{
			//Act
			var actualTemplateFiles = Directory.GetFiles(pathToZipFolder, "Aras*MethodProject.zip")
				.Select(Path.GetFileName)
				.ToArray();

			//Assert
			CollectionAssert.AreEquivalent(expectedTemplateFiles, actualTemplateFiles);
		}


		[TestCase("12SP18", "12sp18")]
		[TestCase("14015", "R27")]
		[TestCase("14035", "R35")]
		[TestCase("14036", "R36")]
		[TestCase("14037", "R37")]
		[TestCase("14038", "R38")]
		[TestCase("14039", "R39")]
		[TestCase("14040", "R40")]
		public void CheckForExistingCommonFiles(string version, string publicVersion)
		{
			//Arrange
			bool expectedResult;
			var files = GetCommonFilesByPublicVersion(publicVersion);
			//Action 
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras{version}MethodProject.zip"), FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen))
				{
					expectedResult = files.All(fileName => archive.Entries.FirstOrDefault(entry => entry.FullName.ToLowerInvariant() == fileName.ToLowerInvariant()) != null);
				}
			}

			//Assert
			Assert.IsTrue(expectedResult);
		}

		[TestCase("12SP18", "12sp18")]
		[TestCase("14015", "R27")]
		[TestCase("14035", "R35")]
		[TestCase("14036", "R36")]
		[TestCase("14037", "R37")]
		[TestCase("14038", "R38")]
		[TestCase("14039", "R39")]
		[TestCase("14040", "R40")]
		public void CheckForExistingDllLibs(string version, string publicVersion)
		{
			//Arrange
			bool expectedResult;
			var libs = GetDllLibsByPublicVersion(publicVersion);

			//Action 
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras{version}MethodProject.zip"), FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen))
				{
					expectedResult = libs.All(fileName => archive.Entries.FirstOrDefault(entry => entry.FullName == fileName) != null);
				}
			}

			//Assert
			Assert.IsTrue(expectedResult);
		}

		[TestCase("14038", "R38", "net8.0")]
		[TestCase("14039", "R39", "net10.0")]
		[TestCase("14040", "R40", "net10.0")]
		public void CheckTargetFramework(string version, string publicVersion, string targetFramework)
		{
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras{version}MethodProject.zip"), FileMode.Open))
			using (ZipArchive archive = new ZipArchive(zipToOpen))
			{
				ZipArchiveEntry projectFile = archive.GetEntry($"Aras.VS.MethodPlugin.{publicVersion}CSharp.csproj");
				Assert.That(projectFile, Is.Not.Null);

				using (var reader = new StreamReader(projectFile.Open()))
				{
					Assert.That(reader.ReadToEnd(), Does.Contain($"<TargetFramework>{targetFramework}</TargetFramework>"));
				}
			}
		}

		private List<string> GetCommonFilesByPublicVersion(string publicVersion)
		{
			var list = new List<string>(listOfCommonFiles)
			{
				$"Aras.VS.MethodPlugin.{publicVersion}CSharp.csproj"
			};
			if (publicVersion.StartsWith("12"))
			{
				list.Add("Rulesets/Aras.All.Rules.ruleset");
				list.Add("packages.config");
			}
			return list;
		}

		private List<string> GetDllLibsByPublicVersion(string publicVersion)
		{
			var list = new List<string>(listOfCommonDlls);
			if (publicVersion.StartsWith("12"))
			{
				list.Add("ArasLibs/Aras.ES.dll");
				list.Add("ArasLibs/SPConnector.dll");
			}
			else
			{
				list.Add("ArasLibs/Microsoft.Data.SqlClient.dll");
				list.Add("ArasLibs/SixLabors.ImageSharp.dll");
			}
			return list;
		}
	}
}

