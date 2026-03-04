# Auto

Execute C# plugins or PowerShell scripts on key combinations or sequences.

Run without arguments to start the background listener. Run with a subcommand to manage commands via the CLI.

## CLI

Commands are stored as JSON files in `~/.config/auto/commands/`.

```powershell
# List / search commands
auto list
auto list --search "browser" --enabled

# Inspect a command (by name or ID)
auto get "Open Browser"
auto get "Open Browser" --json

# Add a command that launches a program on Ctrl+Win+B
auto add "Open Browser" --file default.json `
  --combination "LCtrl+LWin+B" `
  --plugin StartProgram `
  --plugin-arg "https://google.com"

# Add a command that types text on Ctrl+Win+E
auto add "Type Email" --file default.json `
  --combination "LCtrl+LWin+E" `
  --plugin KeyboardInput `
  --plugin-arg "user@example.com"

# Add a PowerShell script triggered by a key sequence
auto add "Stop Procs" --file default.json `
  --sequence "S,T,O,P" `
  --powershell "StopProcs.ps1" `
  --ps-arg "StopProcs.ps1:Path=C:`scripts"

# Use clipboard or highlighted text as an argument
auto add "Search Selection" --file default.json `
  --combination "LCtrl+LWin+S" `
  --plugin StartProgram `
  --plugin-arg "https://google.com/search?q=%{highlighted}"

# Nested plugins — pass the output of one plugin as input to another
auto add "Format and Paste" --file default.json `
  --combination "LCtrl+LAlt+V" `
  --action "plugin:KeyboardInput" `
  --plugin-arg "KeyboardInput:%{plugin:Formatter-guid}" `
  --plugin-arg "Formatter-guid:json" `
  --plugin-arg "Formatter-guid:%{clipboard}"

# List available plugins
auto list-plugins
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
