using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Commands;
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
		MessageManager messageManager;
		ProjectConfigurationManager projectConfigurationManager;
		RefreshConfigCmd refreshConfigCmd;
		IVsUIShell iVsUIShell;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			messageManager = Substitute.For<MessageManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>(messageManager);
			dialogFactory = Substitute.For<IDialogFactory>();	
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
