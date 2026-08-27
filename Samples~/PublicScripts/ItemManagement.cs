using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class ItemManagement
    {
        public static List<StaticItemData> GetAListOfItems()
        {
            var itemsInAssets = Resources.FindObjectsOfTypeAll(typeof(StaticItemData)) as StaticItemData[];
            var originalList = itemsInAssets.ToList();
            return originalList;
        }

        public static List<ItemListElement> GetItemsAsListElements(List<StaticItemData> originalList)
        {
            List<ItemListElement> newList = new();
            foreach (var item in originalList)
            {
                newList.Add(new(item.ID, item.Name, item.Sprite));
            }

            return newList;
        }

        public static List<ItemListElement> GetAllListElements()
        {
            return GetItemsAsListElements(GetAListOfItems());
        }

        public static StaticItemData ListElementToItemData(ItemListElement listElement)
        {
            return GetAListOfItems().Find(i => i.ID == listElement.ID);
        }
    }
}