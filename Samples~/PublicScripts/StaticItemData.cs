using UnityEngine;

namespace Holylib.ItemEditor
{

    [CreateAssetMenu(fileName = "StaticItemData", menuName = "Holylib/StaticItemData")]
    public class StaticItemData : ScriptableObject
    {
        [SerializeField] private string _id;
        public string ID => _id;
        [SerializeField] private string _name;
        public string Name => _name;
        [SerializeField] private Sprite _sprite;
        public Sprite Sprite => _sprite;
        [SerializeField] private bool _isInGame;
        public bool IsIngame => _isInGame;

        public void InitializeValues(string id,string name)
        {
            _id = id;
            _name = name;
            _isInGame = true;
        }
    }

}