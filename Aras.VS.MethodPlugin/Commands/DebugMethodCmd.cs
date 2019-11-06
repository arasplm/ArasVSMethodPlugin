//------------------------------------------------------------------------------
// <copyright file="DebugMethodCmd.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class DebugMethodCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0104;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.DebugMethod;


		private DebugMethodCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
		{
			if (projectManager.CommandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(this.ExecuteCommand, menuCommandID);
				menuItem.BeforeQueryStatus += CheckCommandAccessibility;

				projectManager.CommandService.AddCommand(menuItem);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static DebugMethodCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		///  Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="projectManager"></param>
		/// <param name="authManager"></param>
		/// <param name="dialogFactory"></param>
		/// <param name="projectConfigurationManager"></param>
		/// <param name="codeProviderFactory"></param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new DebugMethodCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			EnvDTE.Configuration config = project.ConfigurationManager.ActiveConfiguration;
			EnvDTE.Properties props = config.Properties;

			var outputPath = props.Item("OutputPath");
			var dllFullPath = Path.Combine(project.Properties.Item("FullPath").Value.ToString(), outputPath.Value.ToString(), project.Properties.Item("OutputFileName").Value.ToString());
			var selectedMethodName = projectManager.MethodName;

			string projectConfigPath = projectManager.ProjectConfigPath;

			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);

			string selectedMethodPath = projectManager.MethodPath;
			string sourceCode = File.ReadAllText(selectedMethodPath, new UTF8Encoding(true));

			var tree = CSharpSyntaxTree.ParseText(sourceCode);
			SyntaxNode root = tree.GetRoot();
			var member = root.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.FirstOrDefault();
			var className = GetFullClassName(member);
			var methodName = GetMethodName(member);

			//(me as IdentifierNameSyntax).Identifier.ValueText

			//var selectedClassName =
			//ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);
			projectManager.ExecuteCommand("Build.BuildSolution");

			var currentDllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var directoryPath = Path.GetDirectoryName(currentDllPath);
			var launcherPath = Path.Combine(directoryPath, "MethodLauncher", "MethodLauncher.exe");

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodWorkingFolder = Path.Combine(projectManager.ServerMethodFolderPath, packageMethodFolderPath, methodInformation.MethodName);
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			string methodCode = codeProvider.LoadMethodCode(methodWorkingFolder, sourceCode);

			var debugMethodView = dialogFactory.GetDebugMethodView(projectConfigurationManager, methodInformation, methodCode, projectConfigPath, project.Name, project.FullName);
			var debugMethodViewResult = debugMethodView.ShowDialog();
			if (debugMethodViewResult?.DialogOperationResult != true)
			{
				return;
			}

			string projectDirectoryPath = Path.GetDirectoryName(projectManager.SelectedProject.FileName);
			string launcherConfigPath = Path.Combine(projectDirectoryPath, "LauncherConfig.xml");

			CreateLauncherConfigFile(dllFullPath, className, methodName, debugMethodViewResult, launcherConfigPath,
				CommonData.EventSpecificDataTypeList.FirstOrDefault(ev => ev.EventSpecificData.ToString() == methodInformation.EventData.ToString()).EventDataClass, methodInformation.TemplateName);

			ProcessStartInfo startInfo = new ProcessStartInfo(launcherPath);
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = launcherConfigPath + " " + authManager.InnovatorUser.passwordHash;

			Process process = Process.Start(startInfo);

			projectManager.AttachToProcess(process);
		}

		private void CreateLauncherConfigFile(string dllFullPath, string className, string methodName, DebugMethodViewResult debugMethodViewResult, string launcherConfigPath, string eventName, string templateName)
		{
			XmlDocument launcherConfig = new XmlDocument();

			XmlElement launcherConfigXmlElement = launcherConfig.CreateElement("LauncherConfig");
			launcherConfig.AppendChild(launcherConfigXmlElement);

			XmlElement dllFullPathXmlElement = launcherConfig.CreateElement("dllFullPath");
			dllFullPathXmlElement.InnerText = dllFullPath;
			launcherConfigXmlElement.AppendChild(dllFullPathXmlElement);

			XmlElement classNameXmlElement = launcherConfig.CreateElement("className");
			classNameXmlElement.InnerText = className;
			launcherConfigXmlElement.AppendChild(classNameXmlElement);

			XmlElement methodNameXmlElement = launcherConfig.CreateElement("methodName");
			methodNameXmlElement.InnerText = methodName;
			launcherConfigXmlElement.AppendChild(methodNameXmlElement);

			XmlElement сontextXmlElement = launcherConfig.CreateElement("сontext");
			сontextXmlElement.InnerText = debugMethodViewResult.MethodContext;
			launcherConfigXmlElement.AppendChild(сontextXmlElement);

			XmlElement urlXmlElement = launcherConfig.CreateElement("url");
			urlXmlElement.InnerText = authManager.GetServerUrl();
			launcherConfigXmlElement.AppendChild(urlXmlElement);

			XmlElement databaseXmlElement = launcherConfig.CreateElement("database");
			databaseXmlElement.InnerText = authManager.GetServerDatabaseName();
			launcherConfigXmlElement.AppendChild(databaseXmlElement);

			XmlElement userNameXmlElement = launcherConfig.CreateElement("userName");
			userNameXmlElement.InnerText = authManager.InnovatorUser.userName;
			launcherConfigXmlElement.AppendChild(userNameXmlElement);

			XmlElement eventClassXmlElement = launcherConfig.CreateElement("eventClass");
			eventClassXmlElement.InnerText = eventName;
			launcherConfigXmlElement.AppendChild(eventClassXmlElement);

			XmlElement templateNameXmlElement = launcherConfig.CreateElement("templateName");
			templateNameXmlElement.InnerText = templateName;
			launcherConfigXmlElement.AppendChild(templateNameXmlElement);

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(true);
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineOnAttributes = false;
			settings.OmitXmlDeclaration = true;
			using (XmlWriter xmlWriter = XmlWriter.Create(launcherConfigPath, settings))
			{
				launcherConfig.Save(xmlWriter);
			}
		}

		private string GetFullClassName(NamespaceDeclarationSyntax node)
		{
			var cls = node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
			if (cls == null)
			{
				throw new ArgumentException(this.messageManager.GetMessage("ClassNotFoundInNamespace"));
			}
			string clsName = cls.Identifier.ValueText;
			string namespaceName = string.Empty;
			if (node.Parent is NamespaceDeclarationSyntax)
				namespaceName = String.Format("{0}.{1}",
					GetFullClassName((NamespaceDeclarationSyntax)node.Parent),
					((IdentifierNameSyntax)node.Name).Identifier.ToString());
			else
				namespaceName = ((IdentifierNameSyntax)node.Name).Identifier.ToString();

			return string.Format("{0}.{1}", namespaceName, clsName);
		}

		private string GetMethodName(NamespaceDeclarationSyntax node)
		{
			var methodName = node.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText;
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException(this.messageManager.GetMessage("MethodNotFoundInClass"));
			}
			return methodName;
		}
	}
}
