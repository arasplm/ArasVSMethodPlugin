//------------------------------------------------------------------------------
// <copyright file="ISavedSearchProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public interface ISavedSearchProvider
	{
		List<SavedSearch> GetSavedSearch(string itemTypeName);
	}
}
