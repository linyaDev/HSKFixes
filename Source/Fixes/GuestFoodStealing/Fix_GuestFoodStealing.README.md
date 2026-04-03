# Fix: GuestFoodStealing — guest caravan animals eat player food

## Bug

**EN:** Animals and pawns from visitor/trader caravans eat food from player stockpiles inside the home area. They consume meals, berries, and other food meant for colonists.

**RU:** Животные и пешки из гостевых/торговых караванов едят еду со складов игрока внутри домашней зоны. Они поедают готовые блюда, ягоды и другую еду, предназначенную для колонистов.

## Root Cause

**EN:** Vanilla `FoodUtility.BestFoodSourceOnMap` has no area or faction check for regular food items. Interestingly, Nutrient Paste Dispensers DO have the check `(t.Faction != getter.Faction && t.Faction != getter.HostFaction)`, but regular food items don't. Guest pawns freely search for and consume any food on the map.

**RU:** Ванильный `FoodUtility.BestFoodSourceOnMap` не проверяет зону или фракцию для обычных предметов еды. Интересно, что для Nutrient Paste Dispenser такая проверка есть, но для обычной еды — нет. Гостевые пешки свободно ищут и поедают любую еду на карте.

## Fix

**EN:** Harmony postfix on `FoodUtility.BestFoodSourceOnMap` — if the getter is a non-player pawn and the found food is inside the player's home area (`map.areaManager.Home`), returns null. Guests can still eat wild berries and food outside the home area. Prisoners and slaves are not affected.

**RU:** Harmony postfix на `FoodUtility.BestFoodSourceOnMap` — если ищущий не из фракции игрока и найденная еда внутри домашней зоны (`map.areaManager.Home`), возвращает null. Гости могут есть дикие ягоды и еду вне домашней зоны. Пленники и рабы не затронуты.
