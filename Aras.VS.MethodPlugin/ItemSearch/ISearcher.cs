//------------------------------------------------------------------------------
// <copyright file="ISearcher.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public interface ISearcher
	{
		List<ItemSearchResult> RunSearch(string itemType, SavedSearch search);

		List<ItemSearchPropertyInfo> GetPropertiesForSearch(string itemType);

		List<ItemTypeItem> GetItemTypes();

		List<SavedSearch> GetSavedSearches(string itemType);

		int GetPageSize(string itemType);
	}
}
