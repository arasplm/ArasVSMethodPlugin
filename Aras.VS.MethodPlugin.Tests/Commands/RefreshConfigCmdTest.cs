using System.Threading;
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
	public class RefreshConfigCmdTest
	{
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		IMessageManager messageManager;
		ProjectConfigurationManager projectConfigurationManager;
		RefreshConfigCmd refreshConfigCmd;
		IVsUIShell iVsUIShell;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			messageManager = Substitute.For<IMessageManager>();
			RefreshConfigCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, messageManager);
			refreshConfigCmd = RefreshConfigCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
		}


		[Test]
		public void ExecuteCommandImpl_ShouldNotReturnAnyExceptions()
		{
			// Arrange

			//Act
			var testDelegate = new TestDelegate(() => refreshConfigCmd.ExecuteCommandImpl(null, null));

			//Assert
			Assert.DoesNotThrow(testDelegate);
		}
	}
}
