//using System;
//using System.IO;
//using System.Threading;
//using Aras.VS.MethodPlugin.Code;
//using Aras.VS.MethodPlugin.Dialogs;
//using Aras.VS.MethodPlugin.SolutionManagement;
//using Microsoft.VisualStudio.Shell.Interop;
//using NSubstitute;
//using NUnit.Framework;

//namespace Aras.VS.MethodPlugin.Tests.Commands
//{
//	[TestFixture]
//	[Apartment(ApartmentState.STA)]
//	public class CreatePartialElementCmdTest
//	{
//		IProjectManager projectManager;
//		IDialogFactory dialogFactory;
//		ProjectConfigurationManager projectConfigurationManager;
//		CreatePartialElementCmd createPartialElementCmd;
//		IVsUIShell iVsUIShell;
//		ICodeProviderFactory codeProviderFactory;
//		ICodeProvider codeProvider;
//		IProjectConfiguraiton projectConfiguration;

//		[SetUp]
//		public void Init()
//		{
//			projectManager = Substitute.For<IProjectManager>();
//			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
//			dialogFactory = Substitute.For<IDialogFactory>();
//			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
//			CreatePartialElementCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
//			createPartialElementCmd = CreatePartialElementCmd.Instance;
//			iVsUIShell = Substitute.For<IVsUIShell>();
//			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
//			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
//			projectManager.MethodName.Returns("TestMethod");
//			projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);
//			projectConfiguration.UseVSFormatting = false;
//			codeProvider = Substitute.For<ICodeProvider>();
//			codeProviderFactory.GetCodeProvider(null, null).ReturnsForAnyArgs(codeProvider);
//		}


//		[Test]
//		public void ExecuteCommandImpl_ShouldReceivedGetCreatePartialClassView()
//		{
//			// Arrange
//			var fileName = "FileName1";
//			codeProvider.CreatePartialCodeInfo(null, null, false).ReturnsForAnyArgs(Substitute.For<CodeInfo>());
//			dialogFactory.GetCreatePartialClassView(iVsUIShell, projectConfiguration.UseVSFormatting).ReturnsForAnyArgs(Substitute.For<CreatePartialElementViewAdapterTest>(fileName));

//			//Act
//			createPartialElementCmd.ExecuteCommandImpl(null, null, iVsUIShell);

//			// Assert
//			dialogFactory.Received().GetCreatePartialClassView(iVsUIShell, projectConfiguration.UseVSFormatting);
//		}

//		[Test]
//		public void ExecuteCommandImpl_ShouldReceivedCreatePartialCodeInfo()
//		{
//			// Arrange
//			var fileName = "FileName2";
//			codeProvider.CreatePartialCodeInfo(null, null, false).ReturnsForAnyArgs(Substitute.For<CodeInfo>());
//			dialogFactory.GetCreatePartialClassView(iVsUIShell, projectConfiguration.UseVSFormatting).ReturnsForAnyArgs(Substitute.For<CreatePartialElementViewAdapterTest>(fileName));

//			//Act
//			createPartialElementCmd.ExecuteCommandImpl(null, null, iVsUIShell);

//			// Assert
//			codeProvider.Received().CreatePartialCodeInfo(Arg.Any<MethodInfo>(), fileName, false);
//		}


//		public class CreatePartialElementViewAdapterTest : IViewAdaper<CreatePartialElementView, CreatePartialElementViewResult>
//		{
//			private string fileName;

//			public CreatePartialElementViewAdapterTest(string fileName)
//			{
//				this.fileName = fileName;
//			}

//			public CreatePartialElementViewResult ShowDialog()
//			{
//				return new CreatePartialElementViewResult
//				{
//					DialogOperationResult = true,
//					IsUseVSFormattingCode = false,
//					FileName = fileName
//				};
//			}
//		}
//	}
//}
