using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
#if UNITY_EDITOR
    public class HolySearchEngine
    {
        private Func<List<IItemListElement>> _getItems;
        private TextField _inputField;
        private Action<string> _refresh;
        private ItemListElementAndPath[] _itemTypes;
        private SearchPreset[] _customSearchPresets;
        private Dictionary<IItemListElement, SearchQuery[]> _searchQueriesByItem;
        public HolySearchEngine(Func<List<IItemListElement>> getItems,TextField inputField,Action<string> refresh,Button guideButton, ItemListElementAndPath[] itemTypes,Button searchPresetsButton, SearchPreset[] customSearchPresets)
        {
            _getItems = getItems;
            _inputField = inputField;
            _refresh = refresh;
            _itemTypes = itemTypes;
            _customSearchPresets = customSearchPresets;
            _setupSearchQueries();

            _inputField.RegisterCallback<ChangeEvent<string>>((v) =>
            {
                _refresh("");
            });

            guideButton.RegisterCallback<MouseUpEvent>((e) =>
            {
                MessagePopup.Show(_getSearchGuide());
            });

            searchPresetsButton.RegisterCallback<MouseUpEvent>((e) =>
            {
                MenuPopup.Show("Search", _getSearchPresets());
            });
        }

        private List<(string name,Action onClick)> _getSearchPresets()
        {
            List<(string name, Action onClick)> list = new();

            foreach (var itemType in _itemTypes)
            {
                string query = $"{_typePrefix}{itemType.Type.Name}";
                list.Add(new(query, () => Search(query)));
            }

            foreach (var preset in _customSearchPresets)
            {
                list.Add(new(preset.Query,()=>Search(preset.Query)));
            }

            return list;
        }

        private List<IItemListElement> _getDistinctItems()
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

        public void Search(string query)
        {
            _inputField.value = query;
        }
        public List<IItemListElement> GetSearchReults()
        {
            return _getListElements(_getItems, _inputField.text); 
        }

        private List<IItemListElement> _getListElements(Func<List<IItemListElement>> getItems, string searchText)
        {
            var items = getItems();

            var conditions = _textToConditions(searchText);
            return items.FindAll(i => conditions.All(j => j(i)));
        }

        private string _typePrefix = "/";
        private string _negatePrefix = "!";
        private Func<IItemListElement, bool>[] _textToConditions(string text)
        {
            List<Func<IItemListElement, bool>> conditions = new();

            var sections = text.ToLower().Split(' ');

            foreach (var section in sections)
            {
                Func<IItemListElement, bool> condition = null;
                bool isNegation = false;
                string sectionText = section;

                if (sectionText.StartsWith(_negatePrefix))    // Negate Condition
                {
                    isNegation = true;
                    sectionText = sectionText.Remove(0, 1);
                }

                if(sectionText.StartsWith(_typePrefix))  // Type Condition
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
                                        {
                                            sectionT = sectionText.Remove(0, customQuery.Prefix.Length);
                                            result = customQuery.Condition(sectionT);
                                        }
                                }
                                else
                                {
                                    Debug.LogWarning($"Custom query of {Item.GetType().Name} doesn't have a prefix", (ScriptableObject)Item);
                                }

                                

                                if (result) { break; }
                            }
                        }

                        return result || Item.GetValues().Name.ToLower().Replace(" ","").Contains(sectionText);
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

#endif

}