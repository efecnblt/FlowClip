# FlowClip

A modern clipboard manager for Windows with a floating widget interface.

## Features

- **Floating Widget** - Always-on-top circular widget that expands to show clipboard history
- **Smart Content Detection** - Automatically detects and categorizes text, code, colors, URLs, and images
- **Drag & Resize** - Freely move and resize the panel to fit your workflow
- **System Tray Integration** - Quick access from the system tray
- **Global Hotkey** - Press `Ctrl+Shift+V` to toggle the history panel
- **Persistent History** - SQLite database stores up to 50 clipboard entries
- **Pin Important Items** - Keep frequently used clips at the top
- **Modern Dark UI** - Clean, minimalist interface with smooth animations

## Requirements

- Windows 10/11
- .NET 8.0 Runtime

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/flowclip.git
cd flowclip
```

2. Build the project:
```bash
dotnet build src/FlowClip/FlowClip.csproj
```

3. Run FlowClip:
```bash
dotnet run --project src/FlowClip/FlowClip.csproj
```

## Usage

### Basic Operations
- **Click** the widget to expand/collapse the clipboard history panel
- **Drag** the widget to move it anywhere on your screen
- **Resize** using the grip in the bottom-right corner of the panel
- **Click any entry** to copy it back to the clipboard
- **Pin entries** using the 📌 button to keep them at the top
- **Delete entries** using the 🗑 button

### Keyboard Shortcuts
- `Ctrl+Shift+V` - Toggle history panel

### System Tray
- Right-click the tray icon for quick access to:
  - Show/Hide window
  - Settings
  - Exit application

## Data Storage

FlowClip stores your clipboard history in:
```
%LocalAppData%\FlowClip\
├── flowclip.db          # SQLite database
└── Images\              # Image thumbnails
```

## Building from Source

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code (optional)

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/FlowClip/FlowClip.csproj

# Publish (single executable)
dotnet publish -c Release -r win-x64 --self-contained
```

## Technologies

- .NET 8 (WPF)
- Entity Framework Core (SQLite)
- CommunityToolkit.Mvvm
- H.NotifyIcon.Wpf

## License

MIT License - feel free to use and modify as needed.
