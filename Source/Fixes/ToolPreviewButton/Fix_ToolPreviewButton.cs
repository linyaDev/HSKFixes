using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// A Dialog_InfoCard subclass that allows multiple info cards open at once.
    /// Vanilla Dialog_InfoCard has onlyOneOfTypeAllowed = true, which closes
    /// the previous one when opening a new one.
    /// </summary>
    public class Dialog_InfoCard_Preview : Dialog_InfoCard
    {
        public Dialog_InfoCard_Preview(Thing thing) : base(thing)
        {
            // Allow multiple info card windows simultaneously
            this.onlyOneOfTypeAllowed = false;
            // Offset so it doesn't overlap the original window
            this.windowRect.x += 50f;
            this.windowRect.y += 50f;
        }
    }

    /// <summary>
    /// Adds a "Preview item" / "Готовый предмет" button to the info card
    /// for any item that supports materials. On click — opens a FloatMenu
    /// with material selection, then creates a temporary Thing with chosen
    /// material and opens its detailed info card in a NEW window.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_ToolPreviewButton
    {
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
            // Don't draw button on our preview windows
            if (__instance is Dialog_InfoCard_Preview) return;

            var def = __instance.def;

            if (def == null || !(def is ThingDef thingDef)) return;

            var allowedStuffs = GenStuff.AllowedStuffsFor((BuildableDef)thingDef);
            if (!allowedStuffs.Any()) return;

            // Position: left of "Change materials" button
            float materialsButtonX = inRect.x + inRect.width - 14f - ButtonWidth - Margin;
            float previewButtonX = materialsButtonX - ButtonWidth - 8f;

            Rect buttonRect = new Rect(
                previewButtonX,
                inRect.y + 18f,
                ButtonWidth,
                ButtonHeight);

            string label = Translator.CanTranslate("HSKFixes_PreviewItem")
                ? "HSKFixes_PreviewItem".Translate()
                : "Preview item";

            if (Widgets.ButtonText(buttonRect, label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (ThingDef stuffDef in allowedStuffs)
                {
                    ThingDef localStuff = stuffDef;
                    options.Add(new FloatMenuOption(stuffDef.LabelCap, delegate
                    {
                        Thing tempItem = ThingMaker.MakeThing(thingDef, localStuff);
                        // Open as Dialog_InfoCard_Preview — doesn't close the original window
                        Find.WindowStack.Add(new Dialog_InfoCard_Preview(tempItem));
                    }, stuffDef));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }
}
