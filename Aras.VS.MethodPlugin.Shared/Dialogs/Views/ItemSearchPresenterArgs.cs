//------------------------------------------------------------------------------
// <copyright file="ItemSearchPresenterArgs.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ItemSearch;

namespace OfficeConnector.Dialogs
{
	public class ItemSearchPresenterArgs
	{
		public ItemSearchPresenterArgs()
		{
			PredefinedPropertyValues = new List<PropertyInfo>();
		}

		public string ItemTypeName { get; set; }

		public string ItemTypeSingularLabel { get; set; }

		public string ItemTypeConfigId { get; set; }

		public List<PropertyInfo> PredefinedPropertyValues { get; set; }

		public string Title { get; set; }
	}
}
