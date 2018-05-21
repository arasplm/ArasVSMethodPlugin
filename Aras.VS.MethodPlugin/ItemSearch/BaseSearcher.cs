//------------------------------------------------------------------------------
// <copyright file="BaseSearcher.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Aras.VS.MethodPlugin.ItemSearch.Preferences;
using Aras.VS.MethodPlugin.Utilities;


namespace Aras.VS.MethodPlugin.ItemSearch
{
	public abstract class BaseSearcher : ISearcher
	{
		protected readonly dynamic innovatorInstance;
		protected readonly IPreferenceProvider preferencesProvider;
		protected readonly ISavedSearchProvider savedSearchProvider;

		public BaseSearcher(dynamic innovatorInstance, IPreferenceProvider preferencesProvider, ISavedSearchProvider savedSearchProvider)
		{
			if (innovatorInstance == null) throw new ArgumentNullException(nameof(innovatorInstance));
			if (preferencesProvider == null) throw new ArgumentNullException(nameof(preferencesProvider));
			if (savedSearchProvider == null) throw new ArgumentNullException(nameof(savedSearchProvider));

			this.innovatorInstance = innovatorInstance;
			this.preferencesProvider = preferencesProvider;
			this.savedSearchProvider = savedSearchProvider;
		}

		public int PageSize { get; set; }

		public abstract List<ItemTypeItem> GetItemTypes();

		public virtual List<PropertyInfo> GetPropertiesForSearch(string itemType)
		{
			List<PropertyInfo> notSortedList = new List<PropertyInfo>();
			List<dynamic> properties = Utils.GetItemTypeProperties(innovatorInstance, itemType);

			foreach (dynamic property in properties)
			{
				//TODO: check this if statesment
				if (string.Equals("1", property.getProperty("is_hidden"))
					 && !string.Equals("locked_by_id", property.getProperty("name")))
				{
					continue;
				}

				var dataType = PropertyDataTypeParser.ParseDataType(property.getProperty("data_type"));
				PropertyInfo propertyInfo;
				if (dataType == PropertyDataType.List)
				{
					var pinfo = new ListPropertyInfo();
					var affectedItem = property.node.GetXmlNodePropertyAttribute("data_source", "keyed_name");
					pinfo.ItemsSource = new List<string>() { string.Empty };
					//TODO: should show lable not a value
					List<dynamic> listValueItems = Utilities.Utils.GetValueListByName(innovatorInstance, affectedItem);
					List<string> listValues = listValueItems.Select(x => x.getProperty("value") as string).ToList();
					pinfo.ItemsSource.AddRange(listValues);

					propertyInfo = pinfo;
				}
				else if (dataType == PropertyDataType.ColorList)
				{
					var pinfo = new ColorListPropertyInfo();
					var affectedItem = property.node.GetXmlNodePropertyAttribute("data_source", "keyed_name");
					List<dynamic> items = Utilities.Utils.GetValueListByName(innovatorInstance, affectedItem);

					var colors = new Dictionary<string, string>() { { string.Empty, string.Empty } };

					string value;
					string label;
					foreach (dynamic item in items)
					{
						value = item.getProperty("value", string.Empty);
						label = item.getProperty("label", value);

						if (!colors.ContainsKey(value))
						{
							colors.Add(value, label);
						}
					}
					pinfo.ColorSource = colors;

					propertyInfo = pinfo;
				}
				else
				{
					propertyInfo = new PropertyInfo();
				}

				propertyInfo.PropertyName = property.getProperty("name");
				propertyInfo.Label = property.getProperty("label");
				if (string.IsNullOrEmpty(propertyInfo.Label))
				{
					propertyInfo.Label = property.getProperty("keyed_name");
				}
				propertyInfo.DataType = dataType;
				propertyInfo.Id = property.getID();
				propertyInfo.Pattern = property.getProperty("pattern");
				string widthString;
				if (propertyInfo.PropertyName == "locked_by_id")
				{
					widthString = property.getProperty("column_width", "24");
				}
				else
				{
					widthString = property.getProperty("column_width", "100");
				}
				propertyInfo.Width = int.Parse(widthString);
				propertyInfo.DataSource = property.getProperty("data_source");
				propertyInfo.Alignment = property.getProperty("column_alignment");
				notSortedList.Add(propertyInfo);
			}
			notSortedList.Add(new PropertyInfo() { PropertyName = "id", IsHidden = true });
			UpdatePropertiesForSearch(notSortedList);
			List<PropertyInfo> resultList = new List<PropertyInfo>();
			var layout = preferencesProvider.GetItemGridLayout(itemType);
			if (layout != null)
			{
				for (int i = 0; i < layout.ColumnOrderList.Count; i++)
				{
					var foundedProperty = notSortedList.FirstOrDefault(p => p.PropertyName == layout.ColumnOrderList[i]);
					if (foundedProperty != null)
					{
						int width;
						if (int.TryParse(layout.ColumnWidthsList[i], out width))
						{
							foundedProperty.Width = width;
						}

						resultList.Add(foundedProperty);
					}
				}
			}

			//Add properties which is not in layoutsettings
			foreach (var item in notSortedList)
			{
				if (!resultList.Contains(item))
				{
					if (item.PropertyName == "locked_by_id")
					{
						resultList.Insert(0, item);
					}
					else
					{
						resultList.Add(item);
					}
				}
			}

			return resultList;
		}

		public virtual List<SavedSearch> GetSavedSearches(string itemType)
		{
			List<SavedSearch> resultSearchList = new List<SavedSearch>();

			var serverSearches = savedSearchProvider.GetSavedSearch(itemType);
			resultSearchList.AddRange(serverSearches);
			if (resultSearchList.Count > 0)
			{
				resultSearchList.Insert(0, new SavedSearch());
			}
			return resultSearchList;
		}

		public List<ItemSearchResult> RunSearch(string itemType, SavedSearch search)
		{
			var itemForSearch = innovatorInstance.newItem(itemType, "get");
			foreach (var prop in search.SavedSearchProperties)
			{
				if (!string.IsNullOrEmpty(prop.PropertyValue) && !string.Equals(prop.PropertyValue, "Indeterminate"))
				{
					if (prop.DataType == PropertyDataType.Item)
					{
						itemForSearch.setProperty(prop.PropertyName, CreateItemSearchProperty(prop.DataSource, prop.PropertyValue));
					}
					else if (prop.DataType == PropertyDataType.Date)
					{
						DateTime date;
						if (DateTime.TryParse(prop.PropertyValue, out date))
						{
							string value = date.ToString("yyyy-MM-dd");

							itemForSearch.setProperty(prop.PropertyName, $"{value}T00:00:00 and {value}T23:59:59");
							itemForSearch.setPropertyAttribute(prop.PropertyName, "condition", "between");
						}
					}
					else
					{
						itemForSearch.setProperty(prop.PropertyName, prop.PropertyValue);
						if (prop.PropertyValue.Contains("*") || prop.PropertyValue.Contains("%"))
						{
							itemForSearch.setPropertyAttribute(prop.PropertyName, "condition", "like");
						}
					}
				}
			}

			itemForSearch.setAttribute("page", search.Page.ToString());
			if (!string.IsNullOrEmpty(search.PageSize))
			{
				itemForSearch.setAttribute("pagesize", search.PageSize.ToString());
			}

			var codes = new Dictionary<string, string>() { { "&gt;", ">" }, { "&lt;", "<" }, };

			string parsedString = itemForSearch.ToString();
			parsedString = codes.Aggregate(parsedString, (current, code) => current.Replace(code.Key, code.Value));

			itemForSearch.loadAML(parsedString);

			UpdateSearch(itemForSearch, itemType, search.SavedSearchProperties);

			itemForSearch = itemForSearch.apply();
			//itemForSearch.IsErrorItem(true);
			List<dynamic> itemForSearchList = new List<dynamic>();
			var itemsCount = itemForSearch.getItemCount();
			for (int i = 0; i < itemsCount; i++)
			{
				var currentItem = itemForSearch.getItemByIndex(i);
				itemForSearchList.Add(currentItem);

			}
			UpdateFoundedItems(itemForSearchList);
			var resultSearchList = new List<ItemSearchResult>();

			for (int i = 0; i < itemForSearchList.Count; i++)
			{
				var currentItem = itemForSearchList[i];
				ItemSearchResult searchResult = new ItemSearchResult();
				foreach (var item in search.SavedSearchProperties)
				{
					var value = currentItem.getProperty(item.PropertyName);
					PropertyInfo foundedPropertyInfo;
					if (item.PropertyName == "locked_by_id")
					{
						var lockedPropertyInfo = new LockedByPropertyInfo();
						lockedPropertyInfo.IsLocked = !string.IsNullOrEmpty(value);
						lockedPropertyInfo.IsLockedByMe = string.Equals(value, innovatorInstance.getUserID());
						lockedPropertyInfo.PropertyName = item.PropertyName;
						foundedPropertyInfo = lockedPropertyInfo;
					}
					else if (item.DataType == PropertyDataType.ColorList)
					{
						var colorProp = item as ColorListPropertyInfo;
						foundedPropertyInfo = new ColorListPropertyInfo()
						{
							PropertyName = item.PropertyName,
							PropertyValue = value,
							DataType = item.DataType,
							ColorSource = colorProp.ColorSource

						};
					}
					else if (item.DataType == PropertyDataType.Item)
					{
						var label = currentItem.getPropertyAttribute(item.PropertyName, "keyed_name", string.Empty);

						foundedPropertyInfo = new PropertyInfo()
						{
							PropertyName = item.PropertyName,
							PropertyValue = value,
							Label = label,
							DataType = item.DataType
						};
					}
					else
					{
						foundedPropertyInfo = new PropertyInfo()
						{
							PropertyName = item.PropertyName,
							PropertyValue = value,
							DataType = item.DataType
						};
					}

					searchResult.FoundedItems.Add(foundedPropertyInfo);
				}

				resultSearchList.Add(searchResult);
			}

			if (resultSearchList.Count > 0)
			{
				var firstItem = itemForSearch.getItemByIndex(0);
				var page = firstItem.getAttribute("page", "1");
				var pageMax = firstItem.getAttribute("pagemax", "1");

				int value;
				search.Page = int.TryParse(page, out value) ? value : 1;
				search.PageMax = int.TryParse(pageMax, out value) ? value : 1;
			}
			else
			{
				search.Page = 1;
				search.PageMax = 1;
			}

			return resultSearchList;
		}

		protected virtual void UpdateSearch(dynamic defaultItemForSearch, string itemType, List<PropertyInfo> propertyForSearch)
		{
		}

		protected virtual void UpdatePropertiesForSearch(List<PropertyInfo> properties)
		{
		}

		protected virtual void UpdateFoundedItems(List<dynamic> foundedItems)
		{

		}

		private string CreateItemSearchProperty(string type, string valToSearch)
		{
			var innerElement = innovatorInstance.newItem();
			innerElement.setAttribute("type", type);
			innerElement.setAttribute("action", "get");
			innerElement.setProperty("keyed_name", valToSearch);
			if (valToSearch.Contains("*") || valToSearch.Contains("%"))
			{
				innerElement.setPropertyAttribute("keyed_name", "condition", "like");
			}
			return innerElement.dom.InnerXml;
		}

		public int GetPageSize(string itemType)
		{
			int pageSize = -1;

			var layout = preferencesProvider.GetItemGridLayout(itemType);
			if (layout != null)
			{
				int i;
				if (int.TryParse(layout.PageSize, out i))
				{
					pageSize = i;
				}
			}

			return pageSize;
		}
	}
}
