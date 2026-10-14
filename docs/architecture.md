# Architecture

A short tour for people who want to read or change the code.

- [Solution layout](#solution-layout)
- [Build pipeline](#build-pipeline)
- [Executable format](#executable-format)
- [Loader at runtime](#loader-at-runtime)
- [Project file (.pyrolith)](#project-file-pyrolith)
- [Error handling](#error-handling)
- [Design notes](#design-notes)

## Solution layout

```
Pyrolith.slnx
├── Pyrolith/                       WinForms GUI (.NET 10)
│   ├── Program.cs              entry point, global exception handler
│   ├── MainForm.cs             UI logic: collects input → BuildOptions
│   ├── MainForm.Designer.cs    generated layout
│   ├── AboutForm.cs            Info → About / Third-Party Libraries
│   ├── AppInfo.cs              version, description, third-party list
│   ├── Core/                   UI-independent logic
│   │   ├── BuildOptions.cs     build inputs + Validate() (errors / warnings)
│   │   ├── PackageBuilder.cs   the build pipeline
│   │   ├── PythonReleaseService.cs   python-build-standalone release lookup
│   │   ├── FileDownloader.cs   streaming download with progress
│   │   ├── ExecutableMetadataWriter.cs  icon + version info (ResourceLib)
│   │   ├── ProjectFile.cs      *.pyrolith load/save
│   │   └── PathUtil.cs         safe "is inside folder" checks
│   └── Resources/PyrolithLoader.exe   AOT-compiled loader, embedded as byte[]
└── PyrolithLoader/                 Native AOT stub (no WinForms, no runtime needed)
    └── Program.cs              payload lookup, extraction, password prompt, process start
```

`Core` does not reference WinForms — everything it needs comes in through `BuildOptions` and two `IProgress<T>` callbacks.

## Build pipeline

`PackageBuilder.BuildAsync(BuildOptions, IProgress<string>, IProgress<int>)`:

1. **Validate** — `BuildOptions.Validate()` again (the UI already did, the builder must not trust it).
2. **Download** — `PythonReleaseService` reads `releases/latest` of *astral-sh/python-build-standalone* (cached per session) and picks
   `cpython-<ver>+<date>-<triple>-install_only_stripped.tar.gz` (`x86_64-pc-windows-msvc` or `aarch64-pc-windows-msvc`, non-free-threaded).
3. **Extract** — `.tar.gz` → `%TEMP%\PyrolithBuild_<guid>\py\python\…`, copied to the staging folder `app\`.
4. **Copy the app** — user folder → `app\main\` (skips `.git`, `__pycache__`, venvs, links, the build folder and the output file).
5. **run.pyex** — one line, e.g. `python.exe "main\src\app.py" --flag`.
6. **pip** (optional) — `python -m ensurepip`, then `python -m pip install -r <requirements>` with the bundled interpreter, isolated from the host Python (`PYTHONHOME`/`PYTHONPATH` removed, `PYTHONNOUSERSITE=1`, `PIP_USER=0`…).
7. **ZIP** — DotNetZip, UTF-8 names, optional WinZip AES-256, no Zip64.
8. **Executable** — write the embedded loader → set version info and icon (ResourceLib) → append the ZIP.

Step 8 order matters: `UpdateResource` rewrites the PE file and would drop anything appended after it.

The temporary build folder is removed in a `finally` block, also when the build fails.

## Executable format

```
┌──────────────────────────────┐  offset 0
│ PyrolithLoader (PE image)        │  icon + VS_VERSIONINFO patched per app
├──────────────────────────────┤  zipStart
│ local file headers + data    │
│ …                            │  ZIP payload (offsets relative to zipStart)
│ central directory            │
│ end of central directory     │  "PK\x05\x06"
└──────────────────────────────┘  end of file
```

Payload content:

| Path | Content |
|---|---|
| `python.exe`, `pythonw.exe`, `DLLs\`, `Lib\`, `tcl\`… | standalone CPython (+ packages installed by pip in `Lib\site-packages`) |
| `main\…` | the user's application folder |
| `run.pyex` | `<interpreter> <arguments>` — interpreter is `python.exe` or `pythonw.exe` |

## Loader at runtime

`PyrolithLoader/Program.cs` — a single file with the logic and the Win32 interop:

1. **Find the payload** — `AppendedZip` reads the last ≤ 64 KB of its own file, finds the EOCD record and computes
   `zipStart = eocdPosition − centralDirSize − centralDirOffset`. `OffsetStream` exposes the file from `zipStart` on, so SharpCompress sees a normal ZIP.
2. **Password** — if any entry is encrypted, a raw Win32 dialog (`PasswordPrompt`) asks for it. A wrong password is only detected while decrypting, so extraction is retried (3 attempts). SharpCompress reports a wrong AES password as `InvalidFormatException("bad password")`.
3. **Extract** to `%TEMP%\PyrolithRun_<guid>`.
4. **Run** — `run.pyex` is split at the first space. Arguments passed to the `.exe` are quoted (CommandLineToArgvW rules) and appended. The child gets:
   - working directory = extraction root,
   - `PYTHONHOME` / `PYTHONPATH` removed, `PYTHONNOUSERSITE=1`,
   - `PYROLITH_EXECUTABLE` = path of the `.exe`,
   - a console window only for `python.exe` (the loader itself is a GUI-subsystem program).
5. **Exit** — waits for Python, deletes the temp folder (with retries — child processes may still hold files) and returns Python's exit code.

The loader is published with Native AOT, `InvariantGlobalization` and `OptimizationPreference=Size`, so it has no .NET runtime dependency.

## Project file (.pyrolith)

Plain JSON written by `ProjectFile`. Property names are the file format — keep them stable.

```json
{
  "Version": 1,
  "PythonVersion": "3.13.1",
  "Architecture": "amd64",
  "TargetAppDir": "C:\\Dev\\MyApp",
  "TargetApp": "C:\\Dev\\MyApp\\main.py",
  "ScriptArguments": "--verbose",
  "IsGuiApp": false,
  "Requirements": "C:\\Dev\\MyApp\\requirements.txt",
  "UsePassword": false,
  "Password": "",
  "AppAuthor": "Jane Doe",
  "AppName": "My App",
  "AppDescription": "Does useful things",
  "IconPath": "C:\\Dev\\MyApp\\app.ico",
  "OutputExePath": "C:\\Dev\\MyApp.exe"
}
```

Files with a higher `Version` than the app supports are rejected with a clear message.

## Error handling

| Layer | Strategy |
|---|---|
| Input | `BuildOptions.Validate()` returns *errors* (block) and *warnings* (confirm). The UI also prevents most mistakes at selection time. |
| Build | Expected failures are thrown as `BuildException` with a user-readable message (network, pip, locked output, archive limits). |
| Network | `PythonReleaseService` turns HTTP failures into readable messages (offline, rate limit, timeout); a failed release lookup is not cached. |
| UI | Every event handler that can fail catches and shows a message box; `Program` installs a last-resort handler for anything else. |
| Loader | One top-level `try/catch` shows a native error box; the temp folder is always cleaned up in `finally`. |

## Design notes

- **Why a separate loader?** The output must run without .NET installed and stay small. A Native AOT console-less stub with raw Win32 UI is ~3 MB; WinForms would not trim well.
- **Why append a ZIP instead of a PE resource?** Resources are limited in size and are loaded into memory; an overlay can be streamed and is trivial to locate.
- **Why python-build-standalone?** It is a complete, relocatable CPython (including Tcl/Tk), unlike the embeddable distribution.
- **Why the `main\` subfolder?** It keeps the user's files apart from the interpreter's, so a user file can never overwrite `python.exe` or `Lib\`.
