#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
        private IItemListElement _currentPreviewedItem;
        private IVisualElementScheduledItem _autoSaveScheduledItem;
        private bool _suppressAutoSave;
        private bool _isAutoSaveEnabled = true;
        private string _autoSaveKey = "AutoSaveItems";
        public HolyItemProperties(VisualElement containerParent, Image itemImage,Label itemName,Action<string> refreshList,Toggle autoSave)
        {
            _itemImage = itemImage;
            _itemLabel = itemName;
            _containerParent = containerParent;
            _refreshList = refreshList;

            _containerParent.RegisterCallback<SerializedPropertyChangeEvent>(_onPropertyChanged);

            autoSave.value = EditorPrefs.GetBool(_autoSaveKey, _isAutoSaveEnabled);
            autoSave.RegisterValueChangedCallback((e)=>_onAutoSaveChanged(e.newValue));

            PreviewItem();
        }

        private void _onAutoSaveChanged(bool state)
        {
            EditorPrefs.SetBool(_autoSaveKey, state);
            _isAutoSaveEnabled = state;
        }

        private void _onPropertyChanged(SerializedPropertyChangeEvent evt)
        {
            if (_currentPreviewedItem == null) return;
            if (_suppressAutoSave) return;
            if (!_isAutoSaveEnabled) return;

            _autoSaveScheduledItem?.Pause();
            _autoSaveScheduledItem = _containerParent.schedule.Execute(()=>SaveChanges(false)).StartingIn(300);
        }

        public void PreviewItem(IItemListElement itemListElement)
        {
            if(itemListElement == null)
            {
                PreviewItem();
                return;
            }

            _autoSaveScheduledItem?.Pause();
            _suppressAutoSave = true;


            ElementPreviewData elementPreviewData = itemListElement.PreviewElement();

            _containerParent.Clear();

            _itemImage.sprite = itemListElement.GetValues().Icon;
            _itemImage.style.display = _itemImage.sprite == null ? DisplayStyle.None : DisplayStyle.Flex;

            _itemLabel.text = itemListElement.GetValues().Name;

            _currentPreviewedItem = itemListElement;

            _containerParent.Add(elementPreviewData.PropertyInspector);

            _containerParent.schedule.Execute(() => _suppressAutoSave = false).StartingIn(150);
        }

        public void PreviewItem()
        {
            _containerParent.Clear();
            _itemImage.sprite = null;
            _itemImage.style.display = DisplayStyle.None;
            _itemLabel.text = "No item selected";
            _currentPreviewedItem = null;
        }

        public void SaveChanges(bool logOnSave)
        {
            if (_currentPreviewedItem == null) return;

            _saveChanges(_currentPreviewedItem.PreviewElement().SerializeObjectsToSave, _currentPreviewedItem, logOnSave);
        }

        private void _saveChanges(SerializedObject[] serializedObjects, IItemListElement itemListElement,bool logOnSave)
        {
            foreach (var serializedObject in serializedObjects)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
            
            AssetDatabase.SaveAssets();

            _refreshList(itemListElement.GetValues().ID);

            if(logOnSave)
            Debug.Log("Changes saved for "+itemListElement.GetValues().Name);
        }
    }

}
#endif