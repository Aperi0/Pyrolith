namespace Pyrolith;

/// <summary>A library or project Pyrolith is built on (shown in Info → Third-Party Libraries).</summary>
internal sealed record ThirdPartyComponent(string Name, string Version, string License, string Url, string UsedFor);

/// <summary>Static product information shown in the UI.</summary>
internal static class AppInfo
{
    public const string Name = "Pyrolith";

    public const string Tagline = "Python to standalone EXE packer";

    public const string Author = "Aperi0";

    public const string Description =
        "Pyrolith packs a Python project into a single Windows executable. Instead of an embeddable " +
        "or NuGet Python, every executable carries a complete standalone CPython build - " +
        "pip, tkinter, venv and the whole standard library behave exactly like a regular installation.";

    /// <summary>Taken from &lt;Version&gt; in Pyrolith.csproj.</summary>
    public static string Version { get; } =
        typeof(AppInfo).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

    // Keep the versions in sync with Pyrolith.csproj / PyrolithLoader.csproj.
    public static IReadOnlyList<ThirdPartyComponent> ThirdParty { get; } =
    [
        new("Vestris.ResourceLib", "2.2.0", "MIT",
            "https://github.com/resourcelib/resourcelib",
            "Writes the version information and icon of the generated .exe."),

        new("DotNetZip", "1.16.0", "Ms-PL",
            "https://github.com/haf/DotNetZip.Semverd",
            "Creates the (optionally AES-256 encrypted) ZIP payload."),

        new("SharpCompress", "0.50.5", "MIT",
            "https://github.com/adamhathcock/sharpcompress",
            "Extracts the payload inside PyrolithLoader at runtime."),

        new("python-build-standalone", "latest release", "MPL-2.0",
            "https://github.com/astral-sh/python-build-standalone",
            "Portable CPython builds downloaded during the build (CPython itself: PSF License)."),
    ];
}
