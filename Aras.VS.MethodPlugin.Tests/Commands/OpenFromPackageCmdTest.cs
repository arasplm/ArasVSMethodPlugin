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
	public class OpenFromPackageCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		OpenFromPackageCmd openFromPackageCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		ICodeProvider codeProvider;
		TemplateLoader templateLoader;
		PackageManager packageManager;
		IProjectConfiguraiton projectConfiguration;

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
			OpenFromPackageCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			openFromPackageCmd = OpenFromPackageCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			templateLoader = new TemplateLoader(dialogFactory);
			projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
			templateLoader.Load(projectManager.MethodConfigPath);
		}


		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGetOpenFromPackageView()
		{
			// Arrange
			dialogFactory.GetOpenFromPackageView(null, null, null).ReturnsForAnyArgs(Substitute.For<OpenFromPackageViewAdapterTest>());
			codeProvider.GenerateCodeInfo(null, null, null, false, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());

			//Act
			openFromPackageCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetOpenFromPackageView(Arg.Any<TemplateLoader>(), codeProvider.Language, Arg.Any<ProjectConfiguraiton>());
		}

		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var openFromPackageViewAdapterTest = Substitute.For<OpenFromPackageViewAdapterTest>();
			dialogFactory.GetOpenFromPackageView(null, null, null).ReturnsForAnyArgs(openFromPackageViewAdapterTest);
			codeProvider.GenerateCodeInfo(null, null, null, false, null, false).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			var showDialogResult = openFromPackageViewAdapterTest.ShowDialog();

			//Act
			openFromPackageCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(Arg.Any<TemplateInfo>(), Arg.Any<EventSpecificDataType>(), showDialogResult.MethodName, false, showDialogResult.MethodCode, showDialogResult.IsUseVSFormattingCode);
		}

		public class OpenFromPackageViewAdapterTest : IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult>
		{
			public OpenFromPackageViewResult ShowDialog()
			{
				return new OpenFromPackageViewResult
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
					SelectedManifestFileName = string.Empty,
					IdentityId = string.Empty,
					IdentityKeyedName = string.Empty,
					SelectedFolderPath = string.Empty,
					SelectedManifestFullPath = string.Empty
				};
			}
		}
	}
}
