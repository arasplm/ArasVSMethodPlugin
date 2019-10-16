//------------------------------------------------------------------------------
// <copyright file="SaveToPackageCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class SaveToPackageCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int ItemCommandId = 0x102;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid ItemCommandSet = CommandIds.SaveToPackage;

		/// <summary>
		/// Initializes a new instance of the <see cref="SaveToPackageCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private SaveToPackageCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
			: base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
		{
			if (projectManager.CommandService != null)
			{
				var itemCommandID = new CommandID(ItemCommandSet, ItemCommandId);
				var itemMenu = new OleMenuCommand(this.ExecuteCommand, itemCommandID);
				itemMenu.BeforeQueryStatus += CheckCommandAccessibility;

				projectManager.CommandService.AddCommand(itemMenu);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static SaveToPackageCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new SaveToPackageCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			var packageManager = new PackageManager(authManager, this.messageManager);

			string selectedMethodPath = projectManager.MethodPath;
			string selectedMethodName = Path.GetFileNameWithoutExtension(selectedMethodPath);
			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception();
			}

			string manifastFileName;
			if (methodInformation is PackageMethodInfo)
			{
				PackageMethodInfo packageMethodInfo = (PackageMethodInfo)methodInformation;
				manifastFileName = packageMethodInfo.ManifestFileName;
			}
			else
			{
				manifastFileName = "imports.mf";
			}

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodWorkingFolder = Path.Combine(projectManager.ServerMethodFolderPath, packageMethodFolderPath, methodInformation.MethodName);

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			string sourceCode = File.ReadAllText(selectedMethodPath, new UTF8Encoding(true));
			CodeInfo codeItemInfo = codeProvider.UpdateSourceCodeToInsertExternalItems(methodWorkingFolder, sourceCode, methodInformation);
			if (codeItemInfo != null)
			{
				var dialogResult = dialogFactory.GetMessageBoxWindow().ShowDialog(messageManager.GetMessage("CouldNotInsertExternalItemsInsideOfMethodCodeSection"),
					messageManager.GetMessage("ArasVSMethodPlugin"),
					MessageButtons.OKCancel,
					MessageIcon.Question);
				if (dialogResult == MessageDialogResult.Cancel)
				{
					return;
				}

				projectManager.AddItemTemplateToProjectNew(codeItemInfo, packageMethodFolderPath, true, 0);
				sourceCode = codeItemInfo.Code;
			}

			var saveView = dialogFactory.GetSaveToPackageView(projectConfigurationManager.CurrentProjectConfiguraiton, templateLoader, packageManager, codeProvider, projectManager, methodInformation, sourceCode);
			var saveViewResult = saveView.ShowDialog();
			if (saveViewResult?.DialogOperationResult != true)
			{
				return;
			}

			string pathPackageToSaveMethod;
			string rootPath = saveViewResult.PackagePath;
			string importFilePath = Path.Combine(rootPath, manifastFileName);
			if (File.Exists(importFilePath))
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(importFilePath);
				XmlNode importsXmlNode = xmlDocument.SelectSingleNode("imports");
				XmlNode packageXmlNode = importsXmlNode.SelectSingleNode($"package[@name='{saveViewResult.SelectedPackage}']");

				if (packageXmlNode == null)
				{
					XmlElement packageXmlElement = xmlDocument.CreateElement("package");
					packageXmlElement.SetAttribute("name", saveViewResult.SelectedPackage.Name);
					packageXmlElement.SetAttribute("path", saveViewResult.SelectedPackage.Path);
					importsXmlNode.AppendChild(packageXmlElement);

					XmlWriterSettings settings = new XmlWriterSettings();
					settings.Encoding = new UTF8Encoding(true);
					settings.Indent = true;
					settings.IndentChars = "\t";
					settings.OmitXmlDeclaration = true;
					using (XmlWriter xmlWriter = XmlWriter.Create(importFilePath, settings))
					{
						xmlDocument.Save(xmlWriter);
					}

				}
				else
				{
					pathPackageToSaveMethod = packageXmlNode.Attributes["path"].Value;
				}
			}
			else
			{
				var xmlDocument = new XmlDocument();
				XmlElement importsXmlNode = xmlDocument.CreateElement("imports");
				XmlElement packageXmlElement = xmlDocument.CreateElement("package");
				packageXmlElement.SetAttribute("name", saveViewResult.SelectedPackage.Name);
				packageXmlElement.SetAttribute("path", saveViewResult.SelectedPackage.Path);
				importsXmlNode.AppendChild(packageXmlElement);
				xmlDocument.AppendChild(importsXmlNode);

				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "\t";
				settings.OmitXmlDeclaration = true;
				settings.Encoding = new UTF8Encoding(true);
				using (XmlWriter xmlWriter = XmlWriter.Create(importFilePath, settings))
				{
					xmlDocument.Save(xmlWriter);
				}
			}

			string methodPath = Path.Combine(rootPath, saveViewResult.SelectedPackage.MethodFolderPath);
			Directory.CreateDirectory(methodPath);

			string methodId = null;
			string methodFilePath = Path.Combine(methodPath, $"{saveViewResult.MethodName}.xml");
			if (File.Exists(methodFilePath))
			{
				var methodXmlDocument = new XmlDocument();
				methodXmlDocument.Load(methodFilePath);
				methodId = methodXmlDocument.SelectSingleNode("/AML/Item/@id").InnerText;
			}
			else
			{
				methodId = saveViewResult.MethodInformation.InnovatorMethodConfigId;
			}

			string updateMethodCodeForSavingToAmlPackage = saveViewResult.MethodCode.Replace("]]>", "]]]]><![CDATA[>");
			string methodTemplate = $@"<AML>
 <Item type=""Method"" id=""{methodId}"" action=""add"">" +
  (!string.IsNullOrWhiteSpace(saveViewResult.MethodComment) ? $"<comments>{saveViewResult.MethodComment}</comments>" : "") +
  $@"<execution_allowed_to keyed_name=""{saveViewResult.SelectedIdentityKeyedName}"" type=""Identity"">{saveViewResult.SelectedIdentityId}</execution_allowed_to>
  <method_code><![CDATA[{updateMethodCodeForSavingToAmlPackage}]]></method_code>
  <method_type>{saveViewResult.MethodInformation.MethodLanguage}</method_type>
  <name>{saveViewResult.MethodName}</name>
 </Item>
</AML>";

			XmlDocument resultXmlDoc = new XmlDocument();
			resultXmlDoc.LoadXml(methodTemplate);
			SaveToFile(methodFilePath, resultXmlDoc);

			if (methodInformation.MethodName == saveViewResult.MethodName &&
				methodInformation.Package.Name == saveViewResult.SelectedPackage.Name)
			{
				methodInformation.Package = saveViewResult.SelectedPackage;
				methodInformation.ExecutionAllowedToKeyedName = saveViewResult.SelectedIdentityKeyedName;
				methodInformation.ExecutionAllowedToId = saveViewResult.SelectedIdentityId;
				methodInformation.MethodComment = saveViewResult.MethodComment;
			}

			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedDir = rootPath;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath);

			// Show a message box to prove we were here
			var messageWindow = dialogFactory.GetMessageBoxWindow();
			messageWindow.ShowDialog(this.messageManager.GetMessage("MethodSavedToPackage", saveViewResult.MethodName, saveViewResult.SelectedPackage.Name),
				string.Empty,
				MessageButtons.OK,
				MessageIcon.Information);
		}

		private void SaveToFile(string filePath, XmlDocument xmlDoc)
		{
			using (StreamWriter streamWriter = new StreamWriter(filePath, false, new UTF8Encoding(true)))
			{
				XmlWriterSettings xwSettings = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = true,
					IndentChars = " "
				};
				using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, xwSettings))
				{
					xmlDoc.Save(xmlWriter);
				}
			}
		}
	}
}
