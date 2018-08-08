//------------------------------------------------------------------------------
// <copyright file="SaveToLocalPackageViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SaveToPackageViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IDialogFactory dialogFactory;
		private readonly ProjectConfiguraiton projectConfiguration;
		private readonly TemplateLoader templateLoader;
		private readonly PackageManager packageManager;
		private readonly IProjectManager projectManager;
		private readonly IArasDataProvider arasDataProvider;

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

		private bool isOkButtonEnabled;

		private ICommand folderBrowserCommand;
		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand selectedIdentityCommand;

		public SaveToPackageViewModel(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			ICodeProvider codeProvider,
			IProjectManager projectManager,
			IArasDataProvider arasDataProvider,
			MethodInfo methodInformation,
			string pathToFileForSave)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (codeProvider == null) throw new ArgumentNullException(nameof(codeProvider));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));
			if (methodInformation == null) throw new ArgumentNullException(nameof(methodInformation));

			this.authManager = authManager;
			this.dialogFactory = dialogFactory;
			this.projectConfiguration = projectConfiguration;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.projectManager = projectManager;
			this.arasDataProvider = arasDataProvider;
			this.MethodInformation = methodInformation;

			this.folderBrowserCommand = new RelayCommand<object>(OnFolderBrowserClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);

			string sourceCode = File.ReadAllText(pathToFileForSave, new UTF8Encoding(true));

			this.methodItemTypeInfo = arasDataProvider.GetMethodItemTypeInfo();
			this.MethodNameMaxLength = methodItemTypeInfo.NameStoredLength;
			this.MethodCommentMaxLength = methodItemTypeInfo.CommentsStoredLength;

			MethodComment = MethodInformation.MethodComment;
			PackagePath = projectConfiguration.LastSelectedDir;
			MethodName = MethodInformation.MethodName;
			MethodCode = codeProvider.LoadMethodCode(sourceCode, MethodInformation, projectManager.ServerMethodFolderPath);
			SelectedPackage = MethodInformation.PackageName;
			selectedIdentityKeyedName = MethodInformation.ExecutionAllowedToKeyedName;
			selectedIdentityId = MethodInformation.ExecutionAllowedToId;

			ValidateOkButton();
		}

		#region Properties

		public string PackagePath
		{
			get { return packagePath; }
			set
			{
				packagePath = value;
				RaisePropertyChanged(nameof(PackagePath));
				ValidateOkButton();
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
				ValidateOkButton();
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
				ValidateOkButton();
			}
		}

		public int MethodNameMaxLength
		{
			get { return methodNameMaxLength; }
			set
			{
				methodNameMaxLength = value;
				RaisePropertyChanged(nameof(MethodNameMaxLength));
				ValidateOkButton();
			}
		}

		public string MethodCode
		{
			get { return methodCode; }
			set { methodCode = value; RaisePropertyChanged(nameof(MethodCode)); }
		}

		public List<string> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }

		public string SelectedPackage
		{
			get { return selectedPackage; }
			set
			{
				selectedPackage = value;
				RaisePropertyChanged(nameof(SelectedPackage));
				ValidateOkButton();
			}
		}

		public bool IsOkButtonEnabled
		{
			get { return isOkButtonEnabled; }
			set
			{
				isOkButtonEnabled = value;
				RaisePropertyChanged(nameof(IsOkButtonEnabled));
			}
		}

		public string SelectedIdentityKeyedName
		{
			get { return this.selectedIdentityKeyedName; }
			set
			{
				this.selectedIdentityKeyedName = value;
				RaisePropertyChanged(nameof(SelectedIdentityKeyedName));
				ValidateOkButton();
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
			SelectPathDialog dialog = new SelectPathDialog();
			SelectPathViewModel viewModel = new SelectPathViewModel(DirectoryItemType.Folder, PackagePath);
			dialog.DataContext = viewModel;
			dialog.Owner = window as Window;

			if (dialog.ShowDialog() == true)
			{
				this.PackagePath = viewModel.SelectedPath;
			}
		}

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;

			string methodPath = Path.Combine(this.PackagePath, $"{this.SelectedPackage}\\Import\\Method\\{this.MethodName}.xml");
			if (File.Exists(methodPath))
			{
				var messageWindow = new MessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog(wnd,
					$"The method {this.MethodName} already exsist in packages. Click OK to replace it.",
					"Save package",
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

		private void ValidateOkButton()
		{
			if (string.IsNullOrEmpty(this.selectedPackage) ||
				string.IsNullOrEmpty(this.methodName) ||
				string.IsNullOrEmpty(this.packagePath) ||
				string.IsNullOrEmpty(this.selectedIdentityKeyedName))
			{
				IsOkButtonEnabled = false;
			}
			else
			{
				IsOkButtonEnabled = true;
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
