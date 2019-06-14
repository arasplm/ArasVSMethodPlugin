using System;
using System.Collections.Generic;
using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class AuthenticationCommandBaseTests
	{
		IAuthenticationManager authManager;
		IDialogFactory dialogFactory;
		IProjectManager projectManager;
		IProjectConfigurationManager projectConfigurationManager;
		ICodeProviderFactory codeProviderFactory;
		MessageManager messageManager;
		private AuthenticationCommandBaseTest authenticationCommandBaseTest;

		internal class AuthenticationCommandBaseTest : AuthenticationCommandBase
		{
			public AuthenticationCommandBaseTest(IAuthenticationManager authManager,
				IDialogFactory dialogFactory,
				IProjectManager projectManager,
				IProjectConfigurationManager projectConfigurationManager,
				ICodeProviderFactory codeProviderFactory,
				MessageManager messageManager)
					: base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
			{

			}

			public override void ExecuteCommandImpl(object sender, EventArgs args)
			{

			}
		}

		[SetUp]
		public void Init()
		{
			authManager = Substitute.For<IAuthenticationManager>();
			projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			projectManager = Substitute.For<IProjectManager>();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			messageManager = Substitute.For<MessageManager>();
			authenticationCommandBaseTest = new AuthenticationCommandBaseTest(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager);

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.Connections.Returns(Substitute.For<List<ConnectionInfo>>());
			projectConfiguraiton.MethodInfos.Returns(Substitute.For<List<MethodInfo>>());

			projectConfigurationManager.When(x => x.Load(projectManager.ProjectConfigPath))
				.Do(callback => 
				{
					projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);
				});
		}

		[Test]
		public void Ctor_CallCtorWithNullAuthManager_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new AuthenticationCommandBaseTest(null, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager);
			}));
		}

		[Test]
		public void Ctor_CallCtorWithNullCodeProvider_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new AuthenticationCommandBaseTest(authManager, dialogFactory, projectManager, projectConfigurationManager, null, messageManager);
			}));
		}


		[Test]
		public void ExecuteCommand_IsSaveDirtyFileAndCallExecuteCommandImpl_ShouldReturnTrue()
		{
			//Arrange 
			authManager.IsLoginedForCurrentProject(null, null).ReturnsForAnyArgs(true);
			projectManager.SaveDirtyFiles(dialogFactory, null).ReturnsForAnyArgs(true);

			//Act
			authenticationCommandBaseTest.ExecuteCommand(null, null);

			//Assert
			Assert.IsTrue(projectManager.SaveDirtyFiles(dialogFactory, projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos));
		}
	}
}
