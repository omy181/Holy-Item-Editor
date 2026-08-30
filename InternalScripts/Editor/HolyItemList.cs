#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemList
    {
        private ListView _listView;
        private Func<List<ItemListElement>> _getListOfSearched;
        private Func<List<ItemListElement>> _getListOfAllItems;
        private Action _refreshItemsCache;
        private ItemListElementAndPath[] _supportedItemListTypes;
        private Action<Type, string, Action<string>, string> _onItemCreated;
        public HolyItemList(ListView listView,
            Func<List<ItemListElement>> getListOfSearched,
            Action<ItemListElement> onItemSelected,
            VisualElement createNewButtonContainer,
            Action<Type, string, Action<string>,string> onItemCreated,
            Func<Type,string, Func<List<ItemListElement>>, bool> isValidNameForItem,
            Action<string, Action, Func<List<ItemListElement>>> deleteItem, 
            ItemListElementAndPath[] supportedItemListTypes,
            Action refreshItemsCache,
            Func<List<ItemListElement>> getListOfAll,
            ListManiplutator[] listManiplutors)
        {
            _listView = listView;
            _getListOfSearched = getListOfSearched;
            _refreshItemsCache = refreshItemsCache;
            _getListOfAllItems = getListOfAll;
            _supportedItemListTypes = supportedItemListTypes;
            _onItemCreated = onItemCreated;

            listView.itemsSource = _getListOfSearched();

            listView.makeItem = () =>
            {
                var listItem = new VisualElement();

                var color = new VisualElement();
                color.style.height = 20;
                color.style.width = 2;
                color.name = "color";
                color.style.marginRight = 4;

                var image = new Image();
                image.style.width = 20;
                image.style.height = 20;
                image.name = "icon";

                listItem.Add(color);
                listItem.Add(image);
                listItem.Add(new Label());
                listItem.style.flexDirection = FlexDirection.Row;
                listItem.style.alignContent = Align.Center;
                listItem.style.alignItems = Align.FlexStart;


                // Manipulators

                ItemListElement currentItem = null;

                foreach (var maniplutator in listManiplutors)
                {
                    _addManipulator(listItem, () => currentItem, maniplutator);
                }

                _addManipulator(listItem, () => currentItem, new("Show in Project Window",
                (evt, item) => {
                    Type projectBrowserType = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
                    EditorWindow projectWindow = EditorWindow.GetWindow(projectBrowserType);
                    projectWindow.Focus();
                    Selection.activeObject = (ScriptableObject)item;
                    EditorGUIUtility.PingObject((ScriptableObject)item);
                }));

                _addManipulator(listItem,()=>currentItem,new("Delete Item",
                    (mousePos, item) => {
                        DeleteItemPopup.Show(item.GetValues().Name, mousePos, () => deleteItem(item.GetValues().ID, () => RefreshItemsCacheAndList(), _getListOfAllItems));
                }));


                listItem.userData = (Action<ItemListElement>)(item => currentItem = item);
                return listItem;
            };

            listView.bindItem = (element, index) =>
            {
                var list = _getListOfSearched();
                var itemData = list[index];

                element.Q<VisualElement>("color").style.backgroundColor = _findItemType(itemData).Color;
                element.Q<Image>("icon").sprite = itemData.GetValues().Icon;
                element.Q<Label>().text = itemData.GetValues().Name;

                ((Action<ItemListElement>)element.userData)(itemData);
            };

            listView.selectionChanged += (item) =>
            {
                onItemSelected(item.Count() > 0 ? item.First() as ItemListElement : null);
            };


            createNewButtonContainer.Clear();
            foreach (var type in _supportedItemListTypes)
            {
                var nButton = new Button();
                nButton.text = $"Create New {type.Type.Name}";
                nButton.RegisterCallback<MouseUpEvent>((a) =>
                CreateItemPopup.Show(
                    (id) => CreateItem(type,id),
                    (name)=>isValidNameForItem(type.Type,name, _getListOfAllItems)));

                createNewButtonContainer.Add(nButton);
            }

            
        }

        private void _addManipulator(VisualElement listItem,Func<ItemListElement> getCurrentItem, ListManiplutator maniplutator)
        {
            listItem.AddManipulator(new ContextualMenuManipulator((evt) =>
            {
                if (getCurrentItem() == null) return;

                var screenPos = GUIUtility.GUIToScreenPoint(evt.mousePosition);

                evt.menu.AppendAction(
                    maniplutator.ManiplutatorName,
                    (x) => maniplutator.OnClicked(screenPos, getCurrentItem()),
                    DropdownMenuAction.AlwaysEnabled
                );
            }));
        }
        private ItemListElementAndPath _findItemType(ItemListElement element)
        {
            foreach (var typeData in _supportedItemListTypes)
            {
                if (typeData.Type.IsInstanceOfType(element))
                {
                    return typeData;
                }
            }

            Debug.LogError("Unkown Element Type");
            return new();
        }

        public void CreateItem(ItemListElementAndPath typeData,string id)
        {
            _onItemCreated(typeData.Type, id, RefreshItemsCacheAndList, typeData.SavePath);
        }
        public void RefreshItemsCacheAndList(string id = "")
        {
            _refreshItemsCache();
            RefreshList(id);
        }
        public void RefreshList(string id = "")
        {
            var items = _getListOfSearched();
            _listView.itemsSource = items;
            _listView.RefreshItems();
            var index = string.IsNullOrEmpty(id) ? -1 : items.IndexOf(items.Find(i => i.GetValues().ID == id));

            _listView.SetSelectionWithoutNotify(new int[] { });

            if (index != -1)
            {
                _listView.selectedIndex = index;
            }
            else
            {
                _listView.selectedIndex = items.Count > 0 ? 0 : -1;
            }
        }

        public ItemListElement GetItemListElementByID(string id)
        {
            var items = _getListOfSearched();
            return items.Find(i => i.GetValues().ID == id);
        }
    }

    /// <summary>
    /// This interface is assumed to be put on ScriptableObjects
    /// </summary>
    public interface ItemListElement
    {
        public ItemListData GetValues();
        public void InitializeValues(string id, string name);
        public ElementPreviewData PreviewElement();
        public bool CustomSearchLogic(string query);
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
        public VisualElement PropertyInspector;
        public SerializedObject[] SerializeObjectsToSave;

        public ElementPreviewData(VisualElement propertyInspector, SerializedObject[] serializeObjectsToSave)
        {
            PropertyInspector = propertyInspector;
            SerializeObjectsToSave = serializeObjectsToSave;
        }
    }

    public struct ItemListElementAndPath
    {
        public Type Type;
        public string SavePath;
        public Color Color;

        public ItemListElementAndPath(Type type, string savePath,Color color)
        {
            Type = type;
            SavePath = savePath;
            Color = color;
        }
    }

    public struct ListManiplutator
    {
        public string ManiplutatorName;
        public Action<Vector2, ItemListElement> OnClicked; // Mouse Position On Click

        public ListManiplutator(string maniplutatorName, Action<Vector2, ItemListElement> onClicked)
        {
            ManiplutatorName = maniplutatorName;
            OnClicked = onClicked;
        }
    }
}
#endif