# Fix: ToolStatsDisplay — tool info window ignores material

## Bug

**EN:** When selecting material for a survival tool (hammer, axe, etc) in the info window, stats like construction speed don't change. All materials show the same values. Pressing Shift reveals the correct material-adjusted stats via a different UI path.

**RU:** При выборе материала для инструмента (молоток, топор и т.д.) в окне информации, характеристики вроде скорости строительства не меняются. Все материалы показывают одинаковые значения. Через Shift можно увидеть правильные значения через другой путь UI.

## Root Cause

**EN:** SurvivalToolsLite patches `ThingDef.SpecialDisplayStats` to show tool work stat factors. When `req.Thing == null` (info window before crafting, no real item yet), it only shows `baseWorkStatFactors` without applying `StuffPropsTool.toolStatFactors` for the selected `req.StuffDef`.

```csharp
// SurvivalToolsLite HarmonyPatches.cs line 283
if (req.Thing == null)
{
    // Shows ONLY base values — BUG: ignores req.StuffDef
    value = baseWorkStatFactor.value;
}
```

When `req.Thing != null` (Shift → detail view of real item), `SurvivalTool.WorkStatFactors` correctly reads `this.Stuff` and applies multipliers.

**RU:** SurvivalToolsLite патчит `ThingDef.SpecialDisplayStats` для отображения характеристик инструментов. Когда `req.Thing == null` (окно информации до крафта, нет реального предмета), мод показывает только `baseWorkStatFactors` без применения множителей `StuffPropsTool.toolStatFactors` для выбранного `req.StuffDef`.

Когда `req.Thing != null` (детальный просмотр через Shift), `SurvivalTool.WorkStatFactors` корректно читает `this.Stuff` и применяет множители.

## Fix

**EN:** Harmony postfix on `ThingDef.SpecialDisplayStats` — after SurvivalToolsLite adds its entries, checks if `req.StuffDef` has `StuffPropsTool` with `toolStatFactors`. If yes, replaces the stat values with `baseValue * stuffMultiplier`.

**RU:** Harmony postfix на `ThingDef.SpecialDisplayStats` — после того как SurvivalToolsLite добавит свои записи, проверяет есть ли у `req.StuffDef` множители `StuffPropsTool.toolStatFactors`. Если да, заменяет значения на `базовое * множитель_материала`.
