using System;
using System.IO;
using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
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
		MessageManager messageManager;
		ICodeProvider codeProvider;
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
			messageManager = Substitute.For<MessageManager>();
			OpenFromArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			openFromArasCmd = OpenFromArasCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null).ReturnsForAnyArgs(codeProvider);
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			 projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
			templateLoader = new TemplateLoader();
			templateLoader.Load(projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath);
			packageManager = new PackageManager(authManager, messageManager);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGetOpenFromArasView()
		{
			// Arrange
			var project = projectManager.SelectedProject;
			dialogFactory.GetOpenFromArasView(projectConfigurationManager, templateLoader, packageManager, projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language).
				ReturnsForAnyArgs(Substitute.For<OpenFromArasViewAdapterTest>());
			codeProvider.GenerateCodeInfo(null, null, null, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());

			//Act
			openFromArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetOpenFromArasView(projectConfigurationManager, Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(), projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var project = projectManager.SelectedProject;
			var openFromPackageViewAdapterTest = Substitute.For<OpenFromArasViewAdapterTest>();
			dialogFactory.GetOpenFromArasView(projectConfigurationManager, templateLoader, packageManager, projectManager.ProjectConfigPath, project.Name, project.FullName, codeProvider.Language).
				ReturnsForAnyArgs(openFromPackageViewAdapterTest);
			codeProvider.GenerateCodeInfo(null, null, null, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			var showDialogResult = openFromPackageViewAdapterTest.ShowDialog();

			//Act
			openFromArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(Arg.Any<TemplateInfo>(), Arg.Any<EventSpecificDataType>(), showDialogResult.MethodName, showDialogResult.MethodCode, false);
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
					Package = new PackageInfo(string.Empty),
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
