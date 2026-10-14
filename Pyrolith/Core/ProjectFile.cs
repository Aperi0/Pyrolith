using System.Text.Json;

namespace Pyrolith.Core;

/// <summary>
/// The *.pyrolith project file - a plain JSON snapshot of the form.
/// Property names are part of the file format; do not rename them.
/// Note: the password is stored in plain text.
/// </summary>
public sealed class ProjectFile
{
    public const string Extension = "pyrolith";
    public const string SaveFilter = "Pyrolith Project (*.pyrolith)|*.pyrolith";

    /// <summary>Also accepts *.pyexp files from the time the project was called PyEx (same JSON format).</summary>
    public const string OpenFilter = "Pyrolith Project (*.pyrolith;*.pyexp)|*.pyrolith;*.pyexp";
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public string? PythonVersion { get; set; }
    public string? Architecture { get; set; }
    public string? TargetAppDir { get; set; }
    public string? TargetApp { get; set; }
    public string? ScriptArguments { get; set; }
    public bool IsGuiApp { get; set; }
    public string? Requirements { get; set; }
    public bool UsePassword { get; set; }
    public string? Password { get; set; }
    public string? AppAuthor { get; set; }
    public string? AppName { get; set; }
    public string? AppDescription { get; set; }
    public string? IconPath { get; set; }
    public string? OutputExePath { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public void Save(string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));

    public static ProjectFile Load(string path)
    {
        ProjectFile? project;
        try
        {
            project = JsonSerializer.Deserialize<ProjectFile>(File.ReadAllText(path));
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is not a valid Pyrolith project file.\n({ex.Message})", ex);
        }

        if (project is null)
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is empty.");
        if (project.Version > CurrentVersion)
            throw new InvalidDataException(
                $"This project was saved by a newer Pyrolith (format v{project.Version}). Update Pyrolith to open it.");

        return project;
    }

    /// <summary>Paths stored in the project that no longer exist (e.g. project moved to another PC).</summary>
    public IEnumerable<string> MissingPaths()
    {
        if (!string.IsNullOrWhiteSpace(TargetAppDir) && !Directory.Exists(TargetAppDir)) yield return TargetAppDir;
        foreach (var file in new[] { TargetApp, Requirements, IconPath })
            if (!string.IsNullOrWhiteSpace(file) && !File.Exists(file)) yield return file;
    }
}
