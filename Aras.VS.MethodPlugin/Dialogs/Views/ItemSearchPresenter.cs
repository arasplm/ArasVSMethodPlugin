//------------------------------------------------------------------------------
// <copyright file="ItemSearchPresenter.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ItemSearch;

namespace OfficeConnector.Dialogs
{
	public class ItemSearchPresenter : BasePresener<IItemSearchView, ItemSearchPresenterArgs, ItemSearchPresenterResult>
	{
		private ISearcher searcher;

		private SavedSearch currentSearch = new SavedSearch();
		public SavedSearch CurrentSearch { get { return currentSearch; } }

		private ItemTypeItem selectedType;
		private List<string> selectedIds = new List<string>();
		private ItemSearchPresenterArgs searchArguments;
		
		public ItemSearchPresenter(
			IItemSearchView view,
			ISearcher searcher) : base(view)
		{
			this.searcher = searcher;
		}

		public override ItemSearchPresenterResult Run(ItemSearchPresenterArgs itemSearchPresenterArgs)
		{
			this.searchArguments = itemSearchPresenterArgs;
			View.SetViewInfo(itemSearchPresenterArgs.Title);

			List<ItemTypeItem> itemTypes;
			if (!string.IsNullOrEmpty(searchArguments.ItemTypeName))
			{
				itemTypes = new List<ItemTypeItem>() { new ItemTypeItem() { itemTypeName = searchArguments.ItemTypeName, itemTypeSingularLabel = searchArguments.ItemTypeSingularLabel } };
			}
			else
			{
				itemTypes = searcher.GetItemTypes();
				searchArguments.ItemTypeName = itemTypes.First().itemTypeName;
			}

			selectedType = itemTypes.First(it => it.itemTypeName == searchArguments.ItemTypeName);
			View.SetItemTypes(itemTypes, selectedType);

			LoadSearchSettings();

			var savedSearches = searcher.GetSavedSearches(selectedType.itemTypeName);
			View.SetSavedSearch(savedSearches);
			SetSearchColumns(selectedType, searchArguments.PredefinedPropertyValues);

			View.SearchValueChanged += View_SearchValueChanged;
			View.ItemTypeChanged += View_ItemTypeChanged;
			View.PageSizeChanged += View_PageSizeChanged;
			View.SavedSearchChanged += View_SavedSearchChanged;
			View.SelectedItemChanged += View_SelectedItemChanged;
			View.RunSearch += View_RunSearch;
			View.ClearSearch += View_ClearSearch;
			View.Cancel += View_Cancel;
			View.Ok += View_Ok;
			View.NextPage += View_NextPage;
			View.PreviousPage += View_PreviousPage;

			View.ShowAsDialog();

			return new ItemSearchPresenterResult()
			{
				ItemId = selectedIds.First(),
				ItemType = selectedType.itemTypeName,
				DialogResult = View.DialogResult,
				LastSavedSearch = this.currentSearch.SavedSearchProperties.Cast<PropertyInfo>().ToList(),
			};
		}

		private void View_Ok()
		{
			View.DialogResult = DialogResult.OK;
		}

		#region View Event Handlers

		private void View_SelectedItemChanged(List<string> selectedIds)
		{
			this.selectedIds = selectedIds;
		}

		private void View_PreviousPage()
		{
			if (currentSearch.Page > 1)
			{
				currentSearch.Page--;
			}

			RunSearchAndSetFoundedItems(selectedType.itemTypeName, currentSearch);
		}

		private void View_NextPage()
		{
			currentSearch.Page++;
			RunSearchAndSetFoundedItems(selectedType.itemTypeName, currentSearch);
		}

		private void View_SavedSearchChanged(SavedSearch savedSearch)
		{
			if (savedSearch.SavedSearchProperties.Count > 0)
			{
				ClearSearchCriteria();
				foreach (var prop in savedSearch.SavedSearchProperties)
				{
					var currentProperty = currentSearch.SavedSearchProperties.FirstOrDefault(p => p.PropertyName == prop.PropertyName && !p.IsReadonly);
					if (currentProperty != null)
					{
						currentProperty.PropertyValue = prop.PropertyValue;
					}
				}
			}

			currentSearch.Page = 1;
			currentSearch.PageMax = 1;

			View.SetSearchInfo(currentSearch.PageSize, currentSearch.Page, currentSearch.PageMax);
			View.SetSearchColumns(currentSearch.SavedSearchProperties);
		}

		private void View_PageSizeChanged(string size)
		{
			currentSearch.PageSize = size;
		}

		private void View_ClearSearch()
		{
			ClearSearchCriteria();

			View.SetSearchColumns(currentSearch.SavedSearchProperties);
		}

		private void View_SearchValueChanged(int propertyIndex, string changedValue)
		{
			currentSearch.SavedSearchProperties[propertyIndex].PropertyValue = changedValue;
		}

		private void View_ItemTypeChanged(ItemTypeItem itemType)
		{
			selectedType = itemType;

			LoadSearchSettings();

			var savedSearches = searcher.GetSavedSearches(itemType.itemTypeName);
			View.SetSavedSearch(savedSearches);

			SetSearchColumns(itemType, searchArguments.PredefinedPropertyValues);
		}

		private void View_RunSearch()
		{
			RunSearchAndSetFoundedItems(selectedType.itemTypeName, currentSearch);
		}

		private void View_Cancel()
		{
			View.DialogResult = DialogResult.Cancel;
		}

		#endregion View Event Handlers

		#region Private Methods

		private void SetSearchColumns(ItemTypeItem itemType, List<PropertyInfo> predefinedPropertyValues)
		{
			//get properties by item type and  Apply layout settings
			var properiesForSearch = searcher.GetPropertiesForSearch(itemType.itemTypeName);

			//update by PredefinedPropertyValues
			foreach (var prop in predefinedPropertyValues)
			{
				var foundedProperty = properiesForSearch.FirstOrDefault(pi => pi.PropertyName == prop.PropertyName);
				if (foundedProperty != null)
				{
					foundedProperty.PropertyValue = prop.PropertyValue;
					foundedProperty.IsReadonly = prop.IsReadonly;
				}
			}

			currentSearch.SavedSearchProperties = properiesForSearch;
			View.SetSearchColumns(currentSearch.SavedSearchProperties);
		}

		private void RunSearchAndSetFoundedItems(string itemType, SavedSearch savedSearch)
		{
			int size;
			if (!string.IsNullOrEmpty(currentSearch.PageSize) &&
				(!int.TryParse(currentSearch.PageSize, out size) ||
				size <= 0))
			{
				//messageHelper.Show(
				//	resourceManager.GetResources(ResourceName.Resources)["ThePageSizeShouldBeAPositiveInteger"],
				//	View,
				//	resourceManager.GetResources(ResourceName.Resources)["messageBoxTitle"], MessageButtons.OK, MessageIcon.Error);

				savedSearch.PageSize = string.Empty;
				savedSearch.Page = 1;
				savedSearch.PageMax = 1;
			}
			else
			{
				var foundedItems = searcher.RunSearch(itemType, savedSearch);
				View.SetSearchResult(foundedItems);
			}

			View.SetSearchInfo(savedSearch.PageSize, savedSearch.Page, savedSearch.PageMax);
		}

		private void ClearSearchCriteria()
		{
			foreach (var item in currentSearch.SavedSearchProperties)
			{
				if (!item.IsReadonly)
				{
					item.PropertyValue = null;
				}
			}
		}

		private void LoadSearchSettings()
		{
			int size = searcher.GetPageSize(selectedType.itemTypeName);
			if (size <= 0)
			{
				size = 25;
			}

			this.currentSearch.Page = 1;
			this.currentSearch.PageMax = 1;
			this.currentSearch.PageSize = size.ToString();

			View.SetSearchInfo(currentSearch.PageSize, currentSearch.Page, currentSearch.PageMax);
		}

		

		#endregion Private Methods
	}
}
