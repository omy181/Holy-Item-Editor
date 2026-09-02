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
        private Func<List<IItemListElement>> _getListOfSearched;
        private Func<List<IItemListElement>> _getListOfAllItems;
        private Action _refreshItemsCache;
        private ItemListElementAndPath[] _supportedItemListTypes;
        private Action<Type, string, Action<string>, string> _onItemCreated;
        public HolyItemList(ListView listView,
            Func<List<IItemListElement>> getListOfSearched,
            Action<IItemListElement> onItemSelected,
            Button createNewButton,
            Action<Type, string, Action<string>,string> onItemCreated,
            Func<Type,string, Func<List<IItemListElement>>, bool> isValidNameForItem,
            Action<string, Action, Func<List<IItemListElement>>> deleteItem, 
            ItemListElementAndPath[] supportedItemListTypes,
            Action refreshItemsCache,
            Func<List<IItemListElement>> getListOfAll,
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

                IItemListElement currentItem = null;

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
                        DeleteItemPopup.Show(item.GetValues().Name, mousePos, () => deleteItem(item.GetID(), () => RefreshItemsCacheAndList(), _getListOfAllItems));
                }));


                listItem.userData = (Action<IItemListElement>)(item => currentItem = item);
                return listItem;
            };

            listView.bindItem = (element, index) =>
            {
                var list = _getListOfSearched();
                var itemData = list[index];

                element.Q<VisualElement>("color").style.backgroundColor = _findItemType(itemData).Color;
                element.Q<Image>("icon").sprite = itemData.GetValues().Icon;
                element.Q<Label>().text = itemData.GetValues().Name;

                ((Action<IItemListElement>)element.userData)(itemData);
            };

            listView.selectionChanged += (item) =>
            {
                onItemSelected(item.Count() > 0 ? item.First() as IItemListElement : null);
            };



            createNewButton.RegisterCallback<MouseUpEvent>((e) =>
            {
                List<(string name, Action onClick)> buttons = new();

                foreach (var type in _supportedItemListTypes)
                {
                    buttons.Add(
                        new (
                            $"{type.Type.Name}",
                            ()=>CreateItemPopup.Show(
                        (id) => CreateItem(type, id),
                        (name) => isValidNameForItem(type.Type, name, _getListOfAllItems))));
                }

                MenuPopup.Show("Create New", buttons);
            });


            
        }

        private void _addManipulator(VisualElement listItem,Func<IItemListElement> getCurrentItem, ListManiplutator maniplutator)
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
        private ItemListElementAndPath _findItemType(IItemListElement element)
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
            var index = string.IsNullOrEmpty(id) ? -1 : items.IndexOf(items.Find(i => i.GetID() == id));

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

        public IItemListElement GetItemListElementByID(string id)
        {
            var items = _getListOfSearched();
            return items.Find(i => i.GetID() == id);
        }
    }

}
#endif