# Product

<!-- impeccable:product-schema 1 -->

## Platform

windows (desktop WinForms exe; the only web surfaces are the README and a local design-preview page)

## Users

Primary: the owner, working on a laptop whose physical `.` / `>` key is broken. Secondary (confirmed 2026-10-13): other keyboard-first Windows users via a public GitHub release.

## Product Purpose

DotGreaterPopup gives both characters back everywhere in Windows. Double right-click anywhere (or Win+Shift+J) pops a two-button `.` / `>` popup at the cursor; clicking a button types that character into whatever field has focus. Success means typing without looking: no focus theft, no swallowed right-clicks, no console window, no admin prompt.

## Positioning

A single tiny exe that refuses to fight the OS: it never swallows the native context menu (the popup settles beside it), never steals activation (NOACTIVATE + topmost), and types through `SendInput(KEYEVENTF_UNICODE)` so layout tricks like Shift-hacks are unnecessary. A neighboring product could copy the buttons; it could not truthfully copy the pass-through guarantee.

## Operating Context

Runs from the Windows system tray, optionally started with Windows via `HKCU\...\Run`. Used inside Notepad, browsers, Explorer rename boxes, and Start-menu search. Elevated windows are a documented limitation: Windows UIPI blocks typing into them unless the exe also runs elevated (default install stays `asInvoker`, no UAC). The SharpKeys F8-to-period remap is the documented fallback for elevated windows.

## Capabilities and Constraints

- System-wide double-right-click trigger via a pass-through low-level mouse hook (`CallNextHookEx` always; single right-click is silent).
- Win+Shift+J fallback hotkey opening the same popup at the cursor.
- Popup auto-closes on pick, click-away (`Deactivate`), or Esc.
- Pure logic (`DoubleClickDecision`, `PopupPosition`, `CharSet`) stays Win32-free so MSTest runs anywhere; hook/window/SendInput behavior is verified by a manual README matrix, not unit tests.
- Buttons derive from the single `CharSet.Chars` list; no second hardcoded list.
- Autostart writes `HKCU` only. No `HKLM`, no admin.
- Stack (existing codebase): C# WinForms `net48`, zero-install single exe, MSTest for pure logic.
- Open decision: release packaging and versioning for the public GitHub release.

## Brand Commitments

Parent brand **axlo**; first product lockup `axlo .>`. Visual authority is `DESIGN.md`: Industrial/Utilitarian, one Fluent blue `#0F6CBD`, Fluent neutrals, Cabinet Grotesk display / Source Sans 3 body / JetBrains Mono glyphs, Segoe UI native inside the app only. The dot-o glyph (dot with `>` knockout) is the tray/app mark; it never contains text.

## Evidence on Hand

- `README.md`: install/use/uninstall plus the manual test matrix (source of truth for verification).
- `docs/superpowers/specs/2026-10-13-dotgreater-popup-design.md`: locked behavior design.
- `DESIGN.md` and `docs/design/axlo-preview.html`: brand system and its three-surface preview.
- No public release exists yet; no testimonials, benchmarks, or download counts. Future work must not fabricate them.

## Product Principles

1. The OS owns the interaction; axlo borrows it for 300 milliseconds and gives it back.
2. Every trigger has a fallback: right-click has Win+Shift+J, typing has the SharpKeys remap.
3. Small and observable beats clever: one exe, one tray icon, two buttons.
4. Never degrade the machine to serve the moment: no swallowed input, no stolen focus, no admin demands by default.

## Accessibility & Inclusion

Fully keyboard-operable by necessity: the popup opens without `.`/`>` keys, closes on Esc, and never moves focus away from the user's field. Popup buttons carry white-on-Fluent-blue contrast (DESIGN.md tokens).
