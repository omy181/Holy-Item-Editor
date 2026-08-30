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

        public void PreviewItem(ItemListElement itemListElement)
        {
            ElementPreviewData elementPreviewData = itemListElement.PreviewElement();

            _containerParent.Clear();
            _containerParent.Add(elementPreviewData.PropertyInspector);

            _itemImage.sprite = itemListElement.GetValues().Icon;
            _itemLabel.text = itemListElement.GetValues().Name;

            var saveButton = new Button();
            saveButton.text = "Save Changes";
            saveButton.RegisterCallback<MouseUpEvent>((e) => _saveChanges(elementPreviewData.SerializeObjectsToSave, itemListElement));
            _containerParent.Add(saveButton);
        }

        public void PreviewItem()
        {
            _containerParent.Clear();
            _itemImage.sprite = null;
            _itemLabel.text = "No item selected";
        }

        private void _saveChanges(SerializedObject[] serializedObjects, ItemListElement itemListElement)
        {
            foreach (var serializedObject in serializedObjects)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
            
            AssetDatabase.SaveAssets();

            _refreshList(itemListElement.GetValues().ID);

            Debug.Log(itemListElement.GetValues().Name + " updated");
        }
    }

}
#endif