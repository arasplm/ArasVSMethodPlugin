//------------------------------------------------------------------------------
// <copyright file="UpdateFromArasViewModel.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class UpdateFromArasViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IProjectConfigurationManager projectConfigurationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly TemplateLoader templateLoader;
		private readonly PackageManager packageManager;
		private readonly MessageManager messageManager;
		private string projectConfigPath;
		private string projectName;
		private string projectFullName;

		private ConnectionInfo connectionInfo;

		private string methodComment;
		private string methodName;
		private string methodConfigId;
		private string methodId;
		private string methodType;
		private string methodLanguage;
		private TemplateInfo selectedTemplate;
		private EventSpecificData eventData;
		private string methodCode;
		private string executionIdentityKeyedName;
		private string executionIdentityId;
		private PackageInfo package;
		private bool isUseVSFormattingCode;

		private ICommand editConnectionInfoCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public UpdateFromArasViewModel(
			IAuthenticationManager authManager,
			IProjectConfigurationManager projectConfigurationManager,
			IDialogFactory dialogFactory,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MessageManager messageManager,
			MethodInfo methodInfo,
			string projectConfigPath,
			string projectName,
			string projectFullName)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			this.authManager = authManager;
			this.projectConfigurationManager = projectConfigurationManager;
			this.dialogFactory = dialogFactory;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.messageManager = messageManager;
			this.projectConfigPath = projectConfigPath;
			this.projectName = projectName;
			this.projectFullName = projectFullName;
			this.isUseVSFormattingCode = projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting;

			this.methodName = methodInfo.MethodName;
			this.eventData = methodInfo.EventData;

			this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			ConnectionInformation = projectConfigurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);

			UpdateMethodView(null);
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
			set { }
		}

		public string MethodName
		{
			get { return methodName; }
			set { }
		}

		public string MethodConfigId
		{
			get { return methodConfigId; }
			set { }
		}

		public string MethodId
		{
			get { return methodId; }
			set { }
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
			get { return selectedTemplate?.TemplateName; }
			set { }
		}

		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set { selectedTemplate = value; }
		}

		public EventSpecificData EventSpecificData
		{
			get { return eventData; }
			set { }
		}

		public string MethodCode
		{
			get { return methodCode; }
			set { }
		}

		public string ExecutionIdentityKeyedName
		{
			get { return executionIdentityKeyedName; }
			set { }
		}

		public string ExecutionIdentityId
		{
			get { return executionIdentityId; }
			set { }
		}

		public string PackageName
		{
			get { return package?.Name; }
			set { }
		}

		public PackageInfo Package
		{
			get { return package; }
			set { }
		}

		public bool IsUseVSFormattingCode
		{
			get { return isUseVSFormattingCode; }
			set { isUseVSFormattingCode = value; }
		}

		#endregion

		#region Commands

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void EditConnectionInfoCommandClick(object view)
		{
			Window viewWindow = view as Window;

			var loginView = new LoginView();
			var loginViewModel = new LoginViewModel(authManager, projectConfigurationManager.CurrentProjectConfiguraiton, projectName, projectFullName);
			loginView.DataContext = loginViewModel;
			loginView.Owner = viewWindow;

			if (loginView.ShowDialog() == true)
			{
				projectConfigurationManager.Save(projectConfigPath);
				ConnectionInformation = projectConfigurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);

				UpdateMethodView(viewWindow);
			}
		}

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;
			wnd.DialogResult = true;
			wnd.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(methodType) || string.IsNullOrEmpty(methodLanguage))
			{
				return false;
			}

			return true;
		}

		private void UpdateMethodView(Window view)
		{
			dynamic methodItem = authManager.InnovatorInstance.newItem("Method", "get");
			methodItem.setProperty("name", methodName);
			methodItem = methodItem.apply();

			if (methodItem.isError())
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(messageManager.GetMessage("MethodIsNotFoundInTheCurrentConnection", methodName),
					messageManager.GetMessage("UpdateMethodFromArasInnovator"),
					MessageButtons.OK,
					MessageIcon.Information);

				methodConfigId = string.Empty;
				methodId = string.Empty;
				methodLanguage = string.Empty;
				methodCode = string.Empty;
				executionIdentityKeyedName = string.Empty;
				executionIdentityId = string.Empty;
				methodType = string.Empty;
				methodComment = string.Empty;
				this.selectedTemplate = null;
				package = null;
			}
			else
			{
				methodConfigId = methodItem.getProperty("config_id", string.Empty);
				methodId = methodItem.getProperty("id", string.Empty);
				methodLanguage = methodItem.getProperty("method_type", string.Empty);
				methodCode = methodItem.getProperty("method_code", string.Empty);
				executionIdentityKeyedName = methodItem.getPropertyAttribute("execution_allowed_to", "keyed_name", string.Empty);
				executionIdentityId = methodItem.getProperty("execution_allowed_to", string.Empty);
				methodComment = methodItem.getProperty("comments", string.Empty);

				if (methodLanguage == "C#" || methodLanguage == "VB")
				{
					methodType = "server";
				}
				else
				{
					methodType = "client";
				}

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
							"Update method from Aras Innovator",
							MessageButtons.OK,
							MessageIcon.Information);

						template = templateLoader.GetDefaultTemplate(methodLanguage);
					}
				}

				this.SelectedTemplate = template;

				try
				{
					this.package = packageManager.GetPackageDefinitionByElementName(methodName);
				}
				catch (Exception ex)
				{
					this.package = null;
				}
			}

			RaisePropertyChanged(string.Empty);
		}
	}
}
