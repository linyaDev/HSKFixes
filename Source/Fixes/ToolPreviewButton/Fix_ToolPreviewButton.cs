using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Adds a "Preview item" button to the info card for any item with material selected.
    /// Creates a temporary Thing with the selected stuff and opens its info card,
    /// showing all stats correctly adjusted for the material.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_ToolPreviewButton
    {
        static Fix_ToolPreviewButton()
        {
            var harmony = new Harmony("linya.hskfixes.toolpreviewbutton");
            harmony.Patch(
                AccessTools.Method(typeof(Dialog_InfoCard), "DoWindowContents"),
                postfix: new HarmonyMethod(typeof(Fix_ToolPreviewButton), nameof(Postfix)));
            Log.Message("[HSKFixes] Fix_ToolPreviewButton applied.");
        }

        public static void Postfix(Dialog_InfoCard __instance, Rect inRect)
        {
            // Access private fields via publicizer
            var def = __instance.def;
            var stuff = __instance.stuff;

            // Only show when viewing a ThingDef with material selected
            if (def == null || !(def is ThingDef thingDef)) return;
            if (stuff == null) return;

            // Draw "Preview item" button to the left of "Change materials" button
            Rect buttonRect = new Rect(
                inRect.x + inRect.width - 260f,
                inRect.y + inRect.height - 30f,
                140f,
                30f);

            if (Widgets.ButtonText(buttonRect, "Preview item"))
            {
                // Create temporary Thing with selected stuff
                Thing tempItem = ThingMaker.MakeThing(thingDef, stuff);

                // Open info card for the temporary item — shows correct stuff-adjusted stats
                Find.WindowStack.Add(new Dialog_InfoCard(tempItem));
            }
        }
    }
}
