//------------------------------------------------------------------------------
// <copyright file="IUpdatedCell.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Drawing;

namespace OfficeConnector.PropertyDataGrid
{
	internal interface IUpdatedCell
	{
		void Update(Rectangle cellBounds,bool displayed, bool isHeaderCell);
	}
}