#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class ItemManagementReferences
    {
        public static readonly ItemListElementAndPath[] SupportedItemListTypes = { 
            new( typeof(StaticItemData), "Assets/Resources/Items",Color.azure), 
            new( typeof(RecipeData), "Assets/Resources/Recipes",Color.yellow) };

        private static string[] GetSearchFolders()
        {
            List<string> supportedItemsList = SupportedItemListTypes.Select(i => i.SavePath).ToList();
            supportedItemsList.Add("Assets");   // You can remove this if you want
            return supportedItemsList.ToArray();
        }
        
        public static List<ItemListElement> GetAListOfAllItems()
        {
            return ItemManagement.GetAListOfAllItems();
        }

        public static void RefreshItemsCache()
        {
            ItemManagement.RefreshItemsCache(SupportedItemListTypes,GetSearchFolders());
        }
    }
}
#endif