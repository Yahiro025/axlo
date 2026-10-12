# ADR 0001: Anchor popup beside the native context menu

Date: 2026-10-13
Status: Accepted

## Context

After double right-click, our popup renders underneath the native context
menu. The low-level hook fires before the target app shows its menu for
the same click, so show-first ordering loses. Scope is global: every app,
including browsers (which draw custom menus instead of system menus).

## Decision

When the popup appears via double right-click, wait for the native menu
to settle, locate the topmost system menu window (class `#32768`), and
move the popup adjacent to its measured rect (right side preferred, left
on overflow), clamped to the work area, then re-pin topmost.

When no system menu is found near the cursor (browsers, custom menus),
keep the existing behavior: cursor offset plus topmost re-pin.

## Alternatives considered

- Larger offset plus border polish: cheap, but overlap stays luck-based.
  Rejected as the primary fix.
- Fixed screen corner: never overlaps, but cursor travel hurts the
  typing flow. Rejected.

## Consequences

- New P/Invoke surface: `FindWindow`, `GetWindowRect`, `RECT`.
- Placement math lives in pure `PopupPosition.PlaceBesideMenu` so it is
  unit-testable; window lookup stays in the form.
- A stale or distant menu is ignored via a cursor-proximity guard.
- Browser behavior is unchanged from the current release.
