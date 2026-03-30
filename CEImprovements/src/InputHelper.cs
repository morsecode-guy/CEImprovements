using Il2CppTMPro;
using UnityEngine.EventSystems;

namespace CEImprovements;

// checks if the user is typing in a text box so we dont eat their keystrokes
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
}
