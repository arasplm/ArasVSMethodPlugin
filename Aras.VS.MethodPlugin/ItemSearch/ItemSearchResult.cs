//------------------------------------------------------------------------------
// <copyright file="ItemSearchResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemSearchResult
	{
		public ItemSearchResult()
		{
			FoundedItems = new List<PropertyInfo>();
		}

		public List<PropertyInfo> FoundedItems { get; set; }
	}
}
