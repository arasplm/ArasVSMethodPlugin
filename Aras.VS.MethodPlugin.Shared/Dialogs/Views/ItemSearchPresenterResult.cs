﻿//------------------------------------------------------------------------------
// <copyright file="ItemSearchPresenterResult.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows.Forms;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;

namespace OfficeConnector.Dialogs
{
	public class ItemSearchPresenterResult
	{
		public string ItemId { get; set; }

		public string ItemType { get; set; }

		public DialogResult DialogResult { get; set; }

		public List<PropertyInfo> LastSavedSearch { get; set; }

		public EventSpecificDataType EventData { get; set; }
	}
}
