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


		[TestCase("12SP18")]
		[TestCase("1406")]
		[TestCase("1407")]
		[TestCase("1408")]
		[TestCase("1409")]
		[TestCase("14010")]
		[TestCase("14011")]
		[TestCase("14012")]
		[TestCase("14015")]
		[TestCase("14018")]
		[TestCase("14020")]
		[TestCase("14022")]
		[TestCase("14025")]
		[TestCase("14028")]
		[TestCase("14030")]
		[TestCase("14034")]
		[TestCase("14035")]
		[TestCase("14036")]
		[TestCase("14037")]
		public void IsZipExists(string version)
		{
			//Act
			var isExists = File.Exists(Path.Combine(pathToZipFolder, $"Aras{version}MethodProject.zip"));

			//Assert
			Assert.IsTrue(isExists);
		}


		[TestCase("12SP18", "12sp18")]
		[TestCase("14015", "R27")]
		[TestCase("14035", "R35")]
		[TestCase("14036", "R36")]
		[TestCase("14037", "R37")]
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
			var list = new List<string>(listOfCommonFiles);
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

