//------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Utilities
{
	public static class Utils
	{
		public static Dictionary<string, string> dateFormatDictionary = new Dictionary<string, string>()
		{
			{"short_date", "MM/dd/yyyy"},
			{"short_date_time", "MM/dd/yyyy hh:mm tt"},
			{"long_date", "ddd, MMM dd, yyyy"},
			{"long_date_time", "ddd, MMM dd, yyyy hh:mm tt"},
			{"", "MM/dd/yyyy"}
		};

		public static List<dynamic> GetItemTypeProperties(dynamic innovatorInstanse, string itemTypeName)
		{
			// We should retrieve all item properties regardless of "is_hidden" property's value
			// But we should visualize only properties which is marked by "is_hidden" = false
			var queryAml = string.Format(@"
																	<AML>
																		<Item type=""Property"" action=""get"" page=""1"" orderBy=""sort_order"">
																			<source_id>
																				<Item type=""ItemType"" action=""get"">
																					<keyed_name condition=""eq"">{0}</keyed_name>
																				</Item>
																			</source_id>
																		</Item>
																	</AML>", itemTypeName);

			dynamic props = innovatorInstanse.newItem();
			props.loadAML(queryAml);
			props = props.apply();

			//props.IsErrorItem(true);
			
			var res = new List<dynamic>();
			for (int i = 0; i < props.getItemCount(); ++i)
			{
				res.Add(props.getItemByIndex(i));
			}
			return res;
		}

		public static List<dynamic> GetValueListByName(dynamic innovatorInstance, string listName, string filter = null, string value = null)
		{
			return GetListByName(innovatorInstance, "Value", listName, filter, value);
		}

		public static List<dynamic> GetFilterValueListByName(dynamic innovatorInstance, string listName, string filter = null, string value = null)
		{
			if (string.IsNullOrEmpty(listName))
			{
				return null;
			}
			return GetListByName(innovatorInstance, "Filter Value", listName, filter, value);
		}

		public static List<dynamic> GetListByName(dynamic innovatorInstance, string listType, string listName, string filter = null, string value = null)
		{
			dynamic items = innovatorInstance.newItem(listType);
			items.setAttribute("select", "label, value, filter");
			dynamic itemList = innovatorInstance.newItem("List");
			itemList.setProperty("keyed_name", listName);
			items.setPropertyItem("source_id", itemList);
			if (!string.IsNullOrEmpty(filter))
			{
				items.setProperty("filter", filter);
			}
			if (!string.IsNullOrEmpty(value))
			{
				items.setProperty("value", value);
			}
			items = items.apply("get");
			//items.IsErrorItem(true);
			return ToList(items);
		}

		public static List<dynamic> ToList(dynamic multiItem)
		{
			List<dynamic> items = new List<dynamic>(multiItem.getItemCount());
			for (int i = 0; i < multiItem.getItemCount(); i++)
			{
				items.Add(multiItem.getItemByIndex(i));
			}
			return items;
		}
	}
}
