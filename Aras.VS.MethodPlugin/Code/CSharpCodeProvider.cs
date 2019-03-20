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
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aras.VS.MethodPlugin.Code
{
	internal class CSharpCodeProvider : ICodeProvider
	{
		private const string startMethodCodeRegion = "#region MethodCode";
		private const string endMethodCodeRegion = "#endregion MethodCode";

		private readonly IProjectManager projectManager;
		private readonly IProjectConfiguraiton projectConfiguration;
		private readonly DefaultCodeProvider defaultCodeProvider;
		private readonly ICodeItemProvider codeItemProvider;
		private readonly IIOWrapper iOWrapper;
		private readonly IDialogFactory dialogFactory;

		public string Language
		{
			get { return "C#"; }
		}

		public CSharpCodeProvider(IProjectManager projectManager, IProjectConfiguraiton projectConfiguration, DefaultCodeProvider defaultCodeProvider, ICodeItemProvider codeItemProvider, IIOWrapper iOWrapper, IDialogFactory dialogFactory)
		{
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (defaultCodeProvider == null) throw new ArgumentNullException(nameof(defaultCodeProvider));
			if (codeItemProvider == null) throw new ArgumentNullException(nameof(codeItemProvider));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));

			this.projectManager = projectManager;
			this.defaultCodeProvider = defaultCodeProvider;
			this.projectConfiguration = projectConfiguration;
			this.codeItemProvider = codeItemProvider;
			this.iOWrapper = iOWrapper;
			this.dialogFactory = dialogFactory;
		}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
		{
			if (string.IsNullOrEmpty(sourceCode))
			{
				throw new ArgumentException(nameof(sourceCode));
			}

			var tree = CSharpSyntaxTree.ParseText(sourceCode);
			SyntaxNode root = tree.GetRoot();

			int classesCount = root.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.First()
				.ChildNodes()
				.OfType<ClassDeclarationSyntax>()
				.Count();

			MemberDeclarationSyntax[] externalsSyntaxNodes = LoadSyntaxNodesByAttribute("ExternalPath", methodInformation.ExternalItems, serverMethodFolderPath);
			MemberDeclarationSyntax[] partialsSyntaxNodes = LoadSyntaxNodesByAttribute("PartialPath", methodInformation.PartialClasses, serverMethodFolderPath);

			string userCode = string.Empty;
			if (!externalsSyntaxNodes.Any() && partialsSyntaxNodes.Any())
			{
				StringBuilder partials = new StringBuilder();
				foreach (var partialsSyntaxNode in partialsSyntaxNodes)
				{
					partials.Append(partialsSyntaxNode.ToFullString());
				}

				userCode = GetSourceCodeBetweenRegion(sourceCode) + Environment.NewLine + "}" + Environment.NewLine + partials.ToString();

				if (classesCount == 1)
				{
					userCode = Regex.Replace(userCode, @"\r\n( |\t)*}(\r\n| |\t)*$", string.Empty);
				}
			}
			else
			{
				if (partialsSyntaxNodes.Any())
				{
					var partialClassNode = root.DescendantNodes()
						.OfType<NamespaceDeclarationSyntax>()
						.First()
						.ChildNodes()
						.OfType<ClassDeclarationSyntax>()
						.Where(x => x.Modifiers.Any(SyntaxKind.PartialKeyword))
						.FirstOrDefault();

					if (partialClassNode == null)
					{
						throw new Exception("No partial classes found.");
					}

					var partialClassNodeWithPartials = partialClassNode.AddMembers(partialsSyntaxNodes);
					root = root.ReplaceNode(partialClassNode, partialClassNodeWithPartials);
				}

				if (externalsSyntaxNodes.Any())
				{
					if (classesCount <= 1)
					{
						var namespaceNode = root.DescendantNodes()
							.OfType<NamespaceDeclarationSyntax>()
							.First();

						if (!CodeIndexInMethodRegions(root, namespaceNode.Span.End))
						{
							throw new FormatException("Wrong format. Could not insert external items to the method code.");
						}

						var namespaceNodeWithPartials = namespaceNode.AddMembers(externalsSyntaxNodes);
						root = root.ReplaceNode(namespaceNode, namespaceNodeWithPartials);
					}
					else
					{
						var classNode = root.DescendantNodes()
							.OfType<NamespaceDeclarationSyntax>()
							.First()
							.ChildNodes()
							.OfType<ClassDeclarationSyntax>()
							.Last();

						if (!CodeIndexInMethodRegions(root, classNode.Span.Start))
						{
							throw new FormatException("Wrong format. Could not insert external items to the method code.");
						}

						root = root.InsertNodesBefore(classNode, externalsSyntaxNodes);
					}
				}

				userCode = GetSourceCodeBetweenRegion(root.ToString());
			}

			return EscapeAttributes(userCode);
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormatting)
		{
			GeneratedCodeInfo codeInfo = this.CreateWrapper(template, eventData, methodName, useCodeFormatting);
			codeInfo = this.CreateMainNew(codeInfo, template, eventData, methodName, useAdvancedCode, codeToInsert);
			codeInfo = this.CreatePartialClasses(codeInfo);
			codeInfo = this.CreateExternalItems(codeInfo);
			codeInfo = this.CreateTestsNew(codeInfo, template, eventData, methodName, useAdvancedCode);

			return codeInfo;
		}

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useVSFormatting)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException("Method name can not be empty");
			}

			DefaultCodeTemplate defaultTemplate = LoadDefaultCodeTemplate(template, eventData);
			string wrapperCode = defaultTemplate.WrapperSourceCode;

			const string fncname = "FNCMethod";
			var eventDataClass = eventData.EventDataClass;
			var interfaceName = eventData.InterfaceName;
			string methodNameWithOutSpases = Regex.Replace(methodName, "[^a-zA-Z0-9]+", string.Empty, RegexOptions.Compiled);
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
					var partialString = member.Parent.ToFullString();
					string stringForReplace = string.Empty;
					string shouldBeReplaced = partialString;

					if (partialString.Contains(endMethodCodeRegion))
					{
						int indexofEndRegion = partialString.IndexOf(endMethodCodeRegion);
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
						string pattern = string.Concat(@"\r\n( |\t)*}( |\t)*\r\n( |\t)*", endMethodCodeRegion);
						string insertRegion = string.Concat(Environment.NewLine, endMethodCodeRegion);
						string replacedCode = Regex.Replace(resultGeneratedCode.MethodCodeInfo.Code, pattern, insertRegion);
						resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? FormattingCode(replacedCode) : replacedCode;
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

			string partialClassTemplate = "{0}using Common;\r\nusing Common.Attributes;\r\nnamespace {3} \r\n{{public partial class {1} \r\n{{\r\n{2}\r\n}}\r\n}}";
			foreach (var partialCodeInfo in resultGeneratedCode.PartialCodeInfoList)
			{
				string code = string.Format(partialClassTemplate, partialUsings, resultGeneratedCode.MethodCodeParentClassName, partialCodeInfo.Code, resultGeneratedCode.Namespace);
				partialCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? FormattingCode(code) : code;
			}

			return resultGeneratedCode;
		}

		public GeneratedCodeInfo CreateExternalItems(GeneratedCodeInfo methodInfo)
		{
			var resultGeneratedCode = new GeneratedCodeInfo(methodInfo);
			resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.MethodCodeInfo.Code.Replace("//[ExternalPath", "[ExternalPath");

			var tree = CSharpSyntaxTree.ParseText(resultGeneratedCode.MethodCodeInfo.Code);
			var root = tree.GetRoot();

			var members = root.DescendantNodes()
				.OfType<AttributeSyntax>()
				.Where(a => a.Name.ToString() == "ExternalPath")
				.Select(a => new { AttributeInfo = a, Parent = a.Parent.Parent })
					.Where(m =>
						m.Parent is EnumDeclarationSyntax ||
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
					var externalString = member.Parent.ToFullString();
					string stringForReplace = string.Empty;
					string shouldBeReplaced = externalString;

					var existingExternalInfo = resultGeneratedCode.ExternalItemsInfoList.FirstOrDefault(ei => ei.Path == path);
					if (existingExternalInfo != null)
					{
						existingExternalInfo.Code += "\r\n" + externalString;
					}
					else
					{
						resultGeneratedCode.ExternalItemsInfoList.Add(new CodeInfo() { Code = externalString, Path = path });
					}

					resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.MethodCodeInfo.Code.Replace(shouldBeReplaced, stringForReplace);
				}
			}

			var externalUsings = string.Empty;
			if (resultGeneratedCode.ExternalItemsInfoList.Count != 0)
			{
				var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
				externalUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
				if (!string.IsNullOrEmpty(externalUsings))
				{
					externalUsings += "\r\n";
				}
			}

			string externalClassTemplate = "{0}using Common;\r\nusing Common.Attributes;\r\nnamespace {2} \r\n{{\r\n{1}\r\n}}";
			foreach (var externalCodeInfo in resultGeneratedCode.ExternalItemsInfoList)
			{
				string code = string.Format(externalClassTemplate, externalUsings, externalCodeInfo.Code, resultGeneratedCode.Namespace);
				externalCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? FormattingCode(code) : code;
			}

			return resultGeneratedCode;
		}

		public CodeInfo CreateCodeItemInfo(MethodInfo methodInformation, string fileName, CodeType codeType, CodeElementType codeElementType, bool useVSFormatting)
		{
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;
			string selectedFolderPath = projectManager.SelectedFolderPath;
			string methodName = projectManager.MethodName;

			string codeItemPath = selectedFolderPath.Substring(serverMethodFolderPath.IndexOf(serverMethodFolderPath) + serverMethodFolderPath.Length);
			codeItemPath = Path.Combine(codeItemPath, fileName);
			string codeItemAttributePath = codeItemPath.Substring(codeItemPath.IndexOf(methodName) + methodName.Length + 1);
			codeItemAttributePath = codeItemAttributePath.Replace("\\", "/");

			var templateLoader = new TemplateLoader(this.dialogFactory, this.projectManager.UIShell);
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

			var referenceUsings = string.Empty;
			var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
			referenceUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
			if (!string.IsNullOrEmpty(referenceUsings))
			{
				referenceUsings += "\r\n";
			}

			string codeItemTemplate = this.codeItemProvider.GetCodeElementTypeTemplate(codeType, codeElementType);
			string code = string.Format(codeItemTemplate, referenceUsings, codeInfo.MethodCodeParentClassName, codeItemAttributePath, codeInfo.Namespace, fileName);
			var codeItemInfo = new CodeInfo()
			{
				Path = codeItemPath,
				Code = useVSFormatting ? FormattingCode(code) : code
			};

			return codeItemInfo;
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

		private MemberDeclarationSyntax[] LoadSyntaxNodesByAttribute(string attributeName, List<string> clasesPaths, string serverMethodPath)
		{
			List<MemberDeclarationSyntax> syntaxNodes = new List<MemberDeclarationSyntax>();

			foreach (string classPath in clasesPaths)
			{
				var updatedPath = classPath.Replace("/", "\\");
				if (!this.iOWrapper.PathHasExtension(updatedPath))
				{
					updatedPath += ".cs";
				}

				string normalizedUpdatedPath = updatedPath.TrimStart(this.iOWrapper.PathDirectorySeparatorChar()).TrimStart(this.iOWrapper.PathAltDirectorySeparatorChar());
				var filePath = this.iOWrapper.PathCombine(serverMethodPath, normalizedUpdatedPath);
				var source = this.iOWrapper.FileReadAllText(filePath, new UTF8Encoding(true));
				var tree = CSharpSyntaxTree.ParseText(source);
				var root = tree.GetRoot();

				var members = root.DescendantNodes()
					.OfType<AttributeSyntax>()
					.Where(a => a.Name.ToString() == attributeName)
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

				syntaxNodes.AddRange(members.Select(x => x.Parent as MemberDeclarationSyntax));
			}

			return syntaxNodes.ToArray();
		}

		private string GetSourceCodeBetweenRegion(string codeWithRegion)
		{
			string startRegionPattern = @"#region MethodCode( |\t)*\r\n";
			string endRegionPattern = string.Concat(@"\r\n( |\t)*", endMethodCodeRegion);

			var startMatch = Regex.Match(codeWithRegion, startRegionPattern);
			var endMatch = Regex.Match(codeWithRegion, endRegionPattern);
			if (!startMatch.Success || !endMatch.Success)
			{
				throw new Exception();
			}

			int userCodeStartIndex = startMatch.Index + startMatch.Length;
			int userCodeEndIndex = endMatch.Index;
			int userCodeLength = userCodeEndIndex - userCodeStartIndex;

			return userCodeLength > 0 ? codeWithRegion.Substring(userCodeStartIndex, userCodeLength) : string.Empty;
		}

		private string FormattingCode(string code)
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
			SyntaxNode node = tree.GetRoot();

			node = Microsoft.CodeAnalysis.Formatting.Formatter.Format(node, this.projectManager.VisualStudioWorkspace);
			string formatedCode = node.ToString();
			return formatedCode;
		}

		private string EscapeAttributes(string userCode)
		{
			return userCode.Replace("[PartialPath", "//[PartialPath").Replace("[ExternalPath", "//[ExternalPath");
		}

		private bool CodeIndexInMethodRegions(SyntaxNode root, int index)
		{
			string code = root.ToString();
			int regionStartEndex = code.IndexOf(startMethodCodeRegion);
			int regionEndIndex = code.IndexOf(endMethodCodeRegion);
			return regionStartEndex < index && index < regionEndIndex;
		}
	}
}
