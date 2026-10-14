using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using Ionic.Zip;

namespace Pyrolith.Core;

/// <summary>
/// Produces the final executable:
/// <code>
///   PyrolithLoader.exe (with patched resources)  +  ZIP payload appended at the end
/// </code>
/// ZIP layout:
/// <code>
///   python.exe, pythonw.exe, Lib\, DLLs\ ...   standalone CPython (python-build-standalone)
///   main\...                                   copy of the user's application folder
///   run.pyex                                   one-line command executed by the loader
/// </code>
/// </summary>
public static class PackageBuilder
{
    /// <summary>File inside the payload that tells the loader what to start.</summary>
    public const string RunFileName = "run.pyex";

    /// <summary>Folder inside the payload that holds the user's application.</summary>
    public const string AppFolderName = "main";

    public static async Task<string> BuildAsync(BuildOptions options, IProgress<string> status, IProgress<int> progress)
    {
        var validation = options.Validate();
        if (validation.HasErrors)
            throw new BuildException(string.Join("\n\n", validation.Errors));

        string buildDir = CreateTempDirectory("PyrolithBuild_");
        try
        {
            string stageDir = Path.Combine(buildDir, "app");  // becomes the root of the ZIP
            string payloadZip = Path.Combine(buildDir, "payload.zip");
            Directory.CreateDirectory(stageDir);

            // 1. Python runtime
            status.Report("Downloading Python...");
            string url = await PythonReleaseService.GetDownloadUrlAsync(options.PythonVersion, options.Architecture, stripped: true);
            string archive = Path.Combine(buildDir, "python-standalone.tar.gz");

            var download = new Progress<DownloadProgress>(p => progress.Report((int)p.Percentage));
            await Step("download Python", () => FileDownloader.DownloadFileAsync(url, archive, download));

            status.Report("Extracting Python...");
            progress.Report(-1);
            string extractDir = Path.Combine(buildDir, "py");
            await Task.Run(() =>
            {
                Directory.CreateDirectory(extractDir);
                using (var file = File.OpenRead(archive))
                using (var gzip = new GZipStream(file, CompressionMode.Decompress))
                    TarFile.ExtractToDirectory(gzip, extractDir, overwriteFiles: true);
                File.Delete(archive);

                // The archive contains a single top-level "python" folder.
                CopyDirectory(Path.Combine(extractDir, "python"), stageDir);
                Directory.Delete(extractDir, true);
            });

            // 2. User application
            status.Report("Copying application files...");
            string appTarget = Path.Combine(stageDir, AppFolderName);
            await Task.Run(() => CopyDirectory(options.AppDirectory, appTarget, skip: [buildDir, options.OutputPath]));

            File.WriteAllText(Path.Combine(stageDir, RunFileName), BuildRunCommand(options));

            // 3. Dependencies (installed into the bundled interpreter's site-packages)
            if (!string.IsNullOrWhiteSpace(options.RequirementsFile))
            {
                string pythonExe = Path.Combine(stageDir, "python.exe");
                string reqFile = Path.GetFullPath(options.RequirementsFile);
                // Run pip from the requirements folder so relative entries (-r base.txt, ./wheels/x.whl) resolve as usual.
                string reqDir = Path.GetDirectoryName(reqFile)!;

                status.Report("Preparing pip...");
                await RunPythonAsync(pythonExe, "-m ensurepip --default-pip --upgrade", reqDir);

                status.Report("Installing requirements...");
                await RunPythonAsync(pythonExe, $"-m pip install --no-warn-script-location -r \"{reqFile}\"", reqDir);
            }

            // 4. Payload
            status.Report(options.UsePassword
                ? "Compressing and encrypting (AES-256)..."
                : "Compressing...");
            await Task.Run(() => CreatePayload(stageDir, payloadZip, options.UsePassword ? options.Password : null));

            // 5. Executable = loader + resources + payload
            status.Report("Writing executable...");
            WriteLoader(options.OutputPath);

            status.Report("Applying icon and details...");
            ExecutableMetadataWriter.Apply(options.OutputPath, options.Metadata);

            status.Report("Appending payload...");
            await Step("append the payload", async () =>
            {
                await using var output = new FileStream(options.OutputPath, FileMode.Append, FileAccess.Write);
                await using var payload = File.OpenRead(payloadZip);
                await payload.CopyToAsync(output);
            });

            return options.OutputPath;
        }
        finally
        {
            TryDeleteDirectory(buildDir);
        }
    }

    /// <summary>
    /// Content of run.pyex, e.g. <c>python.exe "main\src\app.py" --flag</c>.
    /// The loader splits it at the first space: executable / arguments.
    /// </summary>
    public static string BuildRunCommand(BuildOptions options)
    {
        string executable = options.IsGuiApp ? "pythonw.exe" : "python.exe";
        string script = Path.Combine(AppFolderName, options.EntryScriptRelativePath ?? Path.GetFileName(options.EntryScript));
        string arguments = options.ScriptArguments.Trim();

        return arguments.Length == 0
            ? $"{executable} \"{script}\""
            : $"{executable} \"{script}\" {arguments}";
    }

    private static void CreatePayload(string sourceDir, string zipPath, string? password)
    {
        try
        {
            using var zip = new Ionic.Zip.ZipFile
            {
                // Without UTF-8 non-ASCII file names (ą, ü, 日本) are stored in CP437 and get mangled.
                AlternateEncoding = Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.AsNecessary,
                UseZip64WhenSaving = Zip64Option.Never // the loader does not read Zip64
            };

            if (!string.IsNullOrEmpty(password))
            {
                zip.Password = password;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
            }

            zip.AddDirectory(sourceDir, "");
            zip.Save(zipPath);
        }
        catch (Exception ex)
        {
            throw new BuildException(
                "Cannot create the application archive. The package must stay below 4 GB and 65,535 files.\n" +
                $"({ex.Message})", ex);
        }
    }

    private static void WriteLoader(string outputPath)
    {
        try
        {
            File.WriteAllBytes(outputPath, Properties.Resources.PyrolithLoader);
        }
        catch (IOException ex)
        {
            throw new BuildException(
                $"Cannot write '{Path.GetFileName(outputPath)}'. If the previous build is still running, close it and try again.\n({ex.Message})", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new BuildException(
                $"Access denied to '{outputPath}'. Choose a folder you can write to.\n({ex.Message})", ex);
        }
    }

    /// <summary>Runs the bundled interpreter isolated from any Python configuration of the build machine.</summary>
    private static async Task RunPythonAsync(string pythonExe, string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // A system-wide Python setup must not leak into the bundled interpreter.
        startInfo.Environment.Remove("PYTHONHOME");
        startInfo.Environment.Remove("PYTHONPATH");
        startInfo.Environment["PYTHONNOUSERSITE"] = "1";
        startInfo.Environment["PIP_USER"] = "0";
        startInfo.Environment["PIP_REQUIRE_VIRTUALENV"] = "0";
        startInfo.Environment["PIP_DISABLE_PIP_VERSION_CHECK"] = "1";

        using var process = Process.Start(startInfo)
            ?? throw new BuildException($"Cannot start {pythonExe}.");

        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(stdout, stderr);
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            string output = string.IsNullOrWhiteSpace(stderr.Result) ? stdout.Result : stderr.Result;
            throw new BuildException(
                $"'python {arguments}' failed with exit code {process.ExitCode}.\n\n{LastLines(output, 25)}");
        }
    }

    /// <summary>
    /// Recursive copy. Skips paths in <paramref name="skip"/> (the build folder itself, the output .exe),
    /// virtual environments, .git and __pycache__ - they are never needed at runtime.
    /// </summary>
    public static void CopyDirectory(string sourceDir, string destinationDir, IReadOnlyCollection<string>? skip = null)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Cannot find the source folder: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            if (IsSkipped(file.FullName, skip)) continue;
            file.CopyTo(Path.Combine(destinationDir, file.Name), overwrite: true);
        }

        foreach (DirectoryInfo sub in dir.GetDirectories())
        {
            if (IsSkipped(sub.FullName, skip) || IsJunkDirectory(sub)) continue;
            CopyDirectory(sub.FullName, Path.Combine(destinationDir, sub.Name), skip);
        }
    }

    private static bool IsSkipped(string path, IReadOnlyCollection<string>? skip) =>
        skip != null && skip.Any(s => !string.IsNullOrWhiteSpace(s) && PathUtil.SamePath(path, s));

    private static bool IsJunkDirectory(DirectoryInfo dir) =>
        dir.Name is ".git" or "__pycache__"
        || dir.LinkTarget != null                                 // symlinks/junctions can loop forever (OneDrive folders are fine)
        || File.Exists(Path.Combine(dir.FullName, "pyvenv.cfg")); // venv / .venv - bound to the build machine

    private static string CreateTempDirectory(string prefix)
    {
        string path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
        catch { /* leftovers in %TEMP% are harmless */ }
    }

    /// <summary>Adds context to I/O and network errors of a single build step.</summary>
    private static async Task Step(string what, Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException or TaskCanceledException)
        {
            throw new BuildException($"Failed to {what}: {ex.Message}", ex);
        }
    }

    private static string LastLines(string text, int count)
    {
        var lines = text.TrimEnd().Split('\n');
        return string.Join('\n', lines.Skip(Math.Max(0, lines.Length - count))).Trim();
    }
}

/// <summary>A build failure with a message meant for the user.</summary>
public sealed class BuildException(string message, Exception? inner = null) : Exception(message, inner);
