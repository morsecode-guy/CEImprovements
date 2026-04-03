using MelonLoader;
using Il2CppCraftEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(CEImprovements.CEImprovementsMod), "CEImprovements", "1.1.0", "Morse Code Guy")]
[assembly: MelonGame("Stonext Games", "Flyout")]

namespace CEImprovements;

public class CEImprovementsMod : MelonMod
{
    readonly FpsCameraController _fps = new();
    readonly ZoomController _zoom = new();
    readonly FovController _fov = new();
    readonly AutoFocusController _autoFocus = new();
    readonly RotationClamper _rotClamp = new();

    internal static bool ModEnabled = true;

    public override void OnUpdate()
    {
        if (CEManager.instance == null) return;
        var cam = CEManager.instance.camera;
        if (cam == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // End key toggles entire mod
        if (kb.endKey.wasPressedThisFrame)
        {
            ModEnabled = !ModEnabled;
            MelonLogger.Msg($"CEImprovements {(ModEnabled ? "enabled" : "disabled")}");
            if (!ModEnabled && _fps.InFpsMode)
                _fps.Exit(cam, _zoom);
        }

        // F6 toggles auto-focus on selected part
        if (kb.f6Key.wasPressedThisFrame)
        {
            _autoFocus.Toggle();
            MelonLogger.Msg($"Auto-focus {(_autoFocus.Enabled ? "on" : "off")}");
        }

        if (!ModEnabled) return;
        if (InputHelper.IsInputFieldFocused()) return;

        bool cseOpen = InputHelper.IsCrossSectionEditorOpen();

        _fov.Update(cam, kb);

        // in CSE: only allow zoom and FOV, no WASD/FPS
        if (cseOpen) return;

        _fps.CheckEnter(cam, kb);
    }

    public override void OnLateUpdate()
    {
        if (CEManager.instance == null) return;
        var cam = CEManager.instance.camera;
        if (cam == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        bool cseOpen = InputHelper.IsCrossSectionEditorOpen();

        // exit fps if we just entered CSE or mod was disabled or started typing
        if ((cseOpen || !ModEnabled || InputHelper.IsInputFieldFocused()) && _fps.InFpsMode)
        {
            _fps.Exit(cam, _zoom);
            return;
        }

        if (!ModEnabled) return;
        if (InputHelper.IsInputFieldFocused()) return;

        // zoom works everywhere including CSE
        _zoom.Update(cam, kb, _fps.InFpsMode);

        if (cseOpen)
        {
            _fov.ApplyFov(cam);
            _rotClamp.Clamp(cam);
            return;
        }

        _fps.UpdateMovement(cam, kb, _zoom);
        _fov.ApplyFov(cam);

        // auto-focus on selected part
        if (!_fps.InFpsMode)
            _autoFocus.Update(cam);

        // clamp vertical rotation so you cant flip over the top
        if (!_fps.InFpsMode)
            _rotClamp.Clamp(cam);
    }
}
