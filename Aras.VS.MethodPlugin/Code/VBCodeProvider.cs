//------------------------------------------------------------------------------
// <copyright file="VBCodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using EnvDTE80;

namespace Aras.VS.MethodPlugin.Code
{
	public class VBCodeProvider : ICodeProvider
	{
		private readonly Project project;
		private readonly ProjectConfiguraiton projectConfiguration;

		public string Language
		{
			get { return "VB"; }
		}

		public VBCodeProvider(Project project, ProjectConfiguraiton projectConfiguration)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));

			this.project = project;
			this.projectConfiguration = projectConfiguration;
		}

		public void LoadCodeToProject(dynamic methodItem)
		{
			string methodLanguage = methodItem.getProperty("method_type");
			string methodCode = methodItem.getProperty("method_code");
			string methodLocation = methodItem.getProperty("method_location");
			string methodName = methodItem.getProperty("name");
			string methodNameWhithoutWhitespaces = Regex.Replace(methodName, @"[^\w]", "_");
			string innovatorMethodConfigId = methodItem.getProperty("config_id");
			string innovatorMethodId = methodItem.getID();
			string methodExecutionAllowedToId = methodItem.getProperty("execution_allowed_to");
			string methodExecutionAllowedToKeyedName = methodItem.getPropertyAttribute("execution_allowed_to", "keyed_name");

			ProjectItem methodConfigFile = project.ProjectItems.Item("method-config.xml");
			string methodConfigPath = methodConfigFile.FileNames[0];
			var templateLoader = new TemplateLoader();
			templateLoader.Load(methodConfigPath);
			TemplateInfo template = null;

			Match methodTemplateNameMatch = Regex.Match(methodCode, @"MethodTemplateName\s*=\s*(?<templatename>[^\W]*)\s*\;");
			if (methodTemplateNameMatch.Success)
			{
				string value = methodTemplateNameMatch.Groups["templatename"].Value;
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.TemplateName == value && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception(string.Empty);
			}

			ProjectItem serverMethodFolderItem = project.ProjectItems.Item("ServerMethods");
			Solution2 sln = (Solution2)project.DTE.Solution;
			string itemPath = sln.GetProjectItemTemplate("Class.zip", "vbproj");

			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = innovatorMethodConfigId,
				InnovatorMethodId = innovatorMethodId,
				MethodLanguage = methodLanguage,
				MethodName = methodName,
				MethodType = methodLocation,
				PackageName = "some_package",
				TemplateName = template.TemplateName,
				ExecutionAllowedToId = methodExecutionAllowedToId,
				ExecutionAllowedToKeyedName = methodExecutionAllowedToKeyedName
			};

			Dictionary<string, string> methodParts = generateMethodParts(ref methodCode);
			string methodSourceCode = GenerateSourceCodeByTemplate(template, methodNameWhithoutWhitespaces, methodCode, CommonData.EventSpecificDataTypeList.First());

			if (methodParts.Count > 0)
			{
				int index = methodSourceCode.IndexOf("Class", 0);
				methodSourceCode = methodSourceCode.Insert(index, "Partial ");
			}

			if (methodParts.Count > 0)
			{
				int index = methodSourceCode.IndexOf("Class", methodSourceCode.IndexOf("\r\n") + 1) + 5;
				string templateCode = $"{methodSourceCode.Substring(0, index)}\r\n$(MethodCode)\r\nEnd Class\r\nEnd Namespace";

				foreach (var part in methodParts)
				{
					string[] directories = Path.GetDirectoryName(part.Key).Split(Path.DirectorySeparatorChar);
					ProjectItems folder = serverMethodFolderItem.ProjectItems;

					foreach (string directory in directories)
					{
						if (string.IsNullOrEmpty(directory))
						{
							continue;
						}

						try
						{
							folder = folder.Item("Root").ProjectItems;
							continue;
						}
						catch { }

						folder = folder.AddFolder(directory).ProjectItems;
					}

					string partialName = Path.GetFileName(part.Key);
					string partialNameWhithoutWhitespaces = Regex.Replace(partialName, @"[^\w]", "_");
					string partialNameWithExtension = !Path.HasExtension(partialNameWhithoutWhitespaces) ? partialNameWhithoutWhitespaces + ".vb" : partialNameWhithoutWhitespaces;
					string partialCode = templateCode.Replace("$(MethodCode)", part.Value);

					folder.AddFromTemplate(itemPath, partialNameWithExtension);
					string partialFilePath = folder.Item(partialNameWithExtension).FileNames[0];
					using (var sw = new StreamWriter(partialFilePath))
					{
						sw.Write(partialCode);
					}

					methodInfo.PartialClasses.Add(partialFilePath);
				}
			}

			string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".vb" : methodName;
			serverMethodFolderItem.ProjectItems.AddFromTemplate(itemPath, methodNameWithExtension);
			string newFilePath = serverMethodFolderItem.ProjectItems.Item(methodNameWithExtension).FileNames[0];
			using (var sw = new StreamWriter(newFilePath))
			{
				sw.Write(methodSourceCode);
			}

			projectConfiguration.MethodInfos.Add(methodInfo);
		}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation)
		{
			string serverMethodPath = project.ProjectItems.Item("Servermethods").FileNames[0];

			var startRegionString = "#Region \"MethodCode\"\r\n";
			var endRegionString = "#End Region";
			var userCodeStartIndex = sourceCode.IndexOf(startRegionString) + startRegionString.Length;
			var userCodeEndIndex = sourceCode.IndexOf(endRegionString, userCodeStartIndex);
			var userCodeLength = userCodeEndIndex - userCodeStartIndex;
			var userCode = sourceCode.Substring(userCodeStartIndex, userCodeLength);

			// TODO: may be should display result code not all.
			return userCode.ToString();

			// end place
		}

		public string AddItemTemplateToProject(Solution2 solution, string methodName)
		{
			var itemPath = solution.GetProjectItemTemplate("Class.zip", "vbproj");
			var methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".vb" : methodName;
			var serverMethodFolderItem = project.ProjectItems.Item("ServerMethods");
			var projectInfo = serverMethodFolderItem.ProjectItems.AddFromTemplate(itemPath, methodNameWithExtension);
			string filePath = serverMethodFolderItem.ProjectItems.Item(methodNameWithExtension).FileNames[0];
			filePath = filePath.Replace("Method1.vb", methodName + ".vb");
			return filePath;
		}

		//		public string GetSourceCodeDataAccessLayerTests(string methodName, string classPrefix)
		//		{
		//			string pkgname = "PKG_" + methodName + "_version";
		//			string sourceCode = string.Format(@"Imports Aras.IOM
		//Imports NUnit.Framework

		//Namespace {0}

		//	<TestFixture> _
		//	Public Class DataAccessLayerTests
		//		<Test> _
		//		Public Sub Ctor_InnovatorIsNull_ShouldThrowArgumentNullException()
		//			' Arrange
		//			Dim innovator As Innovator = Nothing

		//			' Assert
		//			Assert.Throws(Of ArgumentNullException)(New TestDelegate(Function() 
		//			' Act
		//			Dim dataAccessLayer = New {1}DataAccessLayer(innovator)

		//End Function), ""innovator"")
		//		End Sub
		//	End Class
		//End Namespace", pkgname, classPrefix);

		//			return sourceCode;
		//		}

		//		public string GetSourceCodeBusinessLogicTests(string methodName, string classPrefix)
		//		{
		//			string pkgname = "PKG_" + methodName + "_version";
		//			string sourceCode = string.Format(@"Imports Aras.IOM
		//Imports NSubstitute
		//Imports NUnit.Framework

		//Namespace {0}
		//	<TestFixture>
		//	Public Class BusinessLogicTests
		//		<Test>
		//		Public Sub Ctor_DataAccessLayerIsNull_ShouldThrowArgumentNullException()
		//			' Arrange
		//			Dim dataAccessLayer As {1}IDataAccessLayer = Nothing

		//			' Assert
		//			Assert.Throws(Of ArgumentNullException)(New TestDelegate(Function() 
		//			' Act
		//			Dim businessLogic = New {1}BusinessLogic(dataAccessLayer)

		//End Function), ""dataAccessLayer"")
		//		End Sub

		//		<Test>
		//		Public Sub Run_ShouldReturnSameItem()
		//			' Arrange
		//			Dim serverConnection As IServerConnection = Substitute.[For](Of IServerConnection)()
		//			Dim innovator As New Innovator(serverConnection)

		//			Dim dataAccessLayer As {1}IDataAccessLayer = New {1}DataAccessLayer(innovator)
		//			Dim businessLogic As New {1}BusinessLogic(dataAccessLayer)

		//			Dim expected As Item = innovator.newItem()

		//			' Act
		//			Dim actual As Item = businessLogic.Run(expected)

		//			' Assert
		//			Assert.AreEqual(expected, actual)
		//		End Sub
		//	End Class
		//End Namespace
		//", pkgname, classPrefix);
		//			return sourceCode;
		//		}

		public string GenerateSourceCodeByTemplate(TemplateInfo template, string methodName, string packageName, EventSpecificDataType eventData)
		{
			string methodCode = @"End Function
Friend Interface IDataAccessLayer

End Interface

Friend Class DataAccessLayer
	Implements IDataAccessLayer
	Private ReadOnly innovator As Innovator

	Friend Sub New(innovator As Innovator)
		If innovator Is Nothing Then
			Throw New ArgumentNullException(""innovator"")
		End If

		Me.innovator = innovator
	End Sub
End Class

Friend Class BusinessLogic
	Private ReadOnly dataAccessLayer As IDataAccessLayer

	Friend Sub New(dataAccessLayer As IDataAccessLayer)
		If dataAccessLayer Is Nothing Then
			Throw New ArgumentNullException(""dataAccessLayer"")
		End If

		Me.dataAccessLayer = dataAccessLayer
	End Sub

	Friend Function Run(contextItem As Item) As Item
		Return contextItem
	End Function
End Class
		";

			string sourceCode = GenerateSourceCodeByTemplate(template, methodName, methodCode, packageName, eventData);
			return sourceCode;
		}

		public string GenerateSourceCodeByTemplate(TemplateInfo template, string methodName, string methodCode, string packageName, EventSpecificDataType eventData)
		{
			const string fncname = "FNCMethod";
			var eventDataClass = eventData.EventDataClass;
			var interfaceName = eventData.InterfaceName;
			var clsname = "CLS_" + methodName + "_version";
			var pkgname = "PKG_" + methodName + "_version";

			if (!methodCode.EndsWith("\r\n"))
			{
				methodCode += "\r\n";
			}

			var methodCodeWithRegion = string.Format("\r\n\r\n#Region \"MethodCode\"\r\n{0}#End Region \r\n\r\n", methodCode);
			var resultCode = template.TemplateCode;
			resultCode = resultCode.Replace("$(pkgname)", pkgname);
			resultCode = resultCode.Replace("$(clsname)", clsname);
			resultCode = resultCode.Replace("$(interfacename)", interfaceName);
			resultCode = resultCode.Replace("$(fncname)", fncname);
			resultCode = resultCode.Replace("$(EventDataClass)", eventDataClass);
			resultCode = resultCode.Replace("$(MethodCode)", methodCodeWithRegion);

			if (eventData.EventSpecificData != EventSpecificData.None)
			{
				resultCode = resultCode.Insert(0, "#define EventDataIsAvailable");
			}

			return resultCode;
		}

		private Dictionary<string, string> generateMethodParts(ref string methodCode)
		{
			return new Dictionary<string, string>();
		}

		public void LoadCodeToProject(string methodLanguage, string methodCode, string methodLocation, string methodName, string innovatorMethodConfigId, string innovatorMethodId, EventSpecificDataType eventData, string packageName, string executionAllowedToId, string executionAllowedToKeyedName)
		{
			throw new NotImplementedException();
		}

		public void LoadCodeToProject(dynamic methodItem, EventSpecificDataType eventData, string packageName)
		{
			throw new NotImplementedException();
		}

		public string AddItemTemplateToProjectNew(ProjectItem folder, string methodName, string code, bool openAfterCreation)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo UpdateSourceCodeWithPartialClasses(GeneratedCodeInfo methodInfo)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode)
		{
			throw new NotImplementedException();
		}

		public void CreatePartialPart(PartialCodeInfo partialCodeInfo)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo)
		{
			throw new NotImplementedException();
		}

		public CodeInfo CreatePartialCodeInfo(MethodInfo methodInfo, string fileName)
		{
			throw new NotImplementedException();
		}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert)
		{
			throw new NotImplementedException();
		}
	}
}
