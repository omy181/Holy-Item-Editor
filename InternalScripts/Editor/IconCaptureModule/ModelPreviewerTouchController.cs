#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class ModelPreviewerTouchController
    {
        private bool _isDraggingLMBPreview;
        private bool _isDraggingRMBPreview;
        private Vector2 _lastPointerPosition;
        private Vector2 _dragStartPosition;
        private Quaternion _dragStartRotation;
        private Image _previewImage;
        private ItemCaptureSettings _settings;
        private Action _updateCamera;
        private Action _updateSliders;
        private Transform _previewCamera;
        public ModelPreviewerTouchController(Image previewImage, ItemCaptureSettings settings,Transform camera,Action updateCamera,Action updateSliders)
        {
            _settings = settings;
            _previewImage = previewImage;

            _updateCamera = updateCamera;
            _updateSliders = updateSliders;

            _previewCamera = camera;

            _previewImage.RegisterCallback<PointerDownEvent>(_onPreviewPointerDown);
            _previewImage.RegisterCallback<PointerMoveEvent>(_onPreviewPointerMove);
            _previewImage.RegisterCallback<PointerUpEvent>(_onPreviewPointerUp);
            _previewImage.RegisterCallback<WheelEvent>(_onPreviewScroll);

        }

        private void _onPreviewPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isDraggingLMBPreview = true;
                _isDraggingRMBPreview = false;
            }
            
            if (evt.button == 1)
            {
                _isDraggingRMBPreview = true;
                _isDraggingLMBPreview = false;
            }
            
            if(evt.button != 0 && evt.button != 1)
            {
                return;
            }

            _lastPointerPosition = evt.position;
            _dragStartPosition = evt.position;
            _dragStartRotation = _settings.Rotation;

            _previewImage.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void _onPreviewPointerMove(PointerMoveEvent evt)
        {
            if (!_isDraggingLMBPreview && !_isDraggingRMBPreview) return;

            Vector2 movementDelta = (Vector2)evt.position - _lastPointerPosition;
            _lastPointerPosition = evt.position;

            Vector2 rotationDelta = (Vector2)evt.position - _dragStartPosition;
            

            if (_isDraggingLMBPreview)
            {
                const float rotateSensitivity = 0.5f;

                Vector3 right = -_previewCamera.transform.right;
                Vector3 up = _previewCamera.transform.up;

                Quaternion horizontal =
                    Quaternion.AngleAxis(-rotationDelta.x * rotateSensitivity, up);

                Quaternion vertical =
                    Quaternion.AngleAxis(rotationDelta.y * rotateSensitivity, right);

                _settings.Rotation = horizontal * vertical * _dragStartRotation;
            }

            if (_isDraggingRMBPreview)
            {
                const float moveSensitivity = 0.005f;

                _settings.HorizontalOffset -= movementDelta.x * moveSensitivity;
                _settings.VerticalOffset += movementDelta.y * moveSensitivity;

                _settings.HorizontalOffset = Mathf.Clamp01(_settings.HorizontalOffset);
                _settings.VerticalOffset = Mathf.Clamp01(_settings.VerticalOffset);
            }

            _updateCamera();
            _updateSliders?.Invoke();

            evt.StopPropagation();
        }

        private void _onPreviewPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                _isDraggingLMBPreview = false;
            }
            
            if (evt.button == 1)
            {
                _isDraggingRMBPreview = false;
            }

            if(evt.button != 0 && evt.button != 1)
            {
                return;
            }

            if (_previewImage.HasPointerCapture(evt.pointerId))
                _previewImage.ReleasePointer(evt.pointerId);

            evt.StopPropagation();
        }

        private void _onPreviewScroll(WheelEvent evt)
        {
            const float zoomSensitivity = 0.05f;

            _settings.ZoomFactor += evt.delta.y * zoomSensitivity;

            _settings.ZoomFactor = Mathf.Clamp01(_settings.ZoomFactor);

            _updateCamera();
            _updateSliders?.Invoke();

            evt.StopPropagation();
        }

    }
}
#endif