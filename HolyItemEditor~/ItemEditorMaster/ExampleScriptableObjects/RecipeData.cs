#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{

    [CreateAssetMenu(fileName = "RecipeData", menuName = "Holylib/RecipeData")]
    public class RecipeData : ScriptableObject, IItemListElement
    {
        [SerializeField] private string _id;
        public string ID => _id;
        [SerializeField] private string _name;
        public string Name => _name;
        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;
        [SerializeField] private StaticItemData _ingredientA;
        [SerializeField] private StaticItemData _ingredientB;
        [SerializeField] private StaticItemData _output;
        public void InitializeValues(string name)
        {
            _id = name.ToLower().Replace(" ", "");
            _name = name;
        }

        public ItemListData GetValues() => new(Name, Icon);

#if UNITY_EDITOR
        public ElementPreviewData PreviewElement()
        {
            var scriptableObject = new SerializedObject(this);

            var vis = new VisualElement();
            var title = new Label("Recipe");
            title.style.alignSelf = Align.Center;
            title.style.fontSize = 16;

            vis.Add(title);
            vis.Add(new InspectorElement(scriptableObject));

            return new ElementPreviewData(
                vis,
                new[] { scriptableObject },
                null);
        }
#endif
        public SearchQuery[] GetCustomSearchLogic()
        {
            return new SearchQuery[1]{ new(
                "r/",
                "Recipe Input", "Search by recipe input",
                (s) => (_ingredientA != null && _ingredientA.GetValues().Name.ToLower().Contains(s))
                || (_ingredientB != null && _ingredientB.GetValues().Name.ToLower().Contains(s)))};
        }

    }

}