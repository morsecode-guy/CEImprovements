using System;
using System.Globalization;
using System.Linq;
using Il2Cpp;
using Il2CppCraftEditor;
using Il2CppTMPro;
using UnityEngine;

namespace CEImprovements;

// augments turbine test-run TWR readout with craft-level comparisons
internal class EnginePreviewTwrController
{
    const float Gravity = 9.81f;
    const string AddedLinePrefix = "[CEI]";
    const int ExtraLineCount = 3;

    readonly CultureInfo _inv = CultureInfo.InvariantCulture;
    TMP_Text[]? _extraTwrTexts;
    TMP_Text? _sourceTwrText;

    internal void Update()
    {
        var mode = TurbineEditMode.instance;
        if (mode == null) return;

        var testRun = mode.testRunElements;
        if (testRun == null) return;

        TMP_Text twrText = testRun.twr;
        if (twrText == null) return;

        if (_sourceTwrText == null || _sourceTwrText.gameObject == null)
            _sourceTwrText = twrText;

        var extraTexts = EnsureExtraTwrTexts(_sourceTwrText, ExtraLineCount);
        if (extraTexts == null) return;

        bool hasPreviewThrust = TryGetPreviewNetThrustN(testRun, out float previewThrustN);
        if (!hasPreviewThrust)
            previewThrustN = 0f;

        bool hasCraftWeight = TryGetCraftWeightN(out float craftWeightN, twrText, previewThrustN, hasPreviewThrust);
        if (!hasCraftWeight)
            craftWeightN = 1f;

        bool hasAllEnginesThrust = TryGetAllEnginesThrustN(out float allEnginesThrustN, previewThrustN);
        if (!hasAllEnginesThrust)
            allEnginesThrustN = previewThrustN;

        float previewEngineTwr = previewThrustN / craftWeightN;
        float allEnginesTwr = allEnginesThrustN / craftWeightN;
        float engineSharePct = allEnginesTwr > 0.001f ? (previewEngineTwr / allEnginesTwr) * 100f : 0f;
        engineSharePct = Mathf.Clamp(engineSharePct, 0f, 100f);

        extraTexts[0].enabled = true;
        extraTexts[0].text =
            $"{AddedLinePrefix} Preview Engine TWR: {previewEngineTwr.ToString("0.00", _inv)}";

        extraTexts[1].enabled = true;
        extraTexts[1].text =
            $"{AddedLinePrefix} All Engines TWR: {allEnginesTwr.ToString("0.00", _inv)}";

        extraTexts[2].enabled = true;
        extraTexts[2].text =
            $"{AddedLinePrefix} This Engine Share: {engineSharePct.ToString("0.0", _inv)}%";
    }

    TMP_Text[]? EnsureExtraTwrTexts(TMP_Text source, int count)
    {
        if (_extraTwrTexts != null && _extraTwrTexts.Length == count)
        {
            bool valid = true;
            for (int i = 0; i < _extraTwrTexts.Length; i++)
            {
                if (_extraTwrTexts[i] == null || _extraTwrTexts[i].gameObject == null)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return _extraTwrTexts;
        }

        var twrRoot = source.transform.parent;
        if (twrRoot == null) return null;

        _extraTwrTexts = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            // duplicate the full TWR parent branch
            var clonedRoot = UnityEngine.Object.Instantiate(twrRoot.gameObject, twrRoot.parent);
            clonedRoot.name = $"CEI_TWR_{i + 1}";
            clonedRoot.SetActive(true);

            var clonedTwrChild = FindChildRecursive(clonedRoot.transform, "Twr");
            if (clonedTwrChild == null)
                return null;

            var clonedText = clonedTwrChild.GetComponent<TMP_Text>();
            if (clonedText == null)
                return null;

            clonedTwrChild.gameObject.SetActive(true);

            // keep the Twr text under the cloned TWR parent so the list/layout drives placement
            clonedTwrChild.name = $"CEI_ExtraTwrText_{i + 1}";
            clonedRoot.transform.SetSiblingIndex(twrRoot.GetSiblingIndex() + 1 + i);

            clonedText.enableWordWrapping = false;
            clonedText.overflowMode = TextOverflowModes.Overflow;
            clonedText.raycastTarget = false;
            clonedText.color = source.color;

            var layoutElement = clonedText.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = false;
                layoutElement.enabled = true;
            }

            _extraTwrTexts[i] = clonedText;
        }

        return _extraTwrTexts;
    }

    static Transform? FindChildRecursive(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                return child;

            var found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    static bool TryGetPreviewNetThrustN(TurbineEditMode.TestRunPanel testRun, out float thrustN)
    {
        thrustN = 0f;
        TMP_Text net = testRun.netThrust;
        if (net == null || string.IsNullOrWhiteSpace(net.text))
            return false;

        return TryParseForceToN(net.text, out thrustN);
    }

    static bool TryGetCraftWeightN(out float weightN, TMP_Text sourceTwrText, float previewThrustN, bool hasPreviewThrust)
    {
        weightN = 0f;

        if (!TryGetCraftMassKg(out float massKg))
        {
            // fallback: infer weight from the game's own TWR shown in this panel
            if (hasPreviewThrust && TryParseTwrValue(sourceTwrText.text, out float gameShownTwr) && gameShownTwr > 0.001f)
            {
                weightN = previewThrustN / gameShownTwr;
                return weightN > 0f;
            }

            return false;
        }

        weightN = massKg * Gravity;
        return weightN > 0f;
    }

    static bool TryGetAllEnginesThrustN(out float thrustN, float previewThrustN)
    {
        thrustN = 0f;
        var mgr = CEManager.instance;
        if (mgr == null) return false;

        var parts = mgr.craftParts;
        if (parts == null || parts.Length == 0) return false;

        int engineCount = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            if (p == null) continue;
            if (p.GetComponent<Il2Cpp.STurbine>() != null)
                engineCount++;
        }

        if (engineCount == 0) return false;
        thrustN = previewThrustN * engineCount;
        return thrustN > 0f;
    }

    static bool TryGetCraftMassKg(out float massKg)
    {
        massKg = 0f;
        var mgr = CEManager.instance;
        if (mgr == null) return false;

        var parts = mgr.craftParts;
        if (parts == null || parts.Length == 0) return false;

        float partsMass = 0f;
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            if (p == null) continue;
            partsMass += Mathf.Max(0f, p.mass);
        }

        // part.mass already reflects loaded part state; adding craft.fuelMass again can double-count fuel
        massKg = partsMass;
        return massKg > 0.1f;
    }

    static bool TryParseForceToN(string text, out float n)
    {
        n = 0f;
        if (!TryExtractLeadingNumber(text, out float value))
            return false;

        string t = text.ToLowerInvariant();
        float scale = 1f;
        if (t.Contains("mn")) scale = 1000000f;
        else if (t.Contains("kn")) scale = 1000f;

        n = value * scale;
        return n > 0f;
    }

    static bool TryExtractLeadingNumber(string text, out float value)
    {
        value = 0f;
        if (string.IsNullOrWhiteSpace(text)) return false;

        string cleaned = new string(text
            .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-' || c == '+')
            .ToArray())
            .Replace(",", "");

        return float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    static bool TryParseTwrValue(string text, out float twr)
    {
        twr = 0f;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        int colon = text.LastIndexOf(':');
        string candidate = colon >= 0 ? text[(colon + 1)..] : text;
        return TryExtractLeadingNumber(candidate, out twr) && twr > 0f;
    }
}