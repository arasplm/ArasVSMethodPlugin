//------------------------------------------------------------------------------
// <copyright file="ItemSearchResult.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemSearchResult
	{
		public ItemSearchResult()
		{
			FoundedItems = new List<ItemSearchPropertyInfo>();
		}

		public List<ItemSearchPropertyInfo> FoundedItems { get; set; }
	}
}
