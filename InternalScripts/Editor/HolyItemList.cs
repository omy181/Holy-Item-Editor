#if UNITY_EDITOR
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemList
    {
        private ListView _listView;
        private Func<List<ItemListElement>> _getList;
        public HolyItemList(ListView listView,Func<List<ItemListElement>> getList,Action<ItemListElement> onItemSelected,Button createNewButton,Action<string,Action<string>> onItemCreated,Func<string,bool> isValidNameForItem,Action<string, Action> deleteItem)
        {
            _listView = listView;
            _getList = getList;

            listView.itemsSource = _getList();
            
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
                        (x) => DeleteItemPopup.Show(currentItem.Name, screenPos, () => deleteItem(currentItem.ID,()=> RefreshList())),
                        DropdownMenuAction.AlwaysEnabled
                    );
                }));

                listItem.userData = (Action<ItemListElement>)(item => currentItem = item);
                return listItem;
            };

            listView.bindItem = (element, index) =>
            {
                var list = _getList();
                var itemData = list[index];
                element.Q<Image>().sprite = itemData.Icon;
                element.Q<Label>().text = itemData.Name;

                ((Action<ItemListElement>)element.userData)(itemData);
            };

            listView.selectionChanged += (item) =>
            {
                onItemSelected(item.Count() > 0 ? item.First() as ItemListElement : null);
            };

            createNewButton.RegisterCallback<MouseUpEvent>((a)=> CreateItemPopup.Show((id)=>onItemCreated(id, RefreshList), isValidNameForItem));
        }

        public void RefreshList(string id = "")
        {
            var items = _getList();
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
            var items = _getList();
            return items.Find(i => i.ID == id);
        }
    }

    public class ItemListElement
    {
        public string ID;
        public string Name;
        public Sprite Icon;

        public ItemListElement(string iD, string name, Sprite icon)
        {
            ID = iD;
            Name = name;
            Icon = icon;
        }
    }
}