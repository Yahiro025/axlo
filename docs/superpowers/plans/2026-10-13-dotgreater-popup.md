# DotGreater Popup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship a zero-install Windows exe where double right-click (or Win+J) pops `.` / `>` buttons that type into any field.

**Architecture:** WinForms `net48` exe with a pass-through `WH_MOUSE_LL` hook, non-activating topmost popup, `SendInput(UNICODE)` typer, `HKCU Run` autostart. Pure timing/position logic isolated for unit tests.

**Tech Stack:** C# WinForms `net48`, MSTest, Win32 P/Invoke only (no third-party runtime deps).

**Spec:** `docs/superpowers/specs/2026-10-13-dotgreater-popup-design.md`

## Global Constraints

- Target `net48` (runs on stock Win10/11, no runtime install).
- Mouse hook is pass-through only — always call `CallNextHookEx`, never swallow clicks.
- Popup uses `ShowWithoutActivation=true` + `TopMost` + `WS_EX_TOOLWINDOW`; never steals focus.
- Typing uses `SendInput` with `KEYEVENTF_UNICODE` only; no `SendKeys`, no Shift hacks.
- Autostart writes `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` only; no `HKLM`, no admin requirement.
- Buttons derive from `CharSet.Chars = ['.', '>']`; no second hardcoded list.
- Pure helpers stay Win32-free so `dotnet test` runs headless.

## Review Focus

- Second right-click lands on an open context-menu item and runs it — popup must offset off the menu and Win+J must work when the menu is in the way.
- Multi-monitor / 125–150% DPI pushes popup off-screen — popup must clamp to the cursor monitor's work area.
- Popup click steals focus so the char lands in the wrong window — popup must not activate; char must land where the cursor was.
- Elevated (admin) window ignores injected keys — document as limitation, don't silently fail.
- Fast double-right-click missed or single right-click misfires popup — timing uses `GetDoubleClickTime()` and `SM_CXDOUBLECLK`, verified by unit tests.

---

### Task 1: Solution skeleton + pure logic

**Files:**
- Create: `DotGreaterPopup.sln`
- Create: `src/DotGreaterPopup/DotGreaterPopup.csproj`
- Create: `src/DotGreaterPopup/CharSet.cs`
- Create: `src/DotGreaterPopup/DoubleClickDecision.cs`
- Create: `src/DotGreaterPopup/PopupPosition.cs`
- Create: `tests/DotGreaterPopup.Tests/DotGreaterPopup.Tests.csproj`
- Test: `tests/DotGreaterPopup.Tests/DecisionTests.cs`

**Interfaces:**
- Consumes: none.
- Produces: `static class CharSet { static readonly char[] Chars; }`, `static bool DoubleClickDecision.ShouldFire(Point lastPt, long lastMs, Point curPt, long curMs, int dblTimeMs, int dblSizePx)`, `static Point PopupPosition.Clamp(Point want, Size popup, Rectangle workArea)`.

- [ ] **Step 1: Write failing tests for decision + charset + clamp**

```csharp
[TestMethod] public void Fires_When_Fast_And_Close() =>
  Assert.IsTrue(DoubleClickDecision.ShouldFire(new Point(0,0), 0, new Point(2,2), 200, 500, 8));
[TestMethod] public void Rejects_When_Slow() =>
  Assert.IsFalse(DoubleClickDecision.ShouldFire(new Point(0,0), 0, new Point(2,2), 900, 500, 8));
[TestMethod] public void Rejects_When_Far() =>
  Assert.IsFalse(DoubleClickDecision.ShouldFire(new Point(0,0), 0, new Point(100,100), 200, 500, 8));
[TestMethod] public void Charset_Has_Dot_And_Greater() =>
  CollectionAssert.AreEqual(new[]{'.','>'}, CharSet.Chars);
[TestMethod] public void Clamp_Keeps_Popup_Onscreen() {
  var p = PopupPosition.Clamp(new Point(1900,1000), new Size(160,80), new Rectangle(0,0,1920,1040));
  Assert.IsTrue(p.X + 160 <= 1920 && p.Y + 80 <= 1040);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/DotGreaterPopup.Tests/DotGreaterPopup.Tests.csproj`
Expected: FAIL — types not defined.

- [ ] **Step 3: Implement `CharSet`, `DoubleClickDecision`, `PopupPosition` in `src/DotGreaterPopup/`**

Distance check is Chebyshev/box (`Abs(dx) <= size && Abs(dy) <= size`), time check `curMs - lastMs <= dblTimeMs && curMs >= lastMs`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/DotGreaterPopup.Tests/DotGreaterPopup.Tests.csproj`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add DotGreaterPopup.sln src tests
git commit -m "feat: add pure double-click decision, charset, popup clamp with tests"
```

### Task 2: Unicode typer

**Files:**
- Create: `src/DotGreaterPopup/NativeMethods.cs`
- Create: `src/DotGreaterPopup/CharTyper.cs`

**Interfaces:**
- Consumes: `CharSet.Chars` from Task 1.
- Produces: `sealed class CharTyper { void Insert(char c); }` (throws `ArgumentException` for chars outside `CharSet`).

- [ ] **Step 1: Write failing test for charset guard (no Win32 needed)**

```csharp
[TestMethod] [ExpectedException(typeof(ArgumentException))]
public void Rejects_Unknown_Char() => new CharTyper().Validate('a');
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/DotGreaterPopup.Tests/DotGreaterPopup.Tests.csproj --filter Rejects_Unknown_Char`
Expected: FAIL — `CharTyper` not defined.

- [ ] **Step 3: Implement `NativeMethods` P/Invoke (`SendInput`, `INPUT`, `KEYBDINPUT`, `KEYEVENTF_UNICODE`) and `CharTyper.Insert(char)` in `src/DotGreaterPopup/`**

`Insert` validates against `CharSet`, sends key-down + key-up `SendInput` per char. Keep a `Validate(char)` seam so the test runs headless.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/DotGreaterPopup.Tests/DotGreaterPopup.Tests.csproj`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/DotGreaterPopup/NativeMethods.cs src/DotGreaterPopup/CharTyper.cs tests/
git commit -m "feat: add unicode chartyper with charset guard"
```

### Task 3: Double-right-click hook (pass-through)

**Files:**
- Create: `src/DotGreaterPopup/DoubleRightClickTrigger.cs`
- Modify: `src/DotGreaterPopup/NativeMethods.cs`

**Interfaces:**
- Consumes: `DoubleClickDecision.ShouldFire(...)` from Task 1.
- Produces: `sealed class DoubleRightClickTrigger : IDisposable { event Action<Point> DoubleRightClicked; void Install(); void Dispose(); }`.

- [ ] **Step 1: Add hook-install smoke check (manual, no headless assert)**

No unit test — `WH_MOUSE_LL` needs a desktop. Document the check in this step: install, single right-click shows menu and fires nothing, double right-click fires once.

- [ ] **Step 2: Implement `DoubleRightClickTrigger` in `src/DotGreaterPopup/DoubleRightClickTrigger.cs`**

On `WM_RBUTTONUP` only: read `GetCursorPos`, `GetDoubleClickTime()`, `GetSystemMetrics(SM_CXDOUBLECLK/SM_CYDOUBLECLK)`, delegate to `ShouldFire`; marshal the event to the UI thread via `SynchronizationContext`. Always return `CallNextHookEx`. No UI references here.

- [ ] **Step 3: Manual verify on Windows**

Run: build exe, right-click once in Notepad (menu only, no popup), double-right-click fast (event fires once with cursor point).
Expected: single = silent, double = one event.

- [ ] **Step 4: Commit**

```bash
git add src/DotGreaterPopup/DoubleRightClickTrigger.cs src/DotGreaterPopup/NativeMethods.cs
git commit -m "feat: add pass-through double-right-click hook"
```

### Task 4: Non-activating popup

**Files:**
- Create: `src/DotGreaterPopup/DotGreaterPopupForm.cs`

**Interfaces:**
- Consumes: `CharSet.Chars`, `PopupPosition.Clamp(...)`, `Action<char> onPick` from caller.
- Produces: `sealed class DotGreaterPopupForm : Form { DotGreaterPopupForm(Action<char> onPick); void ShowAt(Point cursor); void HidePopup(); }`.

- [ ] **Step 1: Implement `DotGreaterPopupForm` in `src/DotGreaterPopup/DotGreaterPopupForm.cs`**

Borderless `TopMost` tool-window, `ShowWithoutActivation => true`, `WS_EX_NOACTIVATE|TOOLWINDOW|TOPMOST`, two ≥48px buttons built by looping `CharSet.Chars`, `ShowAt` offsets (+16,+20) then `Clamp` to `Screen.FromPoint(cursor).WorkingArea`. `Deactivate` / `Esc` / button click → idempotent `HidePopup()`.

- [ ] **Step 2: Manual verify on Windows**

Run: trigger `ShowAt` near screen edges and 150% DPI; click `.` — popup closes, focus never leaves Notepad.
Expected: always fully onscreen, never takes focus, Esc/click-away closes.

- [ ] **Step 3: Commit**

```bash
git add src/DotGreaterPopup/DotGreaterPopupForm.cs
git commit -m "feat: add non-activating dot-greater popup"
```

### Task 5: Tray + autostart + Win+J fallback

**Files:**
- Create: `src/DotGreaterPopup/AutostartManager.cs`
- Create: `src/DotGreaterPopup/HotkeyManager.cs`
- Create: `src/DotGreaterPopup/Program.cs`

**Interfaces:**
- Consumes: `DoubleRightClickTrigger`, `DotGreaterPopupForm`, `CharTyper` from Tasks 2–4.
- Produces: running app — `NotifyIcon` tray (Show now / Autostart on-off / Exit), `HKCU Run` entry, `Win+J` → `ShowAt(Cursor.Position)`.

- [ ] **Step 1: Implement `AutostartManager` + `HotkeyManager` + `Program` wiring in `src/DotGreaterPopup/`**

`Program`: single-instance `Mutex`, create trigger + typer + popup (`onPick: c => { HidePopup(); typer.Insert(c); }`), `Win+J` via `RegisterHotKey(MOD_WIN, VK_J=0x4A)` to same `ShowAt`. Tray menu reflects `IsEnabled`.

- [ ] **Step 2: Manual verify on Windows**

Run: reboot or log off/on → tray present; toggle autostart off/on → `HKCU\...\Run` value appears/disappears; `Win+J` opens popup with no `.` key pressed; Exit removes hook + icon cleanly.
Expected: all pass.

- [ ] **Step 3: Commit**

```bash
git add src/DotGreaterPopup/AutostartManager.cs src/DotGreaterPopup/HotkeyManager.cs src/DotGreaterPopup/Program.cs
git commit -m "feat: wire tray, autostart, and Win-J fallback"
```

### Task 6: Release check + README

**Files:**
- Create: `README.md`
- Create: `src/DotGreaterPopup/app.manifest` (optional, `asInvoker` pin if needed)

**Interfaces:**
- Consumes: whole app from Tasks 1–5.
- Produces: `Release/DotGreaterPopup.exe` + README with install, fallback, and remap note.

- [ ] **Step 1: Release-build single exe**

Run: `msbuild DotGreaterPopup.sln /p:Configuration=Release`
Expected: one small `DotGreaterPopup.exe` in `Release/`, runs on stock Win10/11 with no extra install.

- [ ] **Step 2: Run full manual matrix on Windows and record results in README**

Notepad, browser URL bar, Explorer file rename, Start-menu search, 100% + 150% DPI, dual monitor, single right-click (no popup), double right-click (popup), Win+J (popup), Esc/click-away close, elevated Notepad (documented as needs-admin limitation).
Expected: all rows pass except elevated noted as limitation.

- [ ] **Step 3: Write `README.md` (install to Startup or HKCU Run, use, Win+J fallback, uninstall, 2-minute SharpKeys remap fallback: `F8→.` + `Shift+F8→>`)**

- [ ] **Step 4: Commit**

```bash
git add README.md src/DotGreaterPopup/app.manifest
git commit -m "docs: add release verification and readme"
```

## Self-review

- Spec coverage: system-wide hook (T3), keep-menu pass-through (T3), auto-close popup (T4), Unicode typing (T2), tray + autostart (T5), fallback hotkey (T5), DPI clamp (T1+T4), remap fallback note (T6). Covered.
- Step scan: each test/code/verify step names one artifact and its check; no body transcribed beyond the timing-box rule and SendInput shape the signatures don't determine.
- Type consistency: `ShouldFire(lastPt,lastMs,curPt,curMs,dblTimeMs,dblSizePx)`, `Clamp(want,popup,workArea)`, `Insert(char)`, `ShowAt(Point)`, `HidePopup()` used identically across tasks.
- Proportion: plan is longer than the spec but carries only decisions (exact keys, exact registry hive, exact Win32 flags); bodies left to implementers.
