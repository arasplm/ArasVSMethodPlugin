//------------------------------------------------------------------------------
// <copyright file="SaveToPackageCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class SaveToPackageCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int ItemCommandId = 0x102;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid ItemCommandSet = new Guid("694F6136-7CF1-46E1-B9E2-24296488AE96");

		/// <summary>
		/// Initializes a new instance of the <see cref="SaveToPackageCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private SaveToPackageCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory)
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
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory)
		{
			Instance = new SaveToPackageCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell)
		{
			var project = projectManager.SelectedProject;
			string projectConfigPath = projectManager.ProjectConfigPath;
			string methodConfigPath = projectManager.MethodConfigPath;

			ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

			var templateLoader = new TemplateLoader();
			templateLoader.Load(methodConfigPath);

			var packageManager = new PackageManager(authManager);

			string selectedMethodPath = projectManager.MethodPath;
			string selectedMethodName = Path.GetFileNameWithoutExtension(selectedMethodPath);
			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception();
			}

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);
			var saveView = dialogFactory.GetSaveToPackageView(uiShell, projectConfiguration, templateLoader, packageManager, codeProvider, projectManager, methodInformation, selectedMethodPath);
			var saveViewResult = saveView.ShowDialog();
			if (saveViewResult?.DialogOperationResult != true)
			{
				return;
			}

			string rootPath = saveViewResult.PackagePath;
			string methodPath = Path.Combine(rootPath, $"{saveViewResult.SelectedPackage}\\Import\\Method\\");
			Directory.CreateDirectory(methodPath);

			string importFilePath = Path.Combine(rootPath, "imports.mf");
			if (File.Exists(importFilePath))
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(importFilePath);
				XmlNode importsXmlNode = xmlDocument.SelectSingleNode("imports");
				XmlNodeList packageXmlNodes = importsXmlNode.SelectNodes("package");

				bool isPackageExist = false;
				foreach (XmlNode packageXmlNode in packageXmlNodes)
				{
					if (packageXmlNode.Attributes["name"].InnerText == saveViewResult.SelectedPackage)
					{
						isPackageExist = true;
						break;
					}
				}

				if (!isPackageExist)
				{
					XmlElement packageXmlElement = xmlDocument.CreateElement("package");
					packageXmlElement.SetAttribute("name", saveViewResult.SelectedPackage);
					packageXmlElement.SetAttribute("path", $"{saveViewResult.SelectedPackage}\\Import");
					importsXmlNode.AppendChild(packageXmlElement);

					XmlWriterSettings settings = new XmlWriterSettings();
					settings.Encoding = new UTF8Encoding(true);
					using (XmlWriter xmlWriter = XmlWriter.Create(importFilePath, settings))
					{
						xmlDocument.Save(xmlWriter);
					}
				}
			}
			else
			{
				var xmlDocument = new XmlDocument();
				XmlElement importsXmlNode = xmlDocument.CreateElement("imports");
				XmlElement packageXmlElement = xmlDocument.CreateElement("package");
				packageXmlElement.SetAttribute("name", saveViewResult.SelectedPackage);
				packageXmlElement.SetAttribute("path", $"{saveViewResult.SelectedPackage}\\Import");
				importsXmlNode.AppendChild(packageXmlElement);
				xmlDocument.AppendChild(importsXmlNode);

				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Encoding = new UTF8Encoding(true);
				using (XmlWriter xmlWriter = XmlWriter.Create(importFilePath, settings))
				{
					xmlDocument.Save(xmlWriter);
				}
			}

			string methodId = null;
			string methodFilePath = Path.Combine(rootPath, $"{saveViewResult.SelectedPackage}\\Import\\Method\\{saveViewResult.MethodName}.xml");
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

			string updateMethodCodeForSavingToAmlPackage = saveViewResult.MethodCode.Replace("]]", "]]]]><![CDATA[");
			string methodTemplate = $@"<AML>
 <Item type=""Method"" id=""{methodId}"" action=""add"">
  <comments>{saveViewResult.MethodComment}</comments>
  <execution_allowed_to keyed_name=""{saveViewResult.SelectedIdentityKeyedName}"" type=""Identity"">{saveViewResult.SelectedIdentityId}</execution_allowed_to>
  <method_code><![CDATA[{updateMethodCodeForSavingToAmlPackage}]]></method_code>
  <method_type>{saveViewResult.MethodInformation.MethodLanguage}</method_type>
  <name>{saveViewResult.MethodName}</name>
 </Item>
</AML>";

			XmlDocument resultXmlDoc = new XmlDocument();
			resultXmlDoc.LoadXml(methodTemplate);
			SaveToFile(methodFilePath, resultXmlDoc);
			
			if (methodInformation.MethodName == saveViewResult.MethodName)
			{
				methodInformation.PackageName = saveViewResult.SelectedPackage;
				methodInformation.ExecutionAllowedToKeyedName = saveViewResult.SelectedIdentityKeyedName;
				methodInformation.ExecutionAllowedToId = saveViewResult.SelectedIdentityId;
				methodInformation.MethodComment = saveViewResult.MethodComment;
			}

			projectConfiguration.LastSelectedDir = rootPath;
			projectConfigurationManager.Save(projectConfigPath, projectConfiguration);

			string message = string.Format("Method \"{0}\" saved to package \"{1}\"", saveViewResult.MethodName, saveViewResult.SelectedPackage);

			// Show a message box to prove we were here
			var messageWindow = new MessageBoxWindow();
			messageWindow.ShowDialog(null,
				message,
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
