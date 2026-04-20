# Auto

Automations triggered by key combinations (pressed together) or a key sequences (pressed in order).

## Requirements

[.NET 10 Windows Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting Started

Install with the MSI installer, which adds `Auto.exe` to `PATH` and creates a Windows scheduled task that runs `auto start` at logon. Or build from source with `dotnet build` and add the build output directory to your `PATH`.

```powershell
# 1) Create a command bound to Ctrl+Win+B
auto add "Open example.com" --combination LCtrl LWin B

# 2) Add an action
auto action add "Open example.com" StartProgram --arg "https://www.example.com"

# 3) Inspect the command details
auto get "Open example.com"

# 4) Optional: test the command from the CLI
auto execute "Open example.com"

# 5) Start the background listener
auto start
```

After `auto start`, pressing `Ctrl+Win+B` runs the command.

Example `auto get "Open example.com"` output:

```text
Name:        Open example.com
Description:
Id:          15e79fd1-5517-4308-817a-ef21141bf5bd
Enabled:     True
File:        default.json
Trigger:
  Combination:     B+LWin+LControlKey
Actions:
  [0] StartProgram (21092f13-5366-4cba-90df-66bd123e66a5)
    Args:
      https://www.example.com
```

## Concepts

- **Command** - A named automation with one trigger and one or more actions.
- **Trigger** - Keyboard input that activates a command: combination or sequence.
- **Action** - A step that runs when the trigger fires. Actions run in order.
- **Variable** - Optional output captured with `--var`, then reused as `%{VarName}`. When used as input for another plugin, variables are not limited to string objects.
- **Built-in variables** - `%{Clipboard}` and `%{Highlighted}`.
- **Storage** - Commands are JSON files in `~/.config/auto/commands/`.

## CLI

Most commands accept `<name-or-id>`, which means either the command name or its GUID.

### Create Commands

```powershell
# Define triggers directly
auto add "<name>" --combination LCtrl LWin B
auto add "<name>" --sequence S T O P

# Record a trigger interactively
auto add "<name>" --combination
auto add "<name>" --sequence

# Save into a specific file under ~/.config/auto/commands/
auto add "<name>" --file dev-tools.json --combination LCtrl LShift O
```

### Manage Actions

```powershell
# Add plugin actions
auto action add "<name-or-id>" StartProgram --arg "https://www.example.com"
auto action add "<name-or-id>" PowerShell --arg "StopProcess.ps1" "Name=devenv.exe"

# Edit or delete by action index (from `auto get`)
auto action edit "<name-or-id>" 0 --arg "https://example.org"
auto action edit "<name-or-id>" 0 --var Result
auto action edit "<name-or-id>" 0 --var ""
auto action delete "<name-or-id>" 0
```

### View, Run, and Edit Commands

```powershell
auto list
auto get "<name-or-id>"
auto execute "<name-or-id>"

auto edit "<name-or-id>" --name "New name" --description "Updated description"
auto edit "<name-or-id>" --combination  # Record interactively
auto disable "<name-or-id>"
auto enable "<name-or-id>"
auto delete "<name-or-id>"
```

### Variables Between Actions

Built-in variables can be referenced directly (`%{Clipboard}`, `%{Highlighted}`), and action output can be captured with `--var` and reused later.

```powershell
auto action add "Format and Paste" Formatter --arg "%{Clipboard}" --var Formatted
auto action add "Format and Paste" KeyboardInput --arg "%{Formatted}"
```

### Discovery and Utility Commands

| Command | Description |
|---|---|
| `auto list-plugins` | List available plugins |
| `auto list-keys` | List valid key names and aliases for triggers |
| `auto record-input` | Record `KeyboardInput` syntax (double-tap `Esc` to stop) |
| `auto start` | Start the background listener |
| `auto stop` | Stop the background listener |

## Plugins

| Plugin | Description |
|---|---|
| **StartProgram** | Launch a program or URL |
| **KeyboardInput** | Send keyboard input |
| **PowerShell** | Run a script from `~/.config/auto/powershell/` |

Custom plugins are loaded from `~/.config/auto/plugins/`. Each plugin is a subdirectory containing a `plugin.json` and a .NET assembly implementing `ICommand`.
`PowerShell` takes script filename as the first argument, followed by optional script arguments (for example `Name=devenv.exe`).

### Keyboard Input Syntax

The KeyboardInput plugin uses a token syntax for its input argument. Key names use the `System.Windows.Forms.Keys` enum (e.g. `Enter`, `LControlKey`, `Tab`, `F1`).

| Syntax | Description | Example |
|---|---|---|
| `{KeyName}` | Press a named key | `{Enter}`, `{Tab}`, `{F1}` |
| `{+KeyName}` | Hold key down | `{+LControlKey}` |
| `{-KeyName}` | Release key | `{-LControlKey}` |
| `{!ms}` | Sleep (milliseconds) | `{!500}` |
| `{{` / `}}` | Literal `{` / `}` | `{{` → `{` |
| Any character | Type that character | `hello` |

```powershell
# Type "hello" and press Enter
auto action add "MyCommand" KeyboardInput --arg "hello{Enter}"

# Ctrl+C
auto action add "MyCommand" KeyboardInput --arg "{+LControlKey}c{-LControlKey}"
```

Characters like `!`, `@`, `#` etc. can be written directly, the correct modifier keys are applied automatically. `auto record-input` records raw key events, so shifted characters appear as explicit key-down/up sequences (e.g. `{+LShiftKey}1{-LShiftKey}` instead of `!`). Both forms are equivalent. Use `--delay` to also capture timing between keystrokes.

### Creating a Plugin

A dotnet template is included for creating new plugins. Clone the repository and run the following command from project root:

```powershell
# Install the template
dotnet new install ./templates/auto-plugin

# Create a new plugin
dotnet new auto-plugin -n "<Plugin name>" --description "<Plugin description>"
```

Building the plugin automatically deploys it to `~/.config/auto/plugins/`.

## Building

```powershell
dotnet build
```
