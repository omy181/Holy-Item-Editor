using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private HolyItemList _listView;

        [MenuItem("Tools/Holylib/HolyItemEditor")]
        public static void OpenWindow()
        {
            HolyItemEditor wnd = GetWindow<HolyItemEditor>();
            wnd.titleContent = new GUIContent("HolyItemEditor");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Add(m_VisualTreeAsset.Instantiate());

            HolyItemProperties itemProperties = new(
                root.Q<VisualElement>("PropertiesContent"),
                root.Q<Image>("ItemImage"),
                root.Q<Label>("ItemName"),
                _refresh);

            HolySearchEngine searchEngine = new(
                ItemManagement.GetAllListElements,
                root.Q<TextField>("SearchField"),
                _refresh,
                root.Q<Button>("SearchGuide"),
                SearchFeatures.GetListElements,
                SearchFeatures.GetSearchGuide());

            _listView = new(
                root.Q<ListView>("ItemListView"),
                searchEngine.GetSearchReults,
                (item)=> _previewItem(itemProperties,item)
                ,root.Q<Button>("NewItem"),
                ItemCreation.CreateItem,
                ItemCreation.IsValidNameForNewItem,
                ItemCreation.DeleteItem);

        }

        private void _refresh(string id)
        {
            _listView.RefreshList(id);
        }
        private void _previewItem(HolyItemProperties itemProperties, ItemListElement item)
        {
            if(item != null)
            {
                itemProperties.PreviewItem(new SerializedObject(ItemManagement.ListElementToItemData(item)), item);  
            }
            else
            {
                itemProperties.PreviewItem();
            }
            
        }
    }
}