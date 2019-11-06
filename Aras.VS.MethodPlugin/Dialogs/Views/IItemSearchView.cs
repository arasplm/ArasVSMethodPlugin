//------------------------------------------------------------------------------
// <copyright file="IItemSearchView.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Aras.VS.MethodPlugin.ItemSearch;

namespace OfficeConnector.Dialogs
{
	public interface IItemSearchView : IView
	{
		event Action<int, string> SearchValueChanged;

		event Action<ItemTypeItem> ItemTypeChanged;

		event Action<string> PageSizeChanged;

		event Action<SavedSearch> SavedSearchChanged;

		event Action<List<string>> SelectedItemChanged;

		event Action RunSearch;

		event Action ClearSearch;

		event Action Cancel;

		event Action Ok;

		event Action NextPage;

		event Action PreviousPage;

		void SetViewInfo(string title);

		void SetItemTypes(List<ItemTypeItem> itemTypes, ItemTypeItem selectedItem);

		void SetSearchInfo(string pageSize, int page, int pageMax);

		void SetSavedSearch(List<SavedSearch> savedSearches);

		void SetSearchColumns(List<ItemSearchPropertyInfo> searchColumnList);

		void SetSearchResult(List<ItemSearchResult> searchResults);
	}
}
