# Changelog

## 1.0.0 — 2026-10-14

First public version. Cleanup of the original prototype; the build pipeline and the executable format are unchanged.

> Items marked *Packed apps* live in PyrolithLoader. They take effect once the loader is rebuilt with `build.ps1` (CI does this automatically).

### Added
- Renamed from **PyEx** to **Pyrolith** (projects, namespaces, files). Old `*.pyexp` project files can still be opened.
- About page shows version and author (Aperi0); application icons in `assets/`.
- **Info** menu: *About Pyrolith* and *Third-Party Libraries* (Vestris.ResourceLib, DotNetZip, SharpCompress, python-build-standalone).
- Input validation before every build — errors block, warnings ask for confirmation (see the [user guide](docs/user-guide.md#troubleshooting)).
- Project menu: *New*, *Save* (to the current file) and *Save As…* with keyboard shortcuts; the window title shows the project name.
- Clear (×) buttons for the optional requirements file and icon.
- Tooltips with the full path on all path fields.
- Packed apps: arguments given to the `.exe` are passed to the script; Python's exit code is returned.
- Packed apps: `PYROLITH_EXECUTABLE` environment variable with the path of the `.exe`.
- Packed apps: three password attempts instead of one.
- `build.ps1`, GitHub Actions workflow and documentation.

### Changed
- Unified naming: `PyEx_Main` → `MainForm`, `PythonPackerBuilder` → `PackageBuilder`, `PythonVersionService` → `PythonReleaseService`, `ExePatcher` → `ExecutableMetadataWriter`; UI-independent code moved to `Pyrolith.Core`; controls renamed consistently.
- The 15-parameter build method now takes a single `BuildOptions` record.
- Clearer UI labels (*Python Runtime*, *Entry script*, *Executable Details*, *Build EXE*…); fixed window size; logical tab order.
- `pip` runs from the folder of `requirements.txt` (relative entries work) and is isolated from the host's Python/pip configuration.
- Copying the app folder skips `.git`, `__pycache__`, virtual environments and symbolic links.
- SharpCompress is referenced from NuGet instead of a checked-in DLL.
- All texts and comments in English.

### Fixed
- An entry script outside the selected folder produced an executable that could not start (its absolute build-machine path was written to `run.pyex`). The prefix check also accepted wrong folders such as `C:\App2` for `C:\App`.
- Building without loading the Python version list failed with an obscure "Missing Build" error.
- Cancelling the icon dialog crashed the application; invalid `.ico` files crashed it as well.
- Empty or missing output path, missing folder/script/requirements (e.g. after opening a project on another PC) were not detected; a missing requirements file was silently ignored.
- Opening a project did not restore the Python version.
- The temporary build folder was left behind when a build failed.
- The window froze while Python was extracted, files were copied and the archive was compressed (this work now runs in the background).
- Non-ASCII file names (e.g. `zażółć.py`) were stored in CP437 and broke inside the package.
- A wrong password for an AES-encrypted package showed "Critical Error: bad password" instead of the password message.
- Console apps (GUI app unchecked) ran without a visible console window.
- `PYTHONHOME` / `PYTHONPATH` on the target machine could break the packed interpreter.
- Choosing the user profile or `%TEMP%` as the app folder could copy the build folder into itself.
- GitHub API errors (offline, rate limit) produced raw HTTP messages; the release list is now fetched once per session.
