using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class DebugMethodCmd : CmdBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0104;

		public const int ToolbarCommandId = 0x111;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("020DC4DF-2FC3-493E-97D3-4012DE93BCAB");

		/// <summary>
		/// Toolbar menu group (command set GUID).
		/// </summary>
		public static readonly Guid ToolbarCommandSet = new Guid("21D122E1-35BF-4156-B458-7E292CDD9C2D");

		private DebugMethodCmd(IProjectManager projectManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager) : base(projectManager, dialogFactory, projectConfigurationManager)
		{
			if (projectManager.CommandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new MenuCommand(this.ExecuteCommand, menuCommandID);
				var toolbarMenuCommandID = new CommandID(ToolbarCommandSet, ToolbarCommandId);
				var toolbarMenuItem = new MenuCommand(this.ExecuteCommand, toolbarMenuCommandID);

				projectManager.CommandService.AddCommand(menuItem);
				projectManager.CommandService.AddCommand(toolbarMenuItem);
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
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager)
		{
			Instance = new DebugMethodCmd(projectManager, dialogFactory, projectConfigurationManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell)
		{
			var project = projectManager.SelectedProject;

			//var projectConfigPath = projectManager.ProjectConfigPath;
			//var methodConfigPath = projectManager.MethodConfigPath;
			EnvDTE.Configuration config = project.ConfigurationManager.ActiveConfiguration;
			EnvDTE.Properties props = config.Properties;
			
			var outputPath = props.Item("OutputPath");
			var dllFullPath = Path.Combine(project.Properties.Item("FullPath").Value.ToString(), outputPath.Value.ToString(), project.Properties.Item("OutputFileName").Value.ToString());
			var selectedMethodName = projectManager.MethodPath;

			var projectConfigPath = projectManager.ProjectConfigPath;
			ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			var pkgName = methodInformation.PackageName;
			
			string selectedMethodPath = projectManager.MethodPath;
			string sourceCode = File.ReadAllText(selectedMethodPath);

			var tree = CSharpSyntaxTree.ParseText(sourceCode);
			SyntaxNode root = tree.GetRoot();
			var member = root.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.FirstOrDefault();
			var className = GetFullName(member);
			var methodName = string.Format("Aras_PKG_{0}ItemMethod", methodInformation.MethodName);
			//(me as IdentifierNameSyntax).Identifier.ValueText

			//var selectedClassName =
			//ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);
			projectManager.ExecuteCommand("Build.BuildSolution");

			var currentDllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var directoryPath = Path.GetDirectoryName(currentDllPath);
			var launcherPath = Path.Combine(directoryPath, "MethodLauncher", "MethodLauncher.exe");
			ProcessStartInfo startInfo = new ProcessStartInfo(launcherPath);
			startInfo.WindowStyle = ProcessWindowStyle.Minimized;
			startInfo.Arguments = dllFullPath + " " + className + " " + methodName;

			Process.Start(startInfo);

		}

		public static string GetFullName(NamespaceDeclarationSyntax node)
		{
			var cls = node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
			string clsName = cls.Identifier.ValueText;
			string namespaceName = string.Empty;
			if (node.Parent is NamespaceDeclarationSyntax)
				namespaceName = String.Format("{0}.{1}",
					GetFullName((NamespaceDeclarationSyntax)node.Parent),
					((IdentifierNameSyntax)node.Name).Identifier.ToString());
			else
				namespaceName =((IdentifierNameSyntax)node.Name).Identifier.ToString();

			return string.Format("{0}.{1}", namespaceName, clsName);
		}
	}
}
