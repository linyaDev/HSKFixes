# HSK Fixes

General fixes for Hardcore SK modpack. Not tied to any specific DLC.

## Fixes / Фиксы

### WorkTable_Passability (XML)
**EN:** Pawns freely walk through crafting tables (carpenter bench, smithy, etc). HSK's `SK_WorkTable` has `passability=PassThroughOnly` but no `pathCost`. Added `pathCost=70` so pawns prefer to walk around. Vanilla tables also use `pathCost=70`.

**RU:** Пешки свободно ходят через рабочие столы (стол плотника, кузница и т.д.). В HSK `SK_WorkTable` имеет `passability=PassThroughOnly` без `pathCost`. Добавлен `pathCost=70` чтобы пешки предпочитали обходить. В ванилле столы тоже имеют `pathCost=70`.

### HayBed_Component (XML)
**EN:** Hay bed (настил из дерева) requires industrial component instead of medieval. HSK's `BasicBedBase` adds `ComponentIndustrial` to `costList` which is inherited by all beds including primitive hay bed. Fixed to use `ComponentMedieval` with `Inherit="false"`.

**RU:** Настил из дерева требует индустриальный компонент вместо примитивного. `BasicBedBase` в HSK добавляет `ComponentIndustrial` в `costList`, который наследуется всеми кроватями включая примитивный настил. Исправлено на `ComponentMedieval` с `Inherit="false"`.

### Campfire_FuelTab (XML)
**EN:** Can't select fuel type at campfire. HSK replaces vanilla `CompProperties_Refuelable` with `SK.CompFueled_Properties` but doesn't add `SK.ITab_Fuel` inspector tab. Without it, players can't choose between charcoal, coal, kindling, wood, etc.

**RU:** Нельзя выбрать тип топлива у костра. HSK заменяет ванильный `CompProperties_Refuelable` на `SK.CompFueled_Properties`, но не добавляет вкладку `SK.ITab_Fuel`. Без неё игрок не может выбирать между углём, древесным углём, щепой, дровами и т.д.

### AmmoTypeAlert (DLL)
**EN:** When a pawn tries to shoot but has no ammo of the selected type (while having other types available), shows a message: "Pawn: no stone arrows for Long Bow. Switch ammo type." CE without magazine doesn't auto-switch ammo type — player needs to switch manually. This alert makes it obvious what's happening.

**RU:** Когда пешка пытается стрелять, но у неё нет патронов выбранного типа (при наличии других типов), показывает сообщение. CE без магазина не переключает тип патронов автоматически — игроку нужно переключить вручную. Оповещение помогает понять что происходит.

### GuestFoodStealing (DLL)
**EN:** Guest caravan animals eat player food from stockpiles. Vanilla `FoodUtility.BestFoodSourceOnMap` has no area check for regular food — guests freely take food from anywhere on the map. Fix blocks non-player pawns from taking food inside the player's home area. Guests can still eat wild berries and food outside the home area. Prisoners and slaves are not affected.

**RU:** Животные гостевых караванов едят еду игрока со складов. Ванильный `FoodUtility.BestFoodSourceOnMap` не проверяет зону для обычной еды — гости свободно берут еду откуда угодно. Фикс блокирует не-игровым пешкам доступ к еде внутри домашней зоны. Гости могут есть дикие ягоды и еду вне домашней зоны. Пленники и рабы не затронуты.

## Requirements / Требования
- RimWorld 1.5 / 1.6
- Hardcore SK modpack
- Harmony
