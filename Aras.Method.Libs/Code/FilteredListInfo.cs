//------------------------------------------------------------------------------
// <copyright file="FilteredListInfo.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Code
{
	public class FilteredListInfo : ListInfo
	{
		public FilteredListInfo()
		{
			this.Filter = string.Empty;
		}

		public FilteredListInfo(string value, string label, string filter) : base(value, label)
		{
			this.Filter = filter;
		}

		public string Filter { get; private set; }
	}
}
