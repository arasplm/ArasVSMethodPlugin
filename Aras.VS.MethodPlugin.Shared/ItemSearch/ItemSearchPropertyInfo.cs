//------------------------------------------------------------------------------
// <copyright file="ItemSearchPropertyInfo.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemSearchPropertyInfo : PropertyInfo
	{
		public ItemSearchPropertyInfo()
		{

		}

		public bool IsHidden { get; set; }

		public PropertyDataType DataType { get; set; }

		public string Id { get; set; }

		public string Label { get; set; }

		public string DataSource { get; set; }

		public int Width { get; set; }

		public string Alignment { get; set; }

		public string Pattern { get; set; }
	}
}
