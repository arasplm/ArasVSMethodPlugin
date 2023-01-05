//------------------------------------------------------------------------------
// <copyright file="ListPropertyInfo.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ListPropertyInfo : ItemSearchPropertyInfo
	{
		public List<string> ItemsSource { get; set; }
	}
}
