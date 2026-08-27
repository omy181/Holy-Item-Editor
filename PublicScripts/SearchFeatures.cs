using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Holylib.ItemEditor
{
    public static class SearchFeatures
    {
        public static List<ItemListElement> GetListElements(Func<List<ItemListElement>> getItems, string searchText)
        {
            var items = getItems();

            var conditions = _textToConditions(searchText);
            return items.FindAll(i=> conditions.All(j => j(ItemManagement.ListElementToItemData(i))));
        }

        public static string GetSearchGuide()
        {
            string searchGuide =
                "<b><size=120%><color=#FFD700>Search Guide</color></size></b>\n\n" +
                "<b><color=#FFB74D>/ - Properties</color></b>\n" +
                "<color=#B0BEC5>   /isingame  /icon</color>\n\n" +
                "<b><color=#E57373>! - Negation</color></b>\n" +
                "<color=#B0BEC5>   !/isingame  !/icon ...</color>";
            return searchGuide;
        }
        private static Func<StaticItemData, bool>[] _textToConditions(string text)
        {

            List<Func<StaticItemData, bool>> conditions = new();

            var sections = text.ToLower().Split(' ');

            foreach (var section in sections)
            {
                Func<StaticItemData, bool> condition = null;
                bool isNegation = false;
                string sectionText = section;

                if (sectionText.StartsWith("!"))    // Negate Condition
                {
                    isNegation = true;
                    sectionText = sectionText.Remove(0, 1);
                }

                if (sectionText.StartsWith("/"))    // Property Search
                {
                    var q = sectionText.Remove(0, 1);

                    if (q.Length == 0)
                    {
                        condition = (Item) =>
                        {
                            return true;
                        };
                    }
                    else if ("isingame".Contains(q))
                    {
                        condition = (item) =>
                        {
                            return item.IsIngame;
                        };
                    }
                    else if ("icon".Contains(q))
                    {
                        condition = (item) =>
                        {
                            return item.Sprite;
                        };
                    }
                    
                }
                else    // ID Search
                {
                    condition = (Item) =>
                    {
                        return Item.ID.ToLower().Contains(sectionText);
                    };
                }

                if (condition != null) conditions.Add(isNegation ? (i) => !condition(i) : condition);
            }

            return conditions.ToArray();
        }
    }
}