//------------------------------------------------------------------------------
// <copyright file="VBCodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using EnvDTE80;

namespace Aras.VS.MethodPlugin.Code
{
	public class VBCodeProvider : ICodeProvider
	{
		private readonly Project project;
		private readonly IProjectConfiguraiton projectConfiguration;

		public string Language
		{
			get { return "VB"; }
		}

		public VBCodeProvider(Project project, IProjectConfiguraiton projectConfiguration)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));

			this.project = project;
			this.projectConfiguration = projectConfiguration;
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

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useCodeFormating)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo)
		{
			throw new NotImplementedException();
		}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormating)
		{
			throw new NotImplementedException();
		}

		public CodeInfo CreateCodeItemInfo(MethodInfo methodInfo, string fileName, CodeType codeType, CodeElementType codeElementType, bool isUseVSFormattingCode)
		{
			throw new NotImplementedException();
		}
	}
}
