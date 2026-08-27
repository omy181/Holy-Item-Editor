using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemEditor : EditorWindow
    {
        [SerializeField]
        private HolyItemList _listView;
        private HolyItemProperties _itemProperties;

        [MenuItem("Tools/Holylib/HolyItemEditor")]
        public static void OpenWindow()
        {
            HolyItemEditor wnd = GetWindow<HolyItemEditor>();
            wnd.titleContent = new GUIContent("HolyItemEditor");
        }

        public void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/dev.holyperson.holyitemeditor/InternalScripts/Editor/HolyItemEditor.uxml");

            VisualElement root = rootVisualElement;
            root.Add(visualTree.Instantiate());

            _itemProperties = new(
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
                _previewItem,
                root.Q<Button>("NewItem"),
                ItemCreation.CreateItem,
                ItemCreation.IsValidNameForNewItem,
                ItemCreation.DeleteItem);

        }

        private void _refresh(string id)
        {
            _listView.RefreshList(id);
        }
        private void _previewItem(ItemListElement item)
        {
            if(item != null)
            {
                _itemProperties.PreviewItem(new SerializedObject(ItemManagement.ListElementToItemData(item)), item);  
            }
            else
            {
                _itemProperties.PreviewItem();
            }
            
        }

        [OnOpenAsset]
        private static bool _onOpenAsset(EntityId id, int line)
        {
            var obj = EditorUtility.EntityIdToObject(id);
            if (obj is StaticItemData item)
            {
                var wnd = GetWindow<HolyItemEditor>();
                wnd.Focus();
                wnd._previewItem(wnd._listView.GetItemListElementByID(item.ID));
                return true;
            }

            return false; // not our asset
        }
    }
}