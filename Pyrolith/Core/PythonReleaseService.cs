using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Pyrolith.Core;

/// <summary>
/// Talks to the GitHub API of astral-sh/python-build-standalone.
/// Only the latest release is used; it is fetched once and cached for the session.
/// </summary>
public static class PythonReleaseService
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/astral-sh/python-build-standalone/releases/latest";

    /// <summary>Oldest Python version offered in the UI.</summary>
    private static readonly Version MinimumVersion = new(3, 11);

    public static IReadOnlyList<string> Architectures { get; } = ["amd64", "arm64"];

    private static readonly HttpClient Http = CreateClient();

    // Asset names look like: cpython-3.13.1+20250115-x86_64-pc-windows-msvc-install_only_stripped.tar.gz
    private static readonly Regex AssetVersion = new(@"^cpython-(\d+\.\d+\.\d+)\+", RegexOptions.Compiled);

    private static Task<GhRelease>? _latestRelease;

    private sealed record GhRelease(
        [property: JsonPropertyName("tag_name")] string? Tag,
        [property: JsonPropertyName("assets")] List<GhAsset> Assets);

    private sealed record GhAsset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("browser_download_url")] string Url);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Pyrolith/1.0"); // GitHub API rejects requests without a User-Agent
        return client;
    }

    /// <summary>All CPython versions (>= 3.11) in the latest release, newest first.</summary>
    public static async Task<List<string>> GetPythonVersionsAsync()
    {
        var release = await GetLatestReleaseAsync();

        return release.Assets
            .Select(a => AssetVersion.Match(a.Name))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .Select(v => (Raw: v, Parsed: Version.TryParse(v, out var parsed) ? parsed : null))
            .Where(x => x.Parsed is not null && x.Parsed >= MinimumVersion)
            .OrderByDescending(x => x.Parsed)
            .Select(x => x.Raw)
            .ToList();
    }

    /// <summary>Download URL of the Windows "install_only" archive for the given version/architecture.</summary>
    public static async Task<string> GetDownloadUrlAsync(
        string version, string architecture, bool stripped = false, bool freeThreaded = false)
    {
        version = version.Trim();
        string triple = TargetTriple(architecture);
        string suffix = stripped ? "install_only_stripped.tar.gz" : "install_only.tar.gz";

        var release = await GetLatestReleaseAsync();

        var asset = release.Assets.FirstOrDefault(a =>
            a.Name.StartsWith($"cpython-{version}+", StringComparison.Ordinal) &&
            a.Name.Contains(triple, StringComparison.Ordinal) &&
            a.Name.EndsWith(suffix, StringComparison.Ordinal) &&
            a.Name.Contains("freethreaded", StringComparison.Ordinal) == freeThreaded);

        return asset?.Url ?? throw new InvalidOperationException(
            $"Python {version} ({architecture}, {(freeThreaded ? "free-threaded" : "GIL")}) is not available " +
            $"in the latest python-build-standalone release ({release.Tag ?? "unknown"}).\n" +
            "Open the version list again and pick one of the listed versions.");
    }

    private static string TargetTriple(string architecture) => architecture.Trim() switch
    {
        "amd64" => "x86_64-pc-windows-msvc",
        "arm64" => "aarch64-pc-windows-msvc",
        _ => throw new NotSupportedException(
            $"python-build-standalone does not provide '{architecture}' for Windows (only amd64 and arm64).")
    };

    private static Task<GhRelease> GetLatestReleaseAsync()
    {
        // Cache the successful request; a failed one is retried next time.
        var task = _latestRelease;
        if (task is null || task.IsFaulted || task.IsCanceled)
            _latestRelease = task = FetchLatestReleaseAsync();
        return task;
    }

    private static async Task<GhRelease> FetchLatestReleaseAsync()
    {
        try
        {
            return await Http.GetFromJsonAsync<GhRelease>(LatestReleaseUrl)
                ?? throw new InvalidOperationException("GitHub returned an empty release description.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            throw new InvalidOperationException(
                "GitHub API rate limit reached (60 requests per hour without sign-in). Try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Cannot reach GitHub to get the list of Python builds. Check your internet connection.\n({ex.Message})", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("GitHub did not respond in time. Try again.", ex);
        }
    }
}
