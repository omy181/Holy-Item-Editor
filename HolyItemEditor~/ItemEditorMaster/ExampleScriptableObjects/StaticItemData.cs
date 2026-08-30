#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

using UnityEngine;

namespace Holylib.ItemEditor
{

    [CreateAssetMenu(fileName = "StaticItemData", menuName = "Holylib/StaticItemData")]
    public class StaticItemData : ScriptableObject, ItemListElement
    {
        [SerializeField] private string _id;
        public string ID => _id;
        [SerializeField] private string _name;
        public string Name => _name;
        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;
        [SerializeField] private bool _isInGame;
        public bool IsIngame => _isInGame;

        public void InitializeValues(string id,string name)
        {
            _id = id;
            _name = name;
            _isInGame = true;
        }

        public ItemListData GetValues() => new(ID,Name,Icon);

#if UNITY_EDITOR
        public ElementPreviewData PreviewElement()
        {
            var scriptableObject = new SerializedObject(this);

            return new ElementPreviewData(
                new InspectorElement(scriptableObject),
                new[] { scriptableObject });
        }

        public SearchQuery[] GetCustomSearchLogic()
        {
            return null;
        }
#endif
    }

}