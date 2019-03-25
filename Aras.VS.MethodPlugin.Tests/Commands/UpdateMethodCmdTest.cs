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
	public class UpdateMethodCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		UpdateMethodCmd updateMethodCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		TemplateLoader templateLoader;
		IProjectConfiguraiton projectConfiguration;
		ICodeProvider codeProvider;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = Substitute.For<IAuthenticationManager>();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null, null).ReturnsForAnyArgs(codeProvider);
			UpdateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			updateMethodCmd = UpdateMethodCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			templateLoader = new TemplateLoader(dialogFactory);
			projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
			templateLoader.Load(projectManager.MethodConfigPath);
			projectManager.MethodPath.Returns(Path.Combine(currentPath, "TestData\\TestMethod.txt"));
		}

		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGetUpdateFromArasView()
		{
			// Arrange
			dialogFactory.GetUpdateFromArasView(null, null, null, null, null, null, null, null).ReturnsForAnyArgs(new UpdateFromArasViewAdapterTest());
			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, Arg.Any<bool>(), null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());

			//Act
			updateMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetUpdateFromArasView(projectConfigurationManager, Arg.Any<ProjectConfiguraiton>(), Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(),
				Arg.Any<MethodInfo>(), projectManager.ProjectConfigPath, string.Empty, string.Empty);
		}

		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var updateFromArasViewAdapterTest = Substitute.For<UpdateFromArasViewAdapterTest>();
			dialogFactory.GetUpdateFromArasView(null, null, null, null, null, null, null, null).ReturnsForAnyArgs(updateFromArasViewAdapterTest);
			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, Arg.Any<bool>(), null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			var showDialogResult = updateFromArasViewAdapterTest.ShowDialog();

			//Act
			updateMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(Arg.Any<TemplateInfo>(), Arg.Any<EventSpecificDataType>(), showDialogResult.MethodName, false, showDialogResult.MethodCode, false);
		}



		public class UpdateFromArasViewAdapterTest : IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult>
		{
			public UpdateFromArasViewResult ShowDialog()
			{
				return new UpdateFromArasViewResult
				{
					DialogOperationResult = true,
					MethodCode = string.Empty,
					MethodComment = string.Empty,
					MethodName = string.Empty,
					MethodConfigId = string.Empty,
					MethodId = string.Empty,
					PackageName = string.Empty,
					MethodType = string.Empty,
					MethodLanguage = string.Empty,
					SelectedTemplate = new TemplateInfo { TemplateName = string.Empty },
					ExecutionIdentityId = string.Empty,
					ExecutionIdentityKeyedName = string.Empty,
					EventSpecificData = EventSpecificData.None,
					IsUseVSFormattingCode = false
				};
			}
		}
	}
}
