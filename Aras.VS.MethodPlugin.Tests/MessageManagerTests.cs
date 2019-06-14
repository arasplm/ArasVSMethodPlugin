using System;
using Aras.Method.Libs;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests
{
	[TestFixture]
	public class VisualStudioMessageManagerTests
	{
		private MessageManager messageManager;

		[SetUp]
		public void SetUp()
		{
			this.messageManager = new VisualStudioMessageManager();
		}

		[TestCase("AuthenticationFailedFor", "Authentication failed for {0}.")]
		[TestCase("CurrentProjectTypeIsNotSupported", "Current project type is not supported")]
		[TestCase("NoPartialClassesFound", "No partial classes found.")]
		[TestCase("MethodNameCaNotBeEmpty", "Method name can not be empty.")]
		[TestCase("TemplateNotFound", "Template not found.")]
		[TestCase("NoAttributeFound", "No attribute found.")]
		[TestCase("PropertyInTheItemTypeNotFound", "'{0}' property in the {0} ItemType not found.")]
		[TestCase("CurrentCodeTypeIsNotSupported", "Current code type is not supported.")]
		[TestCase("CurrentCodeElementTypeIsNotSupported", "Current code element type is not supported.")]
		[TestCase("MoreThenOneItemFound", "More then one item found.")]
		[TestCase("ProjectIsNotSelectedOnSolutionExplorer", "Project is not selected on Solution Explorer.")]
		[TestCase("OneOrMoreMethodFilesIsNotSavedDoYouWantToSaveChanges", "One or more method files is not saved. Do you want to save changes?")]
		[TestCase("ArasVSMethodPlugin", "Aras VS method plugin")]
		[TestCase("TheTemplateFromSelectedMethodNotFoundDefaultTemplateWillBeUsed", "The template {0} from selected method not found. Default template will be used.")]
		[TestCase("IOMDllInTheCurrentProjectIsNotFound", "IOM.dll in the current project is not found.")]
		[TestCase("CreateNewMethod", "Create new method")]
		[TestCase("MethodAlreadyAddedToProjectDoYouWantReplaceMethod", "Method already added to project. Do you want replace method?")]
		[TestCase("Warning", "Warning")]
		[TestCase("UserCodeTemplateInvalidFormat", "User code template invalid format.")]
		[TestCase("UserCodeTamplateMustBeMethodType", "User code tamplate must be {0} method type.")]
		[TestCase("FolderNameIsEmpty", "Folder name is empty.")]
		[TestCase("OpenMethodArasInnovator", "Open method Aras Innovator")]
		[TestCase("MethodIsNotSelected", "Method is not selected.")]
		[TestCase("OpenMethodFromAMLPackage", "Open method from AML package")]
		[TestCase("FileOrFolderWereNotFound", "File or Folder were not found.")]
		[TestCase("CurrentProjectAndMethodTypesAreDifferent", "Current project and method types are different.")]
		[TestCase("TheMethodAlreadyAttachedToDiffererntPackageClickOKToReasignPackageForThisMethod", "The {0} method already attached to differernt package. Click OK to reasign package for this method.")]
		[TestCase("SaveMethodToArasInnovator", "Save method to Aras Innovator")]
		[TestCase("LatestVersionInArasIsDifferrentThatYouHaveClickOKToRewriteArasMethodCode", "Latest version in Aras is differrent that you have. Click OK to rewrite Aras method code.")]
		[TestCase("TheMethodAlreadyExsistInPackagesClickOKToReplaceIt", "The method {0} already exsist in packages. Click OK to replace it.")]
		[TestCase("SavePackage", "Save package")]
		[TestCase("SelectionWasNotFound", "Selection wasn't found.")]
		[TestCase("SelectPathForSaving", "Select path for saving")]
		[TestCase("AreYouSureYouWantToDeleteTheFolder", "Are you sure you want to delete the {0} folder?")]
		[TestCase("SavingMethodToLocalPackage", "Saving method to local package")]
		[TestCase("MethodIsNotFoundInTheCurrentConnection", "Method {0} is not found in the current connection.")]
		[TestCase("UpdateMethodFromArasInnovator", "Update method from Aras Innovator")]
		[TestCase("ConfigurationsForTheMethodNotFound", "Configurations for the {0} method not found.")]
		[TestCase("CodeItemAlreadyExists", "Code item already exists.")]
		[TestCase("ClassNotFoundInNamespace", "Class not found in namespace")]
		[TestCase("MethodNotFoundInClass", "Method not found in class")]
		[TestCase("SelectedCodeWillBeMovedToMainMethodFileClickOKToContinue", "Selected code will be moved to main method file. Click OK to continue.")]
		[TestCase("MoveToMainMethod", "Move to main method")]
		[TestCase("ItemHasBeenLockedBySomeone", "Item has been locked by someone.")]
		[TestCase("GetLockStatusError", "Get Lock Status Error")]
		[TestCase("MethodSaved", "Method \"{0}\" saved.")]
		[TestCase("MethodSavedToPackage", "Method \"{0}\" saved to package \"{1}\".")]
		public void GetMessage_ShouldReturnExpectedValue(string key, string expected)
		{
			Assert.AreEqual(expected, messageManager.GetMessage(key));
		}

		[Test]
		public void CouldNotInsertExternalItemsInsideOfMethodCodeSection_Key_ShouldReturnExpectedValue()
		{
			Assert.AreEqual($"Could not insert external items inside of 'MethodCode' section.{Environment.NewLine}Click OK to insert fake class inside of 'MethodCode' section.", messageManager.GetMessage("CouldNotInsertExternalItemsInsideOfMethodCodeSection"));
		}

		[Test]
		public void GetMessage_MultipleArguments_ShouldReturnExpectedValue()
		{
			Assert.AreEqual("Method \"arg1\" saved to package \"arg2\".", messageManager.GetMessage("MethodSavedToPackage", "arg1", "arg2"));
		}
	}
}
