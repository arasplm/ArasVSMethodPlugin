//------------------------------------------------------------------------------
// <copyright file="ColorListPropertyInfo.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ColorListPropertyInfo : ItemSearchPropertyInfo
	{
		public Dictionary<string, string> ColorSource { get; set; }
	}
}
