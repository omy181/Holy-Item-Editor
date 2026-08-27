using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Holylib.ItemEditor
{
    public static class ItemManagement
    {
        public static List<StaticItemData> GetAListOfItems()
        {
            var guids = AssetDatabase.FindAssets("t:StaticItemData");
            var originalList = new List<StaticItemData>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<StaticItemData>(path);
                if (item != null)
                {
                    originalList.Add(item);
                }
            }

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