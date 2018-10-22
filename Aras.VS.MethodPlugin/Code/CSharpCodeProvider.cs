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
using Microsoft.VisualStudio.ComponentModelHost;

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

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
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

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormatting)
		{
			GeneratedCodeInfo codeInfo = this.CreateWrapper(template, eventData, methodName, useCodeFormatting);
			codeInfo = this.CreateMainNew(codeInfo, template, eventData, methodName, useAdvancedCode, codeToInsert);
			codeInfo = this.CreatePartialClasses(codeInfo);
			codeInfo = this.CreateTestsNew(codeInfo, template, eventData, methodName, useAdvancedCode);

			return codeInfo;
		}

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useVSFormatting)
		{
			DefaultCodeTemplate defaultTemplate = LoadDefaultCodeTemplate(template, eventData);
			string wrapperCode = defaultTemplate.WrapperSourceCode;

			const string fncname = "FNCMethod";
			var eventDataClass = eventData.EventDataClass;
			var interfaceName = eventData.InterfaceName;
			string methodNameWithOutSpases = methodName.Replace(' ', '_');
			var clsname = "ArasCLS" + methodNameWithOutSpases;
			var pkgname = "ArasPKG" + methodNameWithOutSpases;

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
						.FirstOrDefault(a => a.Identifier.Text.ToString() == parentClassName);
		    if (clss != null)
		    {
		        var clsWithModifier = clss.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
		        clsWithModifier = clsWithModifier.NormalizeWhitespace();
		        root = root.ReplaceNode(clss, clsWithModifier);
            }
			resultCode = root.ToString().Replace("[WrapperMethod]", string.Empty);

			GeneratedCodeInfo resultInfo = new GeneratedCodeInfo();
			resultInfo.WrapperCodeInfo.Code = useVSFormatting ? FormattingCode(resultCode) : resultCode;
			resultInfo.WrapperCodeInfo.Path = Path.Combine(methodName, methodName + "Wrapper.cs");
			resultInfo.MethodName = methodName;
			resultInfo.ClassName = clsname;
			resultInfo.Namespace = pkgname;
			resultInfo.MethodCodeParentClassName = parentClassName;
			resultInfo.IsUseVSFormatting = useVSFormatting;

            return resultInfo;
		}

		public GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template,
			EventSpecificDataType eventData,
			string methodName,
			bool useAdvancedCode,
			string codeToInsert)
		{
			DefaultCodeTemplate defaultTemplate = LoadDefaultCodeTemplate(template, eventData);
			StringBuilder code = new StringBuilder(useAdvancedCode ? defaultTemplate.AdvancedSourceCode : defaultTemplate.SimpleSourceCode);
			code = code.Replace("$(pkgname)", generatedCodeInfo.Namespace);
		    code = code.Replace("$(clsname)", generatedCodeInfo.ClassName);

			if (!string.IsNullOrEmpty(codeToInsert))
			{
                string codeString = code.ToString();
				var defaultCode = GetSourceCodeBetweenRegion(codeString);
                if (string.IsNullOrWhiteSpace(defaultCode))
                {
                    var insertPattern = "#region MethodCode\r\n";
                    var insertIndex = codeString.IndexOf(insertPattern);
                    code = code.Insert(insertIndex + insertPattern.Length, codeToInsert);
                }
                else
                {
                    code = code.Replace(defaultCode, codeToInsert);
                }
			}
            if (eventData.EventSpecificData != EventSpecificData.None)
            {
                code = code.Insert(0, "#define EventDataIsAvailable\r\n");
            }
			
            generatedCodeInfo.MethodCodeInfo.Code = generatedCodeInfo.IsUseVSFormatting ? this.FormattingCode(code.ToString()) : code.ToString();
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
				path = path.Replace("/", "\\").Replace("\"", string.Empty);
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
                        stringForReplace = '\t' + stringForReplace;
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

                        currentCharacter = default(char);
                        while (currentCharacter != '\r')
                        {
                            currentCharacter = resultGeneratedCode.MethodCodeInfo.Code[endRegionIndex];
                            endRegionIndex--;
                        }
                        var endRegionEndString = resultGeneratedCode.MethodCodeInfo.Code.Substring(endRegionIndex + 1);
                        var beginRegionEndString = resultGeneratedCode.MethodCodeInfo.Code.Remove(endMethodIndex + 1).TrimEnd(new char[] {' ', '\r','\n'});
                        resultGeneratedCode.MethodCodeInfo.Code = beginRegionEndString + endRegionEndString;
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
				string code = string.Format(partialClassTemplate, partialUsings, resultGeneratedCode.MethodCodeParentClassName, partialCodeInfo.Code, resultGeneratedCode.Namespace);
				partialCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? FormattingCode(code) : code;
			}

			return resultGeneratedCode;
		}

		public CodeInfo CreatePartialCodeInfo(MethodInfo methodInformation, string fileName, bool useVSFormatting)
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
			GeneratedCodeInfo codeInfo = this.CreateWrapper(template, eventData, methodName, useVSFormatting);

			string methodCode = File.ReadAllText(projectManager.MethodPath, new UTF8Encoding(true));
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
			string code = string.Format(partialClassTemplate, partialUsings, codeInfo.MethodCodeParentClassName, partialAttributePath, codeInfo.Namespace);
			var partialCodeInfo = new CodeInfo()
			{
				Path = partialPath,
				Code = useVSFormatting ? FormattingCode(code) : code
			};

			return partialCodeInfo;
		}

		public GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode)
		{
			var resultCodeInfo = new GeneratedCodeInfo(generatedCodeInfo);
			DefaultCodeTemplate defaultTemplate = LoadDefaultCodeTemplate(template, eventData);

			string code = useAdvancedCode ? defaultTemplate.AdvancedUnitTestsCode : defaultTemplate.SimpleUnitTestsCode;
			code = code.Replace("$(pkgname)", resultCodeInfo.Namespace);
			code = code.Replace("$(clsname)", resultCodeInfo.ClassName);

			resultCodeInfo.TestsCodeInfo.Code = resultCodeInfo.IsUseVSFormatting ? FormattingCode(code) : code;
			resultCodeInfo.TestsCodeInfo.Path = Path.Combine(methodName, methodName + "Tests.cs");

			return resultCodeInfo;
		}

		private DefaultCodeTemplate LoadDefaultCodeTemplate(TemplateInfo template, EventSpecificDataType eventData)
		{
		    var defaultTemplate = defaultCodeProvider.GetDefaultCodeTemplate(projectManager.DefaultCodeTemplatesPath, 
		        template.TemplateName, eventData.EventSpecificData.ToString());
			if (defaultTemplate == null)
			{
				throw new FileNotFoundException($"Default code template file with templateName=\"{template.TemplateName}\" eventData=\"{eventData.EventSpecificData}\" not found.");
			}

			return defaultTemplate;
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
			string startRegionPattern = @"#region MethodCode[\s]*";
			string endRegionPattern = @"#endregion MethodCode";

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

		private string FormattingCode(string code)
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
			SyntaxNode node = tree.GetRoot();

			node = Microsoft.CodeAnalysis.Formatting.Formatter.Format(node, this.projectManager.VisualStudioWorkspace);
			string formatedCode = node.ToString();
			return formatedCode;
		}
	}
}
