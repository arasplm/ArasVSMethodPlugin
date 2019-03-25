using System;
using System.Collections.Generic;
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
	public class CmdBaseTests
	{
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		IProjectConfigurationManager projectConfigurationManager;
		CmdBaseTest cmdBaseTest;

		public class CmdBaseTest : CmdBase
		{
			public CmdBaseTest(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager)
			: base(projectManager, dialogFactory, projectConfigurationManager)
			{

			}

			public override void ExecuteCommandImpl(object sender, EventArgs args)
			{
			}
		}

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			cmdBaseTest = new CmdBaseTest(projectManager, dialogFactory, projectConfigurationManager);
			var projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfigurationManager.Load(projectManager.ProjectConfigPath).Returns(projectConfiguraiton);
			projectConfiguraiton.Connections.Returns(Substitute.For<List<ConnectionInfo>>());
		}

		[Test]
		public void Ctor_CallCtorWhereProjectManagerIsNull_ShouldThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(null, dialogFactory, projectConfigurationManager);
			}));
		}

		[Test]
		public void Ctor_CallCtorWhereDProjectConfigurationManagerIsNull_ShouldProjectThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(projectManager, dialogFactory, null);
			}));
		}

		[Test]
		public void Ctor_CallCtorWhereDialogFactoryIsNull_ShouldDefaultThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(projectManager, null, projectConfigurationManager);
			}));
		}

		[Test]
		public void ExecuteCommand_IsSaveDirtyFileAndCallExecuteCommandImpl_ShouldReturnTrue()
		{
			//Arrange
			projectManager.SaveDirtyFiles(null).ReturnsForAnyArgs(true);

			//Act
			cmdBaseTest.ExecuteCommand(null, null);

			//Assert
			Assert.IsTrue(projectManager.SaveDirtyFiles(Arg.Any<List<MethodInfo>>()));
		}
	}
}
