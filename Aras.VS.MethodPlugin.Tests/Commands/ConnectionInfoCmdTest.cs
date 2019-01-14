using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using NUnit.Framework;
using Aras.VS.MethodPlugin.Commands;
using NSubstitute;
using Aras.VS.MethodPlugin.Authentication;
using Microsoft.VisualStudio.Shell.Interop;
using Aras.VS.MethodPlugin.Dialogs.Views;
using System.Threading;

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

        [SetUp]
        public void Init()
        {
            projectManager = Substitute.For<IProjectManager>();
            projectConfigurationManager = new ProjectConfigurationManager();
            dialogFactory = Substitute.For<IDialogFactory>();
            authManager = Substitute.For<IAuthenticationManager>();
            ConnectionInfoCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager);
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
            сonnectionInfoCmd.ExecuteCommandImpl(null, null, iVsUIShell);

            // Assert
            dialogFactory.Received().GetConnectionInfoView(projectManager, projectConfigurationManager);
        }
    }
}
