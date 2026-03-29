using HarmonyLib;
using RimWorld;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Prevents guest/visitor caravan animals from eating food in player's home area.
    ///
    /// FoodUtility.BestFoodSourceOnMap has no faction/area check for regular food.
    /// Guest pawns and their animals freely take food from player stockpiles.
    ///
    /// Fix: Harmony postfix on BestFoodSourceOnMap — if the getter is a non-player
    /// guest and the found food is inside the player's home area, block it.
    /// Guests can still eat food outside the home area (wild berries, etc).
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

            // Allow player faction pawns
            if (getter.Faction == Faction.OfPlayer) return;

            // Allow prisoners and slaves
            if (getter.guest != null && getter.guest.IsPrisoner) return;
            if (getter.IsSlave) return;

            // Block non-player pawns from eating food inside the home area
            if (getter.Faction != null
                && getter.Map != null
                && getter.Map.areaManager.Home[__result.Position])
            {
                __result = null;
            }
        }
    }
}
