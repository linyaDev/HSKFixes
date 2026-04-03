using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Adds a "Preview item" button to the info card for any item that supports materials.
    /// On click — opens a FloatMenu with material selection, then creates a temporary
    /// Thing with chosen material and opens its detailed info card.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_ToolPreviewButton
    {
        // Button dimensions matching vanilla ShowMaterialsButton style
        private const float ButtonWidth = 200f;
        private const float ButtonHeight = 40f;
        private const float Margin = 16f;

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

            // Only show for ThingDefs that support materials
            if (def == null || !(def is ThingDef thingDef)) return;

            var allowedStuffs = GenStuff.AllowedStuffsFor((BuildableDef)thingDef);
            if (!allowedStuffs.Any()) return;

            // Position: same row as "Change materials" button, to its left
            // "Change materials" is at: x = right - 14 - 200 - 16
            // Our button: left of it with 8px gap
            float materialsButtonX = inRect.x + inRect.width - 14f - ButtonWidth - Margin;
            float previewButtonX = materialsButtonX - ButtonWidth - 8f;

            Rect buttonRect = new Rect(
                previewButtonX,
                inRect.y + 18f,
                ButtonWidth,
                ButtonHeight);

            // Label: use Russian if active, otherwise English
            string label = Translator.CanTranslate("HSKFixes_PreviewItem")
                ? "HSKFixes_PreviewItem".Translate()
                : "Preview item";

            if (Widgets.ButtonText(buttonRect, label))
            {
                // Open FloatMenu with material selection
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (ThingDef stuffDef in allowedStuffs)
                {
                    ThingDef localStuff = stuffDef;
                    options.Add(new FloatMenuOption(stuffDef.LabelCap, delegate
                    {
                        // Create temporary Thing with selected material
                        Thing tempItem = ThingMaker.MakeThing(thingDef, localStuff);
                        // Open info card for the temporary item
                        Find.WindowStack.Add(new Dialog_InfoCard(tempItem));
                    }, stuffDef));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }
}
