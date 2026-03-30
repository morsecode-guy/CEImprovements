using MelonLoader;
using Il2CppCraftEditor;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(CraftEditorWASD.CraftEditorWASDMod), "CraftEditorWASD", "1.0.0", "Morse Code Guy")]
[assembly: MelonGame("Stonext Games", "Flyout")]

namespace CraftEditorWASD;

// adds wasd camera, fps look, smooth zoom, and fov control
// to the craft editor :3
public class CraftEditorWASDMod : MelonMod
{
    readonly FpsCameraController _fps = new();
    readonly ZoomController _zoom = new();
    readonly FovController _fov = new();

    // early update — handle discrete inputs like fov, fps entry
    public override void OnUpdate()
    {
        if (CEManager.instance == null) return;
        var cam = CEManager.instance.camera;
        if (cam == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;
        if (InputHelper.IsInputFieldFocused()) return;

        _fov.Update(cam, kb);
        _fps.CheckEnter(cam, kb);
    }

    // late update — zoom and movement need to run after the game's camera logic
    public override void OnLateUpdate()
    {
        if (CEManager.instance == null) return;
        var cam = CEManager.instance.camera;
        if (cam == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // bail out of fps if user started typing
        if (InputHelper.IsInputFieldFocused())
        {
            _fps.Exit(cam, _zoom);
            return;
        }

        _zoom.Update(cam, kb, _fps.InFpsMode);
        _fps.UpdateMovement(cam, kb, _zoom);
    }
}
