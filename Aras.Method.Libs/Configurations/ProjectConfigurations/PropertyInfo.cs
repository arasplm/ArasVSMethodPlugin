//------------------------------------------------------------------------------
// <copyright file="PropertyInfo.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class PropertyInfo
	{
		public PropertyInfo()
		{

		}

		public string PropertyName { get; set; }

		public string PropertyValue { get; set; }

		public bool IsReadonly { get; set; }
	}
}
