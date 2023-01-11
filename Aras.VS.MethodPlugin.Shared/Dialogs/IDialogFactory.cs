//------------------------------------------------------------------------------
// <copyright file="IDialogFactory.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Forms;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.OpenMethodInVS;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.CodeAnalysis;
using OfficeConnector.Dialogs;

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
		   TemplateLoader templateLoader,
		   PackageManager packageManager,
		   string pathToProjectConfigFile,
		   string projectName,
		   string projectFullName,
		   string projectLanguage,
		   string startMethodId);

		IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult> GetOpenFromPackageView(TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguraiton);

		IViewAdaper<SaveMethodView, SaveMethodViewResult> GetSaveToArasView(IProjectConfigurationManager projectConfigurationManager,
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
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MethodInfo methodInfo,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult> GetCreateCodeItemView(ICodeItemProvider codeItemProvider, bool usedVSFormatting);

		IViewAdaper<DebugMethodView, DebugMethodViewResult> GetDebugMethodView(IProjectConfigurationManager projectConfigurationManager,
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
