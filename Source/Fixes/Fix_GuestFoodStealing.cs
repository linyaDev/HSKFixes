using HarmonyLib;
using RimWorld;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Prevents guest/visitor caravan animals and pawns from eating food off the map.
    ///
    /// FoodUtility.BestFoodSourceOnMap has no faction check for regular food items.
    /// Guest/visitor pawns and their animals can freely take any food from
    /// stockpiles, ground, etc.
    ///
    /// Fix: Harmony postfix on BestFoodSourceOnMap — if the getter belongs to
    /// a non-player faction and is visiting (not a prisoner/slave), block food
    /// from the map. Guests will only eat from their own caravan inventory.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_GuestFoodStealing
    {
        static Fix_GuestFoodStealing()
        {
            var harmony = new Harmony("linya.hskfixes.guestfoodstealing");

            var original = AccessTools.Method(typeof(FoodUtility), "BestFoodSourceOnMap");
            if (original != null)
            {
                harmony.Patch(original,
                    postfix: new HarmonyMethod(typeof(Fix_GuestFoodStealing), nameof(Postfix)));
                Log.Message("[HSKFixes] Fix_GuestFoodStealing applied.");
            }
        }

        public static void Postfix(ref Thing __result, Pawn getter, Pawn eater)
        {
            if (__result == null) return;

            // Allow player faction pawns to eat anything
            if (getter.Faction == Faction.OfPlayer) return;

            // Allow prisoners and slaves to eat (they are fed by the colony)
            if (getter.guest != null && getter.guest.IsPrisoner) return;
            if (getter.IsSlave) return;

            // Block non-player, non-prisoner pawns from taking food off the map
            // They should eat from their own caravan inventory
            if (getter.Faction != null && getter.Faction != Faction.OfPlayer)
            {
                __result = null;
            }
        }
    }
}
