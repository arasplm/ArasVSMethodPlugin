//------------------------------------------------------------------------------
// <copyright file="PropertyInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class PropertyInfo
	{
		public PropertyInfo()
		{
		}

		public bool IsReadonly { get; set; }

		public bool IsHidden { get; set; }

		public string PropertyName { get; set; }

		public string PropertyValue { get; set; }

		public PropertyDataType DataType { get; set; }

		public string Id { get; set; }

		public string Label { get; set; }

		public string DataSource { get; set; }

		public int Width { get; set; }

		public string Alignment { get; set; }

		public string Pattern { get; set; }
	}
}
