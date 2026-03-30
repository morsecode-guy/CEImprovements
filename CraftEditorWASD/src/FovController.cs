using Il2CppCraftEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CraftEditorWASD;

// fov adjustment via numpad +/-, regular +/-, or ctrl+scroll
internal class FovController
{
    const float MinFov = 5f;
    const float MaxFov = 160f;
    const float FovStep = 2f;
    const float FovScrollFactor = 3f;

    internal void Update(CECamera cam, Keyboard kb)
    {
        bool ctrlHeld = kb.ctrlKey.isPressed;
        float fovDelta = 0f;

        // keyboard fov: numpad or regular plus/minus keys
        if (kb.numpadPlusKey.isPressed || kb.equalsKey.isPressed)
            fovDelta += FovStep;
        if (kb.numpadMinusKey.isPressed || kb.minusKey.isPressed)
            fovDelta -= FovStep;

        // ctrl+scroll for fov too
        var mouse = Mouse.current;
        if (ctrlHeld && mouse != null)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollY) > 0.1f)
                fovDelta += scrollY > 0f ? FovScrollFactor : -FovScrollFactor;
        }

        if (Mathf.Abs(fovDelta) > 0.01f)
        {
            float currentFov = cam.camera.fieldOfView;
            cam.camera.fieldOfView = Mathf.Clamp(currentFov + fovDelta, MinFov, MaxFov);
        }
    }
}
