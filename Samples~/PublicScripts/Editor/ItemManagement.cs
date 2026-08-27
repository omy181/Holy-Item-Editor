using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Holylib.ItemEditor
{
    public static class ItemManagement
    {
        private static readonly string[] SearchFolders = { "Assets" };

        private static List<StaticItemData> _itemCache = new();

        public static List<StaticItemData> GetAListOfItems()
        {
            return _itemCache;
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

        public static void RefreshItemsCache()
        {
            var guids = AssetDatabase.FindAssets("t:StaticItemData", SearchFolders);
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

            _itemCache = originalList;
        }
    }
}