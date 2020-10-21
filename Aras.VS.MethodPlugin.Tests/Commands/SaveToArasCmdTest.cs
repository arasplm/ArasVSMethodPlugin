﻿using System;
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
using Aras.VS.MethodPlugin.Tests.Stubs;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class SaveToArasCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		SaveToArasCmd saveToArasCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		MessageManager messageManager;
		ICodeProvider codeProvider;
		TemplateLoader templateLoader;
		PackageManager packageManager;

		[SetUp]
		public void Init(IVsPackageWrapper vsPackageWrapper)
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = new AuthManagerStub();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null).ReturnsForAnyArgs(codeProvider);
			messageManager = Substitute.For<MessageManager>();
			SaveToArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager, vsPackageWrapper);
			saveToArasCmd = SaveToArasCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
			templateLoader = new TemplateLoader();
			templateLoader.Load(projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath);
			projectManager.MethodPath.Returns(Path.Combine(currentPath, "TestData\\TestMethod.txt"));
			packageManager = Substitute.For<PackageManager>(authManager, messageManager);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedLoadMethodCode()
		{
			// Arrange
			dialogFactory.GetSaveToArasView(null, null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<SaveToArasViewAdapterTest>());
			var messageBox = Substitute.For<IMessageBoxWindow>();
			dialogFactory.GetMessageBoxWindow().ReturnsForAnyArgs(messageBox);
			messageBox.ShowDialog(null, null, Arg.Any<MessageButtons>(), Arg.Any<MessageIcon>()).ReturnsForAnyArgs(MessageDialogResult.OK);

			//Act
			saveToArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().LoadMethodCode(Arg.Any<string>(), string.Empty);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGetSaveToArasView()
		{
			// Arrange
			dialogFactory.GetSaveToArasView(null, null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<SaveToArasViewAdapterTest>());
			var messageBox = Substitute.For<IMessageBoxWindow>();
			dialogFactory.GetMessageBoxWindow().ReturnsForAnyArgs(messageBox);
			messageBox.ShowDialog(null, null, Arg.Any<MessageButtons>(), Arg.Any<MessageIcon>()).ReturnsForAnyArgs(MessageDialogResult.OK);

			//Act
			saveToArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetSaveToArasView(projectConfigurationManager, Arg.Any<PackageManager>(), Arg.Any<MethodInfo>(), string.Empty, projectManager.ProjectConfigPath, string.Empty, string.Empty);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedAddPackageElementToPackageDefinition()
		{
			// Arrange
			dialogFactory.GetSaveToArasView(null, null, null, null, null, null, null).ReturnsForAnyArgs(Substitute.For<SaveToArasViewAdapterTest>());

			var messageBox = Substitute.For<IMessageBoxWindow>();
			dialogFactory.GetMessageBoxWindow().ReturnsForAnyArgs(messageBox);
			messageBox.ShowDialog(null, null, Arg.Any<MessageButtons>(), Arg.Any<MessageIcon>()).ReturnsForAnyArgs(MessageDialogResult.OK);

			//Act
			saveToArasCmd.ExecuteCommandImpl(null, null);

			// Assert
			packageManager.Received().AddPackageElementToPackageDefinition(string.Empty, string.Empty, string.Empty);
		}

		public class SaveToArasViewAdapterTest : IViewAdaper<SaveMethodView, SaveMethodViewResult>
		{
			public SaveMethodViewResult ShowDialog()
			{
				return new SaveMethodViewResult
				{
					DialogOperationResult = true,
					MethodItem = new MethodItemStub(),
					CurrentMethodPackage = string.Empty,
					MethodCode = string.Empty,
					MethodComment = string.Empty,
					MethodLanguage = string.Empty,
					SelectedIdentityId = string.Empty,
					SelectedIdentityKeyedName = string.Empty,
					SelectedPackageInfo = new PackageInfo(string.Empty),
					TemplateName = string.Empty,
					MethodName = string.Empty
				};
			}
		}
	}
}
