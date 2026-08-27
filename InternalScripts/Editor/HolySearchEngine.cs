#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolySearchEngine
    {
        private Func<List<ItemListElement>> _getItems;
        private TextField _inputField;
        private Action<string> _refresh;
        private Func<Func<List<ItemListElement>>, string, List<ItemListElement>> _getSearchResults;
        public HolySearchEngine(Func<List<ItemListElement>> getItems,TextField inputField,Action<string> refresh,Button guideButton,Func<Func<List<ItemListElement>>,string, List<ItemListElement>> getSearchResults,string searchGuide)
        {
            _getItems = getItems;
            _inputField = inputField;
            _refresh = refresh;
            _getSearchResults =getSearchResults;

            _inputField.RegisterCallback<ChangeEvent<string>>((v) =>
            {
                _refresh("");
            });

            guideButton.RegisterCallback<MouseUpEvent>((e) =>
            {
                MessagePopup.Show(searchGuide);
            });
        }

        public List<ItemListElement> GetSearchReults()
        {
            return _getSearchResults(_getItems, _inputField.text); 
        }
    }
}