//------------------------------------------------------------------------------
// <copyright file="LockedByPropertyInfo.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class LockedByPropertyInfo : ItemSearchPropertyInfo
	{
		public bool IsLocked { get; set; }

		public bool IsLockedByMe { get; set; }
	}
}
