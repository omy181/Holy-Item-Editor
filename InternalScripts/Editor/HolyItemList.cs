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
        public HolyItemList(ListView listView,
            Func<List<ItemListElement>> getListOfSearched,
            Action<ItemListElement> onItemSelected,
            VisualElement createNewButtonContainer,
            Action<Type, string, Action<string>,string> onItemCreated,
            Func<Type,string, Func<List<ItemListElement>>, bool> isValidNameForItem,
            Action<string, Action, Func<List<ItemListElement>>> deleteItem, 
            ItemListElementAndPath[] supportedItemListTypes,
            Action refreshItemsCache,
            Func<List<ItemListElement>> getListOfAll)
        {
            _listView = listView;
            _getListOfSearched = getListOfSearched;
            _refreshItemsCache = refreshItemsCache;
            _getListOfAllItems = getListOfAll;

            listView.itemsSource = _getListOfSearched();

            listView.makeItem = () =>
            {
                var listItem = new VisualElement();

                var image = new Image();
                image.style.width = 20;
                image.style.height = 20;

                listItem.Add(image);
                listItem.Add(new Label());
                listItem.style.flexDirection = FlexDirection.Row;
                listItem.style.alignContent = Align.Center;
                listItem.style.alignItems = Align.FlexStart;

                ItemListElement currentItem = null;
                listItem.AddManipulator(new ContextualMenuManipulator((evt) =>
                {
                    if (currentItem == null) return;

                    var screenPos = GUIUtility.GUIToScreenPoint(evt.mousePosition);
                    evt.menu.AppendAction(
                        "Delete Item",
                        (x) => DeleteItemPopup.Show(currentItem.Name, screenPos, () => deleteItem(currentItem.ID,()=> _refreshItemsCacheAndList(),_getListOfAllItems)),
                        DropdownMenuAction.AlwaysEnabled
                    );
                }));

                listItem.userData = (Action<ItemListElement>)(item => currentItem = item);
                return listItem;
            };

            listView.bindItem = (element, index) =>
            {
                var list = _getListOfSearched();
                var itemData = list[index];
                element.Q<Image>().sprite = itemData.Icon;
                element.Q<Label>().text = itemData.Name;

                ((Action<ItemListElement>)element.userData)(itemData);
            };

            listView.selectionChanged += (item) =>
            {
                onItemSelected(item.Count() > 0 ? item.First() as ItemListElement : null);
            };


            createNewButtonContainer.Clear();
            foreach (var type in supportedItemListTypes)
            {
                var nButton = new Button();
                nButton.text = $"Create New {type.Type.Name}";
                nButton.RegisterCallback<MouseUpEvent>((a) =>
                CreateItemPopup.Show(
                    (id) => onItemCreated(type.Type,id, _refreshItemsCacheAndList, type.SavePath),
                    (name)=>isValidNameForItem(type.Type,name, _getListOfAllItems)));

                createNewButtonContainer.Add(nButton);
            }

            
        }

        private void _refreshItemsCacheAndList(string id = "")
        {
            _refreshItemsCache();
            RefreshList(id);
        }
        public void RefreshList(string id = "")
        {
            var items = _getListOfSearched();
            _listView.itemsSource = items;
            _listView.RefreshItems();
            var index = string.IsNullOrEmpty(id) ? -1 : items.IndexOf(items.Find(i => i.ID == id));

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
            return items.Find(i => i.ID == id);
        }
    }

    public abstract class ItemListElement : ScriptableObject
    {
        public abstract string ID { get; }
        public abstract string Name { get; }
        public abstract Sprite Icon { get; }
        public abstract void InitializeValues(string id, string name);
        public abstract ElementPreviewData PreviewElement();
        public abstract bool DoesFitSearchQuerry(string querry);
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

        public ItemListElementAndPath(Type type, string savePath)
        {
            Type = type;
            SavePath = savePath;
        }
    }
}
#endif