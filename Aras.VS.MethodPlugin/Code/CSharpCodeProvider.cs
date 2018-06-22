//------------------------------------------------------------------------------
// <copyright file="CSharpCodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aras.VS.MethodPlugin.Code
{
	internal class CSharpCodeProvider : ICodeProvider
	{
		private IProjectManager projectManager;
		private readonly ProjectConfiguraiton projectConfiguration;
		private readonly DefaultCodeProvider defaultCodeProvider;

		public string Language
		{
			get { return "C#"; }
		}

		public CSharpCodeProvider(IProjectManager projectManager, ProjectConfiguraiton projectConfiguration, DefaultCodeProvider defaultCodeProvider)
		{
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (defaultCodeProvider == null) throw new ArgumentNullException(nameof(defaultCodeProvider));

			this.projectManager = projectManager;
			this.defaultCodeProvider = defaultCodeProvider;
			this.projectConfiguration = projectConfiguration;
		}

		//TODO: change method logic by next. project method information should be combined to one instance. which codeprovider create. after this project manager should create file tree in project.
		public void LoadCodeToProject(string methodLanguage,
			string methodCode,
			string methodLocation,
			string methodName,
			string innovatorMethodConfigId,
			string innovatorMethodId,
			EventSpecificDataType eventData,
			string packageName,
			string executionAllowedToId,
			string executionAllowedToKeyedName)
		{
			var templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);
			TemplateInfo template = null;

			string methodTemplatePattern = @"//MethodTemplateName\s*=\s*(?<templatename>[^\W]*)\s*";
			Match methodTemplateNameMatch = Regex.Match(methodCode, methodTemplatePattern);
			if (methodTemplateNameMatch.Success)
			{
				string value = methodTemplateNameMatch.Groups["templatename"].Value;
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.TemplateName == value).FirstOrDefault();
			}
			if (template == null)
			{
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception(string.Empty);
			}
			if (!string.IsNullOrEmpty(methodTemplateNameMatch.Value))
			{
				methodCode = methodCode.Replace(methodTemplateNameMatch.Value, string.Empty);
			}
			//TODO: only project amanger should write to project tree.
			ProjectItem folder;

			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			if (projectManager.ServerMethodFolderItems.Exists(fileNameWithoutExtension))
			{
				folder = projectManager.ServerMethodFolderItems.Item(fileNameWithoutExtension);
			}
			else
			{
				folder = projectManager.ServerMethodFolderItems.AddFolder(fileNameWithoutExtension);
			}

			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = innovatorMethodConfigId,
				InnovatorMethodId = innovatorMethodId,
				MethodLanguage = methodLanguage,
				MethodName = methodName,
				MethodType = methodLocation ?? "server",
				PackageName = packageName,
				TemplateName = template.TemplateName,
				ExecutionAllowedToId = executionAllowedToId,
				ExecutionAllowedToKeyedName = executionAllowedToKeyedName
			};

			string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			bool methodExist = false;
			bool shouldBeUpdated = false;

			if (folder.ProjectItems.Exists(methodNameWithExtension))
			{
				methodExist = true;

				var messageWindow = new MessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog(null,
					"Method already added to project. Do you want replace method?",
					"Warning",
					MessageButtons.YesNo,
					MessageIcon.None);

				if (dialogReuslt == MessageDialogResult.Yes)
				{
					shouldBeUpdated = true;
				}
				else
				{
					return;
				}
			}

			var storedMethodInfo = projectConfiguration.MethodInfos.FirstOrDefault(mi => mi.MethodName == methodName);

			if (methodExist && shouldBeUpdated)
			{
				//RemoveMethod(storedMethodInfo, folder);
			}

			var defaultTemplate = defaultCodeProvider.GetDefaultCodeTemplates(projectManager.DefaultCodeTemplatesPath)
				.FirstOrDefault(dct => dct.TempalteName == template.TemplateName && dct.EventDataType == eventData.EventSpecificData);

			var codeInfo = CreateWrapper(template, eventData, methodName);
			codeInfo = CreateMainNew(codeInfo, template, eventData, methodName, false, methodCode);
			codeInfo = CreatePartialClasses(codeInfo);

			//TODO: error should be fixed
			//foreach (var partial in codeInfo.PartialCode)
			//{
			//	CreatePartialPart(partial);
			//	methodInfo.PartialClasses.Add(partial.Path);
			//}

			CreateTestsNew(codeInfo, template, eventData, methodName, false);

			if (!methodExist)
			{
				projectConfiguration.MethodInfos.Add(methodInfo);
			}
			else
			{
				storedMethodInfo.PartialClasses = methodInfo.PartialClasses;
				storedMethodInfo.InnovatorMethodId = methodInfo.InnovatorMethodId;
				storedMethodInfo.TemplateName = methodInfo.TemplateName;
				storedMethodInfo.ExecutionAllowedToId = methodInfo.ExecutionAllowedToId;
				storedMethodInfo.ExecutionAllowedToKeyedName = methodInfo.ExecutionAllowedToKeyedName;
			}
		}

		////TODO: only project amanger should write to project tree.
		//private void RemoveMethod(MethodInfo methodInfo, ProjectItem methodFolder)
		//{
		//	var methodNameWithExtension = !Path.HasExtension(methodInfo.MethodName) ? methodInfo.MethodName + ".cs" : methodInfo.MethodName;
		//	//remove method
		//	if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
		//	{
		//		var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
		//		var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
		//		methodItem.Remove();
		//		File.Delete(pathToItem);
		//	}
		//	//remove wrapper
		//	var methodName = methodInfo.MethodName + "Wrapper";
		//	methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
		//	if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
		//	{

		//		var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
		//		var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
		//		methodItem.Remove();
		//		File.Delete(pathToItem);
		//	}

		//	//remove tests
		//	methodName = methodInfo.MethodName + "Tests";
		//	methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
		//	if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
		//	{

		//		var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
		//		var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
		//		methodItem.Remove();
		//		File.Delete(pathToItem);
		//	}

		//	//remove partial classes
		//	var partialClassesForDelete = new List<string>();
		//	foreach (var partialCodeInfo in methodInfo.PartialClasses)
		//	{
		//		var folder = methodFolder.ProjectItems;
		//		string pathToFolder = string.Empty;
		//		var updatedPath = partialCodeInfo.Replace("\"", "");
		//		string[] partialFilePath = updatedPath.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

		//		var partialFileName = !Path.HasExtension(partialFilePath.Last()) ? partialFilePath.Last() + ".cs" : partialFilePath.Last();
		//		for (int i = 0; i < partialFilePath.Length - 1; i++)
		//		{
		//			string partialMethodNameWithoutExtension = Path.GetFileNameWithoutExtension(partialFilePath[i]);
		//			if (folder.Exists(partialMethodNameWithoutExtension))
		//			{
		//				pathToFolder = folder.Item(partialMethodNameWithoutExtension).Properties.Item("FullPath").Value.ToString();
		//				folder = folder.Item(partialMethodNameWithoutExtension).ProjectItems;
		//			}
		//		}

		//		if (folder.Exists(partialFileName))
		//		{
		//			var methodItem = folder.Item(partialFileName);
		//			var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
		//			methodItem.Remove();
		//			File.Delete(pathToItem);
		//		}

		//		partialClassesForDelete.Add(partialCodeInfo);
		//	}

		//	foreach (var pcfd in partialClassesForDelete)
		//	{
		//		methodInfo.PartialClasses.Remove(pcfd);
		//	}

		//	//remove folder if no files inside
		//}

		//public void LoadCodeToProject(dynamic methodItem, EventSpecificDataType eventData, string packageName)
		//{
		//	string methodLanguage = methodItem.getProperty("method_type");
		//	string methodCode = methodItem.getProperty("method_code");
		//	string methodLocation = methodItem.getProperty("method_location");
		//	string methodName = methodItem.getProperty("name");
		//	string innovatorMethodConfigId = methodItem.getProperty("config_id");
		//	string innovatorMethodId = methodItem.getID();
		//	string methodExecutionAllowedToId = methodItem.getProperty("execution_allowed_to");
		//	string methodExecutionAllowedToKeyedName = methodItem.getPropertyAttribute("execution_allowed_to", "keyed_name");

		//	LoadCodeToProject(methodLanguage, methodCode, methodLocation, methodName, innovatorMethodConfigId, innovatorMethodId, eventData, packageName, methodExecutionAllowedToId, methodExecutionAllowedToKeyedName);
		//}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation,string serverMethodFolderPath)
		{
			var userCode = GetSourceCodeBetweenRegion(sourceCode);
			string partialCode = this.LoadPartialClassesCode(methodInformation.PartialClasses, serverMethodFolderPath);

			if (!string.IsNullOrEmpty(partialCode))
			{
				partialCode = partialCode.Remove(partialCode.LastIndexOf('}'));
				userCode += Environment.NewLine + "}" + partialCode;
			}

			return userCode;
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert)
		{
			GeneratedCodeInfo codeInfo = this.CreateWrapper(template, eventData, methodName);
			codeInfo = this.CreateMainNew(codeInfo, template, eventData, methodName, useAdvancedCode, codeToInsert);
			codeInfo = this.CreatePartialClasses(codeInfo);
			codeInfo = this.CreateTestsNew(codeInfo, template, eventData, methodName, useAdvancedCode);

			return codeInfo;
		}

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName)
		{
			var defaultTemplate = defaultCodeProvider.GetDefaultCodeTemplates(projectManager.DefaultCodeTemplatesPath)
				.FirstOrDefault(dct => dct.TempalteName == template.TemplateName && dct.EventDataType == eventData.EventSpecificData);

			string wrapperCode = defaultTemplate.WrapperSourceCode;

			const string fncname = "FNCMethod";
			var eventDataClass = eventData.EventDataClass;
			var interfaceName = eventData.InterfaceName;
			string methodNameWithOutSpases = methodName.Replace(' ', '_');
			var clsname = "Aras_CLS_" + methodNameWithOutSpases;
			var pkgname = "Aras_PKG_" + methodNameWithOutSpases;

			if (!wrapperCode.EndsWith("\r\n"))
			{
				wrapperCode += "\r\n";
			}

			var resultCode = template.TemplateCode;
			wrapperCode = wrapperCode.Insert(0, "[WrapperMethod]\r\n");
			resultCode = resultCode.Replace("$(MethodCode)", wrapperCode);
			resultCode = resultCode.Replace("$(pkgname)", pkgname);
			resultCode = resultCode.Replace("$(clsname)", clsname);
			resultCode = resultCode.Replace("$(interfacename)", interfaceName);
			resultCode = resultCode.Replace("$(fncname)", fncname);
			resultCode = resultCode.Replace("$(EventDataClass)", eventDataClass);

			var tree = CSharpSyntaxTree.ParseText(resultCode);
			SyntaxNode root = tree.GetRoot();

			var member = root.DescendantNodes()
			 .OfType<AttributeSyntax>()
			 .Where(a => a.Name.ToString() == "WrapperMethod")
			 .FirstOrDefault();
			var parentClassName = GetParentClassName(member);

			var clss = root.DescendantNodes()
						.OfType<ClassDeclarationSyntax>()
						.Where(a => a.Identifier.Text.ToString() == parentClassName)
						.First();

			var clsWithModifier = clss.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
			clsWithModifier = clsWithModifier.NormalizeWhitespace();
			root = root.ReplaceNode(clss, clsWithModifier);
			resultCode = root.ToString().Replace("[WrapperMethod]", string.Empty);

			if (eventData.EventSpecificData != EventSpecificData.None)
			{
				resultCode = resultCode.Insert(0, "#define EventDataIsAvailable\r\n");
			}

			GeneratedCodeInfo resultInfo = new GeneratedCodeInfo();
			resultInfo.WrapperCodeInfo.Code = resultCode;
			resultInfo.WrapperCodeInfo.Path = Path.Combine(methodName, methodName + "Wrapper.cs");
			resultInfo.MethodName = methodName;
			resultInfo.ClassName = clsname;
			resultInfo.Namespace = pkgname;
			resultInfo.MethodCodeParentClassName = parentClassName;

			return resultInfo;
		}

		public GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template,
			EventSpecificDataType eventData,
			string methodName,
			bool useAdvancedCode,
			string codeToInsert)
		{
			var defaultTemplate = defaultCodeProvider.GetDefaultCodeTemplates(projectManager.DefaultCodeTemplatesPath)
				.FirstOrDefault(dct => dct.TempalteName == template.TemplateName && dct.EventDataType == eventData.EventSpecificData);

			string code = useAdvancedCode ? defaultTemplate.AdvancedSourceCode : defaultTemplate.SimpleSourceCode;
			code = code.Replace("$(pkgname)", generatedCodeInfo.Namespace);

			if (!string.IsNullOrEmpty(codeToInsert))
			{
				var defaultCode = GetSourceCodeBetweenRegion(code);
				code = code.Replace(defaultCode, codeToInsert);
			}
			generatedCodeInfo.MethodCodeInfo.Code = code;
			generatedCodeInfo.MethodCodeInfo.Path = Path.Combine(methodName, methodName + ".cs");

			return generatedCodeInfo;
		}

		public GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo)
		{
			var resultGeneratedCode = new GeneratedCodeInfo(methodInfo);
			resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.MethodCodeInfo.Code.Replace("//[PartialPath", "[PartialPath");

			var tree = CSharpSyntaxTree.ParseText(resultGeneratedCode.MethodCodeInfo.Code);
			var root = tree.GetRoot();

			var members = root.DescendantNodes()
				.OfType<AttributeSyntax>()
				.Where(a => a.Name.ToString() == "PartialPath")
				.Select(a => new { AttributeInfo = a, Parent = a.Parent.Parent })
				.Where(m =>
					m.Parent is FieldDeclarationSyntax ||
					m.Parent is EnumDeclarationSyntax ||
					m.Parent is ConstructorDeclarationSyntax ||
					m.Parent is DestructorDeclarationSyntax ||
					m.Parent is PropertyDeclarationSyntax ||
					m.Parent is MethodDeclarationSyntax ||
					m.Parent is OperatorDeclarationSyntax ||
					m.Parent is IndexerDeclarationSyntax ||
					m.Parent is ClassDeclarationSyntax ||
					m.Parent is StructDeclarationSyntax ||
					m.Parent is InterfaceDeclarationSyntax)
				.ToList();

			foreach (var member in members)
			{
				var path = member.AttributeInfo.ArgumentList.Arguments.FirstOrDefault()?.ToString();
				path = path.Replace("/", "\\").Replace("\"",string.Empty);
				path = Path.Combine(resultGeneratedCode.MethodName, path);
				if (path != null)
				{
					var partialString = member.Parent.ToString();
					string stringForReplace = string.Empty;
					string shouldBeReplaced = partialString;

					if (partialString.Contains("#endregion MethodCode"))
					{
						int indexofEndRegion = partialString.IndexOf("#endregion MethodCode");
						stringForReplace = partialString.Substring(indexofEndRegion, partialString.Length - indexofEndRegion);
						partialString = partialString.Replace(stringForReplace, "}");
					}

					var existingPartialInfo = resultGeneratedCode.PartialCodeInfoList.FirstOrDefault(pi => pi.Path == path);
					if (existingPartialInfo != null)
					{
						existingPartialInfo.Code += "\r\n" + partialString;
					}
					else
					{
						resultGeneratedCode.PartialCodeInfoList.Add(new CodeInfo() { Code = partialString, Path = path });
					}

					resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.MethodCodeInfo.Code.Replace(shouldBeReplaced, stringForReplace);
					if (!string.IsNullOrEmpty(stringForReplace))
					{
						char currentCharacter = ' ';
						int endRegionIndex = resultGeneratedCode.MethodCodeInfo.Code.IndexOf("#endregion MethodCode");
						int endMethodIndex = endRegionIndex;
						do
						{
							currentCharacter = resultGeneratedCode.MethodCodeInfo.Code[endMethodIndex];
							endMethodIndex--;
						}
						while (endMethodIndex > 0 && currentCharacter != '}');

						var stringBuilder = new StringBuilder(resultGeneratedCode.MethodCodeInfo.Code);
						stringBuilder.Replace(Environment.NewLine, string.Empty, endMethodIndex + 2, endRegionIndex - endMethodIndex - 3);
						stringBuilder.Remove(endMethodIndex + 1, 1);
						resultGeneratedCode.MethodCodeInfo.Code = stringBuilder.ToString();
					}
				}
			}

			var partialUsings = string.Empty;
			if (resultGeneratedCode.PartialCodeInfoList.Count != 0)
			{
				var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
				partialUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
				if (!string.IsNullOrEmpty(partialUsings))
				{
					partialUsings += "\r\n";
				}
			}

			string partialClassTemplate = "{0}using Common;\r\nnamespace {3} \r\n{{public partial class {1} \r\n{{\r\n{2}\r\n}}\r\n}}";
			foreach (var partialCodeInfo in resultGeneratedCode.PartialCodeInfoList)
			{
				partialCodeInfo.Code = string.Format(partialClassTemplate, partialUsings, resultGeneratedCode.MethodCodeParentClassName, partialCodeInfo.Code, resultGeneratedCode.Namespace);
			}

			return resultGeneratedCode;
		}

		public CodeInfo CreatePartialCodeInfo(MethodInfo methodInformation, string fileName)
		{
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;
			string selectedFolderPath = projectManager.SelectedFolderPath;
			string methodName = projectManager.MethodName;

			string partialPath = selectedFolderPath.Substring(serverMethodFolderPath.IndexOf(serverMethodFolderPath) + serverMethodFolderPath.Length);
			partialPath = Path.Combine(partialPath, fileName);
			string partialAttributePath = partialPath.Substring(partialPath.IndexOf(methodName) + methodName.Length + 1);
			partialAttributePath = partialAttributePath.Replace("\\", "/");

			var templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			TemplateInfo template = null;
			template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.TemplateName == methodInformation.TemplateName).FirstOrDefault();
			if (template == null)
			{
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception("Template not found.");
			}

			EventSpecificDataType eventData = CommonData.EventSpecificDataTypeList.First(x => x.EventSpecificData == methodInformation.EventData);
			GeneratedCodeInfo codeInfo = this.CreateWrapper(template, eventData, methodName);

			string methodCode = File.ReadAllText(projectManager.MethodPath,new UTF8Encoding(true));
			var tree = CSharpSyntaxTree.ParseText(methodCode);
			var root = tree.GetRoot();

			var partialUsings = string.Empty;
			var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
			partialUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
			if (!string.IsNullOrEmpty(partialUsings))
			{
				partialUsings += "\r\n";
			}

			string partialClassTemplate = "{0}using Common;\r\nnamespace {3} \r\n{{public partial class {1} \r\n{{\r\n//[PartialPath(\"{2}\")]\r\n}}\r\n}}";
			var partialCodeInfo = new CodeInfo()
			{
				Path = partialPath,
				Code = string.Format(partialClassTemplate, partialUsings, codeInfo.MethodCodeParentClassName, partialAttributePath, codeInfo.Namespace)
			};

			return partialCodeInfo;
		}

		public GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode)
		{
			var resultCodeInfo = new GeneratedCodeInfo(generatedCodeInfo);

			var defaultTemplate = defaultCodeProvider.GetDefaultCodeTemplates(projectManager.DefaultCodeTemplatesPath)
				.FirstOrDefault(dct => dct.TempalteName == template.TemplateName && dct.EventDataType == eventData.EventSpecificData);
			string code = useAdvancedCode ? defaultTemplate.AdvancedUnitTestsCode : defaultTemplate.SimpleUnitTestsCode;
			code = code.Replace("$(pkgname)", resultCodeInfo.Namespace);
			code = code.Replace("$(clsname)", resultCodeInfo.ClassName);

			resultCodeInfo.TestsCodeInfo.Code = code;
			resultCodeInfo.TestsCodeInfo.Path = Path.Combine(methodName, methodName + "Tests.cs");

			return resultCodeInfo;
		}

		private string GetParentClassName(SyntaxNode node)
		{
			if (node != null)
			{
				var classNode = node as ClassDeclarationSyntax;
				if (classNode != null)
				{
					return classNode.Identifier.Text;
				}
				else
				{
					return GetParentClassName(node.Parent);
				}
			}
			else
			{
				return string.Empty;
			}

		}

		private string LoadPartialClassesCode(List<string> partialClasses, string serverMethodPath)
		{
			var resultCode = new StringBuilder();

			foreach (string partialClassPath in partialClasses)
			{
				var updatedPath = partialClassPath.Replace("/", "\\");
				if (!Path.HasExtension(updatedPath))
				{
					updatedPath += ".cs";
				}

				string normalizedUpdatedPath = updatedPath.TrimStart(Path.DirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);
				var filePath = Path.Combine(serverMethodPath, normalizedUpdatedPath);
				var source = File.ReadAllText(filePath, new UTF8Encoding(true));
				var tree = CSharpSyntaxTree.ParseText(source);
				var root = tree.GetRoot();

				var members = root.DescendantNodes()
					.OfType<AttributeSyntax>()
					.Where(a => a.Name.ToString() == "PartialPath")
					.Select(a => new { AttributeInfo = a, Parent = a.Parent.Parent })
					.Where(m =>
						m.Parent is FieldDeclarationSyntax ||
						m.Parent is EnumDeclarationSyntax ||
						m.Parent is ConstructorDeclarationSyntax ||
						m.Parent is DestructorDeclarationSyntax ||
						m.Parent is PropertyDeclarationSyntax ||
						m.Parent is MethodDeclarationSyntax ||
						m.Parent is OperatorDeclarationSyntax ||
						m.Parent is IndexerDeclarationSyntax ||
						m.Parent is ClassDeclarationSyntax ||
						m.Parent is StructDeclarationSyntax ||
						m.Parent is InterfaceDeclarationSyntax)
					.ToList();

				foreach (var member in members)
				{
					resultCode.Append(Environment.NewLine + Environment.NewLine + member.Parent.ToString());
				}
			}
			resultCode = resultCode.Replace("[PartialPath", "//[PartialPath");

			return resultCode.ToString();
		}

		private string GetSourceCodeBetweenRegion(string codeWithRegion)
		{
			string startRegionPattern = @"#region MethodCode[\s]*\r\n";
			string endRegionPattern = @"\r\n[\s]*#endregion MethodCode";

			var startMatch = Regex.Match(codeWithRegion, startRegionPattern);
			var endMatch = Regex.Match(codeWithRegion, endRegionPattern);
			if (!startMatch.Success || !endMatch.Success)
			{
				throw new Exception();
			}

			var userCodeStartIndex = startMatch.Index + startMatch.Length;
			var userCodeEndIndex = endMatch.Index;
			var userCodeLength = userCodeEndIndex - userCodeStartIndex;
			var updatedCode = codeWithRegion.Substring(userCodeStartIndex, userCodeLength);

			return updatedCode;
		}
	}
}
