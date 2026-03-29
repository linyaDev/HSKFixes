using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSKFixes
{
    /// <summary>
    /// Shows a letter alert when a pawn tries to shoot but has no ammo
    /// of the selected type. Without magazine, CE doesn't auto-switch
    /// ammo type, so the player needs to switch manually.
    ///
    /// Patches CE Verb_ShootCE.TryCastShot — if result is false and
    /// the weapon has ammo of other types available, notify the player.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Fix_AmmoTypeAlert
    {
        private static HashSet<int> alreadyNotified = new HashSet<int>();

        static Fix_AmmoTypeAlert()
        {
            var harmony = new Harmony("linya.hskfixes.ammotypealert");

            var ceVerbShoot = AccessTools.TypeByName("CombatExtended.Verb_ShootCE");
            if (ceVerbShoot == null) return;

            var tryCastShot = AccessTools.Method(ceVerbShoot, "TryCastShot");
            if (tryCastShot == null) return;

            harmony.Patch(tryCastShot,
                postfix: new HarmonyMethod(typeof(Fix_AmmoTypeAlert), nameof(Postfix)));
            Log.Message("[HSKFixes] Fix_AmmoTypeAlert applied.");
        }

        public static void Postfix(bool __result, Verb __instance)
        {
            if (__result) return; // shot succeeded, no problem

            var pawn = __instance.caster as Pawn;
            if (pawn == null || !pawn.IsColonistPlayerControlled) return;

            var eq = pawn.equipment?.Primary;
            if (eq == null) return;

            // Find CE ammo comp via reflection
            var ceAmmoType = AccessTools.TypeByName("CombatExtended.CompAmmoUser");
            if (ceAmmoType == null) return;

            ThingComp ammoComp = null;
            foreach (var comp in eq.AllComps)
            {
                if (ceAmmoType.IsInstanceOfType(comp))
                {
                    ammoComp = comp;
                    break;
                }
            }
            if (ammoComp == null) return;

            var hasMag = (bool)AccessTools.Property(ceAmmoType, "HasMagazine").GetValue(ammoComp);
            if (hasMag) return; // has magazine — CE handles reload/switch itself

            var hasAmmo = (bool)AccessTools.Property(ceAmmoType, "HasAmmo").GetValue(ammoComp);
            if (!hasAmmo) return; // no ammo at all — CE already shows "out of ammo"

            // Has ammo but can't shoot = wrong ammo type selected
            // Only notify once per pawn per type
            var currentAmmo = AccessTools.Property(ceAmmoType, "CurrentAmmo").GetValue(ammoComp);
            int key = pawn.thingIDNumber ^ (currentAmmo?.GetHashCode() ?? 0);
            if (alreadyNotified.Contains(key)) return;
            alreadyNotified.Add(key);

            string pawnName = pawn.Name?.ToStringShort ?? pawn.LabelShort;
            string ammoName = currentAmmo != null ? ((Def)currentAmmo).label : "unknown";
            string weaponName = eq.def.label;

            Messages.Message(
                $"{pawnName}: no {ammoName} for {weaponName}. Switch ammo type.",
                pawn,
                MessageTypeDefOf.RejectInput,
                false);
        }
    }
}
