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
			"PartialPathAttribute.cs",
			"MyTemplate.vstemplate",
			"method-config.xml",
			"GlobalSuppressions.cs",
			"__TemplateIcon.ico",
			"__PreviewImage.png",
			"Rulesets/Aras.All.Rules.ruleset",
			"Properties/AssemblyInfo.cs",
			"packages.config",

		};

		readonly List<string> listOfCommonDlls = new List<string> {
			"ArasLibs/Aras.Analyzers.dll",
			"ArasLibs/Aras.ES.dll",
			"ArasLibs/Aras.TDF.Base.dll",
			"ArasLibs/Conversion.Base.dll",
			"ArasLibs/ConversionManager.dll",
			"ArasLibs/FileExchangeService.dll",
			"ArasLibs/InnovatorCore.dll",
			"ArasLibs/IOM.dll",
			"ArasLibs/SPConnector.dll"
		};

		readonly List<string> listOfCommonMethods = new List<string>
		{
			"DefaultCodeTemplates/CDLocRule",
			"DefaultCodeTemplates/CDLRSimpl",
			"DefaultCodeTemplates/CSharpIO",
			"DefaultCodeTemplates/CSharp",
			"DefaultCodeTemplates/TDFExtend",
			"DefaultCodeTemplates/TDFStrict"
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


		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[TestCase(11)]
		[TestCase(12)]
		[TestCase(14)]
		[TestCase(15)]

		public void IsZipExists(int spVersion)
		{
			//Act
			var isExists = File.Exists(Path.Combine(pathToZipFolder, $"Aras11SP{spVersion}MethodProject.zip"));

			//Assert
			Assert.IsTrue(isExists);
		}

		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[TestCase(11)]
		[TestCase(12)]
		[TestCase(14)]
		[TestCase(15)]
		public void CheckForExistingCommonFiles(int spVersion)
		{
			//Arrange
			bool expectedResult;

			var files = GetCommonFilesBySpVersion(spVersion);
			//Action 
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras11SP{spVersion}MethodProject.zip"), FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen))
				{
					expectedResult = files.All(fileName => archive.Entries.FirstOrDefault(entry => entry.FullName.ToLowerInvariant() == fileName.ToLowerInvariant()) != null);
				}
			}

			//Assert
			Assert.IsTrue(expectedResult);
		}

		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[TestCase(11)]
		[TestCase(12)]
		[TestCase(14)]
		[TestCase(15)]
		public void CheckForExistingDllLibs(int spVersion)
		{
			//Arrange
			bool expectedResult;
			var libs = GetDllLibsBySpVersion(spVersion);

			//Action 
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras11SP{spVersion}MethodProject.zip"), FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen))
				{
					expectedResult = libs.All(fileName => archive.Entries.FirstOrDefault(entry => entry.FullName == fileName) != null);
				}
			}

			//Assert
			Assert.IsTrue(expectedResult);
		}


		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[TestCase(11)]
		[TestCase(12)]
		[TestCase(14)]
		[TestCase(15)]
		public void CheckForExistingMethodsXml(int spVersion)
		{
			//Arrange
			bool expectedResult;
			var methodNames = GetMethodsNamesBySpVersion(spVersion);

			//Action 
			using (FileStream zipToOpen = new FileStream(Path.Combine(pathToZipFolder, $"Aras11SP{spVersion}MethodProject.zip"), FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen))
				{
					expectedResult = methodNames.All(methodName => archive.Entries.FirstOrDefault(entry => entry.FullName == methodName) != null);
				}
			}

			//Assert
			Assert.IsTrue(expectedResult);
		}

		private List<string> GetCommonFilesBySpVersion(int spVersion)
		{
			var list = new List<string>(listOfCommonFiles);
			list.Add($"Aras.VS.MethodPlugin.11sp{spVersion}CSharp.csproj");
			return list;
		}

		private List<string> GetDllLibsBySpVersion(int spVersion)
		{
			var list = new List<string>(listOfCommonFiles);
			if (spVersion >= 12)
			{
				list.Add("ArasLibs/Aras.TreeGridView.dll");
			}
			return list;
		}

		private List<string> GetMethodsNamesBySpVersion(int spVersion)
		{
			var list = new List<string>(listOfCommonMethods);

			if (spVersion >= 12)
			{
				list.Add("DefaultCodeTemplates/CoreAccess");
				list.Add("DefaultCodeTemplates/CoreConfig");
				list.Add("DefaultCodeTemplates/CoreScope");
				list.Add("DefaultCodeTemplates/TreeGrid");
			}

			return list.SelectMany(method => listOfEvents.Select(evnt => method + evnt + ".xml")).ToList();
		}
	}
}

