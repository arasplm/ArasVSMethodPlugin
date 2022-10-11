//------------------------------------------------------------------------------
// <copyright file="VisulaStudioMessageManager.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.Method.Libs;

namespace Aras.VS.MethodPlugin
{
	public class VisualStudioMessageManager : MessageManager
	{
		private static Dictionary<string, string> messages = new Dictionary<string, string>
		{
			{ "AuthenticationFailedFor", "Authentication failed for {0}." },
			{ "CouldNotInsertExternalItemsInsideOfMethodCodeSection", "Could not insert external items inside of 'MethodCode' section.\r\nClick OK to insert fake class inside of 'MethodCode' section." },
			{ "PropertyInTheItemTypeNotFound", "'{0}' property in the {0} ItemType not found." },
			{ "MoreThenOneItemFound", "More then one item found." },
			{ "ProjectIsNotSelectedOnSolutionExplorer", "Project is not selected on Solution Explorer." },
			{ "OneOrMoreMethodFilesIsNotSavedDoYouWantToSaveChanges", "One or more method files is not saved. Do you want to save changes?" },
			{ "ArasVSMethodPlugin", "Aras VS method plugin" },
			{ "TheTemplateFromSelectedMethodNotFoundDefaultTemplateWillBeUsed", "The template {0} from selected method not found. Default template will be used." },
			{ "IOMDllInTheCurrentProjectIsNotFound", "IOM.dll in the current project is not found." },
			{ "CreateNewMethod", "Create new method" },
			{ "MethodAlreadyAddedToProjectDoYouWantReplaceMethod", "Method already added to project. Do you want replace method?" },
			{ "Warning", "Warning" },
			{ "UserCodeTemplateInvalidFormat", "User code template invalid format." },
			{ "UserCodeTamplateMustBeMethodType", "User code tamplate must be {0} method type." },
			{ "FolderNameIsEmpty", "Folder name is empty." },
			{ "OpenMethodArasInnovator", "Open method Aras Innovator" },
			{ "MethodIsNotSelected", "Method is not selected." },
			{ "OpenMethodFromAMLPackage", "Open method from AML package" },
			{ "FileOrFolderWereNotFound", "File or Folder were not found." },
			{ "CurrentProjectAndMethodTypesAreDifferent", "Current project and method types are different." },
			{ "TheMethodAlreadyAttachedToDiffererntPackageClickOKToReasignPackageForThisMethod", "The {0} method already attached to differernt package. Click OK to reasign package for this method." },
			{ "SaveMethodToArasInnovator", "Save method to Aras Innovator" },
			{ "LatestVersionInArasIsDifferrentThatYouHaveClickOKToRewriteArasMethodCode", "Latest version in Aras is differrent that you have. Click OK to rewrite Aras method code." },
			{ "TheMethodAlreadyExsistInPackagesClickOKToReplaceIt", "The method {0} already exsist in packages. Click OK to replace it." },
			{ "SavePackage", "Save package" },
			{ "SelectionWasNotFound", "Selection wasn't found." },
			{ "SelectPathForSaving", "Select path for saving" },
			{ "AreYouSureYouWantToDeleteTheFolder", "Are you sure you want to delete the {0} folder?" },
			{ "SavingMethodToLocalPackage", "Saving method to local package" },
			{ "MethodIsNotFoundInTheCurrentConnection", "Method {0} is not found in the current connection." },
			{ "UpdateMethodFromArasInnovator", "Update method from Aras Innovator" },
			{ "ConfigurationsForTheMethodNotFound", "Configurations for the {0} method not found." },
			{ "CodeItemAlreadyExists", "Code item already exists." },
			{ "ClassNotFoundInNamespace", "Class not found in namespace" },
			{ "MethodNotFoundInClass", "Method not found in class" },
			{ "SelectedCodeWillBeMovedToMainMethodFileClickOKToContinue", "Selected code will be moved to main method file. Click OK to continue." },
			{ "MoveToMainMethod", "Move to main method" },
			{ "ItemHasBeenLockedBySomeone", "Item has been locked by someone." },
			{ "GetLockStatusError", "Get Lock Status Error" },
			{ "MethodSaved", "Method \"{0}\" saved." },
			{ "MethodSavedToPackage", "Method \"{0}\" saved to package \"{1}\"." },
			{ "CreateNewOrOpenExistingProject", "Do you want to open this method in a new project?" },
			{ "OpenInVSActionImported", "'Open in Visual Studio' action has been imported to Aras Innovator." },
			{ "OpenInVSActionImportFailed", "Failed to import 'Open in Visual Studio' action to Aras Innovator.\r\n{0}" }
		};

		public override string GetMessage(string key)
		{
			string message = base.GetMessage(key);
			if (!string.IsNullOrEmpty(message))
			{
				return message;
			}

			messages.TryGetValue(key, out message);
			return message;
		}

		public override string GetMessage(string key, params string[] args)
		{
			string message =  base.GetMessage(key, args);
			if (!string.IsNullOrEmpty(message))
			{
				return message;
			}

			if (messages.TryGetValue(key, out message))
			{
				message = string.Format(message, args);
			}

			return message;
		}
	}
}
