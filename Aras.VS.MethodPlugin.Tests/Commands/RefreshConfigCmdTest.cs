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
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.PackageManagement;
using System.IO;
using System;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RefreshConfigCmdTest
    {
        IProjectManager projectManager;
        IDialogFactory dialogFactory;
        ProjectConfigurationManager projectConfigurationManager;
        RefreshConfigCmd refreshConfigCmd;
        IVsUIShell iVsUIShell;

        [SetUp]
        public void Init()
        {
            projectManager = Substitute.For<IProjectManager>();
            projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
            dialogFactory = Substitute.For<IDialogFactory>();
            RefreshConfigCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager);
            refreshConfigCmd = RefreshConfigCmd.Instance;
            iVsUIShell = Substitute.For<IVsUIShell>();
        }


        [Test]
        public void ExecuteCommandImpl_ShouldNotReturnAnyExceptions()
        {
            // Arrange

            //Act
            var testDelegate = new TestDelegate(() => refreshConfigCmd.ExecuteCommandImpl(null, null, iVsUIShell));
            
            //Assert
            Assert.DoesNotThrow(testDelegate);
        }
    }
}
