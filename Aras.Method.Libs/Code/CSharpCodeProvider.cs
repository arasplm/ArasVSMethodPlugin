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
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Aras.Method.Libs.Code
{
	public class CSharpCodeProvider : ICodeProvider
	{
		private readonly ICodeItemProvider codeItemProvider;
		private readonly ICodeFormatter codeFormatter;
		private readonly IIOWrapper iOWrapper;
		private readonly MessageManager messageManager;

		private const string FNCMethod = "FNCMethod";
		private const string ArasCLS = "ArasCLS{0}";
		private const string ArasPKG = "ArasPKG{0}";
		private const string MethodCodeMask = "$(MethodCode)";

		public string Language { get { return GlobalConsts.CSharp; } }

		public CSharpCodeProvider(
			ICodeItemProvider codeItemProvider,
			ICodeFormatter codeFormatter,
			IIOWrapper iOWrapper,
			MessageManager messageManager)
		{
			this.codeItemProvider = codeItemProvider ?? throw new ArgumentNullException(nameof(codeItemProvider));
			this.codeFormatter = codeFormatter ?? throw new ArgumentNullException(nameof(codeFormatter));
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
		}

		public string LoadMethodCode(string methodFolderPath, string sourceCode)
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

			MemberDeclarationSyntax[] externalsSyntaxNodes = LoadSyntaxNodesByAttribute(GlobalConsts.ExternalPath, methodFolderPath);
			externalsSyntaxNodes = externalsSyntaxNodes.OrderBy(node => GetIndexInAttribute(node, GlobalConsts.ExternalPath)).ToArray();
			externalsSyntaxNodes = RemoveIndexInAttributes(externalsSyntaxNodes, GlobalConsts.ExternalPath);

			MemberDeclarationSyntax[] partialsSyntaxNodes = LoadSyntaxNodesByAttribute(GlobalConsts.PartialPath, methodFolderPath);
			partialsSyntaxNodes = partialsSyntaxNodes.OrderBy(node => GetIndexInAttribute(node, GlobalConsts.PartialPath)).ToArray();
			partialsSyntaxNodes = RemoveIndexInAttributes(partialsSyntaxNodes, GlobalConsts.PartialPath);

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
						throw new Exception(messageManager.GetMessage("NoPartialClassesFound"));
					}

					var partialClassNodeWithPartials = partialClassNode.AddMembers(partialsSyntaxNodes);
					root = root.ReplaceNode(partialClassNode, partialClassNodeWithPartials);
				}

				if (externalsSyntaxNodes.Any())
				{
					if (classesCount == 0)
					{
						string externals = String.Concat(externalsSyntaxNodes.Select(x => x.ToFullString()));
						string updatedCode = Regex.Replace(root.ToFullString(), $"(\r\n)([' '|\t]*{GlobalConsts.EndregionMethodCode})", $"$1{externals}{GlobalConsts.EndregionMethodCode}");

						tree = CSharpSyntaxTree.ParseText(updatedCode);
						root = tree.GetRoot();
					}
					else if (classesCount == 1)
					{
						var classNode = root.DescendantNodes()
							.OfType<NamespaceDeclarationSyntax>()
							.First()
							.ChildNodes()
							.OfType<ClassDeclarationSyntax>()
							.First();

						root = root.InsertNodesAfter(classNode, externalsSyntaxNodes);
					}
					else
					{
						var classNode = root.DescendantNodes()
							.OfType<NamespaceDeclarationSyntax>()
							.First()
							.ChildNodes()
							.OfType<ClassDeclarationSyntax>()
							.Last();

						root = root.InsertNodesBefore(classNode, externalsSyntaxNodes);
					}
				}

				userCode = GetSourceCodeBetweenRegion(root.ToString());
			}

			return EscapeAttributes(userCode);
		}

		public GeneratedCodeInfo GenerateCodeInfo(GeneratedCodeInfo codeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodCode)
		{
			codeInfo = this.CreateWrapper(codeInfo, template, eventData);
			codeInfo = this.CreateMainNew(codeInfo, template, eventData, methodCode);
			codeInfo = this.CreatePartialClasses(codeInfo);
			codeInfo = this.CreateExternalItems(codeInfo);

			if (eventData.EventSpecificData != EventSpecificData.None)
			{
				codeInfo.MethodCodeInfo.Code = codeInfo.MethodCodeInfo.Code.Insert(0, "#define EventDataIsAvailable\r\n");
				codeInfo.WrapperCodeInfo.Code = codeInfo.WrapperCodeInfo.Code.Insert(0, "#define EventDataIsAvailable\r\n");
			}

			return codeInfo;
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, string methodCode, bool useCodeFormatting)
		{
			GeneratedCodeInfo codeInfo = InitializeCodeInfo(methodName, useCodeFormatting);

			codeInfo = this.CreateWrapper(codeInfo, template, eventData);
			codeInfo = this.CreateMainNew(codeInfo, template, eventData, methodCode);
			codeInfo = this.CreatePartialClasses(codeInfo);
			codeInfo = this.CreateExternalItems(codeInfo);

			if (eventData.EventSpecificData != EventSpecificData.None)
			{
				codeInfo.MethodCodeInfo.Code = codeInfo.MethodCodeInfo.Code.Insert(0, "#define EventDataIsAvailable\r\n");
				codeInfo.WrapperCodeInfo.Code = codeInfo.WrapperCodeInfo.Code.Insert(0, "#define EventDataIsAvailable\r\n");
			}

			return codeInfo;
		}

		public CodeInfo CreatePartialCodeItemInfo(
			MethodInfo methodInformation,
			string fileName,
			CodeElementType codeElementType,
			bool useVSFormatting,
			string serverMethodFolderPath,
			string selectedFolderPath,
			string methodName,
			TemplateLoader templateLoader,
			string MethodPath)
		{
			string codeItemPath = selectedFolderPath.Substring(serverMethodFolderPath.Length).TrimStart('\\', '/');
			codeItemPath = Path.Combine(methodName, codeItemPath, fileName);
			string codeItemAttributePath = codeItemPath.Substring(codeItemPath.IndexOf(methodName) + methodName.Length + 1);
			codeItemAttributePath = codeItemAttributePath.Replace("\\", "/");

			TemplateInfo template = null;
			template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.TemplateName == methodInformation.TemplateName).FirstOrDefault();
			if (template == null)
			{
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception(messageManager.GetMessage("TemplateNotFound"));
			}

			EventSpecificDataType eventData = CommonData.EventSpecificDataTypeList.First(x => x.EventSpecificData == methodInformation.EventData);

			MemberDeclarationSyntax[] partialsSyntaxNodes = LoadSyntaxNodesByAttribute(GlobalConsts.PartialPath, serverMethodFolderPath);
			int nextIndex = 0;
			if (partialsSyntaxNodes.Any())
			{
				nextIndex = partialsSyntaxNodes.Max(x => GetIndexInAttribute(x, GlobalConsts.PartialPath));
				nextIndex += 1;
			}

			GeneratedCodeInfo codeInfo = InitializeCodeInfo(methodName, useVSFormatting);
			codeInfo = this.CreateWrapper(codeInfo, template, eventData);

			string methodCode = this.iOWrapper.FileReadAllText(MethodPath, new UTF8Encoding(true));
			var tree = CSharpSyntaxTree.ParseText(methodCode);
			var root = tree.GetRoot();

			var referenceUsings = string.Empty;
			var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
			referenceUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
			if (!string.IsNullOrEmpty(referenceUsings))
			{
				referenceUsings += "\r\n";
			}

			ClassDeclarationSyntax partialClassDeclarationSyntax = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.FirstOrDefault(x => x.Modifiers.Any(SyntaxKind.PartialKeyword));
			if (partialClassDeclarationSyntax == null)
			{
				throw new Exception(messageManager.GetMessage("NoPartialClassesFound"));
			}

			string codeItemTemplate = this.codeItemProvider.GetCodeElementTypeTemplate(CodeType.Partial, codeElementType);
			string code = string.Format(codeItemTemplate, referenceUsings, partialClassDeclarationSyntax?.Identifier.Text, codeItemAttributePath, nextIndex, codeInfo.Namespace, fileName);
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Path = codeItemPath,
				Code = useVSFormatting ? this.codeFormatter.Format(code) : code
			};

			return codeItemInfo;
		}

		public CodeInfo CreateExternalCodeItemInfo(
			MethodInfo methodInformation,
			string fileName,
			CodeElementType codeElementType,
			bool useVSFormatting,
			string serverMethodFolderPath,
			string selectedFolderPath,
			string methodName,
			TemplateLoader templateLoader,
			string MethodPath)
		{
			string codeItemPath = selectedFolderPath.Substring(serverMethodFolderPath.Length).TrimStart('\\', '/');
			codeItemPath = Path.Combine(methodName, codeItemPath, fileName);
			string codeItemAttributePath = codeItemPath.Substring(codeItemPath.IndexOf(methodName) + methodName.Length + 1);
			codeItemAttributePath = codeItemAttributePath.Replace("\\", "/");

			TemplateInfo template = null;
			template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.TemplateName == methodInformation.TemplateName).FirstOrDefault();
			if (template == null)
			{
				template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodInformation.MethodLanguage && t.IsSupported).FirstOrDefault();
			}
			if (template == null)
			{
				throw new Exception(messageManager.GetMessage("TemplateNotFound"));
			}

			EventSpecificDataType eventData = CommonData.EventSpecificDataTypeList.First(x => x.EventSpecificData == methodInformation.EventData);

			MemberDeclarationSyntax[] externalsSyntaxNodes = LoadSyntaxNodesByAttribute(GlobalConsts.ExternalPath, serverMethodFolderPath);
			int nextIndex = 0;
			if (externalsSyntaxNodes.Any())
			{
				nextIndex = externalsSyntaxNodes.Max(x => GetIndexInAttribute(x, GlobalConsts.ExternalPath));
				nextIndex += 1;
			}

			GeneratedCodeInfo codeInfo = InitializeCodeInfo(methodName, useVSFormatting);
			codeInfo = this.CreateWrapper(codeInfo, template, eventData);

			string methodCode = this.iOWrapper.FileReadAllText(MethodPath, new UTF8Encoding(true));
			var tree = CSharpSyntaxTree.ParseText(methodCode);
			var root = tree.GetRoot();

			var referenceUsings = string.Empty;
			var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
			referenceUsings = string.Join("\r\n", mainUsingDirectiveSyntaxes);
			if (!string.IsNullOrEmpty(referenceUsings))
			{
				referenceUsings += "\r\n";
			}

			string codeItemTemplate = this.codeItemProvider.GetCodeElementTypeTemplate(CodeType.External, codeElementType);
			string code = string.Format(codeItemTemplate, referenceUsings, null, codeItemAttributePath, nextIndex, codeInfo.Namespace, fileName);
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Path = codeItemPath,
				Code = useVSFormatting ? this.codeFormatter.Format(code) : code
			};

			return codeItemInfo;
		}

		public CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath)
		{
			SyntaxNode root;
			activeDocument.TryGetSyntaxRoot(out root);
			SyntaxNode newRoot = root.RemoveNode(activeSyntaxNode, SyntaxRemoveOptions.KeepNoTrivia);

			CodeInfo codeInfo = new CodeInfo()
			{
				Code = newRoot.ToFullString(),
				Path = activeDocument.FilePath.Substring(serverMethodFolderPath.IndexOf(serverMethodFolderPath) + serverMethodFolderPath.Length)
			};

			return codeInfo;
		}

		public CodeInfo InsertActiveNodeToMainMethod(string mainMethodFullPath, string serverMethodFolderPath, SyntaxNode activeSyntaxNode, string activeDocumentPath)
		{
			string code = string.Empty;
			string codeItemPath = mainMethodFullPath.Substring(serverMethodFolderPath.Length).TrimStart('\\', '/');
			if (Path.HasExtension(codeItemPath))
			{
				string extention = Path.GetExtension(codeItemPath);
				codeItemPath = codeItemPath.Substring(0, codeItemPath.Length - extention.Length);
			}

			SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(mainMethodFullPath));
			SyntaxNode root = tree.GetRoot();

			// remove attribute
			List<AttributeListSyntax> attributeListSyntaxes = activeSyntaxNode.DescendantNodes()
				.OfType<AttributeListSyntax>()
				.Where(x => x.Attributes.Any(y => y.Name.ToString().StartsWith(GlobalConsts.PartialPath)|| y.Name.ToString().StartsWith(GlobalConsts.ExternalPath)))
				.ToList();

			SyntaxNode activeSyntaxNodeWithOutAttributes = activeSyntaxNode.RemoveNodes(attributeListSyntaxes, SyntaxRemoveOptions.KeepNoTrivia);
			if (attributeListSyntaxes.Any(x => x.Attributes.Any(y => y.Name.ToString().StartsWith(GlobalConsts.PartialPath))))
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
					throw new Exception(messageManager.GetMessage("NoPartialClassesFound"));
				}

				var partialClassNodeWithPartials = partialClassNode.AddMembers(new MemberDeclarationSyntax[] { (MemberDeclarationSyntax)activeSyntaxNodeWithOutAttributes });
				root = root.ReplaceNode(partialClassNode, partialClassNodeWithPartials);

				code = root.ToFullString();
			}
			else if (attributeListSyntaxes.Any(x => x.Attributes.Any(y => y.Name.ToString().StartsWith(GlobalConsts.ExternalPath))))
			{
				int count = root.DescendantNodes()
					.OfType<NamespaceDeclarationSyntax>()
					.First()
					.ChildNodes()
					.OfType<ClassDeclarationSyntax>()
					.Count();

				if (count <= 1)
				{
					var namespaceNode = root.DescendantNodes()
						.OfType<NamespaceDeclarationSyntax>()
						.First();

					var namespaceNodeWithPartials = namespaceNode.AddMembers((MemberDeclarationSyntax)activeSyntaxNodeWithOutAttributes);
					root = root.ReplaceNode(namespaceNode, namespaceNodeWithPartials);

					code = root.ToFullString();
				}
				else
				{
					var classNode = root.DescendantNodes()
						.OfType<NamespaceDeclarationSyntax>()
						.First()
						.ChildNodes()
						.OfType<ClassDeclarationSyntax>()
						.Last();

					root = root.InsertNodesBefore(classNode, new MemberDeclarationSyntax[] { (MemberDeclarationSyntax)activeSyntaxNodeWithOutAttributes });

					code = root.ToFullString();
				}
			}
			else
			{
				throw new Exception(messageManager.GetMessage("NoAttributeFound"));
			}

			CodeInfo codeInfo = new CodeInfo()
			{
				Code = code,
				Path = codeItemPath
			};

			return codeInfo;
		}

		public CodeInfo InsertActiveNodeToPartial(string partialfullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath)
		{
			string code = string.Empty;
			string codeItemPath = partialfullPath.Substring(serverMethodFolderPath.Length).TrimStart('\\', '/');
			if (Path.HasExtension(codeItemPath))
			{
				string extention = Path.GetExtension(codeItemPath);
				codeItemPath = codeItemPath.Substring(0, codeItemPath.Length - extention.Length);
			}

			string codeItemAttributePath = codeItemPath.Substring(codeItemPath.IndexOf(methodName) + methodName.Length + 1).Replace("\\", "/");

			// remove attributes
			List<AttributeListSyntax> attributeListSyntaxes = activeSyntaxNode.DescendantNodes()
				.OfType<AttributeListSyntax>()
				.Where(x => x.Attributes.Any(y => y.Name.ToString().StartsWith(GlobalConsts.PartialPath) || y.Name.ToString().StartsWith(GlobalConsts.ExternalPath)))
				.ToList();

			SyntaxNode syntaxNode = activeSyntaxNode.RemoveNodes(attributeListSyntaxes, SyntaxRemoveOptions.KeepNoTrivia);

			// insert attribute
			NameSyntax name = SyntaxFactory.ParseName(GlobalConsts.PartialPath);
			AttributeArgumentListSyntax arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{codeItemAttributePath}\")");
			AttributeSyntax attribute = SyntaxFactory.Attribute(name, arguments);
			SeparatedSyntaxList<AttributeSyntax> attributeList = new SeparatedSyntaxList<AttributeSyntax>().Add(attribute);
			AttributeListSyntax attributeListSyntax = SyntaxFactory.AttributeList(attributeList);

			syntaxNode = InsertAttributeListSyntax(syntaxNode, attributeListSyntax);

			if (File.Exists(partialfullPath))
			{
				SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(partialfullPath));
				SyntaxNode root = tree.GetRoot();

				var partialClassNode = root.DescendantNodes()
						.OfType<NamespaceDeclarationSyntax>()
						.First()
						.ChildNodes()
						.OfType<ClassDeclarationSyntax>()
						.Where(x => x.Modifiers.Any(SyntaxKind.PartialKeyword))
						.FirstOrDefault();
				if (partialClassNode == null)
				{
					throw new Exception(messageManager.GetMessage("NoPartialClassesFound"));
				}

				var newPartialClassNode = partialClassNode.AddMembers((MemberDeclarationSyntax)syntaxNode);
				root = root.ReplaceNode(partialClassNode, newPartialClassNode);

				code = root.ToFullString();
			}
			else
			{
				var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(activeDocumentMethodFullPath));
				var root = tree.GetRoot();

				var partialClassNode = root.DescendantNodes()
						.OfType<NamespaceDeclarationSyntax>()
						.First()
						.ChildNodes()
						.OfType<ClassDeclarationSyntax>()
						.Where(x => x.Modifiers.Any(SyntaxKind.PartialKeyword))
						.FirstOrDefault();
				if (partialClassNode == null)
				{
					throw new Exception(messageManager.GetMessage("NoPartialClassesFound"));
				}

				var usings = string.Empty;
				var mainUsingDirectiveSyntaxes = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
				usings = string.Join(Environment.NewLine, mainUsingDirectiveSyntaxes);
				if (!string.IsNullOrEmpty(usings))
				{
					usings += Environment.NewLine;
				}

				var namespaceNode = root.DescendantNodes()
					.OfType<NamespaceDeclarationSyntax>()
					.First();

				string template = "{0}using Common;\r\nusing Common.Attributes;\r\nnamespace {3} \r\n{{public partial class {1} \r\n{{\r\n{2}\r\n}}\r\n}}";
				code = string.Format(template, usings, partialClassNode.Identifier.Text.ToString(), syntaxNode.ToFullString(), namespaceNode.Name.ToString());
			}

			CodeInfo codeInfo = new CodeInfo()
			{
				Code = code,
				Path = codeItemPath
			};

			return codeInfo;
		}

		public CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath)
		{
			string code = string.Empty;
			string codeItemPath = externalFullPath.Substring(serverMethodFolderPath.Length).TrimStart('\\', '/');
			if (Path.HasExtension(codeItemPath))
			{
				string extention = Path.GetExtension(codeItemPath);
				codeItemPath = codeItemPath.Substring(0, codeItemPath.Length - extention.Length);
			}

			string codeItemAttributePath = codeItemPath.Substring(codeItemPath.IndexOf(methodName) + methodName.Length + 1).Replace("\\", "/");

			// remove attributes
			List<AttributeListSyntax> attributeListSyntaxes = activeSyntaxNode.DescendantNodes()
				.OfType<AttributeListSyntax>()
				.Where(x => x.Attributes.Any(y => y.Name.ToString().StartsWith(GlobalConsts.PartialPath) || y.Name.ToString().StartsWith(GlobalConsts.ExternalPath)))
				.ToList();

			SyntaxNode syntaxNode = activeSyntaxNode.RemoveNodes(attributeListSyntaxes, SyntaxRemoveOptions.KeepNoTrivia);

			// insert attribute
			NameSyntax name = SyntaxFactory.ParseName(GlobalConsts.ExternalPath);
			AttributeArgumentListSyntax arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{codeItemAttributePath}\")");
			AttributeSyntax attribute = SyntaxFactory.Attribute(name, arguments);
			SeparatedSyntaxList<AttributeSyntax> attributeList = new SeparatedSyntaxList<AttributeSyntax>().Add(attribute);
			AttributeListSyntax attributeListSyntax = SyntaxFactory.AttributeList(attributeList);

			syntaxNode = InsertAttributeListSyntax(activeSyntaxNode, attributeListSyntax);

			if (File.Exists(externalFullPath))
			{
				SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(externalFullPath));
				SyntaxNode root = tree.GetRoot();

				var namespaceNode = root.DescendantNodes()
					.OfType<NamespaceDeclarationSyntax>()
					.First();

				var namespaceWithExternalItem = namespaceNode.AddMembers((MemberDeclarationSyntax)syntaxNode);
				root = root.ReplaceNode(namespaceNode, namespaceWithExternalItem);
			}
			else
			{
				var methodTree = CSharpSyntaxTree.ParseText(File.ReadAllText(activeDocumentMethodFullPath));
				var methodRoot = methodTree.GetRoot();

				var usings = string.Empty;
				var mainUsingDirectiveSyntaxes = methodRoot.DescendantNodes().OfType<UsingDirectiveSyntax>();
				usings = string.Join(Environment.NewLine, mainUsingDirectiveSyntaxes);
				if (!string.IsNullOrEmpty(usings))
				{
					usings += Environment.NewLine;
				}

				var namespaceNode = methodRoot.DescendantNodes()
					.OfType<NamespaceDeclarationSyntax>()
					.First();

				string template = "{0}using Common;\r\nusing Common.Attributes;\r\nnamespace {2} \r\n{{\r\n{1}\r\n}}";
				code = string.Format(template, usings, syntaxNode.ToFullString(), namespaceNode.Name.ToString());
			}

			CodeInfo codeInfo = new CodeInfo()
			{
				Code = code,
				Path = codeItemPath
			};

			return codeInfo;
		}

		public CodeInfo UpdateSourceCodeToInsertExternalItems(string methodFolderPath, string sourceCode, MethodInfo methodInformation)
		{
			MemberDeclarationSyntax[] externalsSyntaxNodes = LoadSyntaxNodesByAttribute(GlobalConsts.ExternalPath, methodFolderPath);
			if (externalsSyntaxNodes.Length == 0)
			{
				return null;
			}

			var tree = CSharpSyntaxTree.ParseText(sourceCode);
			SyntaxNode root = tree.GetRoot();
			List<ClassDeclarationSyntax> classDeclarationSyntaxNodes = root.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.First()
				.ChildNodes()
				.OfType<ClassDeclarationSyntax>()
				.ToList();

			if (classDeclarationSyntaxNodes.Count == 0)
			{
				return null;
			}
			if (classDeclarationSyntaxNodes.Count == 1)
			{
				ClassDeclarationSyntax classNode = classDeclarationSyntaxNodes.First();
				if (CodeIndexInMethodRegions(root, classNode.Span.End))
				{
					return null;
				}
			}
			else
			{
				ClassDeclarationSyntax classNode = classDeclarationSyntaxNodes.Last();
				if (CodeIndexInMethodRegions(root, classNode.Span.Start))
				{
					return null;
				}
			}

			var codeInfo = new CodeInfo()
			{
				Path = iOWrapper.PathCombine(methodInformation.MethodName, methodInformation.MethodName),
				Code = Regex.Replace(sourceCode, $"(\r\n)([' '|\t]*{GlobalConsts.EndregionMethodCode})", $"$1        }}\r\n    }}\r\n\r\n    [System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Performance\", \"CA1812: AvoidUninstantiatedInternalClasses\")]\r\n    internal class ArasPluginFin\r\n    {{\r\n        internal ArasPluginFin()\r\n        {{\r\n{GlobalConsts.EndregionMethodCode}")
			};

			return codeInfo;
		}

		public GeneratedCodeInfo InitializeCodeInfo(string methodName, bool useCodeFormatting)
		{
			string methodNameWithOutSpases = Regex.Replace(methodName, "[^a-zA-Z0-9]+", string.Empty, RegexOptions.Compiled);
			string clsname = string.Format(ArasCLS, methodNameWithOutSpases);
			string pkgname = string.Format(ArasPKG, methodNameWithOutSpases);

			GeneratedCodeInfo codeInfo = new GeneratedCodeInfo();
			codeInfo.ClassName = clsname;
			codeInfo.Namespace = pkgname;
			codeInfo.MethodName = methodName;
			codeInfo.IsUseVSFormatting = useCodeFormatting;

			return codeInfo;
		}

		private GeneratedCodeInfo CreateWrapper(GeneratedCodeInfo codeInfo, TemplateInfo template, EventSpecificDataType eventData)
		{
			string methodName = codeInfo.MethodName;
			string clsname = codeInfo.ClassName;
			string pkgname = codeInfo.Namespace;
			string eventDataClass = eventData.EventDataClass;
			string interfaceName = eventData.InterfaceName;

			string wrapperCode = template.TemplateCode;
			wrapperCode = wrapperCode.Replace("$(clsname)", clsname);
			wrapperCode = wrapperCode.Replace("$(pkgname)", pkgname);
			wrapperCode = wrapperCode.Replace("$(EventDataClass)", eventDataClass);
			wrapperCode = wrapperCode.Replace("$(interfacename)", interfaceName);
			wrapperCode = wrapperCode.Replace("$(fncname)", FNCMethod);

			codeInfo.WrapperCodeInfo.Code = codeInfo.IsUseVSFormatting ? this.codeFormatter.Format(wrapperCode) : wrapperCode;
			codeInfo.WrapperCodeInfo.Path = this.iOWrapper.PathCombine(methodName, methodName + "Wrapper.cs");

			return codeInfo;
		}

		private GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo codeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodCode)
		{
			string sourceCode = string.Empty;
			string wrapperCode = codeInfo.WrapperCodeInfo.Code;

			string methodName = codeInfo.MethodName;
			string parentClassName = string.Empty;

			SyntaxNode wrapperRoot = CSharpSyntaxTree.ParseText(wrapperCode).GetRoot();
			SyntaxNode methodMaskNode = wrapperRoot.FindNode(new TextSpan(wrapperCode.IndexOf(MethodCodeMask), MethodCodeMask.Length), getInnermostNodeForTie: true);
			SyntaxNode methodContainer = FindContainer(methodMaskNode);

			if (methodContainer is MethodDeclarationSyntax)
			{
				ClassDeclarationSyntax classContainer = FindContainer(methodContainer) as ClassDeclarationSyntax;
				SyntaxNode classContainerPartial = classContainer.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
				wrapperRoot = wrapperRoot.ReplaceNode(classContainer, classContainerPartial);
				parentClassName = classContainer.Identifier.Text;

				methodMaskNode = wrapperRoot.FindNode(new TextSpan(wrapperCode.IndexOf(MethodCodeMask), MethodCodeMask.Length), getInnermostNodeForTie: true);
				methodContainer = FindContainer(methodMaskNode);
				wrapperRoot = wrapperRoot.RemoveNode(wrapperRoot.FindNode(methodContainer.Span), SyntaxRemoveOptions.KeepNoTrivia);

				sourceCode = CreateSourceCode(wrapperRoot, methodContainer, classContainer);
				wrapperCode = wrapperRoot.ToFullString();
			}
			else if (methodMaskNode is ClassDeclarationSyntax methodMaskClassContainer)
			{
				parentClassName = methodMaskClassContainer.Identifier.Text;

				SyntaxNode classContainerPartial = methodMaskClassContainer.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
				wrapperRoot = wrapperRoot.ReplaceNode(methodMaskClassContainer, classContainerPartial);
				wrapperCode = wrapperRoot.ToString();

				sourceCode = CreateSourceCode(parentClassName, wrapperRoot);
				wrapperCode = Regex.Replace(wrapperCode, @"( |\t)*\$\(MethodCode\)", string.Empty);
			}
			else if (methodContainer is ClassDeclarationSyntax methodClassContainer)
			{
				parentClassName = methodClassContainer.Identifier.Text;

				SyntaxNode classContainerPartial = methodClassContainer.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
				wrapperRoot = wrapperRoot.ReplaceNode(methodClassContainer, classContainerPartial);
				wrapperCode = wrapperRoot.ToString();

				sourceCode = CreateSourceCode(parentClassName, wrapperRoot);
				wrapperCode = Regex.Replace(wrapperCode, @"( |\t)*\$\(MethodCode\)", string.Empty);
			}
			else if (methodMaskNode is NamespaceDeclarationSyntax | methodContainer is NamespaceDeclarationSyntax)
			{
				sourceCode = wrapperCode;
				wrapperCode = string.Empty;
			}
			else
			{
				throw new NotSupportedException();
			}

			sourceCode = Regex.Replace(sourceCode, @"( |\t)*\$\(MethodCode\)", $"{messageManager.GetMessage("startYourCodeInsideRegionMethodCodeDoNotChangeCodeAbove")}\r\n{GlobalConsts.RegionMethodCode}\r\n{methodCode}\r\n{GlobalConsts.EndregionMethodCode}\r\n{messageManager.GetMessage("endyourCodeInsideRegionMethodCodeDoNotChangeCodeBelow")}");

			codeInfo.WrapperCodeInfo.Code = codeInfo.IsUseVSFormatting ? this.codeFormatter.Format(wrapperCode) : wrapperCode;
			codeInfo.MethodCodeInfo.Code = codeInfo.IsUseVSFormatting ? this.codeFormatter.Format(sourceCode) : sourceCode;
			codeInfo.MethodCodeInfo.Path = this.iOWrapper.PathCombine(methodName, methodName + ".cs");
			codeInfo.MethodCodeParentClassName = parentClassName;

			return codeInfo;
		}

		private string CreateSourceCode(string parentClassName, SyntaxNode wrapperRoot)
		{
			CompilationUnitSyntax wrapperCompilationUnitSyntax = wrapperRoot as CompilationUnitSyntax;
			NamespaceDeclarationSyntax wrapperNamespace = wrapperCompilationUnitSyntax.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
			UsingDirectiveSyntax[] wrapperUsings = wrapperCompilationUnitSyntax.Usings.OfType<UsingDirectiveSyntax>().ToArray();

			CompilationUnitSyntax sourceCompilationUnitSyntax = CompilationUnit()
				.AddUsings(wrapperUsings)
				.WithMembers(
					SingletonList<MemberDeclarationSyntax>(
						NamespaceDeclaration(
							IdentifierName(wrapperNamespace.Name.ToString()))
						.WithMembers(
							SingletonList<MemberDeclarationSyntax>(
								ClassDeclaration(parentClassName)
								.WithModifiers(
									TokenList(
										new[]{
											Token(SyntaxKind.PublicKeyword),
											Token(SyntaxKind.PartialKeyword)}))
								.WithMembers(
									SingletonList<MemberDeclarationSyntax>(
										IncompleteMember(
											IdentifierName("$(MethodCode)"))))))))
				.NormalizeWhitespace();

			return sourceCompilationUnitSyntax.ToFullString();
		}

		private string CreateSourceCode(SyntaxNode wrapperRoot, SyntaxNode methodContainer, ClassDeclarationSyntax classContainer)
		{
			CompilationUnitSyntax wrapperCompilationUnitSyntax = wrapperRoot as CompilationUnitSyntax;
			NamespaceDeclarationSyntax wrapperNamespace = wrapperCompilationUnitSyntax.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First();
			UsingDirectiveSyntax[] wrapperUsings = wrapperCompilationUnitSyntax.Usings.OfType<UsingDirectiveSyntax>().ToArray();

			ClassDeclarationSyntax sourceClass = SyntaxFactory.ClassDeclaration(classContainer.Identifier.Text)
				.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
				.NormalizeWhitespace()
				.AddMembers(methodContainer as MethodDeclarationSyntax);

			NamespaceDeclarationSyntax sourceNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(wrapperNamespace.Name.ToString()))
				.NormalizeWhitespace()
				.AddMembers(sourceClass.WithTrailingTrivia(SyntaxFactory.Whitespace(Environment.NewLine)))
				.WithLeadingTrivia(SyntaxFactory.Whitespace(Environment.NewLine));

			CompilationUnitSyntax sourceCompilationUnitSyntax = SyntaxFactory.CompilationUnit();
			sourceCompilationUnitSyntax = sourceCompilationUnitSyntax.AddUsings(wrapperUsings);
			sourceCompilationUnitSyntax = sourceCompilationUnitSyntax.AddMembers(sourceNamespace);

			return sourceCompilationUnitSyntax.ToFullString();
		}

		private GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo)
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

			int argumentIndex = 0;

			foreach (var member in members)
			{
				var path = member.AttributeInfo.ArgumentList.Arguments.FirstOrDefault()?.ToString();
				path = path.Replace("/", "\\").Replace("\"", string.Empty);
				path = Path.Combine(resultGeneratedCode.MethodName, path);
				if (path != null)
				{
					string partialString = string.Empty;
					string stringForReplace = string.Empty;
					string shouldBeReplaced = member.Parent.ToFullString();

					if (member.AttributeInfo.ArgumentList.Arguments.Count == 1)
					{
						argumentIndex += 1;

						var newAttribute =  member.AttributeInfo.AddArgumentListArguments(AttributeArgument(ParseExpression(argumentIndex.ToString())).WithLeadingTrivia(Space));
						partialString = member.Parent.ReplaceNode(member.AttributeInfo, newAttribute).ToFullString();
					}
					else
					{
						partialString = shouldBeReplaced = member.Parent.ToFullString();
					}

					if (partialString.Contains(GlobalConsts.EndregionMethodCode))
					{
						int indexofEndRegion = partialString.IndexOf(GlobalConsts.EndregionMethodCode);
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
						string pattern = string.Concat(@"(\r\n|\n)?( |\t)*}( |\t)*(\r\n|\n)?( |\t)*", GlobalConsts.EndregionMethodCode);
						string insertRegion = string.Concat(Environment.NewLine, GlobalConsts.EndregionMethodCode);
						string replacedCode = Regex.Replace(resultGeneratedCode.MethodCodeInfo.Code, pattern, insertRegion);
						resultGeneratedCode.MethodCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? this.codeFormatter.Format(replacedCode) : replacedCode;
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
				partialCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? this.codeFormatter.Format(code) : code;
			}

			return resultGeneratedCode;
		}

		private GeneratedCodeInfo CreateExternalItems(GeneratedCodeInfo methodInfo)
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

			int argumentIndex = 0;

			foreach (var member in members)
			{
				var path = member.AttributeInfo.ArgumentList.Arguments.FirstOrDefault()?.ToString();
				path = path.Replace("/", "\\").Replace("\"", string.Empty);
				path = Path.Combine(resultGeneratedCode.MethodName, path);
				if (path != null)
				{
					string externalString = string.Empty;
					string stringForReplace = string.Empty;
					string shouldBeReplaced = member.Parent.ToFullString();

					if (member.AttributeInfo.ArgumentList.Arguments.Count == 1)
					{
						argumentIndex += 1;

						var newAttribute = member.AttributeInfo.AddArgumentListArguments(AttributeArgument(ParseExpression(argumentIndex.ToString())).WithLeadingTrivia(Space));
						externalString = member.Parent.ReplaceNode(member.AttributeInfo, newAttribute).ToFullString();
					}
					else
					{
						externalString = shouldBeReplaced = member.Parent.ToFullString();
					}

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
				externalCodeInfo.Code = resultGeneratedCode.IsUseVSFormatting ? this.codeFormatter.Format(code) : code;
			}

			return resultGeneratedCode;
		}

		private MemberDeclarationSyntax[] LoadSyntaxNodesByAttribute(string attributeName, string methodFolderPath)
		{
			List<MemberDeclarationSyntax> syntaxNodes = new List<MemberDeclarationSyntax>();

			List<string> csFiles = iOWrapper.DirectoryGetFiles(methodFolderPath, $"*{GlobalConsts.CSExtension}", SearchOption.AllDirectories).ToList();
			foreach (string filePath in csFiles)
			{
				string source = this.iOWrapper.FileReadAllText(filePath, new UTF8Encoding(true));
				SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
				SyntaxNode root = tree.GetRoot();

				var members = root.DescendantNodes()
					.OfType<AttributeSyntax>()
					.Where(a => a.Name.ToString().StartsWith(attributeName))
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
			string endRegionPattern = string.Concat(@"\r\n( |\t)*", GlobalConsts.EndregionMethodCode);

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

		private string EscapeAttributes(string userCode)
		{
			return userCode.Replace("[PartialPath", "//[PartialPath").Replace("[ExternalPath", "//[ExternalPath");
		}

		private bool CodeIndexInMethodRegions(SyntaxNode root, int index)
		{
			string code = root.ToString();
			int regionStartEndex = code.IndexOf(GlobalConsts.RegionMethodCode);
			int regionEndIndex = code.IndexOf(GlobalConsts.EndregionMethodCode);
			return regionStartEndex < index && index < regionEndIndex;
		}

		#region Roslyn methods

		private SyntaxNode InsertAttributeListSyntax(SyntaxNode syntaxNode, AttributeListSyntax attributeListSyntax)
		{
			SyntaxNode newSyntaxNode = null;

			attributeListSyntax = attributeListSyntax.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaratNode)
			{
				newSyntaxNode = interfaceDeclaratNode.AddAttributeLists(attributeListSyntax);
			}
			else if (syntaxNode is ClassDeclarationSyntax classDeclarationNode)
			{
				newSyntaxNode= classDeclarationNode.AddAttributeLists(attributeListSyntax);
			}
			else if (syntaxNode is StructDeclarationSyntax structDeclarationNode)
			{
				newSyntaxNode = structDeclarationNode.AddAttributeLists(attributeListSyntax);
			}
			else if (syntaxNode is MethodDeclarationSyntax methodDeclarationNode)
			{
				newSyntaxNode = methodDeclarationNode.AddAttributeLists(attributeListSyntax);
			}
			else if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationNode)
			{
				newSyntaxNode = interfaceDeclarationNode.AddAttributeLists(attributeListSyntax);
			}

			return newSyntaxNode;
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

		private SyntaxNode FindContainer(SyntaxNode node)
		{
			if (node == null || node.Parent == null)
			{
				return node;
			}

			SyntaxNode parent = node.Parent;
			if (parent.IsKind(SyntaxKind.CompilationUnit))
			{
				return node;
			}

			if (parent.IsKind(SyntaxKind.MethodDeclaration) ||
				parent.IsKind(SyntaxKind.ClassDeclaration) ||
				parent.IsKind(SyntaxKind.NamespaceDeclaration))
			{
				return parent;
			}

			return FindContainer(parent.Parent);
		}

		private int GetIndexInAttribute(MemberDeclarationSyntax node, string attributeName)
		{
			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = node.DescendantNodes()
				.OfType<AttributeSyntax>()
				.First(attribute => attribute.Name.ToString().StartsWith(attributeName))
				.ArgumentList
				.Arguments;

			return arguments.Count == 2 ? int.Parse(arguments[1].ToString()) : int.MaxValue;
		}

		private MemberDeclarationSyntax[] RemoveIndexInAttributes(MemberDeclarationSyntax[] syntaxNodes, string attributeName)
		{
			MemberDeclarationSyntax[] newSyntaxNodes = new MemberDeclarationSyntax[syntaxNodes.Length];

			for (int i = 0; i < syntaxNodes.Length; i++)
			{
				MemberDeclarationSyntax node = syntaxNodes[i];

				AttributeSyntax attributeNode = node.DescendantNodes().OfType<AttributeSyntax>()
					.Where(a => a.Name.ToString().StartsWith(attributeName))
					.First();

				NameSyntax name = SyntaxFactory.ParseName(attributeName);
				AttributeArgumentListSyntax arguments = SyntaxFactory.ParseAttributeArgumentList($"({attributeNode.ArgumentList.Arguments.FirstOrDefault()?.ToString()})");
				AttributeSyntax attribute = SyntaxFactory.Attribute(name, arguments);

				AttributeSyntax newAttributeNode = attributeNode.ReplaceNode(attributeNode, attribute);

				newSyntaxNodes[i] = node.ReplaceNode(attributeNode, newAttributeNode);
			}

			return newSyntaxNodes;
		}

		#endregion
	}
}
