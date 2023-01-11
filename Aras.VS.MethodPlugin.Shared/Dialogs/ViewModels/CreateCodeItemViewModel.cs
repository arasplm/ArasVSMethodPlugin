//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemViewModel.cs" company="Aras Corporation">
//     Copyright © 2023 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Aras.Method.Libs.Code;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class CreateCodeItemViewModel : BaseViewModel
	{
		private readonly ICodeItemProvider codeItemProvider;

		private string fileName;
		private bool isOkButtonEnabled;
		private bool isUseVSFormattingCode;

		private CodeType selectedCodeType;
		private CodeElementType selectedElementType;

		private ICommand okCommand;
		private ICommand cancelCommand;
		private ICommand closeCommand;

		public CreateCodeItemViewModel(ICodeItemProvider codeItemProvider, bool usedVSFormatting)
		{
			if (codeItemProvider == null) throw new ArgumentNullException(nameof(codeItemProvider));

			this.codeItemProvider = codeItemProvider;

			this.okCommand = new RelayCommand<object>(OnOKCliked, IsEnabledOkButton);
			this.cancelCommand = new RelayCommand<object>(OnCancelCliked);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.isUseVSFormattingCode = usedVSFormatting;
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

		public ObservableCollection<CodeType> CodeTypes
		{
			get { return new ObservableCollection<CodeType>(Enum.GetValues(typeof(CodeType)).Cast<CodeType>()); }
			set { }
		}

		public CodeType SelectedCodeType
		{
			get { return this.selectedCodeType; }
			set
			{
				this.selectedCodeType = value;
				RaisePropertyChanged(nameof(SelectedCodeType));
				RaisePropertyChanged(nameof(ElementTypes));
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

		public bool IsUseVSFormattingCode
		{
			get { return isUseVSFormattingCode; }
			set { isUseVSFormattingCode = value; }
		}

		public ObservableCollection<CodeElementType> ElementTypes
		{
			get
			{
				ObservableCollection<CodeElementType> elementTypes = new ObservableCollection<CodeElementType>(this.codeItemProvider.GetSupportedCodeElementTypes(this.selectedCodeType));
				if (!elementTypes.Any(x => x == selectedElementType))
				{
					SelectedElementType = elementTypes.First();
				}

				return elementTypes;
			}
			set { }
		}

		public CodeElementType SelectedElementType
		{
			get { return this.selectedElementType; }
			set { this.selectedElementType = value; RaisePropertyChanged(nameof(SelectedElementType)); }
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
