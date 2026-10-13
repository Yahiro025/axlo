---
# gstack: design-md-format=spec
name: axlo
description: Quiet utilitarian instrument panel for tiny Windows fixes that just work.
colors:
  primary: "#0F6CBD"          # Fluent blue, owns all action
  on-primary: "#FFFFFF"
  surface: "#FFFFFF"
  text: "#1B1A19"             # Fluent neutral 190
  text-muted: "#605E5C"       # Fluent neutral 130
  accent: "#0F6CBD"
  success: "#107C10"
  warning: "#CA5010"
  error: "#C42B1C"
typography:
  display:
    fontFamily: Cabinet Grotesk
    fontWeight: 700
    fontSize: clamp(2rem, 4vw, 3rem)
    letterSpacing: -0.02em
  body:
    fontFamily: Source Sans 3
    fontSize: 1rem
    lineHeight: 1.5
  label:
    fontFamily: Segoe UI
    fontSize: 0.75rem
    letterSpacing: 0.04em
  mono:
    fontFamily: JetBrains Mono
    fontFeature: tnum
rounded:
  sm: 4px
  md: 6px
  lg: 8px
  full: 9999px
spacing:
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  2xl: 48px
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.on-primary}"
    rounded: "{rounded.md}"
  button-primary-hover:
    backgroundColor: "#0B5A9E"
  input:
    borderColor: "{colors.text-muted}"
    rounded: "{rounded.sm}"
  card:
    backgroundColor: "{colors.surface}"
    rounded: "{rounded.lg}"
  nav-link:
    textColor: "{colors.text}"
---

# axlo

## Overview

**Creative North Star:** Industrial/Utilitarian calm. One blue, one mono glyph, nothing decorative. The brand should feel like it shipped with Windows.
**Product context:** axlo is the parent brand for tiny Windows utilities. First product: DotGreaterPopup, a tray-resident popup that restores `.` / `>` via double right-click or Win+Shift+J for a broken laptop key. Users: keyboard-first developers and the owner. Surfaces: 16px tray icon (Operate), two-button popup (Operate), README/GitHub header (Read).
**Mode per surface:** Operate (tray + popup): native Segoe UI, minimal motion. Read (README): Cabinet Grotesk display, Source Sans 3 body.
**Reference sites:** https://fluent2.microsoft.design, https://fluent2.microsoft.design/iconography
**Key characteristics:** lowercase wordmark with a dot-o glyph; `.>` product suffix in mono; monochrome at small sizes; single Fluent blue for action.

## Colors

**Strategy:** Restrained. One accent owns every interactive moment; neutrals do all structural work.
**Light or dark:** Both. Light README/surface is the default scene; tray ships light + dark glyph variants so the mark survives the taskbar theme. Dark surfaces keep the same blue at full saturation rather than inverting the palette.

Primary #0F6CBD signals action and the axlo dot-o. Text #1B1A19 on surface #FFFFFF is the only body pair. Muted #605E5C is metadata only, never body copy. Success/warning/error follow Fluent semantics and appear only as status, never decoration.

## Typography

Display is Cabinet Grotesk 700 (Fontshare, ITF free commercial license, weights 100-900 variable; verified live 2026-10-13 via api.fontshare.com). It carries the axlo wordmark and README headers. Its subtle stroke contrast keeps lowercase `axlo` distinct without quirk.

Body is Source Sans 3 400/600 (Google Fonts, SIL OFL; Adobe Paul D. Hunt). Designed for UI text, it stays legible in README install steps.

Mono is JetBrains Mono 500/700 (Google Fonts, SIL OFL 1.1; JetBrains). It sets the `.>` product suffix, key names like Win+Shift+J, and code. The logo's `>` cut uses the same construction.

In-app UI stays Segoe UI (system font, zero-install). Brand fonts never ship in the exe; they live in README, wordmark art, and web surfaces only. Loading: Google Fonts link for Source Sans 3 + JetBrains Mono; Fontshare `api.fontshare.com/v2/css?f[]=cabinet-grotesk@700,800` for display.

Component scale in use on brand surfaces: lockup 1.75rem and README header 1.8rem display 700; section titles 1.4rem display 700; UI controls 0.95rem semibold; popup action 1.05rem Segoe UI 600; menu text 0.85rem; badges 0.75-0.8rem mono 700; captions and notes 0.72-0.9rem. Body stays 1rem. No functional text below 0.75rem (12px).

## Layout

Grid-disciplined. The popup is two equal buttons plus a small `axlo .>` badge. The tray menu is the native Windows menu, untouched. README is a single left-aligned column: lockup, one-line promise, install, use. Density: popup tight (8/12px), README airy (16/24/32px rhythm on a 4px base).

## Elevation & Depth

Almost none. Popup uses one Windows-native shadow (offset + soft blur) and a 1px neutral border. No glow, no halo, no frosted layers. Depth comes from offset only.

## Shapes

Radius hierarchy: 4px inputs, 6px buttons and popup frame, 8px cards, full for the dot-o glyph. Utilitarian and slightly squared; never bubbly. Nested inner radius equals outer minus gap.

## Components

- Tray icon: monochrome dot-o glyph (dot with `>` knockout), currentColor, light/dark variants. Sizes drawn at 16/20/24, exported through 256 in the .ico. Never contains text.
- Popup buttons: Segoe UI, 6px radius, Fluent blue primary for `.` / `>` actions; hover #0B5A9E; focus-visible 2px blue outline; disabled uses muted text on neutral fill.
- Badge: `axlo .>` in JetBrains Mono 700, muted, non-interactive.
- README lockup: wordmark left, promise right, single blue download/build link.

## Do's and Don'ts

- Do: keep the tray glyph one color; ship light + dark variants.
- Do: let the `o` dot carry the logo at 16px; test every cut at actual size.
- Do: use blue only for action; keep everything else neutral.
- Do: write README steps in Source Sans 3 with mono for keys and code.
- Don't: add gradients, glows, mascots, or illustration doodles.
- Don't: set body copy in muted gray or center-align the README.
- Don't: ship brand fonts inside the exe or redraw the native context menu.
- Don't: nest cards or put icons in colored circles as decoration.

## Motion

- **Approach:** minimal-functional
- **Easing:** enter(ease-out) exit(ease-in) move(ease-in-out)
- **Duration:** micro(50-100ms) short(150-250ms) medium(250-400ms) long(400-700ms)
- **The one authored moment:** the popup appears beside the native menu with a 100ms fade. Nothing springs, nothing slides far.

## Decisions Log
| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-10-13 | Initial design system created | Created by /design-consultation: axlo parent brand + DotGreaterPopup product lockup; Industrial/Utilitarian; Fluent blue + Fluent neutrals; Cabinet Grotesk / Source Sans 3 / JetBrains Mono with Segoe UI native in-app |
