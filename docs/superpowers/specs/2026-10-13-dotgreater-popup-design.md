# DotGreater Popup Design (locked checkpoint)

Date: 2026-10-13
Status: Approved — C# WinForms base + fallback hotkey + remap fallback note.

## Problem

Laptop `.` / `>` physical key is broken (same key on US layout). Need a system-wide
Windows workaround: double right-click pops a window with `.` and `>` buttons;
clicking one types that character into any field, including Windows itself.

## Decisions locked

- System-wide on Windows, any text field.
- Keep native context menu, show popup alongside it (no click swallowing).
- Popup auto-closes on type / click-away / Esc.
- Autostart with Windows, tray icon.
- Stack: C# WinForms, `net48`, ~100KB exe, no runtime install.
- Fallback trigger required (double-right-click can hit a menu item by accident):
  `Win+J` opens the same popup. Fallback must not need `.` or `>`.

## Shape

- `DoubleRightClickTrigger`: owns `WH_MOUSE_LL` pass-through hook + double timing
  (`GetDoubleClickTime()` + `SM_CXDOUBLECLK` distance on `RButtonUp` only).
  Raises `DoubleRightClicked(Point cursor)`. Never swallows.
- `DotGreaterPopupForm`: borderless `TopMost` tool-window,
  `ShowWithoutActivation=true`, two buttons derived from `CharSet.Chars`,
  positioned at cursor + offset, clamped to monitor work area. Single owner
  of visibility; `HidePopup()` idempotent.
- `CharTyper.Insert(char)`: validates against `CharSet`, sends
  `SendInput(KEYEVENTF_UNICODE)` for `U+002E` / `U+003E`.
- `AutostartManager`: `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` only.
- `HotkeyManager`: `RegisterHotKey(MOD_WIN, VK_J)` → same `ShowAt(cursor)`.
- Pure testable helpers (no Win32): `DoubleClickDecision.ShouldFire(...)`,
  `PopupPosition.Clamp(...)`.

## Tradeoffs accepted

- `net48` + WinForms in exchange for zero-install tiny exe.
- `NOACTIVATE` popup in exchange for no focus-restore dance.
- No elevated-app injection without admin in exchange for no UAC prompt.
- Double-right-click kept as primary in exchange for carrying menu-overlap risk,
  mitigated by offset + fallback hotkey.

## Open risks

- Topmost popup vs native menu z-order on all DPIs — verify on 100%/150%.
- Second right-click landing on a menu item — mitigated by fallback hotkey.
- Elevated windows need admin to receive keys — documented, not fixed.
