# Auto

Execute C# plugins or PowerShell scripts on key combinations or sequences.

Run without arguments to start the background listener. Run with a subcommand to manage commands via the CLI.

## CLI

Commands are stored as JSON files in `~/.config/auto/commands/`.

```powershell
# Create a command
auto add "Open example.com" --combination LCtrl LWin B
auto add "Stop process" --file scripts.json --sequence S T O P --description "Kill runaway processes"
auto add "Disabled Example" --combination LCtrl D --disabled

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
auto edit "Open example.com" --name "Go to example.com" --description "Opens example.com in default browser"

# --- Other commands ---
auto list
auto get
auto delete
auto enable
auto disable
auto list-plugins
auto start
```

## Plugins

| Plugin | Description |
|---|---|
| **StartProgram** | Launch a program or URL |
| **KeyboardInput** | Send keyboard input |

Custom plugins are loaded from `~/.config/auto/plugins/`. Each plugin is a subdirectory containing a `plugin.json` and a .NET assembly implementing `ICommand`.

## Building

```powershell
dotnet build
```
