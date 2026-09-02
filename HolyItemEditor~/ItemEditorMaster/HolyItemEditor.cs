#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemEditor : EditorWindow
    {
        [SerializeField]
        private HolyItemList _listView;
        private HolyItemProperties _itemProperties;
        private HolySearchEngine _searchEngine;

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
                (s)=>_listView.RefreshList(s),
                root.Q<Toggle>("AutoSave"));

            _searchEngine = new(
                ItemManagementReferences.GetAListOfAllItems,
                root.Q<TextField>("SearchField"),
                (s) => _listView.RefreshList(s),
                root.Q<Button>("SearchGuide"),
                ItemManagementReferences.SupportedItemListTypes,
                root.Q<Button>("SearchPresets"),
                ItemManagementReferences.GetCustomSearchPresets());

            _listView = new(
                root.Q<ListView>("ItemListView"),
                _searchEngine.GetSearchReults,
                _itemProperties.PreviewItem,
                root.Q<Button>("CreateNewButton"),
                ItemCreation.CreateItem,
                ItemCreation.IsValidNameForNewItem,
                ItemCreation.DeleteItem,
                ItemManagementReferences.SupportedItemListTypes,
                ItemManagementReferences.RefreshItemsCache,
                ItemManagementReferences.GetAListOfAllItems,
                ItemManagementReferences.GetManiplutators());

            root.Q<Button>("RefreshListButton").RegisterCallback<MouseUpEvent>((e)=> _listView.RefreshItemsCacheAndList());

        }

        #region Open Asset
        [OnOpenAsset]
        private static bool _onOpenAsset(EntityId id, int line)
        {
            var obj = EditorUtility.EntityIdToObject(id);
            foreach (var typeData in ItemManagementReferences.SupportedItemListTypes)
            {
                if (typeData.Type.IsInstanceOfType(obj))
                {
                    var item = (IItemListElement)obj;

                    var wnd = GetWindow<HolyItemEditor>();
                    wnd.Focus();
                    wnd._itemProperties.PreviewItem(wnd._listView.GetItemListElementByID(item.GetValues().ID));
                    return true;
                }
            }

            return false; // not our asset
        }
        #endregion

        #region Save Logic
        public class SaveShortcutContext : IShortcutContext
        {
            public bool active => focusedWindow is HolyItemEditor;
        }

        private static readonly SaveShortcutContext s_ShortcutContext = new SaveShortcutContext();

        private void OnEnable()
        {
            ShortcutManager.RegisterContext(s_ShortcutContext);
        }

        private void OnDisable()
        {
            ShortcutManager.UnregisterContext(s_ShortcutContext);
        }

        [Shortcut("HolyItemEditor/Save", typeof(SaveShortcutContext), KeyCode.S, ShortcutModifiers.Action)]
        private static void SaveShortcut(ShortcutArguments args)
        {
            var window = focusedWindow as HolyItemEditor;
            window?._itemProperties?.SaveChanges(true);
        }
        #endregion
    }
}
#endif