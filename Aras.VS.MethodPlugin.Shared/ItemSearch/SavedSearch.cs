//------------------------------------------------------------------------------
// <copyright file="SavedSearch.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class SavedSearch
	{
		public SavedSearch()
		{
			SavedSearchProperties = new List<ItemSearchPropertyInfo>();
		}

		public string SearchId { get; set; }

		public string SearchName { get; set; }

		public string ItemName { get; set; }

		public List<ItemSearchPropertyInfo> SavedSearchProperties { get; set; }

		public int Page { get; set; }

		public int PageMax { get; set; }

		public string PageSize { get; set; }

		public override string ToString()
		{
			return SearchName != null ? SearchName : string.Empty;
		}
	}
}
