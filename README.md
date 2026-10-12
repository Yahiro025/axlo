# DotGreaterPopup

## What it does

Your laptop's `.` / `>` physical key is broken, so DotGreaterPopup gives you
both characters back everywhere in Windows: double right-click anywhere (or
press Win+Shift+J) and a small two-button popup appears next to the cursor; clicking
`.` or `>` types that character into whatever field is under the cursor. It is
a single tiny WinForms exe (`net48`, no runtime to install), runs from the
system tray, can start with Windows, and never steals focus or swallows your
normal right-clicks.

## Install

1. Build (on a Windows laptop with MSBuild):
   `msbuild DotGreaterPopup.sln /p:Configuration=Release`
   This produces one small `Release/DotGreaterPopup.exe` that runs on stock
   Windows 10/11 with no extra install.
2. Copy `DotGreaterPopup.exe` to a permanent folder, e.g.
   `C:\Tools\DotGreaterPopup\DotGreaterPopup.exe`, and run it. A tray icon
   appears (no console window).
3. Right-click the tray icon and check **Start with Windows**. That writes one
   value, `DotGreaterPopup = "<quoted path to your exe>"`, under
   `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`. It touches HKCU only,
   never HKLM, so no admin prompt is needed.

## Use

- **Double right-click** anywhere: the popup opens next to the cursor
  (offset 16 px right, 20 px down, clamped to the monitor work area).
  Your normal context menu is untouched.
- Or press **Win+Shift+J**: the same popup opens at the cursor. Use this whenever a
  second right-click might accidentally hit a menu item.
- Click the `.` or `>` button. The character is typed into the focused field
  and the popup closes. Clicking away or pressing Esc closes it with no typing.

## Uninstall

1. Right-click the tray icon and choose **Exit**.
2. If you enabled autostart, re-launch the exe once, uncheck
   **Start with Windows**, then Exit again. (Or delete the
   `DotGreaterPopup` value under
   `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` in regedit.)
3. Delete `DotGreaterPopup.exe`.
4. Confirm the registry value is gone:
   `reg query "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v DotGreaterPopup`
   should report "unable to find the registry value".

## Win+Shift+J fallback

Double right-click is the primary trigger, but the second click can land on a
native menu item. Whenever that is a risk, press **Win+Shift+J** instead: it opens
the exact same popup at the current cursor position, needs neither `.` nor
`>`, and still works if hotkey registration ever fails (the app keeps running
on double-right-click and says so in a tray balloon).

## SharpKeys remap fallback (2 minutes)

If you would rather have a real key again, remap a spare key with SharpKeys:

1. Install SharpKeys (Microsoft Store or https://github.com/randyrants/sharpkeys).
2. Click Add, map **F8** (From) to **. Period** (To), then Write to Registry.
3. Log off and back on (or reboot).
4. Now F8 types `.`, and **Shift+F8 types `>`** automatically, because
   Shift+Period is `>` on the US layout. One mapping covers both characters.

## Elevated-apps limitation (by design)

Windows UIPI blocks a normal-integrity app from sending keys to an elevated
(run-as-administrator) window. Typing into an elevated Notepad therefore
requires running `DotGreaterPopup.exe` as administrator too. This is accepted
by design: the default install stays at `asInvoker` so there is never a UAC
prompt. Day to day, prefer the SharpKeys remap above for elevated windows, or
right-click the exe and choose Run as administrator for that session.

## App manifest note

`src/DotGreaterPopup/app.manifest` pins `asInvoker` (no UAC prompt) with no
other execution settings. It is not wired into the build yet. To wire it, add
this one line inside the `<PropertyGroup>` in
`src/DotGreaterPopup/DotGreaterPopup.csproj`:

    <ApplicationManifest>app.manifest</ApplicationManifest>

then rebuild the Release exe.

## MANUAL MATRIX (run on your Windows laptop, tick each row)

Release build:

- [ ] `msbuild DotGreaterPopup.sln /p:Configuration=Release` succeeds; one
  small `Release/DotGreaterPopup.exe` exists and launches on stock Win10/11
  with no extra install (tray icon appears, no console window).

Task 2 - live SendInput typing (proves the INPUT union layout by typing):

- [ ] `.` types in Notepad.
- [ ] `>` types in Notepad.
- [ ] `.` / `>` type in a browser URL bar.
- [ ] `.` / `>` type in an Explorer file-rename box.
- [ ] `.` / `>` type in Start-menu search.

Task 3 - double-right-click trigger:

- [ ] Single right-click: native menu only, no popup (hook is silent).
- [ ] Double right-click: popup opens exactly once, at the cursor point.

Task 4 - popup behavior (five checks):

- [ ] Click `.` in Notepad: the char types and Notepad stays foreground.
- [ ] Click `>` in Notepad: the char types and Notepad stays foreground.
- [ ] Click away from the popup: it closes, nothing is typed.
- [ ] Press Esc with the popup open: it closes, nothing is typed.
- [ ] Open it at screen edges, at 150% DPI, and on a second monitor: the
  popup stays fully onscreen every time.
- [ ] Double right-click in Notepad: the popup settles beside the native
  menu (right side, or left near the screen edge), never hidden behind it.
- [ ] Double right-click in a browser: no system menu exists, so the popup
  keeps the cursor-offset position above the page menu.

Task 5 - tray, autostart, hotkey, exit (four checks):

- [ ] Reboot/log off and on: tray icon is present, no console window.
- [ ] Tray toggle Start with Windows ON: the `DotGreaterPopup` value appears
  under `HKCU\...\Run`; toggle OFF: the value disappears; the checkmark
  matches the real registry state each time the menu opens.
- [ ] With no working `.` key, Win+Shift+J opens the popup at the cursor.
- [ ] Tray Exit: hook and tray icon are cleaned up; relaunching works.

DPI + elevated rows:

- [ ] Full pass above repeated at 100% DPI: all green.
- [ ] Full pass above repeated at 150% DPI: all green.
- [ ] Elevated Notepad: typing does NOT land unless DotGreaterPopup is also
  run as administrator (documented limitation, not a bug).
