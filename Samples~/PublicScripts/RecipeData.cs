#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{

    [CreateAssetMenu(fileName = "RecipeData", menuName = "Holylib/RecipeData")]
    public class RecipeData : ItemListElement
    {
        [SerializeField] private string _id;
        public override string ID => _id;
        [SerializeField] private string _name;
        public override string Name => _name;
        [SerializeField] private Sprite _icon;
        public override Sprite Icon => _icon;
        [SerializeField] private StaticItemData _ingredientA;
        [SerializeField] private StaticItemData _ingredientB;
        [SerializeField] private StaticItemData _output;
        public override void InitializeValues(string id,string name)
        {
            _id = id;
            _name = name;
        }

#if UNITY_EDITOR
        public override ElementPreviewData PreviewElement()
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
                new[] { scriptableObject });
        }

        public override bool DoesFitSearchQuerry(string querry)
        {
            return false;
        }
#endif
    }

}