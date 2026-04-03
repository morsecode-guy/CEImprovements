using Il2CppCraftEditor;
using UnityEngine;

namespace CEImprovements;

// auto-focus camera pivot on the currently selected part (toggle with F6)
internal class AutoFocusController
{
    const float LerpSpeed = 8f;

    internal bool Enabled { get; private set; }

    internal void Toggle() => Enabled = !Enabled;

    internal void Update(CECamera cam)
    {
        if (!Enabled) return;

        var mgr = CEManager.instance;
        if (mgr == null) return;

        // Target is the currently selected/highlighted part
        var part = mgr.Target;
        if (part == null) return;

        var targetPos = part.transform.position;
        cam.pivot = Vector3.Lerp(cam.pivot, targetPos, Time.deltaTime * LerpSpeed);
    }
}
