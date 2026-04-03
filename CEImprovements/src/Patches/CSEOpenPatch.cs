using HarmonyLib;
using Il2CppCraftEditor;

namespace CEImprovements.Patches;

// track whether the cross section editor is open so we can disable WASD
[HarmonyPatch(typeof(CEManager))]
public static class CSEOpenPatch
{
    internal static bool IsOpen;

    [HarmonyPatch("OpenCSE")]
    [HarmonyPostfix]
    static void OnOpen() => IsOpen = true;

    [HarmonyPatch("QuitCSE")]
    [HarmonyPostfix]
    static void OnQuit() => IsOpen = false;
}
