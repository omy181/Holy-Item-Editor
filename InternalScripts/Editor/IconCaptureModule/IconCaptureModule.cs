#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{

    public class IconCaptureModule
    {
        private const int IconTextureSize = 256;
        private string _captureIconPath;
        private Action<Sprite> _onIconCaptured;

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

        ItemCaptureSettings _settings;

        public IconCaptureModule(VisualElement root, GameObject itemModel, ItemCaptureSettings settings, Action<Sprite> onIconCaptured, string captureIconPath,out Action disposeUI)
        {
            _captureIconPath = captureIconPath;
            _onIconCaptured = onIconCaptured;
            _settings = settings;

            _instantiateUI(root);
            _makeUIElementConnections(root);
            _cameraModelSetup(itemModel);

            disposeUI = Dispose;
        }

        private void _instantiateUI(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/dev.holyperson.holyitemeditor/InternalScripts/Editor/IconCaptureModule/IconCaptureUI.uxml");

            var uxmlRoot = visualTree.Instantiate();
            root.Add(uxmlRoot);
        }
        private void _makeUIElementConnections(VisualElement root)
        {
            _previewImage = root.Q<Image>("IconPreview");
            Button updateIconButton = root.Q<Button>("UpdateIcon");
            Slider pitchSlider = root.Q<Slider>("PitchSlider");
            Slider yawSlider = root.Q<Slider>("YawSlider");
            Slider rollSlider = root.Q<Slider>("RollSlider");
            Slider horizontalSlider = root.Q<Slider>("HorizontalSlider");
            Slider verticalSlider = root.Q<Slider>("VerticalSlider");
            Slider zoomSlider = root.Q<Slider>("ZoomSlider");

            pitchSlider.value = _settings.Pitch;
            yawSlider.value = _settings.Yaw;
            rollSlider.value = _settings.Roll;
            horizontalSlider.value = _settings.HorizontalOffset;
            verticalSlider.value = _settings.VerticalOffset;
            zoomSlider.value = _settings.ZoomFactor;

            pitchSlider.RegisterValueChangedCallback((e) => _rotate(e.newValue, _settings.Yaw, _settings.Roll));
            yawSlider.RegisterValueChangedCallback((e) => _rotate(_settings.Pitch, e.newValue, _settings.Roll));
            rollSlider.RegisterValueChangedCallback((e) => _rotate(_settings.Pitch, _settings.Yaw, e.newValue));
            horizontalSlider.RegisterValueChangedCallback((e) => _offset(e.newValue, _settings.VerticalOffset));
            verticalSlider.RegisterValueChangedCallback((e) => _offset(_settings.HorizontalOffset, e.newValue));
            zoomSlider.RegisterValueChangedCallback((e) => _zoom(e.newValue));

            updateIconButton.clicked += _saveIcon;
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
            _modelInstance.transform.position = Vector3.zero;
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
            _updateCameraTransform();
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

        private void _rotate(float pitch, float yaw,float roll)
        {
            _settings.Pitch = pitch;
            _settings.Yaw = yaw;
            _settings.Roll = roll;
            _updateCameraTransform();
        }

        private void _offset(float horizontalOffset, float verticalOffset)
        {
            _settings.HorizontalOffset = horizontalOffset;
            _settings.VerticalOffset = verticalOffset;
            _updateCameraTransform();
        }

        private void _zoom(float zoom)
        {
            _settings.ZoomFactor = zoom;
            _updateCameraTransform();
        }

        private void _updateCameraTransform()
        {
            if (_previewCamera == null)
                return;

            float pitch = Mathf.Lerp(0, 360f, _settings.Pitch);
            float yaw = Mathf.Lerp(0f, 360f, _settings.Yaw);
            float roll = Mathf.Lerp(0f, 360f, _settings.Roll);

            float horizontal = Mathf.Lerp(-1f, 1f, _settings.HorizontalOffset);
            float vertical = Mathf.Lerp(-1f, 1f, _settings.VerticalOffset);

            float zoom = Mathf.Lerp(0.5f, 2f, _settings.ZoomFactor);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, roll);
            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            float distance = _baseDistance * zoom;

            // pan the look-at point in camera space, so offset always moves the
            // model left/right/up/down on screen regardless of current rotation
            Vector3 pannedTarget = _modelCenter + right * horizontal + up * vertical;

            _previewCamera.transform.position = pannedTarget - forward * distance;
            _previewCamera.transform.rotation = rotation;

            _previewCamera.Render();
            _previewImage?.MarkDirtyRepaint();
        }

        private void _saveIcon()
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

            string fileName = $"{_modelInstance.name}.png";
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

        // Not part of the original stub, but important: the preview scene, its
        // camera/light/model, and the RenderTexture are native resources that
        // won't be cleaned up automatically. Call this from whatever owns this
        // module (e.g. the parent EditorWindow's OnDisable) once you're done.
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