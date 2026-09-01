using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Holylib.ItemEditor
{

    /// <summary>
    /// This interface is assumed to be put on ScriptableObjects
    /// </summary>
    public interface IItemListElement
    {
        public ItemListData GetValues();
        public void InitializeValues(string id, string name);

#if UNITY_EDITOR
        public ElementPreviewData PreviewElement()
        {

            var scriptableObject = new SerializedObject(this as ScriptableObject);

            return new ElementPreviewData(
                new InspectorElement(scriptableObject),
                new[] { scriptableObject });
        }
#endif
        public SearchQuery[] GetCustomSearchLogic();
    }

    public struct ItemListData
    {
        public string ID;
        public string Name;
        public Sprite Icon;

        public ItemListData(string iD, string name, Sprite icon)
        {
            ID = iD;
            Name = name;
            Icon = icon;
        }
    }
    public struct ElementPreviewData
    {
#if UNITY_EDITOR
        public VisualElement PropertyInspector;
        public SerializedObject[] SerializeObjectsToSave;

        public ElementPreviewData(VisualElement propertyInspector, SerializedObject[] serializeObjectsToSave)
        {
            PropertyInspector = propertyInspector;
            SerializeObjectsToSave = serializeObjectsToSave;
        }
#endif
    }

    public struct ItemListElementAndPath
    {
        public Type Type;
        public string SavePath;
        public Color Color;

        public ItemListElementAndPath(Type type, string savePath, Color color)
        {
            Type = type;
            SavePath = savePath;
            Color = color;
        }
    }

    public struct ListManiplutator
    {
        public string ManiplutatorName;
        public Action<Vector2, IItemListElement> OnClicked; // Mouse Position On Click

        public ListManiplutator(string maniplutatorName, Action<Vector2, IItemListElement> onClicked)
        {
            ManiplutatorName = maniplutatorName;
            OnClicked = onClicked;
        }
    }

    public struct SearchQuery
    {
        public string Prefix;
        public string Name;
        public string Description;
        public Func<string, bool> Condition;

        public SearchQuery(string prefix, string name, string description, Func<string, bool> condition)
        {
            Prefix = prefix;
            Name = name;
            Description = description;
            Condition = condition;
        }
    }

    public struct SearchPreset
    {
        public string Query;

        public SearchPreset(string query)
        {
            Query = query;
        }
    }
}