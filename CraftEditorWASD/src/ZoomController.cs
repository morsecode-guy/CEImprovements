using CraftEditorWASD.Patches;
using Il2CppCraftEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CraftEditorWASD;

// smooth zoom that bypasses the game's built-in clamp (~25) via harmony patch
// we read scroll input ourselves and smoothdamp to the target
internal class ZoomController
{
    const float MinZoom = 0.3f;
    const float MaxZoom = 5000f;
    const float ZoomSmoothTime = 0.2f;
    const float ScrollZoomFactor = 0.08f;

    // -1 means not initialized yet, will grab from game on first frame
    float _targetZoom = -1f;
    float _currentZoom = -1f;
    float _zoomVelocity;

    internal void Update(CECamera cam, Keyboard kb, bool inFpsMode)
    {
        // in fps mode the camera is right on the pivot, keep it tiny
        if (inFpsMode)
        {
            CECameraZoomPatch.OverrideZoom = 0.001f;
            return;
        }

        // first frame — sync with whatever the game has
        if (_targetZoom < 0f)
        {
            _currentZoom = cam.zoom;
            _targetZoom = cam.zoom;
        }

        // read scroll directly, skip if ctrl is held (thats fov)
        var mouse = Mouse.current;
        if (mouse != null && !kb.ctrlKey.isPressed)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > 0.1f)
            {
                float scrollDir = scrollY > 0f ? -1f : 1f;
                _targetZoom *= 1f + scrollDir * ScrollZoomFactor;
                _targetZoom = Mathf.Clamp(_targetZoom, MinZoom, MaxZoom);
            }
        }

        _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _zoomVelocity, ZoomSmoothTime);
        _currentZoom = Mathf.Clamp(_currentZoom, MinZoom, MaxZoom);

        // push our zoom to both the game field and the harmony override
        cam.zoom = _currentZoom;
        cam.zoomVel = 0f;
        CECameraZoomPatch.OverrideZoom = _currentZoom;
    }

    internal void Reset(float zoom)
    {
        _targetZoom = zoom;
        _currentZoom = zoom;
        _zoomVelocity = 0f;
        CECameraZoomPatch.OverrideZoom = zoom;
    }
}
