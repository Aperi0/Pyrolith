# Building from source

## Requirements

- Windows 10/11 (x64 or ARM64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- For the loader (Native AOT): Visual Studio or Build Tools with the **Desktop development with C++** workload
- An IDE with .NET 10 support (Visual Studio, Rider, VS Code + C# Dev Kit) — optional

## One command

```powershell
./build.ps1
```

| Step | Output |
|---|---|
| Publish `PyrolithLoader` with Native AOT | `artifacts\loader\PyrolithLoader.exe` |
| Copy it into the GUI project | `Pyrolith\Resources\PyrolithLoader.exe` |
| Publish `Pyrolith` as a single framework-dependent file | `artifacts\Pyrolith\Pyrolith.exe` |

Parameters:

| Parameter | Default | |
|---|---|---|
| `-Configuration` | `Release` | |
| `-Runtime` | `win-x64` | `win-arm64` for a native ARM64 build |
| `-SkipLoader` | off | reuse the `PyrolithLoader.exe` already in `Pyrolith\Resources` (no C++ tools needed) |

## Working in the IDE

Open `Pyrolith.slnx` and run the **Pyrolith** project. The GUI embeds `Pyrolith\Resources\PyrolithLoader.exe` as it is — **after changing anything in `PyrolithLoader`, run `./build.ps1` (or the publish command below) so the GUI picks up the new loader.**

```powershell
dotnet publish PyrolithLoader -c Release -r win-x64 -o artifacts\loader
copy artifacts\loader\PyrolithLoader.exe Pyrolith\Resources\PyrolithLoader.exe
```

The loader also builds and runs as a normal (JIT) program for debugging: start it next to a test executable or attach the debugger to a built app.

## Continuous integration

`.github/workflows/build.yml` runs `build.ps1` on `windows-latest` for every push and pull request and uploads `Pyrolith.exe` and `PyrolithLoader.exe` as artifacts. Pushing a tag such as `v1.0.0` also publishes a GitHub release with `Pyrolith.exe` attached.

## Dependencies

| Package | Project | Version |
|---|---|---|
| Vestris.ResourceLib | Pyrolith | 2.2.0 |
| DotNetZip | Pyrolith | 1.16.0 |
| SharpCompress | PyrolithLoader | 0.50.5 |

When updating a version, also update `AppInfo.ThirdParty` (shown in **Info → Third-Party Libraries**) and [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).

> **Note on DotNetZip** — the package is no longer maintained and NuGet flags advisory GHSA-xhg6-9j5j-w4vf (path traversal when *extracting* archives). Pyrolith only *creates* archives from local files, so it is not affected, but replacing DotNetZip with a maintained library that can write WinZip-AES archives is on the to-do list.
