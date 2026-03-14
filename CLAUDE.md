# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ClassicAssist is a plugin for [ClassicUO](https://github.com/andreakarasho/ClassicUO) (Ultima Online client) providing a UOSteam-like interface with IronPython 2.7 macro scripting, hotkeys, autoloot, and various automation agents. WPF-only (Windows).

## Build & Test

```bash
dotnet restore
dotnet build                                    # Debug build
dotnet build -c Release                         # Release build
dotnet test --no-build --verbosity normal       # Run all tests (MSTest)
dotnet test --no-build --filter "FullyQualifiedName~ClassName.MethodName"  # Single test
```

Build output goes to `.\Output\`. Configurations: Debug, Develop, Release. Platform: x64.

## Architecture

**Plugin loading:** ClassicUO loads `ClassicAssist.Plugin.dll` (net48 + net9.0) → Plugin uses StreamJsonRpc IPC over named pipes → `ClassicAssist.UI.exe` (standalone WPF app) hosts the main `ClassicAssist.dll` library.

### Key Projects

| Project | Framework | Role |
|---------|-----------|------|
| **ClassicAssist** | net48 | Core library — macros, hotkeys, networking, UI, game logic |
| **ClassicAssist.Plugin** | net48 + net9.0 | ClassicUO plugin entry point |
| **ClassicAssist.Plugin.Shared** | netstandard2.0 | Cross-runtime interfaces (IHostMethods, IPluginMethods, NativeMethods) |
| **ClassicAssist.Shared** | net48 | Localization (Strings.resx), settings, utilities |
| **ClassicAssist.Controls** | net48 | Reusable WPF controls |
| **ClassicAssist.UI** | net48 | Standalone WPF host app |
| **ClassicAssist.Launcher** | net48 | Launches ClassicUO with plugin configured |
| **ClassicAssist.Tests** | net48 | MSTest unit tests |

### Core Singletons

- `Engine` (namespace `Assistant`, ClassicAssist/Engine.cs) — Central static partial class: player state, `Dispatcher` (WPF UI thread), packet send/receive, options, client version. Most game state flows through here.
- `Options` / `Options.CurrentOptions` — Global settings, profile-aware.
- Managers: `MacroManager`, `HotkeyManager`, `AutolootManager`, `ActionPacketQueue` — all use `GetInstance()` singleton pattern.

### Macro System

Macros use IronPython 2.7. Command functions are static methods in `Data/Macros/Commands/*Commands.cs`, decorated with `[CommandsDisplay]` for UI categorization and wiki generation. Each file is a category (e.g., `TargetCommands.cs`, `MovementCommands.cs`). Translations for command descriptions/examples are loaded from `MacroCommandHelp.resx` using the convention `{METHOD_NAME_UPPER}_COMMAND_DESCRIPTION`, `_COMMAND_INSERTTEXT`, `_COMMAND_EXAMPLE`.

### Packet Handling

- `UO/Network/IncomingPacketHandlers.cs` — Server-to-client packet processing
- `UO/Network/OutgoingPacketHandlers.cs` — Client-to-server packet processing
- `UO/Network/IncomingPacketFilters.cs` — Filtered/modified incoming packets (name overrides, message filters)
- `UO/Network/OutgoingPacketFilters.cs` — Filtered outgoing packets
- `UO/Network/Packets/` — Packet construction classes
- `UO/Network/PacketFilter/` — Packet wait/filter infrastructure

Variable-length packets store length big-endian at bytes [1],[2]: high byte = `(byte)(length >> 8)`, low byte = `(byte)length`.

### UI Pattern (MVVM)

- ViewModels in `UI/ViewModels/`, Views in `UI/Views/`
- `BaseViewModel` (UI/ViewModels/BaseViewModel.cs) captures `Dispatcher.CurrentDispatcher` at construction time into `_dispatcher`. This means ViewModels **must be created on the UI thread** — if created from a background thread (e.g., a macro), wrap in `Engine.Dispatcher.Invoke()`.
- `_dispatcher.Invoke()` is used throughout ViewModels for ObservableCollection mutations from background threads.
- Settings persistence uses `ISettingProvider` with `Serialize`/`Deserialize` methods using `Newtonsoft.Json.Linq.JObject`.

### Game Objects

- `UO/Objects/` — `Entity`, `Item`, `Mobile`, `ItemCollection`, `PlayerMobile`
- `UO/Data/` — Static game data (TileData, Art, Cliloc, Statics)
- `UO/Gumps/` — In-game gump rendering
- Serial ranges: `< 0x40000000` = mobile, `>= 0x40000000` = item (`UOMath.IsMobile`)

## Code Style

Defined in `.editorconfig`:
- 4-space indentation for C#/XAML
- **Spaces inside parentheses**: `if ( condition )`, `Method( arg )`, `(int) value`
- Explicit types preferred over `var`
- Braces required for all control flow
- Max line length: 180
- Constants: `ALL_UPPER_CASE`
- Private fields: `_camelCase`
- Local functions: `OnPascalCase` prefix

## Localization

All user-facing strings go through `ClassicAssist.Shared/Resources/Strings.resx`. Translations managed via POEditor.
