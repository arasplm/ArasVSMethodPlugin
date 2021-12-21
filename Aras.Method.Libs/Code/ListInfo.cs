//------------------------------------------------------------------------------
// <copyright file="ListInfo.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Code
{
	public class ListInfo
	{
		public ListInfo()
		{
			this.Value = string.Empty;
			this.Label = string.Empty;
		}

		public ListInfo(string value, string label)
		{
			this.Value = value;
			this.Label = string.IsNullOrEmpty(label) ? value : label;
		}

		public string Value { get; private set; }

		public string Label { get; private set; }

		public override string ToString()
		{
			return Label;
		}
	}
}
