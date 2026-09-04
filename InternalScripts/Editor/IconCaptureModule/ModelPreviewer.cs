#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class ModelPreviewer
    {
        // UI
        private Image _previewImage;

        // Preview scene
        private Scene _previewScene;
        private Camera _previewCamera;
        private Light _previewLight;
        private GameObject _modelInstance;
        private RenderTexture _renderTexture;

        // Framing
        private Vector3 _modelCenter;
        private float _baseDistance = 3f;

        // Settings
        private ItemCaptureSettings _settings;
        private int IconTextureSize;
        private string _captureIconPath;
        private Action<Sprite> _onIconCaptured;
        private ModelPreviewerTouchController _touchController;
        private string _itemName;

        public ModelPreviewer(Image image,GameObject itemModel, string itemName, ItemCaptureSettings settings,int iconTextureSize,string captureIconPath, Action<Sprite> onIconCaptured,Action updateControlsUI)
        {
            _previewImage = image;
            _settings = settings;
            IconTextureSize = iconTextureSize;
            _onIconCaptured = onIconCaptured;
            _captureIconPath = captureIconPath;
            _itemName = itemName;

            if (itemModel != null)
            {
                _cameraModelSetup(itemModel);
            }
            else {
                _previewImage.style.display = DisplayStyle.None;
            }

            _touchController = new(_previewImage,settings, _previewCamera.transform, UpdateCameraTransform, updateControlsUI);
        }

        private void _cameraModelSetup(GameObject itemModel)
        {
            // A preview scene keeps everything we spawn here completely isolated
            // from whatever scene the user actually has open.
            _previewScene = EditorSceneManager.NewPreviewScene();

            // Camera
            var cameraGO = new GameObject("IconCaptureCamera");
            SceneManager.MoveGameObjectToScene(cameraGO, _previewScene);
            _previewCamera = cameraGO.AddComponent<Camera>();
            _previewCamera.scene = _previewScene; // restrict rendering to objects in the preview scene
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _previewCamera.orthographic = false;
            _previewCamera.fieldOfView = 30f;
            _previewCamera.nearClipPlane = 0.01f;
            _previewCamera.farClipPlane = 1000f;
            _previewCamera.allowMSAA = true;

            // Light
            var lightGO = new GameObject("IconCaptureLight");
            SceneManager.MoveGameObjectToScene(lightGO, _previewScene);
            _previewLight = lightGO.AddComponent<Light>();
            _previewLight.type = LightType.Directional;
            _previewLight.intensity = 1.1f;
            _previewLight.shadows = LightShadows.None;
            lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Model
            _modelInstance = UnityEngine.Object.Instantiate(itemModel);
            _modelInstance.name = itemModel.name;
            SceneManager.MoveGameObjectToScene(_modelInstance, _previewScene);
            _modelInstance.transform.position = _modelCenter;
            _modelInstance.transform.rotation = Quaternion.identity;

            // Render target
            _renderTexture = new RenderTexture(IconTextureSize, IconTextureSize, 16, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 2
            };
            _renderTexture.Create();
            _previewCamera.targetTexture = _renderTexture;

            _previewImage.image = _renderTexture;

            _frameModel();
            UpdateCameraTransform();
        }

        // Computes the model's bounds so the camera starts at a sensible distance,
        // and stores the pivot point that pitch/yaw/offset all operate around.
        private void _frameModel()
        {
            var renderers = _modelInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                _modelCenter = _modelInstance.transform.position;
                _baseDistance = 3f;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            _modelCenter = bounds.center;
            _baseDistance = Mathf.Max(bounds.extents.magnitude * 2.2f, 0.1f);
        }

        public void UpdateCameraTransform()
        {
            if (_previewCamera == null || _modelInstance == null)
                return;

            float horizontal = Mathf.Lerp(-1f, 1f, _settings.HorizontalOffset);
            float vertical = Mathf.Lerp(-1f, 1f, _settings.VerticalOffset);

            float zoom = Mathf.Lerp(0.5f, 2f, _settings.ZoomFactor);

            _modelInstance.transform.rotation = _settings.Rotation;

            Quaternion cameraRotation = Quaternion.identity;
            Vector3 forward = cameraRotation * Vector3.forward;
            Vector3 right = cameraRotation * Vector3.right;
            Vector3 up = cameraRotation * Vector3.up;

            float distance = _baseDistance * zoom;

            Vector3 target = _modelCenter
                            + right * horizontal
                            + up * vertical;

            _previewCamera.transform.position = target - forward * distance;
            _previewCamera.transform.rotation = cameraRotation;

            _previewCamera.Render();
            _previewImage?.MarkDirtyRepaint();
        }

        public void SaveIcon()
        {
            if (_previewCamera == null || _renderTexture == null)
            {
                Debug.LogError("IconCaptureModule: preview camera isn't set up, can't save icon.");
                return;
            }

            _previewCamera.Render();

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = _renderTexture;

            var texture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;

            byte[] pngData = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            string directory = Path.Combine(
                Directory.GetCurrentDirectory(),
                _captureIconPath
            );

            Directory.CreateDirectory(directory);

            string fileName = $"{_itemName}.png";
            string fullPath = Path.Combine(directory, fileName);

            string assetPath = $"{_captureIconPath}/{fileName}";

            File.WriteAllBytes(fullPath, pngData);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            _onIconCaptured?.Invoke(iconSprite);
        }
        public void Dispose()
        {
            if (_renderTexture != null)
            {
                if (_previewCamera != null)
                    _previewCamera.targetTexture = null;

                _renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }

            if (_previewCamera)
            {
                UnityEngine.Object.DestroyImmediate(_previewCamera);
                _previewCamera = null;
            }

            if (_modelInstance)
            {
                UnityEngine.Object.DestroyImmediate(_modelInstance);
                _modelInstance = null;
            }

            if (_previewScene.IsValid())
                EditorSceneManager.ClosePreviewScene(_previewScene);
        }
    }
}
#endif