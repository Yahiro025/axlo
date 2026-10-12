# Agent context — axlo / DotGreaterPopup

This repo builds one thing: a system-wide Windows helper that types `.` / `>` for a broken key.

## Layout

- `src/DotGreaterPopup/` — WinForms `net48` exe (no third-party runtime deps).
- `tests/DotGreaterPopup.Tests/` — MSTest project for pure logic only.
- `docs/superpowers/specs/` — locked design. Read before changing behavior.
- `docs/superpowers/plans/` — implementation plan. Executors follow it task-by-task.

## Build / test (Windows)

```bat
REM build exe
msbuild DotGreaterPopup.sln /p:Configuration=Release
REM run unit tests (pure logic, no hook/window needed)
dotnet test tests\DotGreaterPopup.Tests\DotGreaterPopup.Tests.csproj
```

Manual checks need a real Windows desktop (hook + popup + SendInput can't run headless).
See the plan's verification matrix for Notepad / browser / Explorer rename / DPI steps.

## Rules for executors

- Hook is pass-through only. Never swallow right-clicks (`CallNextHookEx` always).
- Popup is `ShowWithoutActivation=true` + `TopMost`. Never steal focus.
- Typing is `SendInput(KEYEVENTF_UNICODE)` only. No `SendKeys`, no Shift hacks.
- Autostart writes `HKCU\...\Run` only. No `HKLM`, no admin.
- Buttons derive from `CharSet.Chars`. One list, derived everywhere — no second hardcoded list.
- Pure logic (`DoubleClickDecision`, `PopupPosition`, `CharSet`) stays Win32-free so tests run anywhere.
