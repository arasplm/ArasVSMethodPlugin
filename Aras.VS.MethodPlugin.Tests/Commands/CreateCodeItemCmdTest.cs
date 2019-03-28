using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	public class CreateCodeItemCmdTest
	{
		private CreateCodeItemCmd createCodeItemCmd;
		private IProjectManager projectManager;
		private IDialogFactory dialogFactory;
		private IProjectConfigurationManager projectConfigurationManager;
		private ICodeProviderFactory codeProviderFactory;
		private IMessageManager messageManager;

		[SetUp]
		public void Setup()
		{
			this.projectManager = Substitute.For<IProjectManager>();
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			this.messageManager = Substitute.For<IMessageManager>();

			CreateCodeItemCmd.Initialize(this.projectManager, this.dialogFactory, this.projectConfigurationManager, this.codeProviderFactory, this.messageManager);
			createCodeItemCmd = CreateCodeItemCmd.Instance;
		}

		[Test]
		public void ExecuteCommandImpl_ShouldThrowException()
		{
			//Arange
			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>());

			this.projectConfigurationManager.Load(Arg.Any<string>()).Returns(projectConfiguraiton);

			//Assert
			Assert.Throws<Exception>(() =>
			{
				//Act
				this.createCodeItemCmd.ExecuteCommandImpl(null, null);
			});
		}

		[Test]
		public void ExecuteCommandImpl_ShouldThrow_CodeItemAlreadyExistsException()
		{
			//Arange
			string partialfileName = "testPartialfileName";
			string methodName = "methodName";
			this.projectManager.MethodName.Returns(methodName);
			MethodInfo testMethodInfo = new MethodInfo()
			{
				MethodName = methodName,
				PartialClasses = new List<string>() { partialfileName }
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			this.projectConfigurationManager.Load(Arg.Any<string>()).Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(partialfileName, MethodPlugin.Code.CodeType.Partial, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			//Assert
			Assert.Throws<Exception>(() =>
			{
				//Act
				this.createCodeItemCmd.ExecuteCommandImpl(null, null);
			}, "Code item already exists.");
		}

		[Test]
		public void ExecuteCommandImpl_ShouldAddPartialClassesAndSaveConfigfile()
		{
			//Arange
			string partialfileName = "testPartialfileName";
			string methodName = "methodName";
			Project selectedProject = Substitute.For<Project>();
			selectedProject.CodeModel.Language.Returns("C#");

			this.projectManager.MethodName.Returns(methodName);
			this.projectManager.SelectedProject.Returns(selectedProject);
			this.projectManager.ProjectConfigPath.Returns("ProjectConfigPath");
			MethodInfo testMethodInfo = new MethodInfo()
			{
				MethodName = methodName,
				PartialClasses = new List<string>()
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			this.projectConfigurationManager.Load(Arg.Any<string>()).Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(partialfileName, MethodPlugin.Code.CodeType.Partial, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			ICodeProvider codeProvider = Substitute.For<ICodeProvider>();
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"path\testPartialfileName.cs"
			};

			codeProviderFactory.GetCodeProvider("C#", projectConfiguraiton).Returns(codeProvider);
			codeProvider.CreateCodeItemInfo(testMethodInfo, partialfileName, Arg.Any<MethodPlugin.Code.CodeType>(), Arg.Any<CodeElementType>(), Arg.Any<bool>()).Returns(codeItemInfo);

			//Act
			this.createCodeItemCmd.ExecuteCommandImpl(null, null);

			//Assert
			Assert.IsTrue(testMethodInfo.PartialClasses.Contains(codeItemInfo.Path));
			this.projectConfigurationManager.Received().Save("ProjectConfigPath", projectConfiguraiton);
		}

		[Test]
		public void ExecuteCommandImpl_ShouldAddExternalClassesAndSaveConfigfile()
		{
			//Arange
			string externalfileName = "testExternalFileName";
			string methodName = "methodName";
			Project selectedProject = Substitute.For<Project>();
			selectedProject.CodeModel.Language.Returns("C#");

			this.projectManager.MethodName.Returns(methodName);
			this.projectManager.SelectedProject.Returns(selectedProject);
			this.projectManager.ProjectConfigPath.Returns("ProjectConfigPath");
			MethodInfo testMethodInfo = new MethodInfo()
			{
				MethodName = methodName,
				PartialClasses = new List<string>()
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			this.projectConfigurationManager.Load(Arg.Any<string>()).Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(externalfileName, MethodPlugin.Code.CodeType.External, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			ICodeProvider codeProvider = Substitute.For<ICodeProvider>();
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"path\testExternalFileName.cs"
			};

			codeProviderFactory.GetCodeProvider("C#", projectConfiguraiton).Returns(codeProvider);
			codeProvider.CreateCodeItemInfo(testMethodInfo, externalfileName, Arg.Any<MethodPlugin.Code.CodeType>(), Arg.Any<CodeElementType>(), Arg.Any<bool>()).Returns(codeItemInfo);

			//Act
			this.createCodeItemCmd.ExecuteCommandImpl(null, null);

			//Assert
			Assert.IsTrue(testMethodInfo.ExternalItems.Contains(codeItemInfo.Path));
			this.projectConfigurationManager.Received().Save("ProjectConfigPath", projectConfiguraiton);
		}
	}

	class CreateCodeItemViewAdaper : IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult>
	{
		private string fileName;
		private bool dialogOperationResult;
		private MethodPlugin.Code.CodeType selectedCodeType;

		public CreateCodeItemViewAdaper(string fileName, MethodPlugin.Code.CodeType selectedCodeType, bool dialogOperationResult)
		{
			this.fileName = fileName;
			this.dialogOperationResult = dialogOperationResult;
			this.selectedCodeType = selectedCodeType;
		}

		public CreateCodeItemViewResult ShowDialog()
		{
			return new CreateCodeItemViewResult()
			{
				FileName = this.fileName,
				DialogOperationResult = this.dialogOperationResult,
				SelectedCodeType = this.selectedCodeType
			};
		}
	}
}
