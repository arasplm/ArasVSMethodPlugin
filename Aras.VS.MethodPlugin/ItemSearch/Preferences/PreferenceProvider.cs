//------------------------------------------------------------------------------
// <copyright file="PreferenceProvider.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aras.VS.MethodPlugin.ItemSearch.Preferences
{
	public class PreferenceProvider : IPreferenceProvider
	{
		private readonly dynamic innovatorInstance;

		private List<ItemGridLayout> itemGridLayouts = new List<ItemGridLayout>();
		private List<dynamic> itemTypesInstances = new List<dynamic>();

		public PreferenceProvider(dynamic innovatorInstance)
		{
			if (innovatorInstance == null) throw new ArgumentNullException(nameof(innovatorInstance));

			this.innovatorInstance = innovatorInstance;
		}

		public dynamic GetItemType(string itemTypeName)
		{
			dynamic property = innovatorInstance.newItem("Property", "get");
			property.setAttribute("orderBy", "sort_order");

			dynamic item = innovatorInstance.newItem("ItemType", "get");
			item.setProperty("keyed_name", itemTypeName);
			item.addRelationship(property);
			item = item.apply();

			return item;
		}

		public ItemGridLayout GetItemGridLayout(string itemTypeName)
		{
			dynamic itemType = GetItemType(itemTypeName);

			ItemGridLayout layout = itemGridLayouts.FirstOrDefault(x => x.ItemTypeId == itemType.getID());
			if (layout == null && !itemType.isError())
			{
				dynamic item = this.GetLayoutPreference(itemType.getID());
				layout = new ItemGridLayout(item);
				ValidateLayout(layout, itemType);

				itemGridLayouts.Add(layout);
			}

			return layout;
		}

		private dynamic GetLayoutPreference(string itemTypeId)
		{
			dynamic layout;
			dynamic preference = innovatorInstance.newItem("Preference", "get");
			var aliasIdentity = GetAliasIdentity();
			//TODO: check is alias identity
			preference.setProperty("identity_id", aliasIdentity);
			preference = preference.apply();
			if (!preference.isError())
			{
				string preferenceId = preference.getID();

				dynamic item = innovatorInstance.newItem("Core_ItemGridLayout", "get");
				item.setProperty("item_type_id", itemTypeId);
				item.setProperty("source_id", preferenceId);
				layout = item.apply();

				if (layout.isEmpty())
				{
					item.setAction("add");
					layout = item;
				}
			}
			else
			{
				layout = innovatorInstance.newItem();
				layout.setProperty("item_type_id", itemTypeId);
			}

			return layout;
		}

		private void ValidateLayout(ItemGridLayout layout, dynamic itemType)
		{
			if (layout.ColumnOrderList.Count > 1 && layout.ColumnOrderList.Count == layout.ColumnWidthsList.Count)
			{
				return;
			}

			layout.UpdateLayout(itemType);
		}

		private string GetAliasIdentity()
		{
			string userId = innovatorInstance.getUserID();
			dynamic user = innovatorInstance.newItem("User", "get");
			user.setAttribute("id", userId);
			user.setAttribute("levels", "1");
			user = user.apply();

			var keyedName = user.getPropertyAttribute("id", "keyed_name");
			var userName = user.getProperty("login_name");


			string selector = "Relationships/Item[@type=\'Alias\']/related_id/Item[@type=\'Identity\']/id";
			string identityId = user.node.SelectSingleNode(selector)?.InnerText;

			if (!string.IsNullOrEmpty(identityId))
			{
				return identityId;
			}
			else
			{
				return userId;
			}
		}
	}
}
