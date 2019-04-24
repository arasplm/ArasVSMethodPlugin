//------------------------------------------------------------------------------
// <copyright file="DebugMethodViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class DebugMethodViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IProjectConfigurationManager projectConfigurationManager;
		private readonly IDialogFactory dialogFactory;

		private IProjectConfiguraiton projectConfiguration;
		private MethodItemTypeInfo methodItemTypeInfo;

		private string projectConfigPath;
		private string projectName;
		private string projectFullName;

		private ConnectionInfo connectionInfo;

		private string methodType;
		private string methodLanguage;
		private string templateName;
		private string eventData;
		private string methodCode;
		private string methodContext;
		private string methodName;

		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand editConnectionInfoCommand;

		public DebugMethodViewModel(
			IAuthenticationManager authManager,
			IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			MethodInfo methodInformation,
			IDialogFactory dialogFactory,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (methodInformation == null) throw new ArgumentNullException(nameof(methodInformation));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));

			this.authManager = authManager;
			this.projectConfigurationManager = projectConfigurationManager;
			this.projectConfiguration = projectConfiguration;
			this.dialogFactory = dialogFactory;

			this.projectConfigPath = projectConfigPath;
			this.projectName = projectName;
			this.projectFullName = projectFullName;

			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClick);

			this.methodCode = methodCode;
			this.methodContext = $"<Item action=\"{methodInformation.MethodName}\" type=\"Method\" />";
			this.methodType = methodInformation.MethodType;
			this.methodLanguage = methodInformation.MethodLanguage;
			this.templateName = methodInformation.TemplateName;
			this.eventData = methodInformation.EventData.ToString();
			this.methodName = methodInformation.MethodName;

			//TODO: How to know current connection?
			ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);
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

		public string MethodContext
		{
			get { return methodContext; }
			set
			{
				methodContext = value;
				RaisePropertyChanged(nameof(MethodContext));
			}
		}

		public string MethodName
		{
			get { return methodName; }
			set { }
		}

		#endregion

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		#endregion Commands

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

		private void EditConnectionInfoCommandClick(object window)
		{
			var loginAdapter = this.dialogFactory.GetLoginView(projectConfiguration, projectName, projectFullName);
			var loginDialogResult = loginAdapter.ShowDialog();
			if (loginDialogResult.DialogOperationResult == true)
			{
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
				ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);
			}
		}
	}
}
