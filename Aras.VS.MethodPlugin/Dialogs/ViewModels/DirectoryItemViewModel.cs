//------------------------------------------------------------------------------
// <copyright file="DirectoryItemViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Dialogs.Directory;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class DirectoryItemViewModel : BaseViewModel
	{
		private DirectoryItemType type;
		private DirectoryItemType searchToLevel;

		private bool isSelected;

		private DirectoryItemViewModel parent;
		private ObservableCollection<DirectoryItemViewModel> children;

		public event Action<DirectoryItemViewModel> SelectDirectoryItem;

		public DirectoryItemViewModel(string fullPath, DirectoryItemType type, DirectoryItemType searchToLevel)
		{
			this.FullPath = fullPath;
			this.Type = type;
			this.searchToLevel = searchToLevel;
			this.ExpandCommand = new RelayCommand(Expand);

			this.ClearChildren();
		}

		#region Properties

		public DirectoryItemViewModel Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		public DirectoryItemType Type
		{
			get { return type; }
			set { type = value; }
		}

		private string fullPath;

		public string FullPath
		{
			get { return fullPath; }
			set
			{
				fullPath = value;
				RaisePropertyChanged(nameof(Name));
			}
		}

		public string Name
		{
			get
			{
				return this.Type == DirectoryItemType.Drive ? this.FullPath : DirectoryStructure.GetFileFolderName(this.FullPath);
			}
		}

		public ObservableCollection<DirectoryItemViewModel> Children
		{
			get { return children; }
			set
			{
				children = value;
				RaisePropertyChanged(nameof(Children));
			}
		}

		public bool CanExpand { get { return this.Type != DirectoryItemType.File; } }

		public bool IsExpanded
		{
			get
			{
				return this.Children?.Count(f => f != null) > 0;
			}
			set
			{
				if (value == true)
				{
					Expand();
				}
				else
				{
					this.ClearChildren();
				}

				RaisePropertyChanged(nameof(IsExpanded));
			}
		}

		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				if (isSelected)
				{
					var selectDirectoryItemEvent = SelectDirectoryItem;
					if (selectDirectoryItemEvent != null)
					{
						selectDirectoryItemEvent(this);
					}
				}

				RaisePropertyChanged(nameof(IsSelected));
			}
		}

		public bool RenameIsEnabled
		{
			get { return Type == DirectoryItemType.Folder; }
			set { }
		}

		public bool DeleteIsEnabled
		{
			get { return Type == DirectoryItemType.Folder; }
			set { }
		}

		#endregion

		#region Commands

		public ICommand ExpandCommand { get; set; }

		#endregion

		private void ClearChildren()
		{
			this.Children = new ObservableCollection<DirectoryItemViewModel>();

			if (this.Type != DirectoryItemType.File)
			{
				this.Children.Add(null);
			}
		}

		public void Expand()
		{
			if (this.Type == DirectoryItemType.File)
			{
				return;
			}

			Update();
		}

		public void Update()
		{
			var directoryItems = DirectoryStructure.GetDirectoryContents(this.FullPath);
			var children = new ObservableCollection<DirectoryItemViewModel>();

			foreach (DirectoryItem directoryItem in directoryItems)
			{
				if (directoryItem.Type != DirectoryItemType.Folder && this.searchToLevel == DirectoryItemType.Folder)
				{
					continue;
				}

				var childViewModel = new DirectoryItemViewModel(directoryItem.FullPath, directoryItem.Type, searchToLevel);
				childViewModel.Parent = this;
				childViewModel.SelectDirectoryItem += SelectDirectoryItem;
				children.Add(childViewModel);
			}

			this.Children = children;
		}
	}
}
