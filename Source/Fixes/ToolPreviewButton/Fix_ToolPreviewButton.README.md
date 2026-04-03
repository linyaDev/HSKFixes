# Fix: ToolPreviewButton — preview item with selected material

## Feature

**EN:** Adds a "Preview item" button to the info card of any item that supports material selection. Clicking it creates a temporary item with the currently selected material and opens its detailed info card — showing all stats correctly adjusted for the material (armor, damage, weight, tool effectiveness, etc).

**RU:** Добавляет кнопку "Preview item" в окно информации любого предмета с выбором материала. При нажатии создаётся временный предмет с текущим выбранным материалом и открывается его подробная карточка — со всеми характеристиками, правильно пересчитанными для материала (броня, урон, вес, эффективность инструмента и т.д.).

## Why needed

**EN:** The default info card shows stats for the ThingDef (definition), not for a real item with material. Some stats don't update when changing material — especially modded stats like SurvivalToolsLite tool effectiveness. The preview button opens a full item view where all stats are correct.

**RU:** Стандартное окно показывает статы для ThingDef (определения), а не для реального предмета с материалом. Некоторые статы не обновляются при смене материала — особенно модовые, например эффективность инструментов SurvivalToolsLite. Кнопка превью открывает полный вид предмета где все статы корректны.

## Implementation

Harmony postfix on `Dialog_InfoCard.DoWindowContents`. When viewing a ThingDef with stuff selected, draws a button. On click: `ThingMaker.MakeThing(thingDef, stuff)` → `new Dialog_InfoCard(tempItem)`.

Uses `BepInEx.AssemblyPublicizer` for direct access to `Dialog_InfoCard.def` and `Dialog_InfoCard.stuff` private fields.
