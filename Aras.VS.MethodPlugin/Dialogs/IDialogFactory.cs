//------------------------------------------------------------------------------
// <copyright file="IDialogFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeConnector.Dialogs;
using Aras.VS.MethodPlugin.Configurations;
using System.Windows.Forms;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;

namespace Aras.VS.MethodPlugin.Dialogs
{
	public interface IDialogFactory
	{
		ItemSearchPresenter GetItemSearchPresenter(string itemTypeName, string itemTypeSingularLabel);

		ItemSearchPresenter GetItemSearchPresenter(ISearcher searcher);

		IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectManager projectManager, IProjectConfiguraiton projectConfiguration);

		IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectConfiguraiton projectConfiguration, string projectName, string projectFullName);

		IViewAdaper<CreateMethodView, CreateMethodViewResult> GetCreateView(IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			IProjectManager projectManager,
			ICodeProvider codeProvider,
			IGlobalConfiguration globalConfiguration);

		IViewAdaper<ConnectionInfoView, ViewResult> GetConnectionInfoView(IProjectManager projectManager, IProjectConfigurationManager configurationManager);

		IViewAdaper<OpenFromArasView, OpenFromArasViewResult> GetOpenFromArasView(IProjectConfigurationManager configurationManager,
		   IProjectConfiguraiton projectConfiguration,
		   TemplateLoader templateLoader,
		   PackageManager packageManager,
		   string pathToProjectConfigFile,
		   string projectName,
		   string projectFullName,
		   string projectLanguage);

		IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult> GetOpenFromPackageView(TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguraiton);

		IViewAdaper<SaveMethodView, SaveMethodViewResult> GetSaveToArasView(IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			PackageManager packageManager,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<SaveToPackageView, SaveToPackageViewResult> GetSaveToPackageView(IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			ICodeProvider codeProvider,
			IProjectManager projectManager,
			MethodInfo methodInformation,
			string sourceCode);

		IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult> GetUpdateFromArasView(IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MethodInfo methodInfo,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult> GetCreateCodeItemView(ICodeItemProvider codeItemProvider, bool usedVSFormatting);

		IViewAdaper<DebugMethodView, DebugMethodViewResult> GetDebugMethodView(IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<SelectPathDialog, SelectPathDialogResult> GetSelectPathDialog(DirectoryItemType searchToLevel,
			string rootPath = "",
			string startPath = "",
			string fileExtantion = "");

		IViewAdaper<MoveToView, MoveToViewResult> GetMoveToView(string methodPath, SyntaxNode node);

		IMessageBoxWindow GetMessageBoxWindow();

		IViewAdaper<OpenFileDialog, OpenFileDialogResult> GetOpenFileDialog(string filter, string defaultExtention);

		IViewAdaper<FolderNameDialog, FolderNameDialogResult> GetFolderNameDialog();

		IViewAdaper<OpenFromPackageTreeView, OpenFromPackageTreeViewResult> GetOpenFromPackageTreeView(string actualFolderPath, string package, string methodName, string selectedSearchType);
	}
}
