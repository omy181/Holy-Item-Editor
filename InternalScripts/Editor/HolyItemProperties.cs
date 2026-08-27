#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class HolyItemProperties
    {
        private VisualElement _containerParent;
        private Image _itemImage;
        private Label _itemLabel;
        private Action<string> _refreshList;
        public HolyItemProperties(VisualElement containerParent, Image itemImage,Label itemName,Action<string> refreshList)
        {
            _itemImage = itemImage;
            _itemLabel = itemName;
            _containerParent = containerParent;
            _refreshList = refreshList;
            PreviewItem();
        }

        public void PreviewItem(SerializedObject serializedObject, ItemListElement itemListElement)
        {
            var container = new InspectorElement(serializedObject);

            _containerParent.Clear();
            _containerParent.Add(container);

            _itemImage.sprite = itemListElement.Icon;
            _itemLabel.text = itemListElement.Name;

            var saveButton = new Button();
            saveButton.text = "Save Changes";
            saveButton.RegisterCallback<MouseUpEvent>((e) => _saveChanges(serializedObject, itemListElement));
            _containerParent.Add(saveButton);
        }

        public void PreviewItem()
        {
            _containerParent.Clear();
            _itemImage.sprite = null;
            _itemLabel.text = "No item selected";
        }

        private void _saveChanges(SerializedObject serializedObject, ItemListElement itemListElement)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssets();

            _refreshList(itemListElement.ID);

            Debug.Log(itemListElement.Name + " updated");
        }
    }

}
#endif