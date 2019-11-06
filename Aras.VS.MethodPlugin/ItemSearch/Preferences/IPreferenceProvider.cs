//------------------------------------------------------------------------------
// <copyright file="IPreferenceProvider.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.ItemSearch.Preferences
{
	public interface IPreferenceProvider
	{
		ItemGridLayout GetItemGridLayout(string itemTypeName);
	}
}
