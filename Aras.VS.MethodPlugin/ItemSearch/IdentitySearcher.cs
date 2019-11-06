//------------------------------------------------------------------------------
// <copyright file="IdentitySearcher.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.VS.MethodPlugin.ItemSearch.Preferences;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class DefaultSearcher : BaseSearcher
	{
		private string itemTypeName;
		private string itemTypeSingularLabel;

		public DefaultSearcher(
			dynamic innovatorInstance,
			IPreferenceProvider preferencesProvider,
			ISavedSearchProvider savedSearchProvider,
			string itemTypeName,
			string itemTypeSingularLabel) : base((object)innovatorInstance, preferencesProvider, savedSearchProvider)
		{
			this.itemTypeName = itemTypeName;
			this.itemTypeSingularLabel = itemTypeSingularLabel;
		}

		public override List<ItemTypeItem> GetItemTypes()
		{
			return new List<ItemTypeItem>() { new ItemTypeItem() { itemTypeName = itemTypeName, itemTypeSingularLabel = itemTypeSingularLabel } };
		}
	}
}
