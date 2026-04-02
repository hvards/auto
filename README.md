# Auto

Execute C# plugins or PowerShell scripts on key combinations or sequences.

Run without arguments to start the background listener. Run with a subcommand to manage commands via the CLI.

## CLI

Commands are stored as JSON files in `~/.config/auto/commands/`.

```powershell
# Create a command with explicit keys
auto add "Open example.com" --combination LCtrl LWin B
auto add "Stop process" --file scripts.json --sequence S T O P --description "Stop process"
auto add "Disabled Example" --combination LCtrl D --disabled

# Create a command with interactive key recording
auto add "Open example.com" --combination
auto add "Stop process" --sequence

# Adding actions
auto action add "Open Browser" --plugin StartProgram --arg "https://www.example.com"
auto action add "Stop process" --powershell "StopProcess.ps1" --arg "devenv.exe"

# Variable interpolation, %{Clipboard}, %{Highlighted}, etc.
auto action add "Search Selection" --plugin StartProgram `
  --arg "https://google.com/search?q=%{Clipboard}"

# Capture output with --var, reference it in later actions with %{VarName}
auto action add "Format and Paste" --plugin Formatter --arg "%{Clipboard}" --var Formatted
auto action add "Format and Paste" --plugin KeyboardInput --arg "%{Formatted}"

# Action remove
auto action remove "Open Browser" --index 0
auto action remove "Format and Paste" --var Formatted # By variable
auto action remove "Open Browser" --plugin StartProgram # By plugin

# Edit actions, action index after command identifier
auto action edit "Format and Paste" 0 --arg "https://newurl.com"
auto action edit "Format and Paste" 1 --var NewVarName
auto action edit "Format and Paste" 0 --var ""

# Edit command
auto edit "Open example.com" --combination LCtrl LWin LAlt B
auto edit "Open example.com" --combination  # Record interactively
auto edit "Open example.com" --name "Go to example.com" --description "Opens example.com in default browser"

# --- Other commands ---
auto list
auto get
auto delete
auto enable
auto disable
auto list-plugins
auto list-powershell
auto record-input
auto start
```

## Plugins

| Plugin | Description |
|---|---|
| **StartProgram** | Launch a program or URL |
| **KeyboardInput** | Send keyboard input |

Custom plugins are loaded from `~/.config/auto/plugins/`. Each plugin is a subdirectory containing a `plugin.json` and a .NET assembly implementing `ICommand`.

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
auto action add "MyCommand" --plugin KeyboardInput --arg "hello{Enter}"

# Ctrl+C
auto action add "MyCommand" --plugin KeyboardInput --arg "{+LControlKey}c{-LControlKey}"
```

Characters like `!`, `@`, `#` etc. can be written directly, the correct modifier keys are applied automatically. `auto record-input` records raw key events, so shifted characters appear as explicit key-down/up sequences (e.g. `{+LShiftKey}1{-LShiftKey}` instead of `!`). Both forms are equivalent. Use `--delay` to also capture timing between keystrokes.

### Creating a Plugin

A dotnet template is included for creating new plugins:

```powershell
# Install the template
dotnet new install ./templates/auto-plugin

# Create a new plugin
dotnet new auto-plugin -n MyPlugin --description "What my plugin does"
```

Building the plugin automatically deploys it to `~/.config/auto/plugins/`.

## Building

```powershell
dotnet build
```
