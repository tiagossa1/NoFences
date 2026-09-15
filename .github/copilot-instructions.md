# NoFences

A Windows desktop app (WinForms, .NET 10, SDK-style project) that recreates Stardock Fences: draggable/resizable
"fence" windows glued to the desktop, each holding shortcuts to files/folders.

## Build

No CI, tests, or lint config exist in this repo — build with the .NET SDK or Visual Studio.

```powershell
# From repo root
dotnet build NoFences.sln -c Release
dotnet run --project NoFences\NoFences.csproj
```

- Open `NoFences.sln` in Visual Studio (2022 17.14+, with the .NET 10 SDK installed) and build/run with F5 — this
  also works.
- Target framework is `net10.0-windows`; platform target is `x64` for both Debug and Release (see
  `NoFences.csproj`, an SDK-style project — most files are included implicitly via globbing, not listed explicitly).
- Assembly metadata (title, version, copyright, etc.) lives in `<PropertyGroup>` in the `.csproj`, not in a
  `Properties\AssemblyInfo.cs` file.
- There is no test project and no automated tests. Verify changes by running the app manually.

## Architecture

- **`Program.cs`** — entry point. Uses a named `Mutex` ("No_fences") to enforce a single instance, then calls
  `FenceManager.Instance.LoadFences()` to restore saved fences (or creates a default "First fence" if none exist).
- **`Model/FenceManager`** (singleton `Instance`) — owns persistence. Each fence is a folder under
  `%LOCALAPPDATA%\NoFences\<fence-guid>\`, containing a `__fence_metadata.xml` file (XML-serialized `FenceInfo`).
  `CreateFence`/`UpdateFence`/`RemoveFence` read/write these files; there is no database.
- **`Model/FenceInfo`** — the serialized state of one fence (position, size, title height, locked/minify flags,
  list of file paths). **Do not rename its properties** — the XML serializer depends on the property names for
  backward compatibility with existing users' saved fences (see the comment in the file).
- **`Model/FenceEntry`** — represents one shortcut inside a fence (a file or folder path). Resolves icons via
  `Util/ThumbnailProvider` (thumbnail generation, async, raises `IconThumbnailLoaded`) or
  `Win32/IconUtil`/`Icon.ExtractAssociatedIcon`, and opens items with `Process.Start`.
- **`FenceWindow`** (WinForms `Form`) — one instance per fence; does almost everything: custom-paints the
  fence background/title/icons directly in `OnPaint` (no child controls for items — icons are hit-tested manually
  using stored mouse position vs. computed rectangles), overrides `WndProc` to strip the window border, block
  maximize/focus-stealing, and implement custom drag/resize via `WM_NCHITTEST` hit-testing. Position/size changes
  are persisted through `Save()` → `FenceManager.Instance.UpdateFence`, throttled via `Util/ThrottledExecution`
  (`throttledMove`/`throttledResize`, 4s interval) to avoid excessive disk writes.
- **`Win32/*`** — P/Invoke wrappers for desktop-specific behavior: `DesktopUtil.GlueToDesktop` (keeps fence
  windows pinned above the desktop/below other apps), `BlurUtil` (acrylic/blur backdrop), `DropShadow`,
  `WindowUtil` (dark-mode context menu, DPI-related helpers, hit-test constants), `ShellContextMenu` (shows the
  native Windows Explorer right-click menu for a file, via the `Peter` namespace class in `ShellContextMenu.cs`).
- Fences are drag-and-drop targets (`AllowDrop`); dropped file paths are appended to `FenceInfo.Files` and saved.

## Conventions

- All persisted state flows through `FenceManager`; don't write fence XML files directly from other classes.
- Any UI change to fence position/size/appearance that should survive restarts must update the corresponding
  `FenceInfo` field and call `Save()`.
- Localization: some Forms have `.zh-CN.resx` resource variants alongside the default `.resx` — keep both in
  sync when changing form layout/strings that are translated.
- DPI-scaled values (e.g. `TitleHeight`) are stored as *logical* units in `FenceInfo` and converted with
  `LogicalToDeviceUnits` before use in `FenceWindow`.
