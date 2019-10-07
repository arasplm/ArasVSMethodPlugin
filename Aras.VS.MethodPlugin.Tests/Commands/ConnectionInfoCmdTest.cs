using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class ConnectionInfoCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		ConnectionInfoCmd сonnectionInfoCmd;
		IVsUIShell iVsUIShell;
		MessageManager messageManager;

		[SetUp]
		public void Init()
		{
			messageManager = Substitute.For<MessageManager>();
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = new ProjectConfigurationManager(messageManager);
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = Substitute.For<IAuthenticationManager>();
			ConnectionInfoCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, messageManager);
			сonnectionInfoCmd = ConnectionInfoCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
		}


		[Test]
		public void ExecuteCommandImpl_ShouldReceivedGetConnectionInfoView()
		{
			// Arrange
			var view = new ConnectionInfoView();
			var adapter = Substitute.For<ConnectionInfoViewAdapter>(view);
			dialogFactory.GetConnectionInfoView(projectManager, projectConfigurationManager).Returns(adapter);

			//Act
			сonnectionInfoCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetConnectionInfoView(projectManager, projectConfigurationManager);
		}
	}
}
