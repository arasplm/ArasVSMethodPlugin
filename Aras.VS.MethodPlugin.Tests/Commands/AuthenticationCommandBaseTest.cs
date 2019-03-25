using System;
using System.Collections.Generic;
using System.Threading;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell.Interop;
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
		private AuthenticationCommandBaseTest authenticationCommandBaseTest;

		public class AuthenticationCommandBaseTest : AuthenticationCommandBase
		{
			public AuthenticationCommandBaseTest(IAuthenticationManager authManager,
				IDialogFactory dialogFactory,
				IProjectManager projectManager,
				IProjectConfigurationManager projectConfigurationManager,
				ICodeProviderFactory codeProviderFactory)
			: base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory)
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
			authenticationCommandBaseTest = new AuthenticationCommandBaseTest(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory);
			var projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfigurationManager.Load(projectManager.ProjectConfigPath).Returns(projectConfiguraiton);
			projectConfiguraiton.Connections.Returns(Substitute.For<List<ConnectionInfo>>());
		}

		[Test]
		public void Ctor_CallCtorWithNullAuthManager_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new AuthenticationCommandBaseTest(null, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory);
			}));
		}

		[Test]
		public void Ctor_CallCtorWithNullCodeProvider_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new AuthenticationCommandBaseTest(authManager, dialogFactory, projectManager, projectConfigurationManager, null);
			}));
		}


		[Test]
		public void ExecuteCommand_IsSaveDirtyFileAndCallExecuteCommandImpl_ShouldReturnTrue()
		{
			//Arrange 
			authManager.IsLoginedForCurrentProject(null, null).ReturnsForAnyArgs(true);
			projectManager.SaveDirtyFiles(null).ReturnsForAnyArgs(true);

			//Act
			authenticationCommandBaseTest.ExecuteCommand(null, null);

			//Assert
			Assert.IsTrue(projectManager.SaveDirtyFiles(Arg.Any<List<MethodInfo>>()));
		}
	}
}
