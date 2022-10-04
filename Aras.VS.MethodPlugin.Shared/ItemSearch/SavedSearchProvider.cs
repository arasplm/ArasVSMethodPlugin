//------------------------------------------------------------------------------
// <copyright file="SavedSearchProvider.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class SavedSearchProvider : ISavedSearchProvider
	{
		private readonly dynamic innovatorInstance;
		private readonly Dictionary<string, List<SavedSearch>> cachedSavedSearches = new Dictionary<string, List<SavedSearch>>();

		public SavedSearchProvider(dynamic innovatorInstance)
		{
			if (innovatorInstance == null) throw new ArgumentNullException(nameof(innovatorInstance));

			this.innovatorInstance = innovatorInstance;
		}

		public List<SavedSearch> GetSavedSearch(string itemTypeName)
		{
			List<SavedSearch> resultList;

			if (!cachedSavedSearches.TryGetValue(itemTypeName, out resultList))
			{
				resultList = new List<SavedSearch>();
				dynamic savedSearches = innovatorInstance.newItem();
				savedSearches.loadAML(string.Format(@"
				<AML>
					<Item type=""SavedSearch"" action=""get"">
						<itname>{0}</itname>
						<auto_saved>0</auto_saved>
					</Item>
				</AML>", itemTypeName));
				savedSearches = savedSearches.apply();

				var searchesCount = savedSearches.getItemCount();
				for (int i = 0; i < searchesCount; i++)
				{
					var search = new SavedSearch();
					
					var savedSearch = savedSearches.getItemByIndex(i);
					search.SearchId = savedSearch.getID();
					search.SearchName = savedSearch.getProperty("label");
					search.ItemName = itemTypeName;

					string criteria = savedSearch.getProperty("criteria");
					if (!string.IsNullOrEmpty(criteria))
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml(criteria);
						var properties = doc.FirstChild.ChildNodes;
						foreach (XmlNode prop in properties)
						{
							var propertyInfo = new ItemSearchPropertyInfo();
							propertyInfo.PropertyName = prop.Name;
							propertyInfo.PropertyValue = prop.InnerText;
							search.SavedSearchProperties.Add(propertyInfo);
						}
					}
					
					resultList.Add(search);
				}
				cachedSavedSearches.Add(itemTypeName, resultList);
			}

			return resultList;
		}
	}
}
