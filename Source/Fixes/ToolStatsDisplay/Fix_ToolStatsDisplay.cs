using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using SurvivalToolsLite; // Direct access via BepInEx.AssemblyPublicizer (no reflection)
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
    [StaticConstructorOnStartup] // Runs once when the game loads all mods
    public static class Fix_ToolStatsDisplay
    {
        static Fix_ToolStatsDisplay()
        {
            var harmony = new Harmony("linya.hskfixes.toolstatsdisplay");

            // Attach our Postfix to run AFTER ThingDef.SpecialDisplayStats
            // This method is called every time RimWorld draws the info window for a def
            harmony.Patch(
                AccessTools.Method(typeof(ThingDef), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(Fix_ToolStatsDisplay), nameof(Postfix)));

            Log.Message("[HSKFixes] Fix_ToolStatsDisplay applied.");
        }

        /// <summary>
        /// Runs after ThingDef.SpecialDisplayStats (and after SurvivalToolsLite's own postfix).
        /// __instance = the ThingDef being inspected (e.g. hammer def)
        /// __result   = list of stat entries shown in the info window (we modify this)
        /// req        = contains StuffDef (selected material) and Thing (null if no real item yet)
        /// </summary>
        public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            // If req.Thing != null, this is a real crafted item (e.g. Shift+click detail view)
            // SurvivalToolsLite already handles this correctly via SurvivalTool.WorkStatFactors
            if (req.Thing != null) return;

            // No material selected — nothing to adjust
            if (req.StuffDef == null) return;

            // Check if this def is a survival tool (hammer, axe, etc.)
            // SurvivalToolProperties is a DefModExtension on tool ThingDefs
            // Contains baseWorkStatFactors: e.g. ConstructionSpeed = 1.5
            var toolProps = __instance.GetModExtension<SurvivalToolProperties>();
            if (toolProps?.baseWorkStatFactors == null) return;

            // Check if the selected material has tool-specific multipliers
            // StuffPropsTool is a DefModExtension on stuff ThingDefs (wood, steel, etc.)
            // Contains toolStatFactors: e.g. ConstructionSpeed = 0.8 (wood) or 1.2 (steel)
            var stuffProps = req.StuffDef.GetModExtension<StuffPropsTool>();
            if (stuffProps?.toolStatFactors == null || stuffProps.toolStatFactors.Count == 0) return;

            // Convert IEnumerable to List so we can replace individual entries
            var resultList = __result.ToList();

            for (int i = 0; i < resultList.Count; i++)
            {
                var entry = resultList[i];

                // Only process entries in the "Survival Tool" stat category
                // Skip all other stats (mass, beauty, etc.)
                if (entry.category != ST_StatCategoryDefOf.SurvivalTool) continue;

                // Find the matching base stat by label (e.g. "Construction Speed")
                foreach (var baseFactor in toolProps.baseWorkStatFactors)
                {
                    if (entry.LabelCap == baseFactor.stat.LabelCap)
                    {
                        // Find the material multiplier for this specific stat
                        float stuffMult = 1f;
                        foreach (var sf in stuffProps.toolStatFactors)
                        {
                            if (sf.stat == baseFactor.stat)
                            {
                                stuffMult = sf.value; // e.g. 1.2 for steel
                                break;
                            }
                        }

                        // Replace the entry with adjusted value: base * material multiplier
                        // e.g. 1.5 (base) * 1.2 (steel) = 1.8
                        if (stuffMult != 1f)
                        {
                            float adjustedValue = baseFactor.value * stuffMult;
                            resultList[i] = new StatDrawEntry(
                                ST_StatCategoryDefOf.SurvivalTool,
                                baseFactor.stat.LabelCap,
                                adjustedValue.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                                baseFactor.stat.description,
                                99999); // display priority (high = shown first)
                        }
                        break; // Found matching stat, no need to check others
                    }
                }
            }

            // Return modified list with stuff-adjusted values
            __result = resultList;
        }
    }
}
