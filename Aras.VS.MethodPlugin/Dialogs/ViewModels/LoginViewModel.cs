//------------------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using System.Windows.Controls;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class LoginViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IProjectConfiguraiton projectConfiguration;

		private ICommand loginClick;
		private ICommand closeClick;

		private string selectedUrl;
		private string selectedDatabase;
		private ObservableCollection<string> databases;
		private string login;

		private bool isLoginEnabled;
		private bool isPasswordEnabled;

        private string projectName;
		private string projectFullName;
	    
		public LoginViewModel(
			IAuthenticationManager authManager,
			IProjectConfiguraiton projectConfiguration,
			string projectName,
			string projectFullName)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));

			this.authManager = authManager;
			this.projectConfiguration = projectConfiguration;
			this.projectName = projectName;
			this.projectFullName = projectFullName;

			this.UrlSource = new ObservableCollection<string>(projectConfiguration.Connections.Select(c => c.ServerUrl).ToList());

			ConnectionInfo lastConnection = projectConfiguration.Connections.FirstOrDefault(x => x.LastConnection);
			if (lastConnection != null)
			{
				this.SelectedUrl = lastConnection.ServerUrl;
				if (string.IsNullOrEmpty(this.SelectedDatabase))
				{
					this.SelectedDatabase = lastConnection.Database;
				}
				if (!IsLoginEnabled)
				{
					this.Login = lastConnection.Login;
				}
			}

			this.loginClick = new RelayCommand<object>(OnLoginClicked, IsEnabledLoginCommand);
			this.closeClick = new RelayCommand<object>(OnCloseCliked);
        }

		#region Properties

		public string SelectedUrl
		{
			get { return selectedUrl; }
			set
			{
				selectedUrl = value;
				RaisePropertyChanged(nameof(SelectedUrl));

				string[] dataBases = authManager.GetBases(selectedUrl, projectFullName);
				Databases = new ObservableCollection<string>(dataBases);
            }
		}

		public string SelectedDatabase
		{
			get { return selectedDatabase; }
			set
			{
				selectedDatabase = value;
				RaisePropertyChanged(nameof(SelectedDatabase));

				bool isWindowsAuthentication = authManager.TryWindowsLogin(projectName, projectFullName, selectedUrl, selectedDatabase);
				if (isWindowsAuthentication)
				{
					Login = authManager.InnovatorUser.userName;
					IsLoginEnabled = true;
					IsPasswordEnabled = false;
				}
				else
				{
					IsLoginEnabled = false;
					IsPasswordEnabled = true;
				}
            }
		}

		public ObservableCollection<string> UrlSource { get; set; }

		public ObservableCollection<string> Databases
		{
			get { return databases; }
			set
			{
				databases = value;
				if (databases.Count == 1)
				{
					SelectedDatabase = databases.First();
				}

				RaisePropertyChanged(nameof(Databases));
            }
		}

		public string Login
		{
			get { return login; }
			set
			{
				login = value;
				RaisePropertyChanged(nameof(Login));
            }
		}

		public bool IsLoginEnabled
		{
			get { return isLoginEnabled; }
			set
			{
				isLoginEnabled = value;
				RaisePropertyChanged(nameof(IsLoginEnabled));
			}
		}

		public bool IsPasswordEnabled
		{
			get { return isPasswordEnabled; }
			set
			{
				isPasswordEnabled = value;
				RaisePropertyChanged(nameof(IsPasswordEnabled));
			}
		}

        #endregion

        #region Commands

        public ICommand LoginClick
		{
			get { return loginClick; }
		}

		public ICommand CloseCommand
		{
			get { return closeClick; }
		}

		#endregion

		private void OnLoginClicked(object parameter)
		{
			var values = (object[])parameter;
			var window = (Window)values[0];
			var passwordBox = (PasswordBox)values[1];

			if (authManager.Login(projectName, projectFullName, SelectedUrl, SelectedDatabase, Login, passwordBox.Password, window))
			{
				this.projectConfiguration.AddConnection(new ConnectionInfo() { ServerUrl = this.SelectedUrl, Database = this.SelectedDatabase, Login = this.Login, LastConnection = true });
				window.DialogResult = true;
			}
			else
			{
				passwordBox.Password = string.Empty;
			}
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private bool IsEnabledLoginCommand(object obj)
		{
			if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(selectedUrl) || string.IsNullOrWhiteSpace(selectedDatabase))
			{
				return false;
			}

			return true;
		}
	}
}
