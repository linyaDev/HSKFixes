using System.Collections.Generic;
using System.Linq;
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
            if (req.Thing != null) return;
            if (req.StuffDef == null) return;

            var toolProps = __instance.GetModExtension<SurvivalToolProperties>();
            if (toolProps?.baseWorkStatFactors == null) return;

            var stuffProps = req.StuffDef.GetModExtension<StuffPropsTool>();
            if (stuffProps?.toolStatFactors == null || stuffProps.toolStatFactors.Count == 0) return;

            var resultList = __result.ToList();
            for (int i = 0; i < resultList.Count; i++)
            {
                var entry = resultList[i];
                if (entry.category != ST_StatCategoryDefOf.SurvivalTool) continue;

                foreach (var baseFactor in toolProps.baseWorkStatFactors)
                {
                    if (entry.LabelCap == baseFactor.stat.LabelCap)
                    {
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
                            resultList[i] = new StatDrawEntry(
                                ST_StatCategoryDefOf.SurvivalTool,
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
