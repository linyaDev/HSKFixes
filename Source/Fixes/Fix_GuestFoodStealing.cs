using HarmonyLib;
using RimWorld;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Prevents guest/visitor caravan animals and pawns from eating player food.
    ///
    /// FoodUtility.BestFoodSourceOnMap validates food items but has no faction check
    /// for regular food (line 421). Nutrient Paste Dispensers DO have the check
    /// (t.Faction != getter.Faction), but regular food items don't.
    ///
    /// Fix: Harmony postfix on BestFoodSourceOnMap — if the getter is a guest
    /// and the found food belongs to the player, return null (no food found).
    /// Guest pawns will only eat food from their own inventory.
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

            // If getter is not from player faction and food belongs to player — deny
            if (getter.Faction != null
                && getter.Faction != Faction.OfPlayer
                && getter.HostFaction != Faction.OfPlayer
                && __result.Faction == Faction.OfPlayer)
            {
                __result = null;
            }
        }
    }
}
