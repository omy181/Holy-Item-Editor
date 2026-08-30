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
        private ItemListElementAndPath[] _itemTypes;
        private Dictionary<ItemListElement, SearchQuery[]> _searchQueriesByItem;
        public HolySearchEngine(Func<List<ItemListElement>> getItems,TextField inputField,Action<string> refresh,Button guideButton, ItemListElementAndPath[] itemTypes)
        {
            _getItems = getItems;
            _inputField = inputField;
            _refresh = refresh;
            _itemTypes = itemTypes;
            _setupSearchQueries();

            _inputField.RegisterCallback<ChangeEvent<string>>((v) =>
            {
                _refresh("");
            });

            guideButton.RegisterCallback<MouseUpEvent>((e) =>
            {
                MessagePopup.Show(_getSearchGuide());
            });
        }

        private List<ItemListElement> _getDistinctItems()
        {
            return _getItems()
                .GroupBy(item => item.GetType())
                .Select(group => group.First())
                .ToList();
        }
        private void _setupSearchQueries()
        {
            _searchQueriesByItem = new();

            foreach (var item in _getItems())
            {
                _searchQueriesByItem[item] = item.GetCustomSearchLogic();
            }
        }

        public List<ItemListElement> GetSearchReults()
        {
            return _getListElements(_getItems, _inputField.text); 
        }

        private List<ItemListElement> _getListElements(Func<List<ItemListElement>> getItems, string searchText)
        {
            var items = getItems();

            var conditions = _textToConditions(searchText);
            return items.FindAll(i => conditions.All(j => j(i)));
        }

        private Func<ItemListElement, bool>[] _textToConditions(string text)
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
                        bool result = false;

                        SearchQuery[] customQueries = null;
                        if (_searchQueriesByItem.TryGetValue(Item,out var querry))
                        {
                            customQueries = querry;
                        }
                        else
                        {
                            _searchQueriesByItem[Item] = Item.GetCustomSearchLogic();
                            customQueries = _searchQueriesByItem[Item];
                        }

                        if(customQueries != null)
                        foreach (var customQuery in customQueries)
                        {
                            if (customQuery.Condition != null)
                            {
                                string sectionT = sectionText;

                                if (!string.IsNullOrEmpty(customQuery.Prefix))
                                {
                                    if (sectionText.StartsWith(customQuery.Prefix))
                                        sectionT = sectionText.Remove(0, customQuery.Prefix.Length);
                                }
                                else
                                {
                                    Debug.LogWarning($"Custom query of {Item.GetType().Name} doesn't have a prefix", (ScriptableObject)Item);
                                }

                                result = customQuery.Condition(sectionT);

                                if (result) { break; }
                            }
                        }

                        return result || Item.GetValues().ID.ToLower().Contains(sectionText) || Item.GetValues().Name.ToLower().Contains(sectionText);
                    };
                }
                
                if (condition != null) conditions.Add(isNegation ? (i) => !condition(i) : condition);
            }

            return conditions.ToArray();
        }

        private string _getSearchGuide()
        {
            string searchGuide =
                "<b><size=120%><color=#FFD700>Search Guide</color></size></b>\n\n" +
                "<b><color=#E57373>/ - Type</color></b>\n" +
                "<color=#B0BEC5>   By type of the ScriptableObject</color>\n" +
                "<b><color=#E57373>! - Negation</color></b>\n" +
                "<color=#B0BEC5>   Put before any query to negate</color>";

            var distinctItems = _getDistinctItems();

            foreach (var item in distinctItems)
            {
                var queryOutputs = item.GetCustomSearchLogic();

                if(queryOutputs != null)
                foreach (var queryOutput in queryOutputs)
                {
                    if (queryOutput.Condition == null || string.IsNullOrEmpty(queryOutput.Prefix))
                    {
                        continue;
                        //Debug.LogWarning($"Custom query of {item.GetType().Name} doesn't have a prefix", (ScriptableObject)item);
                    }

                    searchGuide +=
                    $"\n<b><color=#E57373>{queryOutput.Prefix} - {queryOutput.Name}</color></b>\n" +
                    $"<color=#B0BEC5>   {queryOutput.Description}</color>";
                }
                
            }
            return searchGuide;
        }
    }

    public struct SearchQuery
    {
        public string Prefix;
        public string Name;
        public string Description;
        public Func<string, bool> Condition;

        public SearchQuery(string prefix,string name, string description, Func<string, bool> condition)
        {
            Prefix = prefix;
            Name = name;
            Description = description;
            Condition = condition;
        }
    }
}
#endif