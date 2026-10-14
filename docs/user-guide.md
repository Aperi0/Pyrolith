# User guide

This guide walks through every part of the Pyrolith window and explains the checks Pyrolith performs before a build.

- [The window](#the-window)
- [Python Runtime](#python-runtime)
- [Python Application](#python-application)
- [Options](#options)
- [Executable Details](#executable-details)
- [Build](#build)
- [Project files](#project-files)
- [Info menu](#info-menu)
- [Troubleshooting](#troubleshooting)

## The window

```
┌ Project  Info ────────────────────────────────────────────────────────┐
│ ┌ Python Runtime ─────────┐  ┌ Python Application ──────────────────┐ │
│ │ Version       [3.13.1 ▾]│  │ Folder         [C:\Dev\MyApp ] [...] │ │
│ │ Architecture  [amd64  ▾]│  │ Entry script   [main.py      ] [...] │ │
│ └─────────────────────────┘  │ Requirements   [(optional)   ][…][×] │ │
│ ┌ Options ────────────────┐  │ Arguments      [(optional)         ] │ │
│ │ ☐ GUI app (no console)  │  │ python.exe "main\main.py"            │ │
│ │ ☐ Protect with password │  └──────────────────────────────────────┘ │
│ └─────────────────────────┘                                           │
│ ┌ Executable Details ──────────────────────────┐ ┌──────────────────┐ │
│ │ Product name / Description / Author / Icon   │ │   icon preview   │ │
│ └──────────────────────────────────────────────┘ └──────────────────┘ │
│ ┌ Build ────────────────────────────────────────────────────────────┐ │
│ │ Output .exe [C:\Dev\MyApp.exe                             ] [...] │ │
│ │ [ Build EXE ]   [██████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░]    │ │
│ │ Downloading Python...                                             │ │
│ └───────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

Path fields are read-only — use the **…** buttons. Hover a path to see it in full; optional paths can be removed with **×**.

## Python Runtime

| Field | Notes |
|---|---|
| **Version** | The list is downloaded from GitHub the first time you open it (latest python-build-standalone release, versions ≥ 3.11). If GitHub is unreachable or rate-limited (60 anonymous requests/hour), an error is shown — open the list again to retry. |
| **Architecture** | `amd64` for regular PCs, `arm64` for Windows on ARM. |

When a project is opened, its Python version is shown immediately. The next time you open the list, Pyrolith reloads the versions and keeps your selection; if that version is no longer published, you are told which one was selected instead.

## Python Application

| Field | Notes |
|---|---|
| **Folder** | Your project root. Its whole content is packed into the executable (as `main\`). A drive root such as `C:\` is refused. |
| **Entry script** | The `.py`, `.pyw` or `.pyz` file to start. It **must be inside the folder** above. |
| **Requirements** | Optional `requirements.txt`. Installed with `pip` into the bundled Python during the build. |
| **Arguments** | Optional default arguments for your script. Arguments passed to the `.exe` are added after them. |

The read-only line underneath shows the exact command the executable will run.

**What Pyrolith does for you**

- Selecting an entry script **before** a folder uses the script's folder as the application folder.
- Selecting a script **outside** the chosen folder asks whether to switch the folder to the script's folder — otherwise the script would simply not be in the package.
- Changing the folder clears an entry script that is no longer inside it.
- Picking a `.pyw` script ticks *GUI app* automatically.

**Skipped while copying:** `.git`, `__pycache__`, virtual environments (any folder containing `pyvenv.cfg`, e.g. `.venv`), symbolic links/junctions and the output `.exe` itself. Everything else is copied as is.

**Requirements tips**

- `pip` runs from the folder of `requirements.txt`, so relative entries (`-r base.txt`, `./wheels/pkg.whl`) work as usual.
- A Python installation on the build machine does not interfere: `PYTHONHOME`, `PYTHONPATH`, user site-packages and `PIP_USER` / `PIP_REQUIRE_VIRTUALENV` settings are ignored for the build.
- If `pip` fails, the last lines of its output are shown in the error message.

## Options

| Option | Effect |
|---|---|
| **GUI app (no console window)** | Runs `pythonw.exe` instead of `python.exe`. Use it for tkinter / Qt / wx programs. Console apps get their own console window. |
| **Protect with password** | Encrypts the payload with AES-256. The executable asks for the password on every start (three attempts). |

> The password protects the file at rest. While the app is running, its files are unencrypted in `%TEMP%` — it is not a copy-protection scheme.

## Executable Details

Written into the Windows version resource of the `.exe` (Explorer → *Properties* → *Details*):

| Field | Version-info keys | Default |
|---|---|---|
| Product name | `ProductName`, `InternalName` | Python Application |
| Description | `FileDescription` | Python Script |
| Author | `CompanyName`, `LegalCopyright` (`© <year> <author>`) | Unknown |
| Icon | application icon | PyrolithLoader icon |

The icon must be a real `.ico` file — it is validated when selected. Multi-size icons (16–256 px) look best.

## Build

1. Choose **Output .exe** (by default suggested next to the project folder, named after the product or script).
2. Press **Build EXE**. All inputs are locked until the build finishes.

Before anything is downloaded Pyrolith validates the inputs. **Errors** stop the build; **warnings** ask for confirmation.

| Step | Status text |
|---|---|
| Download the Python archive | *Downloading Python...* (with progress) |
| Extract it | *Extracting Python...* |
| Copy your folder to `main\` and write `run.pyex` | *Copying application files...* |
| `ensurepip` + `pip install -r` (if requirements are set) | *Preparing pip... / Installing requirements...* |
| Create the ZIP (optionally encrypted) | *Compressing...* |
| Write loader, icon and details, append ZIP | *Writing executable... / Applying icon and details... / Appending payload...* |

Temporary files live in `%TEMP%\PyrolithBuild_*` and are removed after the build — also when it fails.

## Project files

**Project** menu:

| Command | Shortcut | |
|---|---|---|
| New | `Ctrl+N` | Clears all fields. |
| Open… | `Ctrl+O` | Loads a `*.pyrolith` file. Paths that no longer exist are listed in a warning. |
| Save | `Ctrl+S` | Saves to the current project file (asks for a name the first time). |
| Save As… | `Ctrl+Shift+S` | Saves under a new name. |

A `.pyrolith` file is plain JSON (see [architecture](architecture.md#project-file-pyrolith)). **It contains the password in plain text** if one is set — do not share it.

## Info menu

- **About Pyrolith…** — version and short description.
- **Third-Party Libraries…** — the open-source projects Pyrolith uses, with licenses and links.

## Troubleshooting

| Message | Cause / fix |
|---|---|
| *Select a Python version (open the list to load the available versions).* | The version list was never opened or failed to load. Open it and pick a version. |
| *GitHub API rate limit reached…* | More than 60 anonymous GitHub API calls in an hour from your IP. Wait and retry. |
| *The entry script must be inside the application folder.* | Pick the script from within the folder, or choose a higher folder. |
| *The entry script must be a .py, .pyw, .pyz file* | Another file type (e.g. `python.exe`, `.txt`) was selected. |
| *The output folder does not exist* | The output folder was moved or deleted (common after opening an old project). |
| *Cannot write '…exe'. If the previous build is still running…* | The output file is locked — close the running app. |
| *'python -m pip install …' failed* | A package could not be installed for the chosen Python version/architecture. Check the pip output in the message. |
| *Cannot create the application archive…* | Package larger than 4 GB or more than 65,535 files. |
| Packed app: *No application payload found* | The `.exe` was modified or truncated after the build (e.g. by a tool that rewrites executables). Rebuild. |
| Packed app: *Invalid password* | Wrong password; after three attempts the app exits. |
