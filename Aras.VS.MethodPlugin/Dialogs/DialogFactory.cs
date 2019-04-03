//------------------------------------------------------------------------------
// <copyright file="DialogFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Interop;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.ItemSearch.Preferences;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeConnector.Dialogs;
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Microsoft.CodeAnalysis;
using System.Windows.Forms;

namespace Aras.VS.MethodPlugin.Dialogs
{
	public class DialogFactory : IDialogFactory
	{
		private IAuthenticationManager authManager;
		private readonly IArasDataProvider arasDataProvider;
		private readonly IServiceProvider serviceProvider;
		private readonly IIOWrapper iOWrapper;
		private readonly IMessageManager messageManager;

		public DialogFactory(IAuthenticationManager authManager, IArasDataProvider arasDataProvider, IServiceProvider serviceProvider, IIOWrapper iOWrapper, IMessageManager messageManager)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));

			this.arasDataProvider = arasDataProvider;
			this.authManager = authManager;
			this.serviceProvider = serviceProvider;
			this.iOWrapper = iOWrapper;
			this.messageManager = messageManager;
		}

		public ItemSearchPresenter GetItemSearchPresenter(string itemTypeName, string itemTypeSingularLabel)
		{
			var preferenceProvider = new PreferenceProvider(authManager.InnovatorInstance);
			var savedSearchProvider = new SavedSearchProvider(authManager.InnovatorInstance);
			var searcher = new DefaultSearcher(authManager.InnovatorInstance, preferenceProvider, savedSearchProvider, itemTypeName, itemTypeSingularLabel);

			ItemSearchPresenter presenter = GetItemSearchPresenter(searcher);
			return presenter;
		}

		public ItemSearchPresenter GetItemSearchPresenter(ISearcher searcher)
		{
			var itemSearchColumnProvider = new ItemSearchColumnProvider();
			var itemSearchCellProvider = new ItemSearchCellProvider();
			var dialog = new ItemSearchDialog(itemSearchColumnProvider, itemSearchCellProvider);
			var presenter = new ItemSearchPresenter(dialog, searcher);
			return presenter;
		}

		public IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectManager projectManager, IProjectConfiguraiton projectConfiguration)
		{
			var view = new LoginView();
			var viewModel = new LoginViewModel(authManager, projectConfiguration, projectManager.SelectedProject.Name, projectManager.SelectedProject.FullName);
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new LoginViewAdapter(view);
		}

		public IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectConfiguraiton projectConfiguration, string projectName, string projectFullName)
		{
			var view = new LoginView();
			var viewModel = new LoginViewModel(authManager, projectConfiguration, projectName, projectFullName);
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new LoginViewAdapter(view);
		}

		public IViewAdaper<CreateMethodView, CreateMethodViewResult> GetCreateView(IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, IProjectManager projectManager, ICodeProvider codeProvider, IGlobalConfiguration globalConfiguration)
		{
			CreateMethodView view = new CreateMethodView();
			CreateMethodViewModel viewModel = new CreateMethodViewModel(authManager, this, projectConfiguration, templateLoader, packageManager, projectManager, arasDataProvider, codeProvider, globalConfiguration, messageManager);
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new CreateMethodViewAdapter(view);
		}

		public IViewAdaper<ConnectionInfoView, ViewResult> GetConnectionInfoView(IProjectManager projectManager, IProjectConfigurationManager configurationManager)
		{
			var projectConfiguration = configurationManager.Load(projectManager.ProjectConfigPath);

			var viewModel = new ConnectionInfoViewModel(authManager, this, configurationManager, projectManager, projectConfiguration);
			var view = new ConnectionInfoView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new ConnectionInfoViewAdapter(view);
		}

		public IViewAdaper<OpenFromArasView, OpenFromArasViewResult> GetOpenFromArasView(IProjectConfigurationManager configurationManager,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			string pathToProjectConfigFile,
			string projectName,
			string projectFullName,
			string projectLanguage)
		{
			var viewModel = new OpenFromArasViewModel(authManager, this, configurationManager, projectConfiguration, templateLoader, packageManager, messageManager, pathToProjectConfigFile, projectName, projectFullName, projectLanguage);
			var view = new OpenFromArasView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new OpenFromArasViewAdapter(view);
		}

		public IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult> GetOpenFromPackageView(TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguration)
		{
			var viewModel = new OpenFromPackageViewModel(this, templateLoader, messageManager, projectLanguage, projectConfiguration);
			var view = new OpenFromPackageView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new OpenFromPackageViewAdapter(view);
		}

		public IViewAdaper<SaveMethodView, SaveMethodViewResult> GetSaveToArasView(IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, PackageManager packageManager, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
		{
			var view = new SaveMethodView();
			var viewModel = new SaveMethodViewModel(
				authManager,
				this,
				projectConfigurationManager,
				projectConfiguration,
				packageManager,
				arasDataProvider,
				methodInformation,
				messageManager,
				methodCode,
				projectConfigPath,
				projectName,
				projectFullName);

			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new SaveMethodViewAdapter(view);
		}

		public IViewAdaper<SaveToPackageView, SaveToPackageViewResult> GetSaveToPackageView(IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, ICodeProvider codeProvider, IProjectManager projectManager, MethodInfo methodInformation, string sourceCode)
		{
			var saveToLocalPackageView = new SaveToPackageView();
			var viewModel = new SaveToPackageViewModel(authManager, this, projectConfiguration, templateLoader, packageManager, codeProvider, projectManager, arasDataProvider, this.iOWrapper, messageManager, methodInformation, sourceCode);
			saveToLocalPackageView.DataContext = viewModel;

			AttachToParentWindow(saveToLocalPackageView);
			return new SaveToPackageViewAdapter(saveToLocalPackageView);
		}

		public IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult> GetUpdateFromArasView(IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, MethodInfo methodInfo, string projectConfigPath, string projectName, string projectFullName)
		{
			var viewModel = new UpdateFromArasViewModel(authManager, projectConfigurationManager, projectConfiguration, this, templateLoader, packageManager, messageManager, methodInfo, projectConfigPath, projectName, projectFullName);
			var view = new UpdateFromArasView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new UpdateFromArasViewAdapter(view);
		}

		public IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult> GetCreateCodeItemView(ICodeItemProvider codeItemProvider, bool usedVSFormatting)
		{
			var viewModel = new CreateCodeItemViewModel(codeItemProvider, usedVSFormatting);
			var view = new CreateCodeItemView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new CreateCodeItemViewAdapter(view);
		}

		public IViewAdaper<DebugMethodView, DebugMethodViewResult> GetDebugMethodView(IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
		{
			var viewModel = new DebugMethodViewModel(authManager, projectConfigurationManager, projectConfiguration, methodInformation, this, methodCode, projectConfigPath, projectName, projectFullName);
			var view = new DebugMethodView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new DebugMethodViewAdapter(view);
		}

		public IViewAdaper<OpenFromPackageTreeView, OpenFromPackageTreeViewResult> GetOpenFromPackageTreeView(string actualFolderPath, string package, string methodName, string selectedSearchType)
		{
			var viewModel = new OpenFromPackageTreeViewModel(this, this.iOWrapper, this.messageManager, actualFolderPath, package, methodName, selectedSearchType);
			var view = new OpenFromPackageTreeView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new OpenFromPackageTreeViewAdapter(view);
		}

		public IViewAdaper<OpenFileDialog, OpenFileDialogResult> GetOpenFileDialog(string filter, string defaultExtention)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = filter,
				FilterIndex = 0,
				DefaultExt = defaultExtention
			};

			return new OpenFileDialogAdapter(openFileDialog);
		}
		
				public IViewAdaper<FolderNameDialog, FolderNameDialogResult> GetFolderNameDialog()
		{
			var viewModel = new FolderNameViewModel(this, messageManager);
			var view = new FolderNameDialog();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new FolderNameDialogAdapter(view);
		}
		
		public IViewAdaper<SelectPathDialog, SelectPathDialogResult> GetSelectPathDialog(DirectoryItemType searchToLevel,
			string rootPath = "",
			string startPath = "",
			string fileExtantion = "")
		{
			SelectPathDialog dialog = new SelectPathDialog();
			SelectPathViewModel viewModel = new SelectPathViewModel(this, searchToLevel, this.iOWrapper, this.messageManager, rootPath, startPath, fileExtantion);
			dialog.DataContext = viewModel;

			return new SelectPathDialogAdapter(dialog);
		}

		public IViewAdaper<MoveToView, MoveToViewResult> GetMoveToView(string methodFolderPath, SyntaxNode node)
		{
			var viewModel = new MoveToViewModel(this, methodFolderPath, node);
			var view = new MoveToView();
			view.DataContext = viewModel;

			AttachToParentWindow(view);
			return new MoveToViewAdapter(view);
		}

		public IMessageBoxWindow GetMessageBoxWindow()
		{
			var view = new MessageBoxWindow();

			AttachToParentWindow(view);
			return view;
		}


		private IVsUIShell UIShell
		{
			get
			{
				return (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));
			}
		}

		private void AttachToParentWindow(System.Windows.Window view)
		{
			IntPtr hwnd;
			this.UIShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;
		}
	}
}
