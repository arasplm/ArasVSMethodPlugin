//------------------------------------------------------------------------------
// <copyright file="ItemTypeItem.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Aras.VS.MethodPlugin.ItemSearch
{
	public class ItemTypeItem : IEquatable<ItemTypeItem>
	{
		public string itemTypeName;
		public string itemTypeSingularLabel;

		public override string ToString()
		{
			return itemTypeSingularLabel;
		}

		public bool Equals(ItemTypeItem other)
		{

			//Check whether the compared object is null. 
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			//Check whether the compared object references the same data. 
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			//Check whether the products' properties are equal. 
			return itemTypeName.Equals(other.itemTypeName) && itemTypeSingularLabel.Equals(other.itemTypeSingularLabel);
		}
		// If Equals() returns true for a pair of objects  
		// then GetHashCode() must return the same value for these objects. 

		public override int GetHashCode()
		{
			//Get hash code for the Name field if it is not null. 
			int hashItemTypeName = itemTypeName?.GetHashCode() ?? 0;

			//Get hash code for the Code field. 
			int hashItemTypeSingularLabel = itemTypeSingularLabel?.GetHashCode() ?? 0;

			//Calculate the hash code for the product. 
			return hashItemTypeName ^ hashItemTypeSingularLabel;
		}
	}
}
