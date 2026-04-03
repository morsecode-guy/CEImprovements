using Il2CppCraftEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CEImprovements;

// fov adjustment via numpad +/-, regular +/-, or ctrl+scroll
// ctrl+middle click resets to default
// syncs from game when not actively adjusting so menu changes arent overwritten
internal class FovController
{
    const float MinFov = 5f;
    const float MaxFov = 160f;
    const float FovStep = 2f;
    const float FovScrollFactor = 3f;
    const float DefaultFov = 60f;

    float _targetFov = -1f;
    bool _adjustingThisFrame; // true only on frames where mod fov input is active

    internal void Update(CECamera cam, Keyboard kb)
    {
        _adjustingThisFrame = false;

        // sync on first frame
        if (_targetFov < 0f)
            _targetFov = cam.camera.fieldOfView;

        bool ctrlHeld = kb.ctrlKey.isPressed;
        var mouse = Mouse.current;

        // ctrl+middle click resets fov to default
        if (ctrlHeld && mouse != null && mouse.middleButton.wasPressedThisFrame)
        {
            _targetFov = DefaultFov;
            _adjustingThisFrame = true;
            return;
        }

        float fovDelta = 0f;

        // keyboard fov: numpad or regular plus/minus keys (minus = wider, plus = narrower)
        if (kb.numpadPlusKey.isPressed || kb.equalsKey.isPressed)
            fovDelta -= FovStep;
        if (kb.numpadMinusKey.isPressed || kb.minusKey.isPressed)
            fovDelta += FovStep;

        // ctrl+scroll for fov too
        if (ctrlHeld && mouse != null)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > 0.1f)
                fovDelta += scrollY > 0f ? -FovScrollFactor : FovScrollFactor;
        }

        if (Mathf.Abs(fovDelta) > 0.01f)
        {
            _targetFov = Mathf.Clamp(_targetFov + fovDelta, MinFov, MaxFov);
            _adjustingThisFrame = true;
        }
        else
        {
            // not adjusting this frame — sync from game so menu/settings changes stick
            _targetFov = cam.camera.fieldOfView;
        }
    }

    // only override game fov on frames where mod input is active
    internal void ApplyFov(CECamera cam)
    {
        if (_adjustingThisFrame && _targetFov > 0f)
            cam.camera.fieldOfView = _targetFov;
    }
}
