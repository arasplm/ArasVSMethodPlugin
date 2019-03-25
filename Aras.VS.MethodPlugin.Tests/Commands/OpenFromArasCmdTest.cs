using System;
using System.IO;
using System.Threading;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class OpenFromArasCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		OpenFromArasCmd openFromArasCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		ICodeProvider codeProvider;
		IProjectConfiguraiton projectConfiguration;
		TemplateLoader templateLoader;
		PackageManager packageManager;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = Substitute.For<IAuthenticationManager>();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			OpenFromArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			openFromArasCmd = OpenFromArasCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null, null).ReturnsForAnyArgs(codeProvider);
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
			projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			templateLoader = new TemplateLoader(dialogFactory);
			templateLoader.Load(projectManager.MethodConfigPath);
			packageManager = new PackageManager(authManager);
		}

		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGetOpenFromArasView()
		{
			// Arrange
			var project = projectManager.SelectedProject;
			dialogFactory.GetOpenFromArasView(projectConfigurationManager, projectConfiguration, templateLoader, packageManager, projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language).
				ReturnsForAnyArgs(Substitute.For<OpenFromArasViewAdapterTest>());
			codeProvider.GenerateCodeInfo(null, null, null, false, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());

			//Act
			openFromArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetOpenFromArasView(projectConfigurationManager, Arg.Any<ProjectConfiguraiton>(), Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(), projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language);
		}

		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var project = projectManager.SelectedProject;
			var openFromPackageViewAdapterTest = Substitute.For<OpenFromArasViewAdapterTest>();
			dialogFactory.GetOpenFromArasView(projectConfigurationManager, projectConfiguration, templateLoader, packageManager, projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language).
				ReturnsForAnyArgs(openFromPackageViewAdapterTest);
			codeProvider.GenerateCodeInfo(null, null, null, false, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			var showDialogResult = openFromPackageViewAdapterTest.ShowDialog();

			//Act
			openFromArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(Arg.Any<TemplateInfo>(), Arg.Any<EventSpecificDataType>(), showDialogResult.MethodName, false, showDialogResult.MethodCode, false);
		}


		public class OpenFromArasViewAdapterTest : IViewAdaper<OpenFromArasView, OpenFromArasViewResult>
		{
			public OpenFromArasViewResult ShowDialog()
			{
				return new OpenFromArasViewResult
				{
					DialogOperationResult = true,
					IsUseVSFormattingCode = false,
					MethodCode = string.Empty,
					MethodComment = string.Empty,
					MethodConfigId = string.Empty,
					MethodId = string.Empty,
					MethodName = string.Empty,
					MethodLanguage = string.Empty,
					Package = string.Empty,
					SelectedEventSpecificData = new EventSpecificDataType { EventSpecificData = EventSpecificData.None },
					MethodType = string.Empty,
					SelectedTemplate = new TemplateInfo { TemplateName = string.Empty },
					SelectedIdentityId = string.Empty,
					SelectedIdentityKeyedName = string.Empty,

				};
			}
		}
	}
}
