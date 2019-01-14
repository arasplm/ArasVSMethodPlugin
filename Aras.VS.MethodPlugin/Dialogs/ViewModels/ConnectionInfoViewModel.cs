//------------------------------------------------------------------------------
// <copyright file="ConnectionInfoViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class ConnectionInfoViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authenticationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IProjectConfigurationManager configurationManager;
		private readonly IProjectManager projectManager;

		private IProjectConfiguraiton projectConfiguration;
		private ConnectionInfo connectionInfo;

		private ICommand closeCommand;
		private ICommand editConnectionInfoCommand;
		private ICommand logOutCommand;

		public ConnectionInfoViewModel(
			IAuthenticationManager authenticationManager,
			IDialogFactory dialogFactory,
			IProjectConfigurationManager configurationManager,
			IProjectManager projectManager,
			IProjectConfiguraiton projectConfiguration)
		{
			if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (configurationManager == null) throw new ArgumentNullException(nameof(configurationManager));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));

			this.authenticationManager = authenticationManager;
			this.dialogFactory = dialogFactory;
			this.configurationManager = configurationManager;
			this.projectManager = projectManager;
			this.projectConfiguration = projectConfiguration;

			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.editConnectionInfoCommand = new RelayCommand<object>(OnEditConnectionInfoCommandCliked);
			this.logOutCommand = new RelayCommand<object>(OnLogOutCommandCliked, LogOutButtonIsEnabled);

			ConnectionInformation = authenticationManager.InnovatorInstance == null ? null : projectConfiguration.Connections.First(c => c.LastConnection);
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

		#endregion

		#region Commands

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand LogOutCommand { get { return logOutCommand; } }

		#endregion

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void OnEditConnectionInfoCommandCliked(object view)
		{
			var loginView = dialogFactory.GetLoginView(projectManager, projectConfiguration);

			if (loginView.ShowDialog()?.DialogOperationResult == true)
			{
				configurationManager.Save(projectManager.ProjectConfigPath, projectConfiguration);
				ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);
			}
		}

		private void OnLogOutCommandCliked(object view)
		{
			authenticationManager.LogOut();
			(view as Window).Close();
		}

		private bool LogOutButtonIsEnabled(object obj)
		{
			if (authenticationManager.InnovatorInstance == null)
			{
				return false;
			}

			return true;
		}
	}
}
