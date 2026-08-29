#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolySearchEngine
    {
        private Func<List<ItemListElement>> _getItems;
        private TextField _inputField;
        private Action<string> _refresh;
        public HolySearchEngine(Func<List<ItemListElement>> getItems,TextField inputField,Action<string> refresh,Button guideButton,string searchGuide)
        {
            _getItems = getItems;
            _inputField = inputField;
            _refresh = refresh;

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
            return _getListElements(_getItems, _inputField.text); 
        }

        private static List<ItemListElement> _getListElements(Func<List<ItemListElement>> getItems, string searchText)
        {
            var items = getItems();

            var conditions = _textToConditions(searchText);
            return items.FindAll(i => conditions.All(j => j(i)));
        }

        private static Func<ItemListElement, bool>[] _textToConditions(string text)
        {

            List<Func<ItemListElement, bool>> conditions = new();

            var sections = text.ToLower().Split(' ');

            foreach (var section in sections)
            {
                Func<ItemListElement, bool> condition = null;
                bool isNegation = false;
                string sectionText = section;

                if (sectionText.StartsWith("!"))    // Negate Condition
                {
                    isNegation = true;
                    sectionText = sectionText.Remove(0, 1);
                }

                if(sectionText.StartsWith("/"))  // Type Condition
                {
                    sectionText = sectionText.Remove(0, 1);

                    condition = (Item) =>
                    {
                        return Item.GetType().Name.ToLower().Contains(sectionText);
                    };
                }
                else
                {
                    condition = (Item) =>
                    {
                        return Item.DoesFitSearchQuerry(sectionText) || Item.ID.ToLower().Contains(sectionText) || Item.Name.ToLower().Contains(sectionText);
                    };
                }
                
                if (condition != null) conditions.Add(isNegation ? (i) => !condition(i) : condition);
            }

            return conditions.ToArray();
        }
    }
}
#endif