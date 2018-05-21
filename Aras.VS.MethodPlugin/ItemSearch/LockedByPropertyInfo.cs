//------------------------------------------------------------------------------
// <copyright file="LockedByPropertyInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class LockedByPropertyInfo : PropertyInfo
	{
		public bool IsLocked { get; set; }

		public bool IsLockedByMe { get; set; }
	}
}
