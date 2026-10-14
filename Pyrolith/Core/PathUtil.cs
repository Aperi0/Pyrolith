namespace Pyrolith.Core;

/// <summary>
/// Path helpers that are safe against the classic string-prefix mistakes
/// (e.g. "C:\App2\main.py" is NOT inside "C:\App").
/// </summary>
public static class PathUtil
{
    /// <summary>Full path without a trailing separator.</summary>
    public static string Normalize(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    /// <summary>True when <paramref name="path"/> is located somewhere below <paramref name="directory"/>.</summary>
    public static bool IsInside(string path, string directory)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(directory))
            return false;

        string relative = Path.GetRelativePath(Normalize(directory), Normalize(path));
        return relative != "."
            && relative != ".."
            && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relative); // different drive
    }

    /// <summary>
    /// Path of <paramref name="path"/> relative to <paramref name="directory"/>,
    /// or null when it is not inside that directory.
    /// </summary>
    public static string? TryGetRelative(string path, string directory) =>
        IsInside(path, directory)
            ? Path.GetRelativePath(Normalize(directory), Normalize(path))
            : null;

    /// <summary>True for "C:\", "D:\", "\\server\share" etc.</summary>
    public static bool IsRoot(string directory)
    {
        string full = Path.GetFullPath(directory);
        return string.Equals(
            Path.TrimEndingDirectorySeparator(full),
            Path.TrimEndingDirectorySeparator(Path.GetPathRoot(full) ?? ""),
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SamePath(string a, string b) =>
        string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
}
