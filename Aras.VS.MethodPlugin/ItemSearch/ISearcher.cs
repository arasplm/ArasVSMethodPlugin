//------------------------------------------------------------------------------
// <copyright file="ISearcher.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public interface ISearcher
	{
		List<ItemSearchResult> RunSearch(string itemType, SavedSearch search);

		List<PropertyInfo> GetPropertiesForSearch(string itemType);

		List<ItemTypeItem> GetItemTypes();

		List<SavedSearch> GetSavedSearches(string itemType);

		int GetPageSize(string itemType);
	}
}
