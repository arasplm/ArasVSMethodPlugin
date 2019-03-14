using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.SolutionManagement
{
	[TestFixture]
	public class ProjectManagerTest
	{
		private ProjectManager projectManager;
		private IServiceProvider serviceProvider;
		private IDialogFactory dialogFactory;
		private IIOWrapper iOWrapper;
		private IVsPackageWrapper vsPackageWrapper;

		private ProjectItem mainMethodFileProjectItem;
		private ProjectItem methodWrapperFileProjectItem;
		private ProjectItem methodTestsFileProjectItem;
		private ProjectItem specialMethodFolder;

		[OneTimeSetUp]
		public void Setup()
		{
			this.serviceProvider = Substitute.For<IServiceProvider>();
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.iOWrapper = Substitute.For<IIOWrapper>();
			this.vsPackageWrapper = Substitute.For<IVsPackageWrapper>();
			this.projectManager = new ProjectManager(serviceProvider, dialogFactory, iOWrapper, vsPackageWrapper);
		}

		[Test]
		[TestCase("B69B1AC9-3D7E-4553-9786-A852B873DF01")]
		[TestCase("AEA8535B-C666-4112-9BDD-5ECFA4934B47")]
		[TestCase("DB77AE9E-9CB5-4C13-9EB3-ED388DC94B66")]
		[TestCase("E15DDF0A-1B6E-46A8-8B78-AEC2A7BB4922")]
		public void IsCommandForMethod_ShouldReturnTrue(string commandId)
		{
			//Arange

			//Act
			bool result = this.projectManager.IsCommandForMethod(Guid.Parse(commandId));

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsMethodExist_ShouldReturnTrue()
		{
			//Arange
			string methodName = "TestMethodName";
			InitTestprojectStructure();

			//Act
			bool result = this.projectManager.IsMethodExist(methodName);

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsMethodExist_ShouldReturnFalse()
		{
			//Arange
			string methodName = "methodName";
			InitTestprojectStructure();

			//Act
			bool result = this.projectManager.IsMethodExist(methodName);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsFileExist_shouldReturnFalse()
		{
			//Arange
			string filePath = @"DefunctFolder\DefunctFileName.cs";
			InitTestprojectStructure();

			//Act
			bool result = this.projectManager.IsFileExist(filePath);

			////Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsFileExist_shouldReturnTrue()
		{
			//Arange
			string filePath = @"TestMethodName\TestMethodName.cs";
			InitTestprojectStructure();

			//Act
			bool result = this.projectManager.IsFileExist(filePath);

			////Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void SaveDirtyFiles_ShouldReceiveFileSaveAndReturnTrue()
		{
			//Arange
			InitTestprojectStructure();
			this.mainMethodFileProjectItem.IsDirty.Returns(true);

			IVsUIShell vsUIShell = Substitute.For<IVsUIShell>();
			this.serviceProvider.GetService(typeof(SVsUIShell)).Returns(vsUIShell);

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow(vsUIShell).Returns(messageBoxWindow);
			messageBoxWindow.ShowDialog("One or more method files is not saved. Do you want to save changes?",
										"Aras VS method plugin",
										MessageButtons.YesNoCancel,
										MessageIcon.Question).Returns(MessageDialogResult.Yes);

			MethodInfo methodInfo = new MethodInfo()
			{
				MethodName = "TestMethodName",
				PartialClasses = new List<string>(),
				ExternalItems = new List<string>()
			};

			List<MethodInfo> methodInfos = new List<MethodInfo>();
			methodInfos.Add(methodInfo);

			//Act
			bool result = this.projectManager.SaveDirtyFiles(methodInfos);

			//Assert
			this.mainMethodFileProjectItem.Received().Save();
			Assert.IsTrue(result);
		}

		[Test]
		public void SaveDirtyFiles_ShouldReturnFalse()
		{
			//Arange
			InitTestprojectStructure();
			this.mainMethodFileProjectItem.IsDirty.Returns(true);

			IVsUIShell vsUIShell = Substitute.For<IVsUIShell>();
			this.serviceProvider.GetService(typeof(SVsUIShell)).Returns(vsUIShell);

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow(vsUIShell).Returns(messageBoxWindow);
			messageBoxWindow.ShowDialog("One or more method files is not saved. Do you want to save changes?",
										"Aras VS method plugin",
										MessageButtons.YesNoCancel,
										MessageIcon.Question).Returns(MessageDialogResult.Cancel);

			MethodInfo methodInfo = new MethodInfo()
			{
				MethodName = "TestMethodName",
				PartialClasses = new List<string>(),
				ExternalItems = new List<string>()
			};

			List<MethodInfo> methodInfos = new List<MethodInfo>();
			methodInfos.Add(methodInfo);

			//Act
			bool result = this.projectManager.SaveDirtyFiles(methodInfos);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void SaveDirtyFiles_ShouldReturnTrue()
		{
			//Arange
			InitTestprojectStructure();
			this.mainMethodFileProjectItem.IsDirty.Returns(true);

			IVsUIShell vsUIShell = Substitute.For<IVsUIShell>();
			this.serviceProvider.GetService(typeof(SVsUIShell)).Returns(vsUIShell);

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow(vsUIShell).Returns(messageBoxWindow);
			messageBoxWindow.ShowDialog("One or more method files is not saved. Do you want to save changes?",
										"Aras VS method plugin",
										MessageButtons.YesNoCancel,
										MessageIcon.Question).Returns(MessageDialogResult.No);

			MethodInfo methodInfo = new MethodInfo()
			{
				MethodName = "TestMethodName",
				PartialClasses = new List<string>(),
				ExternalItems = new List<string>()
			};

			List<MethodInfo> methodInfos = new List<MethodInfo>();
			methodInfos.Add(methodInfo);

			//Act
			bool result = this.projectManager.SaveDirtyFiles(methodInfos);

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void ExecuteCommand_ShouldReceiveExpected()
		{
			//Arange
			string commandName = "commandName";
			DTE dte = Substitute.For<DTE>();
			this.vsPackageWrapper.GetGlobalService(Arg.Any<Type>()).Returns(dte);

			//Act
			this.projectManager.ExecuteCommand(commandName);

			//Assert
			dte.Received().ExecuteCommand(commandName);
		}

		[Test]
		public void AddSuppressionWithDefaultParameters_ShouldReceivExpected()
		{
			//Arange
			string suppressName = "suppressName";
			string ruleCategory = "ruleCategory";
			string ruleId = "ruleId";
			string filePath = "filePath";

			string expectedString = "[suppressName(\"ruleCategory\", \"ruleId\")]\r\n";

			ProjectItem projectItem = Substitute.For<ProjectItem>();
			projectItem.FileNames[0].Returns(filePath);

			ProjectItems projectItems = Substitute.For<ProjectItems>();
			projectItems.Item(Arg.Any<object>()).Returns(projectItem);

			Project project = Substitute.For<Project>();
			project.ProjectItems.Returns(projectItems);

			Project[] projects = new Project[] { project };
			DTE2 dte = Substitute.For<DTE2>();
			dte.ActiveSolutionProjects.Returns(projects);
			serviceProvider.GetService(Arg.Any<Type>()).Returns(dte);

			this.iOWrapper.ReadAllText(Arg.Any<string>(), Arg.Any<Encoding>()).Returns("");

			//Act
			this.projectManager.AddSuppression(suppressName, ruleCategory, ruleId);

			//Assert
			this.iOWrapper.Received().WriteAllTextIntoFile(filePath, expectedString, Arg.Any<Encoding>());
		}

		[Test]
		public void AddSuppressionReceived_ShouldReceivExpected()
		{
			//Arange
			string suppressName = "suppressName";
			string ruleCategory = "ruleCategory";
			string ruleId = "ruleId";
			string scope = "scope";
			string target = "target";
			string filePath = "filePath";

			string expectedString = "[suppressName(\"ruleCategory\", \"ruleId\", Scope = \"scope\", Target = \"target\")]\r\n";

			ProjectItem projectItem = Substitute.For<ProjectItem>();
			projectItem.FileNames[0].Returns(filePath);

			ProjectItems projectItems = Substitute.For<ProjectItems>();
			projectItems.Item(Arg.Any<object>()).Returns(projectItem);

			Project project = Substitute.For<Project>();
			project.ProjectItems.Returns(projectItems);

			Project[] projects = new Project[] { project };
			DTE2 dte = Substitute.For<DTE2>();
			dte.ActiveSolutionProjects.Returns(projects);
			serviceProvider.GetService(Arg.Any<Type>()).Returns(dte);

			this.iOWrapper.ReadAllText(Arg.Any<string>(), Arg.Any<Encoding>()).Returns("");

			//Act
			this.projectManager.AddSuppression(suppressName, ruleCategory, ruleId, scope, target);

			//Assert
			this.iOWrapper.Received().WriteAllTextIntoFile(filePath, expectedString, Arg.Any<Encoding>());
		}

		[Test]
		public void AddItemTemplateToProjectNew_ShouldReturnExpcted()
		{
			//Arange
			CodeInfo codeInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"TestMethodName\TestFileName"
			};

			string expectedFilePath = @"ServerMethods\TestMethodName\TestFileName.cs";

			InitTestprojectStructure();
			EnvDTE.Property specialMethodFolderFullPathProperty = Substitute.For<EnvDTE.Property>();
			specialMethodFolderFullPathProperty.Value = @"ServerMethods\TestMethodName";

			EnvDTE.Properties specialMethodFolderProperties = Substitute.For<EnvDTE.Properties>();
			specialMethodFolderProperties.Item(Arg.Any<string>()).Returns(specialMethodFolderFullPathProperty);
			this.specialMethodFolder.Properties.Returns(specialMethodFolderProperties);

			this.specialMethodFolder.ProjectItems.AddFromFile(expectedFilePath).Returns(Substitute.For<ProjectItem>());

			//Act
			string resultFilePath = this.projectManager.AddItemTemplateToProjectNew(codeInfo, false);

			//Assert
			Assert.AreEqual(expectedFilePath, resultFilePath);
		}

		[Test]
		public void AddItemTemplateToProjectNew_ShouldReturnExpctedAndOpenCreatedFile()
		{
			//Arange
			CodeInfo codeInfo = new CodeInfo()
			{
				Code = "code",
				Path = @"TestMethodName\TestFileName"
			};

			string expectedFilePath = @"ServerMethods\TestMethodName\TestFileName.cs";

			InitTestprojectStructure();
			Property specialMethodFolderFullPathProperty = Substitute.For<Property>();
			specialMethodFolderFullPathProperty.Value.Returns(@"ServerMethods\TestMethodName");

			EnvDTE.Properties specialMethodFolderProperties = Substitute.For<EnvDTE.Properties>();
			specialMethodFolderProperties.Item(Arg.Any<string>()).Returns(specialMethodFolderFullPathProperty);
			this.specialMethodFolder.Properties.Returns(specialMethodFolderProperties);

			Window createdNewCodeFileWindow = Substitute.For<Window>();
			ProjectItem createdNewCodeFile = Substitute.For<ProjectItem>();
			createdNewCodeFile.Name.Returns("TestFileName");
			createdNewCodeFile.Open(EnvDTE.Constants.vsViewKindCode).Returns(createdNewCodeFileWindow);

			this.specialMethodFolder.ProjectItems.AddFromFile(expectedFilePath).Returns(createdNewCodeFile);

			DTE dte = Substitute.For<DTE>();
			this.vsPackageWrapper.GetGlobalService(typeof(DTE)).Returns(dte);

			//Act
			string resultFilePath = this.projectManager.AddItemTemplateToProjectNew(codeInfo, true, 20);

			//Assert
			Assert.AreEqual(expectedFilePath, resultFilePath);
			dte.Received().ExecuteCommand("Edit.Goto", "20");
		}

		[Test]
		public void RemoveMethod_ShouldReceiveExpected()
		{
			//Arange
			MethodInfo methodInfo = new MethodInfo() { MethodName = "TestMethodName" };

			InitTestprojectStructure();
			string basePath = @"ServerMethods\TestMethodName";
			string mainMethodFilePath = Path.Combine(basePath, mainMethodFileProjectItem.Name);
			string methodWrapperFilePath = Path.Combine(basePath, methodWrapperFileProjectItem.Name);
			string methodTestsFilePath = Path.Combine(basePath, methodTestsFileProjectItem.Name);

			Property mainMethodFileFullPathProperty = Substitute.For<Property>();
			mainMethodFileFullPathProperty.Value.Returns(mainMethodFilePath);
			EnvDTE.Properties mainMethodFileProperties = Substitute.For<EnvDTE.Properties>();
			mainMethodFileProperties.Item(Arg.Any<string>()).Returns(mainMethodFileFullPathProperty);

			Property methodWrapperFileFullPathProperty = Substitute.For<Property>();
			methodWrapperFileFullPathProperty.Value.Returns(methodWrapperFilePath);
			EnvDTE.Properties methodWrapperFileProperties = Substitute.For<EnvDTE.Properties>();
			methodWrapperFileProperties.Item(Arg.Any<string>()).Returns(methodWrapperFileFullPathProperty);

			Property methodTestsFileFullPathProperty = Substitute.For<Property>();
			methodTestsFileFullPathProperty.Value.Returns(methodTestsFilePath);
			EnvDTE.Properties methodTestsFileProperties = Substitute.For<EnvDTE.Properties>();
			methodTestsFileProperties.Item(Arg.Any<string>()).Returns(methodTestsFileFullPathProperty);

			this.mainMethodFileProjectItem.Properties.Returns(mainMethodFileProperties);
			this.methodWrapperFileProjectItem.Properties.Returns(methodWrapperFileProperties);
			this.methodTestsFileProjectItem.Properties.Returns(methodTestsFileProperties);

			//Act
			this.projectManager.RemoveMethod(methodInfo);

			//Assert
			mainMethodFileProjectItem.Received().Remove();
			methodWrapperFileProjectItem.Received().Remove();
			methodTestsFileProjectItem.Received().Remove();

			this.iOWrapper.Received().FileDelete(mainMethodFilePath);
			this.iOWrapper.Received().FileDelete(methodWrapperFilePath);
			this.iOWrapper.Received().FileDelete(methodTestsFilePath);
		}
		private void InitTestprojectStructure()
		{
			//mock exist file *.cs with name equal to name method
			mainMethodFileProjectItem = Substitute.For<ProjectItem>();
			mainMethodFileProjectItem.Name.Returns("TestMethodName.cs");

			methodWrapperFileProjectItem = Substitute.For<ProjectItem>();
			methodWrapperFileProjectItem.Name.Returns("TestMethodNameWrapper.cs");

			methodTestsFileProjectItem = Substitute.For<ProjectItem>();
			methodTestsFileProjectItem.Name.Returns("TestMethodNameTests.cs");

			List<ProjectItem> methodFiles = new List<ProjectItem>() { mainMethodFileProjectItem, methodWrapperFileProjectItem, methodTestsFileProjectItem };

			ProjectItems specialMethodFolderProjectItems = Substitute.For<ProjectItems>();
			specialMethodFolderProjectItems.GetEnumerator().Returns(methodFiles.GetEnumerator());
			
			specialMethodFolderProjectItems.Item("TestMethodName.cs").Returns(mainMethodFileProjectItem);
			specialMethodFolderProjectItems.Item("TestMethodNameWrapper.cs").Returns(methodWrapperFileProjectItem);
			specialMethodFolderProjectItems.Item("TestMethodNameTests.cs").Returns(methodTestsFileProjectItem);

			//mock folder with method
			specialMethodFolder = Substitute.For<ProjectItem>();
			specialMethodFolder.Name.Returns("TestMethodName");
			specialMethodFolder.ProjectItems.Returns(specialMethodFolderProjectItems);

			List<ProjectItem> methodFolders = new List<ProjectItem>() { specialMethodFolder };

			ProjectItems serverMethodsFolderItems = Substitute.For<ProjectItems>();
			serverMethodsFolderItems.GetEnumerator().Returns(methodFolders.GetEnumerator());
			serverMethodsFolderItems.Item("TestMethodName").Returns(specialMethodFolder);

			//mock main methods folder "ServerMethods"
			ProjectItem serverMethodsFolderProjectItem = Substitute.For<ProjectItem>();
			serverMethodsFolderProjectItem.Name.Returns("ServerMethods");
			serverMethodsFolderProjectItem.ProjectItems.Returns(serverMethodsFolderItems);

			List<ProjectItem> projectFolders = new List<ProjectItem>() { serverMethodsFolderProjectItem };

			ProjectItems projectItems = Substitute.For<ProjectItems>();
			projectItems.Item("ServerMethods").Returns(serverMethodsFolderProjectItem);
			projectItems.GetEnumerator().Returns(projectFolders.GetEnumerator());

			Project project = Substitute.For<Project>();
			project.UniqueName.Returns("ProjectName");
			project.ProjectItems.Returns(projectItems);

			DTE2 dte2 = Substitute.For<DTE2>();
			dte2.ActiveSolutionProjects.Returns(new Project[] { project });
			this.serviceProvider.GetService(Arg.Any<Type>()).Returns(dte2);
		}
	}
}
