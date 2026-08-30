#if UNITY_EDITOR
using System.Linq;
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
            var uxmlRoot = visualTree.Instantiate();
            
            root.style.flexGrow = 1;

            uxmlRoot.style.flexGrow = 1;
            uxmlRoot.style.flexShrink = 0;
            uxmlRoot.style.height = new StyleLength(Length.Percent(100));
            uxmlRoot.style.width = new StyleLength(Length.Percent(100));

            root.Add(uxmlRoot);


            ItemManagementReferences.RefreshItemsCache();

            _itemProperties = new(
                root.Q<VisualElement>("PropertiesContent"),
                root.Q<Image>("ItemImage"),
                root.Q<Label>("ItemName"),
                _refreshList);

            HolySearchEngine searchEngine = new(
                ItemManagementReferences.GetAListOfAllItems,
                root.Q<TextField>("SearchField"),
                _refreshList,
                root.Q<Button>("SearchGuide"),
                ItemManagementReferences.SupportedItemListTypes);

            _listView = new(
                root.Q<ListView>("ItemListView"),
                searchEngine.GetSearchReults,
                _previewItem,
                root.Q<VisualElement>("NewItemButtonContainer"),
                ItemCreation.CreateItem,
                ItemCreation.IsValidNameForNewItem,
                ItemCreation.DeleteItem,
                ItemManagementReferences.SupportedItemListTypes,
                ItemManagementReferences.RefreshItemsCache,
                ItemManagementReferences.GetAListOfAllItems,
                ItemManagementReferences.GetManiplutators());

            root.Q<Button>("RefreshListButton").RegisterCallback<MouseUpEvent>((e)=> _listView.RefreshItemsCacheAndList());

        }

        private void _refreshList(string id)
        {
            _listView.RefreshList(id);
        }

        private void _previewItem(ItemListElement item)
        {
            if(item != null)
            {
                _itemProperties.PreviewItem(item);  
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
            foreach (var typeData in ItemManagementReferences.SupportedItemListTypes)
            {
                if (typeData.Type.IsInstanceOfType(obj))
                {
                    var item = (ItemListElement)obj;

                    var wnd = GetWindow<HolyItemEditor>();
                    wnd.Focus();
                    wnd._previewItem(wnd._listView.GetItemListElementByID(item.GetValues().ID));
                    return true;
                }
            }

            return false; // not our asset
        }
    }
}
#endif