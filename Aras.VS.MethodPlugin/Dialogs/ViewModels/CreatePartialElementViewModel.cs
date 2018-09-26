//------------------------------------------------------------------------------
// <copyright file="CreatePartialElementViewModel.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class CreatePartialElementViewModel : BaseViewModel
	{
		private string fileName;
		private bool isOkButtonEnabled;

		private ICommand okCommand;
		private ICommand cancelCommand;
		private ICommand closeCommand;

		public CreatePartialElementViewModel()
		{
			this.okCommand = new RelayCommand<object>(OnOKCliked, IsEnabledOkButton);
			this.cancelCommand = new RelayCommand<object>(OnCancelCliked);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			this.FileName = "File1";
		}

		#region Properties

		public string FileName
		{
			get { return this.fileName; }
			set
			{
				this.fileName = value;
				RaisePropertyChanged(nameof(FileName));
			}
		}

		public bool IsOkButtonEnabled
		{
			get { return this.isOkButtonEnabled; }
			set
			{
				this.isOkButtonEnabled = value;
				RaisePropertyChanged(nameof(IsOkButtonEnabled));
			}
		}

		#endregion

		#region Commands

		public ICommand OKCommand { get { return this.okCommand; } }

		public ICommand CancelCommand { get { return this.cancelCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void OnOKCliked(object view)
		{
			var window = view as System.Windows.Window;
			window.DialogResult = true;
			window.Close();
		}

		private void OnCloseCliked(object view)
		{
			var window = view as System.Windows.Window;
			window.DialogResult = false;
			window.Close();
		}

		private void OnCancelCliked(object view)
		{
			var window = view as System.Windows.Window;
			window.Close();
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(this.fileName))
			{
				return false;
			}

			return true;
		}
	}
}
