using HarmonyLib;
using RimWorld;
using SurvivalToolsLite;
using UnityEngine;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Adds a "Preview" button to the info card for survival tools.
    /// Creates a temporary Thing with the selected material and opens
    /// its info card — showing all stats correctly adjusted for stuff.
    /// </summary>
    [HarmonyPatch(typeof(Dialog_InfoCard), "DoWindowContents")]
    public static class Fix_ToolPreviewButton
    {
        public static void Postfix(Dialog_InfoCard __instance, Rect inRect)
        {
            // Access private fields via publicizer
            var def = __instance.def;
            var stuff = __instance.stuff;

            // Only show for survival tools with a material selected
            if (def == null || !(def is ThingDef thingDef)) return;
            if (stuff == null) return;
            if (thingDef.GetModExtension<SurvivalToolProperties>() == null) return;

            // Draw "Preview" button next to "Change materials" button
            // "Change materials" is at bottom-right, we put ours to its left
            Rect buttonRect = new Rect(
                inRect.x + inRect.width - 230f,
                inRect.y + inRect.height - 30f,
                120f,
                30f);

            if (Widgets.ButtonText(buttonRect, "Preview"))
            {
                // Create temporary Thing with selected stuff
                Thing tempTool = ThingMaker.MakeThing(thingDef, stuff);

                // Open info card for the temporary item — shows correct stuff-adjusted stats
                Find.WindowStack.Add(new Dialog_InfoCard(tempTool));
            }
        }
    }
}
