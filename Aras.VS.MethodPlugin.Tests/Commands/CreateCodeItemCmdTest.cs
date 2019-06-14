using System;
using System.Collections.Generic;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
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
		private MessageManager messageManager;
		private TemplateLoader templateLoader;

		[SetUp]
		public void Setup()
		{
			this.projectManager = Substitute.For<IProjectManager>();
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			this.messageManager = Substitute.For<MessageManager>();
			this.templateLoader = new TemplateLoader();

			CreateCodeItemCmd.Initialize(this.projectManager, this.dialogFactory, this.projectConfigurationManager, this.codeProviderFactory, this.messageManager);
			createCodeItemCmd = CreateCodeItemCmd.Instance;
		}

		[Test]
		public void ExecuteCommandImpl_ShouldThrowException()
		{
			//Arange
			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>());

			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);

			//Assert
			Assert.Throws<Exception>(() =>
			{
				//Act
				this.createCodeItemCmd.ExecuteCommandImpl(null, null);
			});
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldThrow_CodeItemAlreadyExistsException()
		{
			//Arange
			string partialfileName = "testPartialfileName";
			string methodName = "methodName";

			this.projectManager.MethodName.Returns(methodName);
			this.projectManager.ServerMethodFolderPath.Returns(@"TEST:\Users\Vladimir\source\repos\Aras11SP15MethodProject12\Aras11SP15MethodProject12\ServerMethods\");
			this.projectManager.SelectedFolderPath.Returns(@"TEST:\Users\Vladimir\source\repos\Aras11SP15MethodProject12\Aras11SP15MethodProject12\ServerMethods\MSO_Standard\Import\Method\MSO_GetAllSettings\Partials");

			MethodInfo testMethodInfo = new MethodInfo()
			{
				MethodName = methodName,
				Package = new PackageInfo("MSO_Standard")
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(partialfileName, Aras.Method.Libs.Code.CodeType.Partial, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			//Assert
			Assert.Throws<Exception>(() =>
			{
				//Act
				this.createCodeItemCmd.ExecuteCommandImpl(null, null);
			}, "Code item already exists.");
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_Partial_ShouldSaveConfigfile()
		{
			//Arange
			string partialfileName = "testPartialfileName";
			string methodName = "methodName";
			Project selectedProject = Substitute.For<Project>();
			selectedProject.CodeModel.Language.Returns("C#");

			this.projectManager.MethodName.Returns(methodName);
			this.projectManager.ServerMethodFolderPath.Returns(@"C:\Users\Vladimir\source\repos\Aras11SP15MethodProject12\Aras11SP15MethodProject12\ServerMethods\");
			this.projectManager.SelectedFolderPath.Returns(@"C:\Users\Vladimir\source\repos\Aras11SP15MethodProject12\Aras11SP15MethodProject12\ServerMethods\MSO_Standard\Import\Method\MSO_GetAllSettings\Partials");
			this.projectManager.SelectedProject.Returns(selectedProject);
			this.projectManager.ProjectConfigPath.Returns("ProjectConfigPath");

			MethodInfo testMethodInfo = new MethodInfo()
			{
				MethodName = methodName,
				Package = new PackageInfo("MSO_Standard")
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(partialfileName, Aras.Method.Libs.Code.CodeType.Partial, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			ICodeProvider codeProvider = Substitute.For<ICodeProvider>();
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"path\testPartialfileName.cs"
			};

			codeProviderFactory.GetCodeProvider("C#").Returns(codeProvider);
			codeProvider.CreatePartialCodeItemInfo(testMethodInfo, partialfileName, Arg.Any<CodeElementType>(), Arg.Any<bool>(), projectManager.ServerMethodFolderPath,
				projectManager.SelectedFolderPath,
				projectManager.MethodName,
				this.templateLoader,
				projectManager.MethodPath)
				.Returns(codeItemInfo);

			//Act
			this.createCodeItemCmd.ExecuteCommandImpl(null, null);

			//Assert
			this.projectConfigurationManager.Received().Save("ProjectConfigPath");
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_External_ShouldSaveConfigfile()
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
				MethodName = methodName
			};

			IProjectConfiguraiton projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			projectConfiguraiton.MethodInfos.Returns(new List<MethodInfo>() { testMethodInfo });

			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguraiton);

			CreateCodeItemViewAdaper createCodeItemViewAdaper = new CreateCodeItemViewAdaper(externalfileName, Aras.Method.Libs.Code.CodeType.External, true);
			this.dialogFactory.GetCreateCodeItemView(Arg.Any<ICodeItemProvider>(), Arg.Any<bool>()).Returns(createCodeItemViewAdaper);

			ICodeProvider codeProvider = Substitute.For<ICodeProvider>();
			CodeInfo codeItemInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"path\testExternalFileName.cs"
			};

			codeProviderFactory.GetCodeProvider("C#").Returns(codeProvider);
			codeProvider.CreateExternalCodeItemInfo(testMethodInfo, externalfileName, Arg.Any<CodeElementType>(), Arg.Any<bool>(), this.projectManager.ServerMethodFolderPath,
				this.projectManager.SelectedFolderPath,
				this.projectManager.MethodName,
				this.templateLoader,
				this.projectManager.MethodPath)
				.Returns(codeItemInfo);

			//Act
			this.createCodeItemCmd.ExecuteCommandImpl(null, null);

			//Assert
			this.projectConfigurationManager.Received().Save("ProjectConfigPath");
		}
	}

	class CreateCodeItemViewAdaper : IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult>
	{
		private string fileName;
		private bool dialogOperationResult;
		private Aras.Method.Libs.Code.CodeType selectedCodeType;

		public CreateCodeItemViewAdaper(string fileName, Aras.Method.Libs.Code.CodeType selectedCodeType, bool dialogOperationResult)
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
