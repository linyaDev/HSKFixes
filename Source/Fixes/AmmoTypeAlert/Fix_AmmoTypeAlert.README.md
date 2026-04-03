# Fix: AmmoTypeAlert — no feedback when selected ammo type is empty

## Bug

**EN:** When a pawn tries to shoot but has no ammo of the selected type (while having other types available), nothing happens — no error, no message. The pawn just stands there. CE without magazine doesn't auto-switch ammo type, so the player has no idea why the pawn isn't shooting.

**RU:** Когда пешка пытается стрелять, но у неё нет патронов выбранного типа (при наличии других типов), ничего не происходит — ни ошибки, ни сообщения. Пешка просто стоит. CE без магазина не переключает тип патронов автоматически, и игрок не понимает почему пешка не стреляет.

## Root Cause

**EN:** Combat Extended's `CompAmmoUser.TryFindAmmoInInventory()` only searches for the currently selected ammo type when `EmptyMagazine=false`. Without a magazine (`HasMagazine=false`), `EmptyMagazine` is always false, so CE never searches for alternative ammo types. The shot silently fails with no player feedback.

**RU:** `CompAmmoUser.TryFindAmmoInInventory()` в CE ищет только текущий выбранный тип патронов когда `EmptyMagazine=false`. Без магазина (`HasMagazine=false`) `EmptyMagazine` всегда false, и CE никогда не ищет альтернативные типы. Выстрел молча проваливается без обратной связи.

## Fix

**EN:** Harmony postfix on `Verb_ShootCE.TryCastShot` — when shot fails, checks if the pawn has ammo of other types available. If yes, shows a message: "Pawn: no stone arrows for Long Bow. Switch ammo type." Alert shown on every failed shot for player-controlled colonists only.

**RU:** Harmony postfix на `Verb_ShootCE.TryCastShot` — когда выстрел не удался, проверяет есть ли у пешки патроны других типов. Если да, показывает сообщение. Оповещение на каждый неудавшийся выстрел, только для колонистов под управлением игрока.
