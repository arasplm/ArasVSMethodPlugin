using System;
using System.IO;
using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Tests.Stubs;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class DebugMethodCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		MessageManager messageManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		DebugMethodCmd debugMethodCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		ICodeProvider codeProvider;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			messageManager = Substitute.For<MessageManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>(messageManager);
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = new AuthManagerStub();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null).ReturnsForAnyArgs(codeProvider);
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			projectManager.MethodName.Returns("TestMethod");
			projectManager.MethodPath.Returns(Path.Combine(currentPath, "TestData\\TestMethod.txt"));
			DebugMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			debugMethodCmd = DebugMethodCmd.Instance;

			var project = Substitute.For<Project>();
			var properties = Substitute.For<EnvDTE.Properties>();
			var property = Substitute.For<Property>();
			var propertiesForActiveConfigurations = Substitute.For<EnvDTE.Properties>();
			var propertyForActiveConfiguration = Substitute.For<Property>();
			var configurationManager = Substitute.For<ConfigurationManager>();
			var activeConfigurator = Substitute.For<Configuration>();

			projectManager.SelectedProject.Returns(project);
			project.FileName.Returns(currentPath);
			project.Properties.Returns(properties);
			properties.Item(Arg.Any<string>()).Returns(property);
			property.Value = "";

			project.ConfigurationManager.Returns(configurationManager);
			configurationManager.ActiveConfiguration.Returns(activeConfigurator);
			activeConfigurator.Properties.Returns(propertiesForActiveConfigurations);
			propertiesForActiveConfigurations.Item(Arg.Any<string>()).Returns(propertyForActiveConfiguration);
			propertyForActiveConfiguration.Value = "";
			projectManager.When(x => x.AttachToProcess(Arg.Any<System.Diagnostics.Process>())).Do(x => { });
			var codeModel = Substitute.For<CodeModel>();
			project.CodeModel.Returns(codeModel);
			codeModel.Language.Returns("");
		}


		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGetDebugMethodView()
		{
			// Arrange
			dialogFactory.GetDebugMethodView(null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<DebugMethodViewAdapterTest>());

			//Act
			debugMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetDebugMethodView(projectConfigurationManager, Arg.Any<MethodInfo>(), Arg.Any<string>(), projectManager.ProjectConfigPath, string.Empty, string.Empty);

		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedLoadMethodCode()
		{
			// Arrange
			dialogFactory.GetDebugMethodView(null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<DebugMethodViewAdapterTest>());

			//Act
			debugMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().LoadMethodCode(Arg.Any<string>(), string.Empty);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedAttachToProcess()
		{
			// Arrange
			dialogFactory.GetDebugMethodView(null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<DebugMethodViewAdapterTest>());

			//Act
			debugMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			projectManager.Received().AttachToProcess(Arg.Any<System.Diagnostics.Process>());
		}


		public class DebugMethodViewAdapterTest : IViewAdaper<DebugMethodView, DebugMethodViewResult>
		{
			public DebugMethodViewResult ShowDialog()
			{
				return new DebugMethodViewResult
				{
					DialogOperationResult = true,
					MethodContext = ""
				};
			}
		}
	}
}
