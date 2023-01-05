//------------------------------------------------------------------------------
// <copyright file="SaveToLocalPackageViewModel.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SaveToPackageViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IProjectConfiguraiton projectConfiguration;
		private readonly TemplateLoader templateLoader;
		private readonly PackageManager packageManager;
		private readonly IProjectManager projectManager;
		private readonly IArasDataProvider arasDataProvider;
		private readonly IIOWrapper iOWrapper;
		private readonly MessageManager messageManager;

		private MethodInfo methodInfo;
		private MethodItemTypeInfo methodItemTypeInfo;

		private string methodComments;
		private int methodCommentMaxLength;
		private string packagePath;
		private string methodName;
		private int methodNameMaxLength;
		private string methodCode;
		private string selectedPackage;
		private string selectedIdentityKeyedName;
		private string selectedIdentityId;

		private ICommand folderBrowserCommand;
		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand selectedIdentityCommand;

		public SaveToPackageViewModel(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			ICodeProvider codeProvider,
			IProjectManager projectManager,
			IArasDataProvider arasDataProvider,
			IIOWrapper iOWrapper,
			MessageManager messageManager,
			MethodInfo methodInformation,
			string sourceCode)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (codeProvider == null) throw new ArgumentNullException(nameof(codeProvider));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));
			if (methodInformation == null) throw new ArgumentNullException(nameof(methodInformation));

			this.authManager = authManager;
			this.dialogFactory = dialogFactory;
			this.projectConfiguration = projectConfiguration;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.projectManager = projectManager;
			this.arasDataProvider = arasDataProvider;
			this.iOWrapper = iOWrapper;
			this.messageManager = messageManager;
			this.MethodInformation = methodInformation;

			this.folderBrowserCommand = new RelayCommand<object>(OnFolderBrowserClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);

			this.methodItemTypeInfo = arasDataProvider.GetMethodItemTypeInfo();
			this.MethodNameMaxLength = methodItemTypeInfo.NameStoredLength;
			this.MethodCommentMaxLength = methodItemTypeInfo.CommentsStoredLength;

			MethodComment = MethodInformation.MethodComment;
			PackagePath = projectConfiguration.LastSelectedDir;
			MethodName = MethodInformation.MethodName;

			string packageMethodFolderPath = projectConfiguration.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodWorkingFolder = Path.Combine(projectManager.ServerMethodFolderPath, packageMethodFolderPath, methodInformation.MethodName);
			MethodCode = codeProvider.LoadMethodCode(methodWorkingFolder, sourceCode);
			SelectedPackage = MethodInformation.Package.Name;
			selectedIdentityKeyedName = MethodInformation.ExecutionAllowedToKeyedName;
			selectedIdentityId = MethodInformation.ExecutionAllowedToId;
		}

		#region Properties

		public string PackagePath
		{
			get { return packagePath; }
			set
			{
				packagePath = value;
				RaisePropertyChanged(nameof(PackagePath));
			}
		}

		public MethodInfo MethodInformation
		{
			get { return methodInfo; }
			set
			{
				methodInfo = value;
				RaisePropertyChanged(nameof(MethodInfo));
			}
		}

		public string MethodComment
		{
			get { return methodComments; }
			set
			{
				if (value != null && value.Length > this.MethodCommentMaxLength)
				{
					methodComments = value.Substring(0, this.MethodCommentMaxLength);
				}
				else
				{
					methodComments = value;
				}

				RaisePropertyChanged(nameof(MethodComment));
			}
		}

		public int MethodCommentMaxLength
		{
			get { return methodCommentMaxLength; }
			set
			{
				methodCommentMaxLength = value;
				RaisePropertyChanged(nameof(MethodCommentMaxLength));
			}
		}

		public string SelectedEventSpecificData
		{
			get { return MethodInformation.EventData.ToString(); }
			set { } // this is only for binding
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				if (value != null && value.Length > this.MethodNameMaxLength)
				{
					methodName = value.Substring(0, this.MethodNameMaxLength);
				}
				else
				{
					methodName = value;
				}

				RaisePropertyChanged(nameof(MethodName));
			}
		}

		public int MethodNameMaxLength
		{
			get { return methodNameMaxLength; }
			set
			{
				methodNameMaxLength = value;
				RaisePropertyChanged(nameof(MethodNameMaxLength));
			}
		}

		public string MethodCode
		{
			get { return methodCode; }
			set { methodCode = value; RaisePropertyChanged(nameof(MethodCode)); }
		}

		public List<PackageInfo> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }

		public string SelectedPackage
		{
			get { return selectedPackage; }
			set
			{
				selectedPackage = value;
				RaisePropertyChanged(nameof(SelectedPackage));
			}
		}

		public PackageInfo SelectedPackageInfo { get { return new PackageInfo(selectedPackage ?? string.Empty); } }

		public string SelectedIdentityKeyedName
		{
			get { return this.selectedIdentityKeyedName; }
			set
			{
				this.selectedIdentityKeyedName = value;
				RaisePropertyChanged(nameof(SelectedIdentityKeyedName));
			}
		}

		public string SelectedIdentityId
		{
			get
			{
				return this.selectedIdentityId;
			}
			set { }
		}

		#endregion

		#region Commands

		public ICommand FolderBrowserCommand { get { return folderBrowserCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand SelectedIdentityCommand { get { return selectedIdentityCommand; } }

		#endregion

		private void OnFolderBrowserClick(object window)
		{
			var adapter = this.dialogFactory.GetSelectPathDialog(DirectoryItemType.Folder, startPath: PackagePath);
			var dialogResult = adapter.ShowDialog();

			if (dialogResult.DialogOperationResult == true)
			{
				this.PackagePath = dialogResult.SelectedFullPath;
			}
		}

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;

			string methodPath = Path.Combine(this.PackagePath, $"{this.SelectedPackage}\\Import\\Method\\{this.MethodName}.xml");
			if (File.Exists(methodPath))
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog(
					messageManager.GetMessage("TheMethodAlreadyExsistInPackagesClickOKToReplaceIt", this.MethodName),
					messageManager.GetMessage("SavePackage"),
					MessageButtons.OKCancel,
					MessageIcon.None);

				if (dialogReuslt == MessageDialogResult.Cancel)
				{
					return;
				}
			}

			wnd.DialogResult = true;
			wnd.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(this.selectedPackage) ||
				string.IsNullOrEmpty(this.methodName) ||
				string.IsNullOrEmpty(this.packagePath) ||
				string.IsNullOrEmpty(this.selectedIdentityKeyedName))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private void SelectedIdentityCommandClick()
		{
			string itemTypeName = "Identity";
			string itemTypeSingularLabel = "Identity";

			List<PropertyInfo> predefinedSearchItems;
			if (!projectConfiguration.LastSavedSearch.TryGetValue(itemTypeName, out predefinedSearchItems))
			{
				predefinedSearchItems = new List<PropertyInfo>();
			}

			var searchArguments = new ItemSearchPresenterArgs()
			{
				Title = "Search dialog",
				PredefinedPropertyValues = predefinedSearchItems
			};

			ItemSearchPresenter presenter = dialogFactory.GetItemSearchPresenter(itemTypeName, itemTypeSingularLabel);
			var result = presenter.Run(searchArguments);
			if (result.DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				// TODO : should be replaced by better approach
				dynamic item = authManager.InnovatorInstance.newItem(itemTypeName, "get");
				item.setID(result.ItemId);
				item.setAttribute("select", "keyed_name");
				item = item.apply();

				this.SelectedIdentityKeyedName = item.getProperty("keyed_name");
				this.selectedIdentityId = result.ItemId;

				if (projectConfiguration.LastSavedSearch.ContainsKey(result.ItemType))
				{
					projectConfiguration.LastSavedSearch[result.ItemType] = result.LastSavedSearch;
				}
				else
				{
					projectConfiguration.LastSavedSearch.Add(result.ItemType, result.LastSavedSearch);
				}
			}
		}
	}
}
