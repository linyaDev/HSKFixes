# HSK Fixes

**[Native] [Patch Collection] [1.5 & 1.6]**

A collection of bug fixes for Hardcore SK modpack. Built entirely for HSK.

## Description

Various quality-of-life and bug fixes for HSK. Install like any other mod, requires Harmony.

## Fixes for 1.5

- **AmmoTypeAlert** — When a pawn can't shoot because selected ammo type is empty (while having other types), shows a message to switch ammo type.
- **ToolStatsDisplay** — Survival tool info window now correctly updates stats when changing material.

## Fixes for 1.6

*Temporary — will be removed once fixed in the main HSK release.*

- **WorkTable Passability** — Pawns no longer freely walk through crafting tables.
- **HayBed Component** — Hay bed now requires primitive component instead of industrial.
- **Campfire Fuel Tab** — Can now select fuel type at campfire.
- **Armchair Passability** — Armchair can now be placed at workbenches.
- **GuestFoodStealing** — Guest caravan animals no longer eat player food from home area.

## Changelog

29.03.2026
- Split fixes by version (1.5 / 1.6)
- Fixed ToolStatsDisplay breaking other mod stats (lazy enumeration fix)

28.03.2026
- Initial release with 7 fixes

## Links
GitHub: https://github.com/linyaDev/HSKFixes

---

# HSK Odyssey Fixes

**[Native] [Patch Collection] [1.5 & 1.6]**

Fixes for Odyssey DLC compatibility issues with Hardcore SK modpack.

## Description

Makes Odyssey DLC playable with HSK. Fixes pawn generation, hot springs, and Gravship scenario resources. Install like any other mod, requires Harmony and Dubs Bad Hygiene.

## Fixes

- **ResetPawnKindDefs** — Fixes pawn contamination across scenarios (vacsuits leaking from Gravship to other scenarios).
- **DBH HotSpring Wash** — Pawns can now wash in Odyssey hot springs.
- **Gravship Starting Resources** — Hygiene I research unlocked from start + 25 reinforced concrete blocks for chem refinery.

## Changelog

29.03.2026
- Removed CopyPawnKindTechHediffs (caused Mechanitor to lose mechlink)
- Added ResetPawnKindDefs to prevent cross-scenario contamination

28.03.2026
- Initial release with Odyssey compatibility fixes
- CopyPawnKindBasics and PowerSwitchDrawOrder accepted upstream into Core_SK.dll

## Links
GitHub: https://github.com/linyaDev/HSKOdysseyFixes
Blockers: https://github.com/linyaDev/HSKOdysseyFixes/blob/main/BLOCKERS.md
