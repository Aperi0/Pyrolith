using Vestris.ResourceLib;

namespace Pyrolith.Core;

/// <summary>
/// Writes Win32 resources of the generated executable: the VS_VERSIONINFO strings
/// (what Explorer shows in Properties → Details) and the application icon.
/// Must run BEFORE the ZIP payload is appended - UpdateResource rewrites the PE file
/// and would drop any data appended after it.
/// </summary>
public static class ExecutableMetadataWriter
{
    public static void Apply(string exePath, AppMetadata metadata)
    {
        string product = Fallback(metadata.ProductName, "Python Application");
        string description = Fallback(metadata.Description, "Python Script");
        string company = Fallback(metadata.Author, "Unknown");

        WriteVersionStrings(exePath, product, description, company);

        if (!string.IsNullOrWhiteSpace(metadata.IconPath))
            WriteIcon(exePath, metadata.IconPath);
    }

    private static void WriteVersionStrings(string exePath, string product, string description, string company)
    {
        var versions = new List<VersionResource>();
        using (var info = new ResourceInfo())
        {
            info.Load(exePath);
            foreach (Resource resource in info)
                if (resource is VersionResource version)
                    versions.Add(version);
        } // ResourceInfo must be closed before the file can be written

        if (versions.Count == 0)
            throw new InvalidOperationException(
                "The loader executable has no version resource (VS_VERSIONINFO), so its details cannot be set. " +
                "Rebuild PyrolithLoader.exe.");

        try
        {
            foreach (VersionResource version in versions)
            {
                var stringFileInfo = (StringFileInfo)version["StringFileInfo"];

                foreach (StringTable table in stringFileInfo.Strings.Values)
                {
                    // ResourceLib expects null-terminated values
                    table["ProductName"] = product + "\0";
                    table["FileDescription"] = description + "\0";
                    table["CompanyName"] = company + "\0";
                    table["LegalCopyright"] = $"© {DateTime.Now.Year} {company}\0";
                    table["OriginalFilename"] = Path.GetFileName(exePath) + "\0";
                    table["InternalName"] = product + "\0";
                }

                version.SaveTo(exePath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot write the version information: {ex.Message}", ex);
        }
    }

    private static void WriteIcon(string exePath, string iconPath)
    {
        try
        {
            var icon = new IconDirectoryResource(new IconFile(iconPath)) { Language = 0 };
            icon.SaveTo(exePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot set the icon - make sure '{Path.GetFileName(iconPath)}' is a valid .ico file.\n({ex.Message})", ex);
        }
    }

    private static string Fallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
