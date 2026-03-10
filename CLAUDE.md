# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ClassicAssist is a plugin for [ClassicUO](https://github.com/andreakarasho/ClassicUO) (Ultima Online client) providing a UOSteam-like interface with Python (IronPython 2.7) macro scripting, hotkeys, autoloot, and various automation agents.

## Build Commands

```bash
dotnet restore
dotnet build                          # Debug build (default)
dotnet build -c Release               # Release build
dotnet test --no-build --verbosity normal  # Run all tests (MSTest)
```

Output goes to `.\Output\` directory. Build configurations: Debug, Develop, Release. Platform: x64.

## Project Architecture

**Plugin loading flow:** ClassicUO loads `ClassicAssist.Plugin.dll` -> Plugin uses StreamJsonRpc IPC to communicate with `ClassicAssist.UI.exe` -> UI hosts the main `ClassicAssist.dll` library.

### Key Projects

- **ClassicAssist** (net48) - Core library: macros, hotkeys, networking, UI ViewModels/Views, UO game logic. This is where most development happens.
- **ClassicAssist.Plugin** (net48 + net9.0) - Thin plugin wrapper loaded by ClassicUO. Uses reflection (`ClassicAssist.Plugin.Shared/Reflection/`) to interact with ClassicUO internals.
- **ClassicAssist.Plugin.Shared** (netstandard2.0) - Cross-runtime interfaces (`IHostMethods`, `IPluginMethods`, `NativeMethods`).
- **ClassicAssist.UI** (net48) - Standalone WPF app, communicates with plugin via StreamJsonRpc over named pipes.
- **ClassicAssist.Shared** (net48) - Shared utilities, localization resources (`Strings.resx`), settings.
- **ClassicAssist.Controls** (net48) - Reusable WPF controls.
- **ClassicAssist.Launcher** (net48) - Launches ClassicUO with the plugin configured.
- **ClassicAssist.Tests** (net48) - MSTest unit tests.

### Important Directories Within ClassicAssist/

- `Data/Macros/Commands/` - All macro command implementations (Python-exposed functions). Each `*Commands.cs` file is a category (e.g., `ActionCommands.cs`, `TargetCommands.cs`, `MovementCommands.cs`).
- `Data/Hotkeys/` - Hotkey system with command classes.
- `Data/Autoloot/` - Autoloot agent logic and property matching.
- `UO/Network/` - Packet handling (`IncomingPacketHandlers.cs`, `OutgoingPacketHandlers.cs`), packet construction, filtering.
- `UO/Objects/` - Game object model (`Item`, `Mobile`, `ItemCollection`, `Entity`).
- `UO/Gumps/` - In-game gump rendering.
- `UI/ViewModels/` - WPF ViewModels (MVVM pattern).
- `UI/Views/` - WPF XAML views.

### Key Singletons / Static Classes

- `Engine` (ClassicAssist/Engine.cs) - Central static class: player state, packet sending, dispatcher, client version, options. Partial class split across files.
- `Engine` (ClassicAssist.Plugin/Engine.cs) - Plugin-side engine handling ClassicUO integration.
- `Options` / `CurrentOptions` - Global settings.
- Various managers: `MacroManager`, `HotkeyManager`, `AutolootManager`, `ActionPacketQueue`.

## Code Style

Defined in `.editorconfig`. Key conventions:
- **Spaces for indentation** (4 spaces) in C#, XAML, and most code files.
- **Spaces inside parentheses** for method calls, declarations, and control flow: `if ( condition )`, `Method( arg )`.
- **Spaces after cast**: `(int) value`.
- **Explicit types preferred** over `var` (`csharp_style_var_*: false`).
- **Braces required** for all control flow (`if`, `for`, `foreach`, `while`).
- **Max line length**: 180 characters.
- Constants: `ALL_UPPER_CASE`. Local functions: `OnPascalCase` prefix.
- Private fields: `_camelCase` prefix convention used throughout.

## Threading Model

WPF UI thread is accessed via `Engine.Dispatcher`. When creating UI elements (windows, ViewModels with ObservableCollections) from background threads (e.g., macros), wrap in `Engine.Dispatcher.Invoke()`. The `_dispatcher` field in ViewModels is used for the same purpose within collection change handlers.

`ActionPacketQueue` serializes game actions (drag/drop, use object) with configurable delays.

## Macro System

Macros use IronPython 2.7. Command functions are defined as static methods in `Data/Macros/Commands/*Commands.cs` files, decorated with `[CommandsDisplay]` attribute for UI categorization. Macro commands are documented in the GitHub wiki.

## Localization

UI strings go through `ClassicAssist.Shared/Resources/Strings.resx`. Translations managed via POEditor. Use `Strings.*` for all user-facing text.
