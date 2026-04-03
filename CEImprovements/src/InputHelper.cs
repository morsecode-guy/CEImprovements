using Il2CppTMPro;
using UnityEngine.EventSystems;

namespace CEImprovements;

internal static class InputHelper
{
    internal static bool IsInputFieldFocused()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        var selected = eventSystem.currentSelectedGameObject;
        if (selected == null)
            return false;

        return selected.GetComponent<TMP_InputField>() != null;
    }

    internal static bool IsCrossSectionEditorOpen() => Patches.CSEOpenPatch.IsOpen;

    // true when the mouse pointer is over any UI element (panels, buttons, etc)
    internal static bool IsPointerOverUI()
    {
        var eventSystem = EventSystem.current;
        return eventSystem != null && eventSystem.IsPointerOverGameObject();
    }
}
