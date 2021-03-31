//------------------------------------------------------------------------------
// <copyright file="OpenFromArasViewModel.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromArasViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authenticationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IProjectConfigurationManager configurationManager;
		private readonly TemplateLoader templateLoader;
		private readonly PackageManager packageManager;
		private readonly MessageManager messageManager;
		private string pathToProjectConfigFile;
		private string projectName;
		private string projectFullName;
		private string projectLanguage;

		private ConnectionInfo connectionInfo;

		private string methodComment;
		private string methodType;
		private string methodLanguage;
		private TemplateInfo selectedTemplate;
		private EventSpecificDataType selectedEventSpecificData;
		private string methodName;
		private string methodId;
		private string methodConfigId;
		private string methodCode;
		private string identityKeyedName;
		private string identityId;
		private string package;
		private bool isUseVSFormattingCode;

		private ICommand editConnectionInfoCommand;
		private ICommand searchMethodDialogCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public OpenFromArasViewModel(
			IAuthenticationManager authenticationManager,
			IDialogFactory dialogFactory,
			IProjectConfigurationManager configurationManager,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MessageManager messageManager,
			string pathToProjectConfigFile,
			string projectName,
			string projectFullName,
			string projectLanguage,
			string startMethodId)
		{
			if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (configurationManager == null) throw new ArgumentNullException(nameof(configurationManager));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));

			this.authenticationManager = authenticationManager;
			this.dialogFactory = dialogFactory;
			this.configurationManager = configurationManager;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.messageManager = messageManager;

			this.pathToProjectConfigFile = pathToProjectConfigFile;
			this.projectName = projectName;
			this.projectFullName = projectFullName;
			this.projectLanguage = projectLanguage;
			this.isUseVSFormattingCode = configurationManager.CurrentProjectConfiguraiton.UseVSFormatting;

			this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClicked);
			this.searchMethodDialogCommand = new RelayCommand<object>(SearchMethodDialogCommandClicked);
			this.okCommand = new RelayCommand<object>(OkCommandCliked, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			ConnectionInformation = configurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);
			SelectedEventSpecificData = EventSpecificData.First();

			if (!string.IsNullOrEmpty(startMethodId))
			{
				IsSearchButtonEnabled = false;
				InitializeItemData("Method", startMethodId);
			}
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

		public string MethodComment
		{
			get { return methodComment; }
			set
			{
				methodComment = value;
				RaisePropertyChanged(nameof(MethodComment));
			}
		}

		public string MethodType
		{
			get { return methodType; }
			set
			{
				methodType = value;
				RaisePropertyChanged(nameof(MethodType));
			}
		}

		public string MethodLanguage
		{
			get { return methodLanguage; }
			set
			{
				methodLanguage = value;
				RaisePropertyChanged(nameof(MethodLanguage));
			}
		}

		public string TemplateName
		{
			get { return selectedTemplate != null ? selectedTemplate.TemplateName : "None"; }
			set { }
		}

		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set
			{
				selectedTemplate = value;

				if (selectedTemplate != null && !selectedTemplate.IsSuccessfullySupported)
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					var dialogReuslt = messageWindow.ShowDialog(selectedTemplate.Message,
						messageManager.GetMessage("OpenMethodArasInnovator"),
						MessageButtons.OK,
						MessageIcon.None);
				}

				RaisePropertyChanged(nameof(TemplateName));
			}
		}

		public List<EventSpecificDataType> EventSpecificData { get { return CommonData.EventSpecificDataTypeList; } }

		public EventSpecificDataType SelectedEventSpecificData
		{
			get { return selectedEventSpecificData; }
			set
			{
				selectedEventSpecificData = value;
				RaisePropertyChanged(nameof(SelectedEventSpecificData));
			}
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				methodName = value;
				RaisePropertyChanged(nameof(MethodName));
			}
		}

		public string MethodId
		{
			get { return methodId; }
			set { methodId = value; }
		}

		public string MethodConfigId
		{
			get { return methodConfigId; }
			set { methodConfigId = value; }
		}

		public string MethodCode
		{
			get { return methodCode; }
			set
			{
				methodCode = value;
				RaisePropertyChanged(nameof(MethodCode));
			}
		}

		public string IdentityKeyedName
		{
			get { return identityKeyedName; }
			set
			{
				identityKeyedName = value;
				RaisePropertyChanged(nameof(IdentityKeyedName));
			}
		}

		public string IdentityId
		{
			get { return identityId; }
			set { identityId = value; }
		}

		public string Package
		{
			get { return package; }
			set
			{
				package = value;
				RaisePropertyChanged(nameof(Package));
			}
		}

		public PackageInfo SelectedPackageInfo { get { return new PackageInfo(package ?? string.Empty); } }

		public bool IsUseVSFormattingCode
		{
			get { return isUseVSFormattingCode; }
			set { isUseVSFormattingCode = value; }
		}

		public bool IsSearchButtonEnabled { get; } = true;

		#endregion

		#region Commands

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand SearchMethodDialogCommand { get { return searchMethodDialogCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void EditConnectionInfoCommandClicked(object window)
		{
			var loginView = new LoginView();
			var loginViewModel = new LoginViewModel(authenticationManager, configurationManager.CurrentProjectConfiguraiton, projectName, projectFullName);
			loginView.DataContext = loginViewModel;
			loginView.Owner = window as Window;

			if (loginView.ShowDialog() == true)
			{
				configurationManager.Save(pathToProjectConfigFile);
				ConnectionInformation = configurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);

				this.MethodName = string.Empty;
				this.MethodId = string.Empty;
				this.MethodConfigId = string.Empty;
				this.MethodType = string.Empty;
				this.MethodLanguage = string.Empty;
				this.SelectedTemplate = null;
				this.MethodCode = string.Empty;
				this.IdentityKeyedName = string.Empty;
				this.IdentityId = string.Empty;
				this.Package = string.Empty;
				this.MethodComment = string.Empty;
			}
		}

		private void SearchMethodDialogCommandClicked(object window)
		{
			string itemTypeName = "Method";
			string itemTypeSingularLabel = "Method";

			List<PropertyInfo> predefinedPropertyValues = new List<PropertyInfo> { new PropertyInfo() { PropertyName = "method_type", PropertyValue = projectLanguage, IsReadonly = true } };

			List<PropertyInfo> predefinedSearchItems;
			if (!configurationManager.CurrentProjectConfiguraiton.LastSavedSearch.TryGetValue(itemTypeName, out predefinedSearchItems))
			{
				predefinedSearchItems = new List<PropertyInfo>();
			}

			foreach (var searchItem in predefinedSearchItems)
			{
				if (predefinedPropertyValues.Any(x => x.PropertyName == searchItem.PropertyName))
				{
					continue;
				}

				predefinedPropertyValues.Add(new ItemSearchPropertyInfo() { PropertyName = searchItem.PropertyName, PropertyValue = searchItem.PropertyValue });
			}

			var presenter = dialogFactory.GetItemSearchPresenter(itemTypeName, itemTypeSingularLabel);
			var searchArguments = new ItemSearchPresenterArgs()
			{
				Title = "Search dialog",
				PredefinedPropertyValues = predefinedPropertyValues
			};

			var result = presenter.Run(searchArguments);
			if (result.DialogResult == DialogResult.OK)
			{
				InitializeItemData(result.ItemType, result.ItemId);

				if (configurationManager.CurrentProjectConfiguraiton.LastSavedSearch.ContainsKey(result.ItemType))
				{
					configurationManager.CurrentProjectConfiguraiton.LastSavedSearch[result.ItemType] = result.LastSavedSearch.Cast<PropertyInfo>().ToList();
				}
				else
				{
					configurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Add(result.ItemType, result.LastSavedSearch.Cast<PropertyInfo>().ToList());
				}
			}
		}


		private void OkCommandCliked(object view)
		{
			var window = view as Window;
			window.DialogResult = true;
			window.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(package))
			{
				return false;
			}

			return true;
		}

		private void InitializeItemData(string itemType, string id)
		{
			dynamic item = authenticationManager.InnovatorInstance.newItem(itemType, "get");
			item.setID(id);
			item = item.apply();
			if (item.isError())
			{
				return;
			}

			this.MethodName = item.getProperty("name", string.Empty);
			this.MethodId = item.getProperty("id", string.Empty);
			this.MethodConfigId = item.getProperty("config_id", string.Empty);
			this.MethodLanguage = item.getProperty("method_type", string.Empty);
			this.IdentityKeyedName = item.getPropertyAttribute("execution_allowed_to", "keyed_name", string.Empty);
			this.IdentityId = item.getProperty("execution_allowed_to", string.Empty);
			this.MethodComment = item.getProperty("comments", string.Empty);

			var methodCode = item.getProperty("method_code", string.Empty);
			this.MethodCode = Regex.Replace(methodCode, @"//MethodTemplateName=[\S]+\r\n", "");

			if (methodLanguage == "C#" || methodLanguage == "VB")
			{
				this.MethodType = "server";
			}
			else
			{
				this.MethodType = "client";
			}

			var packageName = string.Empty;

			try
			{
				packageName = packageManager.GetPackageDefinitionByElementId(MethodConfigId).Name;
			}
			catch (Exception ex) { }

			this.Package = packageName;

			TemplateInfo template;
			string templateName = templateLoader.GetMethodTemplateName(methodCode);
			if (string.IsNullOrEmpty(templateName))
			{
				template = templateLoader.GetDefaultTemplate(methodLanguage);
			}
			else
			{
				template = templateLoader.GetTemplateFromCodeString(templateName, methodLanguage);
				if (template == null)
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					messageWindow.ShowDialog(messageManager.GetMessage("TheTemplateFromSelectedMethodNotFoundDefaultTemplateWillBeUsed", templateName),
						"Open method from Aras Innovator",
						MessageButtons.OK,
						MessageIcon.Information);

					template = templateLoader.GetDefaultTemplate(methodLanguage);
				}
			}

			this.SelectedTemplate = template;
		}
	}
}
