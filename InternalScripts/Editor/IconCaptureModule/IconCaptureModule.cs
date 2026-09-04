#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{

    public class IconCaptureModule
    {
        private const int IconTextureSize = 256;
        private string _captureIconPath;
        private Action<Sprite> _onIconCaptured;
        private ItemCaptureSettings _settings;
        private ModelPreviewer _modelPreviewer;
        private Action _updateControls;

        public IconCaptureModule(VisualElement root, GameObject itemModel,string itemName,ItemCaptureSettings settings, Action<Sprite> onIconCaptured, string captureIconPath,out Action disposeUI,bool hideUpdateSpriteButton = false)
        {
            if(itemModel == null)
            {
                _nothingToShowView(root);

                disposeUI = null;
            }
            else
            {
                _captureIconPath = captureIconPath;
                _onIconCaptured = onIconCaptured;
                _settings = settings;

                _instantiateUI(root);
                _makeUIElementConnections(root, out Image previewImage, hideUpdateSpriteButton);
                _modelPreviewer = new(previewImage, itemModel,itemName, _settings, IconTextureSize, _captureIconPath, _onIconCaptured, _updateControls);

                disposeUI = _modelPreviewer.Dispose;
            }
        }

        private void _instantiateUI(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/dev.holyperson.holyitemeditor/InternalScripts/Editor/IconCaptureModule/IconCaptureUI.uxml");

            var uxmlRoot = visualTree.Instantiate();
            root.Add(uxmlRoot);
        }
        private void _makeUIElementConnections(VisualElement root, out Image previewImage,bool hideUpdateButton)
        {
            previewImage = root.Q<Image>("IconPreview");
            Button updateIconButton = root.Q<Button>("UpdateIcon");

            _updateControls = null; // when settings changed update ui

            _updateControls?.Invoke();

            updateIconButton.clicked += SaveIcon;

            updateIconButton.style.display = hideUpdateButton ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SaveIcon()
        {
            _modelPreviewer.SaveIcon();
        }

        public void UpdateModel(out GameObject spawnedModel)
        {
            _modelPreviewer.UpdateModel(out spawnedModel);
        }

        private void _nothingToShowView(VisualElement root)
        {
            Label nothingToPreviewLabel = new("Nothing to preview");
            nothingToPreviewLabel.style.paddingTop = 20;
            nothingToPreviewLabel.style.paddingBottom = 20;
            nothingToPreviewLabel.style.alignSelf = Align.Center;
            nothingToPreviewLabel.style.fontSize = 15;
            root.Add(nothingToPreviewLabel);
        }
    }
}
#endif