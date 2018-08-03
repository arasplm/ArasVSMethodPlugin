//------------------------------------------------------------------------------
// <copyright file="SaveMethodViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SaveMethodViewModel:BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IDialogFactory dialogFactory;
		private readonly ProjectConfigurationManager projectConfigurationManager;
		private readonly PackageManager packageManager;
		private ProjectConfiguraiton projectConfiguration;

		private string projectConfigPath;
		private string projectName;
		private string projectFullName;

		private ConnectionInfo connectionInfo;
		private string innovatorMethodId;

		private string methodComment;
		private string methodType;
		private string methodLanguage;
		private string templateName;
		private string eventData;
		private string methodCode;
		private string selectedIdentityKeyedName;
		private string selectedIdentityId;
		private string selectedPackage;
		private string currentMethodPackage;
		private string methodName;

		private bool isOkButtonEnabled;

		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand editConnectionInfoCommand;
		private ICommand selectedIdentityCommand;

		public SaveMethodViewModel(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			ProjectConfigurationManager projectConfigurationManager,
			ProjectConfiguraiton projectConfiguration,
			PackageManager packageManager,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (methodInformation == null) throw new ArgumentNullException(nameof(methodInformation));

			this.authManager = authManager;
			this.dialogFactory = dialogFactory;
			this.projectConfigurationManager = projectConfigurationManager;
			this.projectConfiguration = projectConfiguration;
			this.packageManager = packageManager;

			this.projectConfigPath = projectConfigPath;
			this.projectName = projectName;
			this.projectFullName = projectFullName;

			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClick);
			this.selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);

			this.methodComment = methodInformation.MethodComment;
			this.innovatorMethodId = methodInformation.InnovatorMethodId;

			this.methodCode = methodCode;
			this.methodType = methodInformation.MethodType;
			this.methodLanguage = methodInformation.MethodLanguage;
			this.templateName = methodInformation.TemplateName;
			this.eventData = methodInformation.EventData.ToString();
			this.selectedIdentityKeyedName = methodInformation.ExecutionAllowedToKeyedName;
			this.selectedIdentityId = methodInformation.ExecutionAllowedToId;
			this.selectedPackage = methodInformation.PackageName;
			this.methodName = methodInformation.MethodName;

			//TODO: How to know current connection?
			ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);

			ValidateOkButton();
		}

		#region Properties

		public ConnectionInfo ConnectionInformation
		{
			get { return connectionInfo; }
			set
			{
				connectionInfo = value;
				RaisePropertyChanged(nameof(ConnectionInformation));
			}
		}

		public string MethodType
		{
			get { return methodType; }
			set { }
		}

		public string MethodLanguage
		{
			get { return methodLanguage; }
			set { }
		}

		public string TemplateName
		{
			get { return templateName; }
			set { }
		}

		public string SelectedEventSpecificData
		{
			get { return eventData; }
			set { }
		}

		public string MethodCode
		{
			get { return methodCode; }
			set { }
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
			get { return this.selectedIdentityId; }
			set { }
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

		public string CurrentMethodPackage
		{
			get { return currentMethodPackage; }
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				methodName = value;
				RaisePropertyChanged(nameof(MethodName));
				ValidateOkButton();
			}
		}

		public string MethodComment
		{
			get { return methodComment; }
			set
			{
				methodComment = value;
				RaisePropertyChanged(nameof(MethodComment));
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

		private dynamic methodItem;

		public dynamic MethodItem
		{
			get { return methodItem; }
			set { methodItem = value; }
		}

		#endregion

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		//public ICommand EditMethodInfoCommand { get { return editMethodInfoCommand; } }

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand SelectedIdentityCommand { get { return selectedIdentityCommand; } }

		#endregion Commands

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;

			try
			{
				this.currentMethodPackage = packageManager.GetPackageDefinitionByElementName(this.methodName);
				if (!string.IsNullOrEmpty(currentMethodPackage) && !string.Equals(this.currentMethodPackage, this.selectedPackage))
				{
					var messageWindow = new MessageBoxWindow();
					var dialogReuslt = messageWindow.ShowDialog(null,
						$"The {this.methodName} method already attached to differernt package. Click OK to reasign package for this method.",
						"Save method to Aras Innovator",
						MessageButtons.OKCancel,
						MessageIcon.None);

					if (dialogReuslt == MessageDialogResult.Cancel)
					{
						this.SelectedPackage = this.currentMethodPackage;
						return;
					}
				}

				methodItem = authManager.InnovatorInstance.newItem("Method", "get");
				methodItem.setProperty("name", methodName);
				methodItem = methodItem.apply();

				if (!methodItem.isError() && methodItem.getID() != innovatorMethodId)
				{
					var messageWindow = new MessageBoxWindow();
					var dialogResult = messageWindow.ShowDialog(wnd,
						"Latest version in Aras is differrent that you have. Click OK to rewrite Aras method code.",
						"Save method to Aras Innovator",
						MessageButtons.OKCancel,
						MessageIcon.None);

					if (dialogResult != MessageDialogResult.OK)
					{
						return;
					}
				}

				wnd.DialogResult = true;
				wnd.Close();
			}
			catch (Exception ex)
			{
				var messageWindow = new MessageBoxWindow();
				messageWindow.ShowDialog(wnd,
					ex.Message,
					"Aras VS method plugin",
					MessageButtons.OK,
					MessageIcon.Error);
			}
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void EditConnectionInfoCommandClick(object window)
		{
			var loginView = new LoginView();
			var loginViewModel = new LoginViewModel(authManager, projectConfiguration, projectName, projectFullName);
			loginView.DataContext = loginViewModel;
			loginView.Owner = window as Window;

			if (loginView.ShowDialog() == true)
			{
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
				ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);
			}
		}

		private void ValidateOkButton()
		{
			if (string.IsNullOrEmpty(this.SelectedPackage) ||
				string.IsNullOrEmpty(this.MethodName) ||
				string.IsNullOrEmpty(this.SelectedIdentityKeyedName))
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
