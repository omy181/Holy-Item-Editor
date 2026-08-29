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
            root.Add(visualTree.Instantiate());

            ItemManagementReferences.RefreshItemsCache();

            _itemProperties = new(
                root.Q<VisualElement>("PropertiesContent"),
                root.Q<Image>("ItemImage"),
                root.Q<Label>("ItemName"),
                _refresh);

            HolySearchEngine searchEngine = new(
                ItemManagementReferences.GetAListOfAllItems,
                root.Q<TextField>("SearchField"),
                _refresh,
                root.Q<Button>("SearchGuide"),
                _getSearchGuide());

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
                ItemManagementReferences.GetAListOfAllItems);

        }

        private string _getSearchGuide()
        {
            string searchGuide =
                "<b><size=120%><color=#FFD700>Search Guide</color></size></b>\n\n" +
                "<b><color=#E57373>/ - Type</color></b>\n" +
                "<color=#B0BEC5>   /StaticItemData  /RecipeData ...</color>\n" +
                "<b><color=#E57373>! - Negation</color></b>\n" +
                "<color=#B0BEC5>   !stone  !/RecipeData ...</color>";
            return searchGuide;
        }

        private void _refresh(string id)
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
                    wnd._previewItem(wnd._listView.GetItemListElementByID(item.ID));
                    return true;
                }
            }

            return false; // not our asset
        }
    }
}
#endif