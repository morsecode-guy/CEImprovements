using HarmonyLib;
using Il2CppCraftEditor;

namespace CEImprovements.Patches;

// runs right after the game clamps zoom — we force our own value
// and reposition the camera so the built-in clamp is fully bypassed
[HarmonyPatch(typeof(CECamera), "LateUpdate")]
public static class CECameraZoomPatch
{
    // negative means no override active
    internal static float OverrideZoom = -1f;

    static void Postfix(CECamera __instance)
    {
        if (OverrideZoom > 0f)
        {
            __instance.zoom = OverrideZoom;
            var t = __instance.camera.transform;
            // push camera back from pivot by our zoom distance
            t.position = __instance.pivot - t.forward * OverrideZoom;
        }
    }
}
