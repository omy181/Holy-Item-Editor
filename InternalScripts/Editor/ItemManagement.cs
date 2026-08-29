#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class ItemManagement
    {
        private static List<ItemListElement> _itemCache = new();

        public static List<ItemListElement> GetAListOfAllItems()
        {
            return _itemCache;
        }
        public static void RefreshItemsCache(ItemListElementAndPath[] supportedItemListTypes, string[] searchFolders)
        {
            var combined = new List<ItemListElement>();

            var openMethod = typeof(ItemManagement).GetMethod(
                nameof(_refreshItemsCache),
                BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var type in supportedItemListTypes)
            {
                var closedMethod = openMethod.MakeGenericMethod(type.Type);
                var result = (List<ItemListElement>)closedMethod.Invoke(null, new object[] { searchFolders });
                combined.AddRange(result);
            }

            _itemCache = combined;
        }

        private static List<ItemListElement> _refreshItemsCache<T>(string[] searchFolders) where T : ItemListElement
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchFolders);
            var originalList = new List<ItemListElement>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<T>(path);
                if (item != null)
                {
                    originalList.Add(item);
                }
            }

            return originalList;
        }
    }
}
    #endif