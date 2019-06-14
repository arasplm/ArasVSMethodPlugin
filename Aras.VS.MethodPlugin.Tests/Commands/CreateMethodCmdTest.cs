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
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Tests.Stubs;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class CreateMethodCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		CreateMethodCmd createMethodCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		MessageManager messageManager;
		IGlobalConfiguration globalConfiguration;
		static TemplateInfo template;
		static EventSpecificDataType eventSpecificDataType;
		TemplateLoader templateLoader;
		PackageManager packageManager;
		ICodeProvider codeProvider;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = new ProjectConfigurationManager();
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = new AuthManagerStub();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			globalConfiguration = Substitute.For<IGlobalConfiguration>();
			messageManager = Substitute.For<MessageManager>();
			CreateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, globalConfiguration, messageManager);
			createMethodCmd = CreateMethodCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			template = new TemplateInfo { TemplateName = string.Empty };
			eventSpecificDataType = new EventSpecificDataType { EventSpecificData = EventSpecificData.None };
			templateLoader = new TemplateLoader();
			packageManager = new PackageManager(authManager, messageManager);
			codeProvider = Substitute.For<ICodeProvider>();
			projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
			templateLoader.Load(projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath);
			codeProviderFactory.GetCodeProvider(null).ReturnsForAnyArgs(codeProvider);
		}


		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGetCreateView()
		{
			// Arrange

			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			dialogFactory.GetCreateView(projectConfigurationManager.CurrentProjectConfiguraiton, templateLoader, packageManager, projectManager, codeProvider, globalConfiguration).ReturnsForAnyArgs(Substitute.For<CreateMethodViewAdapterTest>());

			//Act
			createMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetCreateView(Arg.Any<ProjectConfiguraiton>(), Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(), projectManager, codeProvider, globalConfiguration);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var createMethodViewAdapterTest = Substitute.For<CreateMethodViewAdapterTest>();
			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			dialogFactory.GetCreateView(projectConfigurationManager.CurrentProjectConfiguraiton, templateLoader, packageManager, projectManager, codeProvider, globalConfiguration).ReturnsForAnyArgs(createMethodViewAdapterTest);
			var showDialogResult = createMethodViewAdapterTest.ShowDialog();

			//Act
			createMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(template, eventSpecificDataType, showDialogResult.MethodName, null, false);
		}


		public class CreateMethodViewAdapterTest : IViewAdaper<CreateMethodView, CreateMethodViewResult>
		{
			public CreateMethodViewResult ShowDialog()
			{
				return new CreateMethodViewResult
				{
					DialogOperationResult = true,
					SelectedLanguage = new FilteredListInfo(string.Empty, string.Empty, string.Empty),
					MethodName = string.Empty,
					SelectedActionLocation = new ListInfo(),
					MethodComment = string.Empty,
					SelectedPackage = new PackageInfo(string.Empty),
					SelectedTemplate = template,
					SelectedEventSpecificData = eventSpecificDataType,
					SelectedIdentityId = string.Empty,
					SelectedIdentityKeyedName = string.Empty,
					UseRecommendedDefaultCode = false,
					IsUseVSFormattingCode = false,
					SelectedUserCodeTemplate = new XmlMethodInfo()
				};
			}
		}
	}
}
