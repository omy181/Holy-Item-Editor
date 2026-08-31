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
        private static List<IItemListElement> _itemCache = new();

        public static List<IItemListElement> GetListOfAllItems()
        {
            return _itemCache;
        }
        public static void RefreshItemsCache(ItemListElementAndPath[] supportedItemListTypes, string[] searchFolders)
        {
            var combined = new List<IItemListElement>();

            var openMethod = typeof(ItemManagement).GetMethod(
                nameof(_refreshItemsCache),
                BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var type in supportedItemListTypes)
            {
                var closedMethod = openMethod.MakeGenericMethod(type.Type);
                var result = (List<IItemListElement>)closedMethod.Invoke(null, new object[] { searchFolders });
                combined.AddRange(result);
            }

            _itemCache = combined;
        }

        private static List<IItemListElement> _refreshItemsCache<T>(string[] searchFolders) where T : ScriptableObject,IItemListElement
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchFolders);
            var originalList = new List<IItemListElement>();

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