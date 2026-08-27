#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class ItemCreation
    {
        public static string ItemsPath = "Assets/Resources/Items";
        public static void CreateItem(string name,Action<string> refreshList)
        {
            var id = name.Replace(" ", "").ToLower();
            var newItem = ScriptableObject.CreateInstance<StaticItemData>();
            newItem.InitializeValues(id,name);

            if(!Directory.Exists(ItemsPath))
                Directory.CreateDirectory(ItemsPath);

            AssetDatabase.CreateAsset(newItem, $"{ItemsPath}/{id}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ItemManagement.RefreshItemsCache();

            refreshList(id);
        }

        public static bool IsValidNameForNewItem(string name)
        {
            name = name.Replace(" ", "").ToLower();

            var itemsInAssets = Resources.FindObjectsOfTypeAll(typeof(StaticItemData)) as StaticItemData[];

            return !itemsInAssets.ToList().Exists(i => i.ID.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public static void DeleteItem(string id, Action refreshList)
        {
            try
            {
                var item = ItemManagement.GetAListOfItems().Find(i => i.ID == id);

                var path = AssetDatabase.GetAssetPath(item);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ItemManagement.RefreshItemsCache();

                Debug.Log($"{id} deleted");
            }
            catch {
                Debug.LogWarning($"{id} couldn't be deleted");
            }

            
            refreshList();
        }
    }
}
#endif