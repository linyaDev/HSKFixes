using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using SurvivalToolsLite;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Fixes SurvivalToolsLite info window not updating stats when material changes.
    ///
    /// SurvivalToolsLite patches ThingDef.SpecialDisplayStats to show tool work
    /// stat factors. But when req.Thing == null (info window before crafting),
    /// it only shows baseWorkStatFactors without multiplying by StuffPropsTool
    /// toolStatFactors for the selected material.
    ///
    /// Fix: Harmony postfix on ThingDef.SpecialDisplayStats — wraps the result
    /// IEnumerable to adjust SurvivalTool stat entries with stuff multipliers.
    /// Uses yield return to preserve lazy enumeration chain for other postfixes.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_ToolStatsDisplay
    {
        static Fix_ToolStatsDisplay()
        {
            var harmony = new Harmony("linya.hskfixes.toolstatsdisplay");

            harmony.Patch(
                AccessTools.Method(typeof(ThingDef), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(Fix_ToolStatsDisplay), nameof(Postfix)));

            Log.Message("[HSKFixes] Fix_ToolStatsDisplay applied.");
        }

        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> __result,
            ThingDef __instance,
            StatRequest req)
        {
            // Early exit conditions — pass through original results unchanged
            if (req.Thing != null || req.StuffDef == null)
            {
                foreach (var entry in __result)
                    yield return entry;
                yield break;
            }

            var toolProps = __instance.GetModExtension<SurvivalToolProperties>();
            if (toolProps?.baseWorkStatFactors == null)
            {
                foreach (var entry in __result)
                    yield return entry;
                yield break;
            }

            var stuffProps = req.StuffDef.GetModExtension<StuffPropsTool>();

            // Pass through each entry, adjusting SurvivalTool entries if needed
            foreach (var entry in __result)
            {
                // Only modify entries in SurvivalTool category
                if (stuffProps?.toolStatFactors != null
                    && stuffProps.toolStatFactors.Count > 0
                    && entry.category == ST_StatCategoryDefOf.SurvivalTool)
                {
                    // Try to find matching base stat and stuff multiplier
                    StatDrawEntry adjusted = TryAdjustEntry(entry, toolProps, stuffProps);
                    yield return adjusted ?? entry;
                }
                else
                {
                    yield return entry;
                }
            }
        }

        private static StatDrawEntry TryAdjustEntry(
            StatDrawEntry entry,
            SurvivalToolProperties toolProps,
            StuffPropsTool stuffProps)
        {
            foreach (var baseFactor in toolProps.baseWorkStatFactors)
            {
                if (entry.LabelCap != baseFactor.stat.LabelCap) continue;

                float stuffMult = 1f;
                foreach (var sf in stuffProps.toolStatFactors)
                {
                    if (sf.stat == baseFactor.stat)
                    {
                        stuffMult = sf.value;
                        break;
                    }
                }

                if (stuffMult != 1f)
                {
                    float adjustedValue = baseFactor.value * stuffMult;
                    return new StatDrawEntry(
                        ST_StatCategoryDefOf.SurvivalTool,
                        baseFactor.stat.LabelCap,
                        adjustedValue.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        baseFactor.stat.description,
                        99999);
                }
                break;
            }
            return null;
        }
    }
}
