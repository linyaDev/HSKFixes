using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
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
    /// Fix: Harmony postfix on ThingDef.SpecialDisplayStats — if the def is
    /// a survival tool and req.StuffDef has StuffPropsTool, replace the
    /// StatDrawEntry values with stuff-adjusted ones.
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

        public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            if (req.Thing != null) return; // real item — SurvivalToolsLite handles it correctly
            if (req.StuffDef == null) return; // no stuff selected

            // Check if this is a survival tool
            var stToolPropsType = AccessTools.TypeByName("SurvivalToolsLite.SurvivalToolProperties");
            if (stToolPropsType == null) return;

            var toolProps = __instance.GetModExtension<DefModExtension>();
            bool isTool = false;
            List<StatModifier> baseFactors = null;

            foreach (var ext in __instance.modExtensions ?? new List<DefModExtension>())
            {
                if (stToolPropsType.IsInstanceOfType(ext))
                {
                    isTool = true;
                    var field = AccessTools.Field(stToolPropsType, "baseWorkStatFactors");
                    baseFactors = field?.GetValue(ext) as List<StatModifier>;
                    break;
                }
            }

            if (!isTool || baseFactors == null) return;

            // Get stuff tool multipliers
            var stuffPropsType = AccessTools.TypeByName("SurvivalToolsLite.StuffPropsTool");
            if (stuffPropsType == null) return;

            List<StatModifier> stuffFactors = null;
            foreach (var ext in req.StuffDef.modExtensions ?? new List<DefModExtension>())
            {
                if (stuffPropsType.IsInstanceOfType(ext))
                {
                    var field = AccessTools.Field(stuffPropsType, "toolStatFactors");
                    stuffFactors = field?.GetValue(ext) as List<StatModifier>;
                    break;
                }
            }

            if (stuffFactors == null || stuffFactors.Count == 0) return;

            // Get the ST stat category for matching
            var stCategoryType = AccessTools.TypeByName("SurvivalToolsLite.ST_StatCategoryDefOf");
            if (stCategoryType == null) return;
            var survToolCategory = AccessTools.Field(stCategoryType, "SurvivalTool")?.GetValue(null) as StatCategoryDef;
            if (survToolCategory == null) return;

            // Replace entries with stuff-adjusted values
            var resultList = __result.ToList();
            for (int i = 0; i < resultList.Count; i++)
            {
                var entry = resultList[i];
                if (entry.category != survToolCategory) continue;

                // Find matching base factor by label
                foreach (var baseFactor in baseFactors)
                {
                    if (entry.LabelCap == baseFactor.stat.LabelCap)
                    {
                        // Find stuff multiplier for this stat
                        float stuffMult = 1f;
                        foreach (var sf in stuffFactors)
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
                            resultList[i] = new StatDrawEntry(
                                survToolCategory,
                                baseFactor.stat.LabelCap,
                                adjustedValue.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                                baseFactor.stat.description,
                                99999);
                        }
                        break;
                    }
                }
            }

            __result = resultList;
        }
    }
}
