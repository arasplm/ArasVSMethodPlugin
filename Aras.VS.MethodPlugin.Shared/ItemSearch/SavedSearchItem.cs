//------------------------------------------------------------------------------
// <copyright file="SavedSearchItem.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class SavedSearchItem
	{
		public KeyValuePair<string, string> arasSavedSearch;
		public override string ToString()
		{
			return arasSavedSearch.Value;
		}
	}
}
