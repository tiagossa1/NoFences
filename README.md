# NoFences

A free, open-source alternative to Stardock Fences for Windows. Didn't want to pay 11€, made my own.

![Screenshot](screenshot.png "NoFences in action")

## Features

- **Fences** — draggable, resizable desktop containers that hold shortcuts to files and folders
- **Drag & drop** — drop files or folders straight onto a fence to add them
- **Rename, lock, and minify** fences from the right-click menu
- **Adjustable title bar size** per fence
- **Native context menu** — right-click an item to get the real Windows Explorer shell menu
- **Dark mode aware** context menus
- **Persistent layout** — position, size, and contents are saved automatically and restored on startup
- Available in English and Chinese (zh-CN)

## Requirements

- Windows 10/11 (x64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or the .NET 10 Desktop Runtime to just run the app)

## Getting Started

Clone the repo and build/run with the .NET SDK:

```powershell
git clone https://github.com/tiagossa1/NoFences.git
cd NoFences
dotnet build NoFences.sln -c Release
dotnet run --project NoFences/NoFences.csproj
```

Alternatively, open `NoFences.sln` in Visual Studio 2022 (17.14+) and press F5.

On first launch, a default fence is created automatically. Right-click a fence's title bar for options (new fence, rename, lock, minify, title size, remove).

## How it works

Each fence is persisted as an XML file under `%LOCALAPPDATA%\NoFences\<fence-guid>\__fence_metadata.xml`, so your layout survives restarts without needing a database. See [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for a deeper dive into the architecture.

## Contributing

Issues and pull requests are welcome. There's no CI or automated test suite yet, so please verify changes by running the app manually before submitting a PR.

## License

Licensed under the [MIT License](LICENSE).
