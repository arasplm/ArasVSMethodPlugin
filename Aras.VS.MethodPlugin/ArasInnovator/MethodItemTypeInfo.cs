//------------------------------------------------------------------------------
// <copyright file="MethodItemTypeInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Xml;

namespace Aras.VS.MethodPlugin.ArasInnovator
{
	public class MethodItemTypeInfo
	{
		private readonly dynamic methodItemTypeItem;

		public MethodItemTypeInfo(dynamic methodItemTypeItem)
		{
			if (methodItemTypeItem == null)
			{
				throw new ArgumentNullException(nameof(methodItemTypeItem));
			}

			this.methodItemTypeItem = methodItemTypeItem;
		}

		public int NameStoredLength
		{
			get
			{
				XmlNode propertyItem = this.methodItemTypeItem.dom.SelectSingleNode("//Relationships//Item[@type='Property' and name='name']");
				if (propertyItem == null)
				{
					throw new Exception("'name' property in the Method ItemType not found.");
				}

				// if stored_length is not set and number is more then 32 simbols sql exception will be throw
				int stored_length = 32;
				string length = propertyItem.SelectSingleNode("stored_length")?.InnerText;

				int i;
				if (int.TryParse(length, out i))
				{
					stored_length = i;
				}

				return stored_length;
			}
		}

		public int CommentsStoredLength
		{
			get
			{
				XmlNode propertyItem = this.methodItemTypeItem.dom.SelectSingleNode("//Relationships//Item[@type='Property' and name='comments']");
				if (propertyItem == null)
				{
					throw new Exception("'comments' property in the Method ItemType not found.");
				}

				// if stored_length is not set and number is more then 32 simbols sql exception will be throw
				int stored_length = 32;
				string length = propertyItem.SelectSingleNode("stored_length")?.InnerText;

				int i;
				if (int.TryParse(length, out i))
				{
					stored_length = i;
				}

				return stored_length;
			}
		}
	}
}
