using System.Text.RegularExpressions;

namespace Pyrolith.Core;

/// <summary>Version-info strings and icon written into the generated .exe.</summary>
public sealed record AppMetadata(
    string? ProductName = null,
    string? Description = null,
    string? Author = null,
    string? IconPath = null);

/// <summary>Everything <see cref="PackageBuilder"/> needs to produce one executable.</summary>
public sealed record BuildOptions
{
    public string PythonVersion { get; init; } = "";
    public string Architecture { get; init; } = "";

    /// <summary>Folder that is copied into the package as <c>main\</c>.</summary>
    public string AppDirectory { get; init; } = "";

    /// <summary>Absolute path of the script started by the loader. Must be inside <see cref="AppDirectory"/>.</summary>
    public string EntryScript { get; init; } = "";

    public string ScriptArguments { get; init; } = "";
    public string? RequirementsFile { get; init; }
    public bool IsGuiApp { get; init; }
    public bool UsePassword { get; init; }
    public string? Password { get; init; }
    public string OutputPath { get; init; } = "";
    public AppMetadata Metadata { get; init; } = new();

    public static readonly string[] ScriptExtensions = [".py", ".pyw", ".pyz"];

    private static readonly Regex VersionPattern = new(@"^\d+\.\d+\.\d+$");

    /// <summary>True for "3.13.1"-style strings (placeholders like "Loading..." are rejected).</summary>
    public static bool IsPythonVersion(string? value) =>
        value != null && VersionPattern.IsMatch(value.Trim());

    /// <summary>Entry script relative to the app folder, e.g. "src\main.py". Null when it is outside.</summary>
    public string? EntryScriptRelativePath => PathUtil.TryGetRelative(EntryScript, AppDirectory);

    /// <summary>
    /// Checks the options for mistakes a user can easily make in the UI.
    /// Errors block the build, warnings only need a confirmation.
    /// </summary>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        // Python runtime
        if (!IsPythonVersion(PythonVersion))
            result.Error("Select a Python version (open the list to load the available versions).");

        if (string.IsNullOrWhiteSpace(Architecture))
            result.Error("Select the Python architecture.");
        else if (!PythonReleaseService.Architectures.Contains(Architecture))
            result.Error($"Unsupported architecture '{Architecture}'.");

        // Application folder
        bool appDirOk = false;
        if (string.IsNullOrWhiteSpace(AppDirectory))
            result.Error("Select the application folder.");
        else if (!Directory.Exists(AppDirectory))
            result.Error($"The application folder does not exist:\n{AppDirectory}");
        else if (PathUtil.IsRoot(AppDirectory))
            result.Error("A whole drive cannot be used as the application folder. Select your project folder.");
        else
            appDirOk = true;

        // Entry script
        if (string.IsNullOrWhiteSpace(EntryScript))
        {
            result.Error("Select the entry script (e.g. main.py).");
        }
        else if (!File.Exists(EntryScript))
        {
            result.Error($"The entry script does not exist:\n{EntryScript}");
        }
        else
        {
            if (appDirOk && EntryScriptRelativePath is null)
                result.Error(
                    "The entry script must be inside the application folder.\n" +
                    $"Script: {EntryScript}\nFolder: {AppDirectory}\n" +
                    "Only the application folder is packed into the executable.");

            string ext = Path.GetExtension(EntryScript);
            if (!ScriptExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                result.Error($"The entry script must be a {string.Join(", ", ScriptExtensions)} file (got '{ext}').");
            else if (ext.Equals(".pyw", StringComparison.OrdinalIgnoreCase) && !IsGuiApp)
                result.Warning("The entry script is a .pyw file but \"GUI app\" is not checked - a console window will be shown.");
        }

        // Optional inputs
        if (!string.IsNullOrWhiteSpace(RequirementsFile) && !File.Exists(RequirementsFile))
            result.Error($"The requirements file does not exist:\n{RequirementsFile}");

        if (!string.IsNullOrWhiteSpace(Metadata.IconPath) && !File.Exists(Metadata.IconPath))
            result.Error($"The icon file does not exist:\n{Metadata.IconPath}");

        if (UsePassword && string.IsNullOrWhiteSpace(Password))
            result.Error("Password protection is enabled but no password was entered.");

        if (ScriptArguments.Count(c => c == '"') % 2 != 0)
            result.Warning("The script arguments contain an unbalanced quote (\").");

        // Output
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            result.Error("Select where the output .exe should be saved.");
        }
        else
        {
            string? outDir = Path.GetDirectoryName(Path.GetFullPath(OutputPath));
            if (!Path.GetExtension(OutputPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                result.Error("The output file must have the .exe extension.");
            else if (outDir is null || !Directory.Exists(outDir))
                result.Error($"The output folder does not exist:\n{outDir}");
        }

        return result;
    }
}

/// <summary>Result of <see cref="BuildOptions.Validate"/>.</summary>
public sealed class ValidationResult
{
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public bool HasErrors => Errors.Count > 0;

    internal void Error(string message) => Errors.Add(message);
    internal void Warning(string message) => Warnings.Add(message);
}
