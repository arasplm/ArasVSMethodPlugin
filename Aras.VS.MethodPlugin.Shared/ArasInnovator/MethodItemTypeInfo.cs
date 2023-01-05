//------------------------------------------------------------------------------
// <copyright file="MethodItemTypeInfo.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Xml;
using Aras.Method.Libs;

namespace Aras.VS.MethodPlugin.ArasInnovator
{
	public class MethodItemTypeInfo
	{
		private readonly dynamic methodItemTypeItem;
		private readonly MessageManager messageManager;

		public MethodItemTypeInfo(dynamic methodItemTypeItem, MessageManager messageManager)
		{
			if (methodItemTypeItem == null)
			{
				throw new ArgumentNullException(nameof(methodItemTypeItem));
			}
			if (messageManager == null)
			{
				throw new ArgumentNullException(nameof(messageManager));
			}

			this.methodItemTypeItem = methodItemTypeItem;
			this.messageManager = messageManager;
		}

		public int NameStoredLength
		{
			get
			{
				XmlNode propertyItem = this.methodItemTypeItem.dom.SelectSingleNode("//Relationships//Item[@type='Property' and name='name']");
				if (propertyItem == null)
				{
					throw new Exception(messageManager.GetMessage("PropertyInTheItemTypeNotFound", "name", "Method"));
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
					throw new Exception(messageManager.GetMessage("PropertyInTheItemTypeNotFound", "comments", "Method"));
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
