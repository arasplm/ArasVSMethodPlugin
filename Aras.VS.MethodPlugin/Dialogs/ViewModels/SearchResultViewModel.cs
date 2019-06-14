//------------------------------------------------------------------------------
// <copyright file="SearchResultViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using Aras.VS.MethodPlugin.ItemSearch;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SearchResultViewModel: BaseViewModel
	{
		private SavedSearch savedSearch;

		private ObservableCollection<ItemSearchPropertyInfo> foundedItems;

		public ObservableCollection<ItemSearchPropertyInfo> FoundedItems 
		{
			get { return foundedItems; }
			set { foundedItems = value; RaisePropertyChanged(nameof(FoundedItems)); }
		}

	}
}
