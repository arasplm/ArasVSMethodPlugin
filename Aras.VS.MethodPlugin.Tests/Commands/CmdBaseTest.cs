using System;
using System.Collections.Generic;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
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
		MessageManager messageManager;
		CmdBaseTest cmdBaseTest;

		internal class CmdBaseTest : CmdBase
		{
			public CmdBaseTest(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, MessageManager messageManager)
				: base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
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
			messageManager = Substitute.For<MessageManager>();
			cmdBaseTest = new CmdBaseTest(projectManager, dialogFactory, projectConfigurationManager, messageManager);

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.Connections.Returns(Substitute.For<List<ConnectionInfo>>());
			projectConfiguraiton.MethodInfos.Returns(Substitute.For<List<MethodInfo>>());

			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);
		}

		[Test]
		public void Ctor_CallCtorWhereProjectManagerIsNull_ShouldThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(null, dialogFactory, projectConfigurationManager, messageManager);
			}));
		}

		[Test]
		public void Ctor_CallCtorWhereDProjectConfigurationManagerIsNull_ShouldProjectThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(projectManager, dialogFactory, null, messageManager);
			}));
		}

		[Test]
		public void Ctor_CallCtorWhereDialogFactoryIsNull_ShouldDefaultThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CmdBaseTest(projectManager, null, projectConfigurationManager, messageManager);
			}));
		}

		[Test]
		public void ExecuteCommand_IsSaveDirtyFileAndCallExecuteCommandImpl_ShouldReturnTrue()
		{
			//Arrange
			projectManager.SaveDirtyFiles(dialogFactory, null).ReturnsForAnyArgs(true);

			//Act
			cmdBaseTest.ExecuteCommand(null, null);

			//Assert
			Assert.IsTrue(projectManager.SaveDirtyFiles(dialogFactory, projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos));
		}
	}
}
