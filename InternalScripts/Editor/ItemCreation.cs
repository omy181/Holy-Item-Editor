#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class ItemCreation
    {
        public static void CreateItem(Type type,string name, Action<string> refreshList, string path)
        {
            var openMethod = typeof(ItemCreation)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(CreateItem) && m.IsGenericMethodDefinition);

            var closedMethod = openMethod.MakeGenericMethod(type);
            closedMethod.Invoke(null, new object[] { name, refreshList, path });
        }
        public static T CreateItem<T>(string name,Action<string> refreshList,string path) where T : ScriptableObject,IItemListElement
        {

            var newItem = ScriptableObject.CreateInstance<T>();
            newItem.InitializeValues(name);

            var id = newItem.GetID();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            AssetDatabase.CreateAsset(newItem, $"{path}/{id}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            refreshList(id);

            return newItem;
        }

        public static bool IsValidNameForNewItem(Type type, string name,Func<List<IItemListElement>> getAllItems)
        {
            var openMethod = typeof(ItemCreation)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(IsValidNameForNewItem) && m.IsGenericMethodDefinition);

            var closedMethod = openMethod.MakeGenericMethod(type);
            return (bool)closedMethod.Invoke(null, new object[] { name, getAllItems });
        }
        public static bool IsValidNameForNewItem<T>(string name, Func<List<IItemListElement>> getAllItems)
        {
            name = $"{typeof(T).Name.ToLower()}_{name.Replace(" ", "").ToLower()}";

            var allItems = getAllItems.Invoke();

            return !allItems.Exists(i => i.GetID().Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public static void DeleteItem(string id, Action refreshList,Func<List<IItemListElement>> getAllItems)
        {
            try
            {
                var item = getAllItems().Find(i => i.GetID() == id);

                var path = AssetDatabase.GetAssetPath((ScriptableObject)item);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

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