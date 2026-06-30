# 🐦 FlappyBird Console Game

A smooth **60 FPS** Flappy Bird game for the terminal — with themes, two-player co-op, custom game configuration, and a persistent high score board.

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/FlappyBird)](https://www.nuget.org/packages/FlappyBird)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

---

## 🚀 Quick Install & Play

```bash
# Install globally via .NET tool
dotnet tool install --global FlappyBird

# Play anywhere
flappybird
```

> **Requirements:** .NET 10.0 Runtime · 80×24 terminal with color support · Windows recommended for audio (Console.Beep fallback on other platforms)

---

## 🎮 Game Modes

| Mode | Description |
|------|-------------|
| **Single Player** | Classic Flappy Bird — avoid pipes, beat your high score |
| **Two Player** | Local co-op on a split screen; P1 = `Space`, P2 = `Enter` |
| **Custom Game** | Set your own starting level, speed, and gap size |
| **Dual AI** | Watch Conservative AI vs Aggressive AI in a 10-round tournament |
| **High Scores** | View your personal leaderboard across all sessions |

---

## 🕹️ Controls

### Single Player
| Key | Action |
|-----|--------|
| `Space` | Flap |
| `ESC` | Exit to menu |
| `R` (game over) | Restart |

### Two Player
| Player | Flap Key |
|--------|----------|
| P1 (left screen) | `Space` |
| P2 (right screen) | `Enter` |

### Menus
| Key | Action |
|-----|--------|
| `↑` / `↓` | Navigate |
| `Enter` | Confirm |
| `ESC` | Back / Exit |

---

## 🛠️ Custom Game

Configure exactly how you want to play before each session:

**By Level** — pick a starting level (1–10). Speed and gap size are automatically derived; difficulty still progresses as you score.

**By Manual** — set Speed (1–4, where 1 = fastest) and Gap (5–10 rows) yourself. Difficulty is **fixed** for the entire session — great for practice runs.

The two modes are mutually exclusive. Your last configuration is saved automatically and restored next time.

---

## 🏆 High Scores

Every completed session is saved (last 1 000 plays). View them from the main menu:

- `←` / `→` — page through records (10 per page)
- `Tab` — filter: All · Single Player · Custom Game
- `ESC` — back to menu

Custom Game entries display the exact difficulty configuration used that session.

---

## 🎨 Themes

Switch themes in **Settings**:

| Theme | Border | Primary Color |
|-------|--------|---------------|
| Classic | ╔═╗ Double | Cyan |
| Matrix | ┌─┐ Single | Green |
| Neon | ╭─╮ Rounded | Magenta |
| Retro | +-+ ASCII | White |

Theme applies to all menus **and** in-game borders. Colors and border style are individually configurable for a custom look.

---

## ⚙️ Settings

All settings persist between sessions (`%AppData%\FlappyBird\settings.json` on Windows):

- **Language** — English / Vietnamese
- **Theme** — pick a preset or configure individually
- **Border Style** — Double / Single / Rounded / ASCII
- **Primary & Accent Colors**
- **Music** — toggle on/off, volume control
- **Sound Effects** — volume control, audio calibration
- **Font** — switch to a bundled monospace font (requires Windows)
- **Target FPS** — 60 or 120

---

## 📊 Physics & Difficulty

| Parameter | Value |
|-----------|-------|
| Gravity | 0.06f per frame |
| Jump strength | −0.7f velocity |
| Terminal velocity | 1.0f |
| Starting gap | 7 rows |
| Minimum gap | 5 rows (at level 5+) |
| Level up | Every 5 points |
| Pipe speed | Increases every 2 levels |

---

## 🛠️ Development

```bash
git clone https://github.com/TranDat1114/FlappyBird.git
cd FlappyBird
dotnet build
dotnet run --project FlappyBird
```

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## 📦 Advanced Usage

```bash
# Update to latest version
dotnet tool update --global FlappyBird

# Uninstall
dotnet tool uninstall --global FlappyBird
```

---

**Made with ❤️ in C# .NET 10** · [GitHub](https://github.com/TranDat1114/FlappyBird) · [NuGet](https://www.nuget.org/packages/FlappyBird)
