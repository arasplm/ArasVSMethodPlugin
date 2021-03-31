//------------------------------------------------------------------------------
// <copyright file="PropertyDataTypeParser.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public static class PropertyDataTypeParser
	{
		private static readonly Dictionary<string, PropertyDataType> SupportedTypes = new Dictionary<string, PropertyDataType>()
		{
			{"string", PropertyDataType.String},
			{"text", PropertyDataType.Text},
			{"integer", PropertyDataType.Integer},
			{"float", PropertyDataType.Float},
			{"decimal", PropertyDataType.Decimal},
			{"boolean", PropertyDataType.Boolean},
			{"date", PropertyDataType.Date},
			{"item", PropertyDataType.Item},
			{"list", PropertyDataType.List},
			{"filter list", PropertyDataType.FilterList},
			{"foreign", PropertyDataType.Foreign},
			{"color", PropertyDataType.Color},
			{"color list", PropertyDataType.ColorList}
		};

		public static PropertyDataType ParseDataType(string value)
		{
			PropertyDataType res;
			if (!SupportedTypes.TryGetValue(value, out res))
			{
				res = PropertyDataType.String;
			}

			return res;
		}
	}
}
