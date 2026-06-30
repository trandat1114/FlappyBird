# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [2.0.0] - 2026-06-30

### Added

#### Custom Game Mode
- New **Custom Game** menu option with two mutually exclusive configuration modes:
  - **By Level** — choose a starting level (1–10); Speed and Gap are auto-derived; difficulty progresses normally.
  - **By Manual** — set Speed (1–4) and Gap (5–10) directly; difficulty is **fixed** for the entire session.
- Level and manual parameters are mutually locked in the UI (locked rows appear grayed-out and are skipped during navigation).
- `CustomGameConfig` persists to `settings.json` so the last-used configuration is restored the next time Custom Game is opened.

#### High Scores
- **High Scores** screen accessible from the main menu.
- Stores the last **1,000 play sessions** in `%AppData%\FlappyBird\scores.json` (newest-first, never lost on update).
- Displays scores sorted by score descending (highest first); ties broken by most-recent date.
- **Pagination**: 10 records per page, navigate with `←` / `→`.
- **Filter tabs**: All | Single Player | Custom Game — cycle with `Tab`.
- Custom Game entries show session config (Level or Manual S/G values) inline.
- Scores are saved automatically on game-over; restarting resets the save flag so each life counts once.

#### Settings Persistence
- All settings now survive restarts — saved to `%AppData%\FlappyBird\settings.json` via `System.Text.Json`.
- Persisted fields: Language, TargetFps, MusicEnabled, MusicVolume, EffectsVolume, BorderStyle, PrimaryColor, AccentColor, FontFaceName, FontSize, Custom Game config.
- File is written on startup (first-run defaults), after leaving the Settings menu, and when starting a Custom Game.

#### Theme System
- 4 built-in themes selectable from the Settings menu:
  | Theme   | Border  | Primary Color |
  |---------|---------|---------------|
  | Classic | Double  | Cyan          |
  | Matrix  | Single  | Green         |
  | Retro   | ASCII   | White         |
  | Neon    | Rounded | Magenta       |
- Theme name displays as "Custom" when colors/border don't match a preset.

#### Dynamic Border Styles
- All game screens (SinglePlayer game area, TwoPlayer panels, game-over overlays) now render borders using the active theme's `BorderStyle` instead of hardcoded Double-style characters (`╔╗╚╝═║`).
- `GameSettings.GetBorderSet()` returns the active `BorderSet`; renderers cache it once per frame.

#### Font Management
- Console font can be configured in Settings (scans `Resources/Fonts/` for bundled TTF/OTF files).
- Font change applies immediately without restarting the game.

#### Audio Calibration
- New **Audio Calibration** menu for trimming per-sound start-offset to correct MP3 pre-roll on different systems.
- Offsets saved to `%AppData%\FlappyBird\audio_offset.json`.

#### Localization
- English and Vietnamese UI language support.
- Switch language in Settings; change takes effect immediately.

### Changed

#### Two Player — Pipe Spacing Fix
- `PipeSpacing` converted from a `const` to an **instance property** so each mode can override it.
- TwoPlayer sets `PipeSpacing = 70` (vs SinglePlayer's 35) to compensate for the 38-column panel displaying a 78-column game area (≈ 0.49× scale), preserving the same visual pipe density as SinglePlayer.

#### Difficulty System
- `DifficultyState.Fixed = true` makes `Update()` a no-op — difficulty never progresses in Manual Custom Game.
- `DifficultyState.FixedGapSize` overrides the computed gap when set.
- `DifficultyState.SetLevel(n)` jumps directly to a starting level without regression — `Update()` now uses `<= Level` guard instead of `== Level`.

#### Console Layout
- Dynamic console dimension tracking (`ConsoleLayout`); on window resize SinglePlayer forces a full redraw without visual corruption.
- Minimum enforced size: 78×24.

### Fixed
- Border styles in SinglePlayer and TwoPlayer game screens were always rendered as Double style regardless of the active theme.
- TwoPlayer pipes appeared visually too close together due to unscaled pipe spacing.
- `TargetFps` setting was not included in the settings DTO and therefore not persisted.
- `FontFaceName` deserialization was not null-safe on first run.

---

## [1.1.0] - 2025

### Added
- **Two Player** local co-op mode: P1 (cols 0–39) vs P2 (cols 40–79), independent physics, countdown before start, shared game-over screen with restart/exit options.
- **Audio system**: Background music (Harry Potter theme) via NAudio on Windows with Console.Beep fallback; sound effects for Jump, Score, and GotHit (MP3).
- **Dual AI Comparison** mode: conservative AI vs. aggressive AI in a side-by-side 10-round tournament.
- God Mode rule-based AI: 3-priority decision tree (emergency avoidance → pattern-matched height → predictive trajectory).
- Q-Learning AI agent with tabular Q-table (6 000 states), JSONL training logger, reward calculator.
- `TwoPlayerBuffer` diff-renderer: only changed cells written per frame, eliminating flicker in side-by-side layout.

### Changed
- Menu system redesigned with anti-flicker in-place selection updates.
- Game over handling changed from immediate exit to an interactive overlay menu (Restart / Exit).
- Progressive difficulty: gap shrinks from 7 → 5 rows every 5 points; pipe speed increases every level.

---

## [1.0.0] - 2025

### Added
- Initial release: Single Player Flappy Bird on a 78×20 console game area.
- 60 FPS game loop (16 ms target) with separate update and input threads.
- Bird physics: gravity (0.06f), jump strength (−0.7f), terminal velocity clamped at 1.0f.
- Pipe generation and scrolling; score increments on passing a pipe.
- Diff-buffer rendering via `GameState.PreviousScreen` — only changed cells written.
- `UIPanel` border engine with `Double | Single | Rounded | ASCII` styles.
- Main menu with keyboard navigation.
- Published as a global .NET tool (`dotnet tool install --global FlappyBird`).
