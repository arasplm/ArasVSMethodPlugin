using System;
using System.Collections.Generic;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
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

		[Test]
		public void ExecuteCommand_LoadConfigurationThrows_ShouldShowStructuredDiagnosticMessage()
		{
			var messageWindow = Substitute.For<IMessageBoxWindow>();
			dialogFactory.GetMessageBoxWindow().Returns(messageWindow);
			projectConfigurationManager.When(manager => manager.Load(Arg.Any<string>()))
				.Do(callback => { throw new ApplicationException("Configuration load failed.", new InvalidOperationException("Invalid XML element.")); });

			cmdBaseTest.ExecuteCommand(null, null);

			messageWindow.Received().ShowDiagnosticDialog(
				Arg.Is<string>(message => message.Contains("Error code: AVS-CMD-001")
					&& message.Contains("Operation: Execute CmdBaseTest")
					&& message.Contains("Reason: Invalid XML element.")
					&& !message.Contains("Technical details:")),
				Arg.Is<string>(details => details.Contains(typeof(ApplicationException).FullName)
					&& details.Contains("Configuration load failed.")
					&& details.Contains("Invalid XML element.")
					&& details.Contains("Technical details:")),
				Arg.Any<string>());
		}
	}
}
