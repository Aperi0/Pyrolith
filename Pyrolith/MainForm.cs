using Pyrolith.Core;
using Vestris.ResourceLib;

namespace Pyrolith;

/// <summary>
/// The only window of Pyrolith. It just collects input into <see cref="BuildOptions"/>;
/// the actual work lives in <see cref="PackageBuilder"/>.
/// </summary>
public partial class MainForm : Form
{
    private const string VersionsHint = "Click to load versions...";

    private bool _versionsLoaded;
    private bool _versionsLoading;
    private bool _isBuilding;
    private string? _projectPath;

    public MainForm()
    {
        InitializeComponent();
        UpdateTitle();
    }

    #region Startup

    private void MainForm_Load(object? sender, EventArgs e)
    {
        SetupComboBox(PythonVersionComboBox);
        SetupComboBox(ArchitectureComboBox);

        SetPythonVersionItems([VersionsHint]);
        ArchitectureComboBox.DataSource = PythonReleaseService.Architectures.ToList();

        AppDirectoryTextBox.TextChanged += (_, _) => UpdateCommandPreview();
        EntryScriptTextBox.TextChanged += (_, _) => UpdateCommandPreview();
        ArgumentsTextBox.TextChanged += (_, _) => UpdateCommandPreview();
        GuiAppCheckBox.CheckedChanged += (_, _) => UpdateCommandPreview();

        // Read-only path boxes are narrow - show the full path on hover.
        foreach (var box in new[] { AppDirectoryTextBox, EntryScriptTextBox, RequirementsTextBox, IconPathTextBox, OutputPathTextBox })
            box.TextChanged += (_, _) => HintToolTip.SetToolTip(box, box.Text);

        UpdateCommandPreview();
    }

    /// <summary>DropDownList + owner draw = read-only combo that still looks like a white input field.</summary>
    private void SetupComboBox(ComboBox combo)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.DrawItem += ComboBox_DrawItem;
    }

    private void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo || e.Index < 0) return;

        bool isEditPart = e.State.HasFlag(DrawItemState.ComboBoxEdit);
        Color back = isEditPart ? SystemColors.Window : e.BackColor;
        Color fore = isEditPart ? SystemColors.WindowText : e.ForeColor;

        using (var brush = new SolidBrush(back))
            e.Graphics.FillRectangle(brush, e.Bounds);

        string text = combo.GetItemText(combo.Items[e.Index]);
        using (var brush = new SolidBrush(fore))
            e.Graphics.DrawString(text, e.Font ?? combo.Font, brush, e.Bounds.X + 2, e.Bounds.Y + 2);
    }

    #endregion

    #region Python runtime

    /// <summary>The selected version, or null while a placeholder ("Click to load...") is shown.</summary>
    private string? SelectedPythonVersion =>
        PythonVersionComboBox.SelectedItem is string v && BuildOptions.IsPythonVersion(v) ? v : null;

    private void SetPythonVersionItems(List<string> items, string? select = null)
    {
        PythonVersionComboBox.DataSource = items;
        if (select != null && items.Contains(select))
            PythonVersionComboBox.SelectedItem = select;
    }

    /// <summary>The version list is downloaded lazily, the first time the list is opened.</summary>
    private async void PythonVersionComboBox_DropDown(object? sender, EventArgs e)
    {
        if (_versionsLoaded || _versionsLoading) return;
        _versionsLoading = true;

        string? previous = SelectedPythonVersion; // e.g. restored from a project file
        SetPythonVersionItems(["Loading..."]);

        try
        {
            var versions = await PythonReleaseService.GetPythonVersionsAsync();
            if (versions.Count == 0)
            {
                SetPythonVersionItems(previous != null ? [previous] : ["No versions found - click to retry"], previous);
                return;
            }

            SetPythonVersionItems(versions, previous);
            _versionsLoaded = true;

            if (previous != null && !versions.Contains(previous))
                ShowWarning($"Python {previous} (from the project) is no longer available. Python {versions[0]} was selected instead.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Cannot load Python versions");
            SetPythonVersionItems(previous != null ? [previous] : ["Error - click to retry"], previous);
        }
        finally
        {
            _versionsLoading = false;
        }
    }

    #endregion

    #region Application inputs

    private void BrowseAppDirectoryButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the folder of your Python application",
            UseDescriptionForTitle = true
        };
        if (Directory.Exists(AppDirectoryTextBox.Text))
            dialog.InitialDirectory = AppDirectoryTextBox.Text;

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string folder = dialog.SelectedPath;
        if (PathUtil.IsRoot(folder))
        {
            ShowWarning("A whole drive cannot be packed. Select your project folder.");
            return;
        }

        AppDirectoryTextBox.Text = folder;

        // The entry script must live inside the packed folder - drop it if it no longer does.
        if (EntryScriptTextBox.Text.Length > 0 && !PathUtil.IsInside(EntryScriptTextBox.Text, folder))
        {
            EntryScriptTextBox.Text = "";
            ShowInfo("The entry script is not inside the new folder, so it was cleared. Select the entry script again.");
        }
    }

    private void BrowseEntryScriptButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select the entry script (e.g. main.py)",
            Filter = "Python scripts (*.py;*.pyw;*.pyz)|*.py;*.pyw;*.pyz"
        };
        string appDir = AppDirectoryTextBox.Text;
        if (Directory.Exists(appDir))
            dialog.InitialDirectory = appDir;

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string script = dialog.FileName;
        string scriptDir = Path.GetDirectoryName(script)!;

        if (string.IsNullOrWhiteSpace(appDir))
        {
            // No folder yet - the script's folder is the natural choice.
            AppDirectoryTextBox.Text = scriptDir;
            StatusLabel.Text = "Application folder set to the script's folder.";
        }
        else if (!PathUtil.IsInside(script, appDir))
        {
            var answer = MessageBox.Show(this,
                "The entry script is not inside the application folder.\n\n" +
                $"Script:\n{script}\n\nFolder:\n{appDir}\n\n" +
                "Only the application folder is packed into the executable, so the script would be missing.\n\n" +
                $"Use the script's folder instead?\n{scriptDir}",
                "Script outside the application folder", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;
            AppDirectoryTextBox.Text = scriptDir;
        }

        EntryScriptTextBox.Text = script;

        if (Path.GetExtension(script).Equals(".pyw", StringComparison.OrdinalIgnoreCase))
            GuiAppCheckBox.Checked = true; // .pyw = windowed by convention
    }

    private void BrowseRequirementsButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select requirements.txt (optional)",
            Filter = "Requirements (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = "requirements.txt"
        };
        if (Directory.Exists(AppDirectoryTextBox.Text))
            dialog.InitialDirectory = AppDirectoryTextBox.Text;

        if (dialog.ShowDialog(this) == DialogResult.OK)
            RequirementsTextBox.Text = dialog.FileName;
    }

    private void ClearRequirementsButton_Click(object? sender, EventArgs e) => RequirementsTextBox.Text = "";

    private void PasswordCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        PasswordTextBox.Enabled = PasswordCheckBox.Checked;
        if (!PasswordCheckBox.Checked)
            PasswordTextBox.Text = "";
        else
            PasswordTextBox.Focus();
    }

    /// <summary>Shows the exact line written to run.pyex.</summary>
    private void UpdateCommandPreview()
    {
        var options = ReadOptions();
        string executable = options.IsGuiApp ? "pythonw.exe" : "python.exe";

        if (string.IsNullOrWhiteSpace(options.EntryScript))
            CommandPreviewTextBox.Text = $"{executable} \"{PackageBuilder.AppFolderName}\\<script.py>\"";
        else if (!string.IsNullOrWhiteSpace(options.AppDirectory) && options.EntryScriptRelativePath is null)
            CommandPreviewTextBox.Text = "(!) The entry script is outside the application folder";
        else
            CommandPreviewTextBox.Text = PackageBuilder.BuildRunCommand(options);
    }

    #endregion

    #region Executable details

    private void BrowseIconButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Icon files (*.ico)|*.ico", Title = "Select the application icon" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _ = new IconFile(dialog.FileName); // the same parser that writes the icon at build time
        }
        catch (Exception ex)
        {
            ShowError($"'{Path.GetFileName(dialog.FileName)}' is not a valid .ico file.\n({ex.Message})");
            return;
        }

        IconPathTextBox.Text = dialog.FileName;
        ShowIconPreview(dialog.FileName);
    }

    private void ClearIconButton_Click(object? sender, EventArgs e)
    {
        IconPathTextBox.Text = "";
        ShowIconPreview(null);
    }

    private void ShowIconPreview(string? iconPath)
    {
        var old = IconPreviewPictureBox.BackgroundImage;
        IconPreviewPictureBox.BackgroundImage = null;
        old?.Dispose();

        if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath)) return;

        try
        {
            using var icon = new Icon(iconPath, 256, 256); // picks the closest (largest) frame
            IconPreviewPictureBox.BackgroundImage = icon.ToBitmap();
        }
        catch
        {
            // Preview only - the icon was already validated, a missing preview is not an error.
        }
    }

    #endregion

    #region Build

    private void BrowseOutputButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Executable (*.exe)|*.exe",
            Title = "Save the executable as",
            DefaultExt = "exe",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = SuggestedExeName()
        };

        string? initialDir = Path.GetDirectoryName(OutputPathTextBox.Text);
        if (string.IsNullOrEmpty(initialDir) && Directory.Exists(AppDirectoryTextBox.Text))
            initialDir = Path.GetDirectoryName(PathUtil.Normalize(AppDirectoryTextBox.Text)); // next to the project, not inside it
        if (Directory.Exists(initialDir))
            dialog.InitialDirectory = initialDir;

        if (dialog.ShowDialog(this) == DialogResult.OK)
            OutputPathTextBox.Text = dialog.FileName;
    }

    private string SuggestedExeName()
    {
        string name = !string.IsNullOrWhiteSpace(ProductNameTextBox.Text) ? ProductNameTextBox.Text
            : !string.IsNullOrWhiteSpace(EntryScriptTextBox.Text) ? Path.GetFileNameWithoutExtension(EntryScriptTextBox.Text)
            : "app";
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim() + ".exe";
    }

    private async void BuildButton_Click(object? sender, EventArgs e)
    {
        var options = ReadOptions();

        var validation = options.Validate();
        if (validation.HasErrors)
        {
            ShowWarning(string.Join("\n\n", validation.Errors), "Cannot build yet");
            return;
        }
        if (validation.Warnings.Count > 0 &&
            !Confirm(string.Join("\n\n", validation.Warnings) + "\n\nBuild anyway?"))
            return;

        // Progress<T> captures the UI context, so the builder can report from any thread.
        var status = new Progress<string>(message => StatusLabel.Text = message);
        var progress = new Progress<int>(SetProgress);

        SetBusy(true);
        try
        {
            string exePath = await PackageBuilder.BuildAsync(options, status, progress);

            StatusLabel.Text = "Build succeeded.";
            SetProgress(100);
            MessageBox.Show(this, $"Build succeeded.\n\n{exePath}", "Pyrolith", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Build failed.";
            SetProgress(0);
            ShowError(ex.Message, "Build failed");
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Negative value = unknown duration (marquee).</summary>
    private void SetProgress(int percent)
    {
        if (percent < 0)
        {
            BuildProgressBar.Style = ProgressBarStyle.Marquee;
            return;
        }
        BuildProgressBar.Style = ProgressBarStyle.Continuous;
        BuildProgressBar.Value = Math.Clamp(percent, BuildProgressBar.Minimum, BuildProgressBar.Maximum);
    }

    /// <summary>Locks every input while a build runs, so the options cannot change mid-build.</summary>
    private void SetBusy(bool busy)
    {
        _isBuilding = busy;
        foreach (Control c in new Control[] { PythonGroupBox, ApplicationGroupBox, OptionsGroupBox, MetadataGroupBox, BuildButton, BrowseOutputButton })
            c.Enabled = !busy;
        ProjectMenuItem.Enabled = !busy;
        UseWaitCursor = busy;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isBuilding && !Confirm("A build is still running. Quit anyway?"))
            e.Cancel = true;
    }

    private BuildOptions ReadOptions() => new()
    {
        PythonVersion = SelectedPythonVersion ?? "",
        Architecture = ArchitectureComboBox.SelectedItem as string ?? "",
        AppDirectory = AppDirectoryTextBox.Text.Trim(),
        EntryScript = EntryScriptTextBox.Text.Trim(),
        ScriptArguments = ArgumentsTextBox.Text.Trim(),
        RequirementsFile = NullIfEmpty(RequirementsTextBox.Text),
        IsGuiApp = GuiAppCheckBox.Checked,
        UsePassword = PasswordCheckBox.Checked,
        Password = PasswordCheckBox.Checked ? PasswordTextBox.Text : null,
        OutputPath = OutputPathTextBox.Text.Trim(),
        Metadata = new AppMetadata(
            ProductNameTextBox.Text,
            DescriptionTextBox.Text,
            AuthorTextBox.Text,
            NullIfEmpty(IconPathTextBox.Text))
    };

    #endregion

    #region Project file

    private void NewProjectMenuItem_Click(object? sender, EventArgs e)
    {
        if (!Confirm("Clear all fields and start a new project?")) return;
        ApplyProject(new ProjectFile());
        _projectPath = null;
        UpdateTitle();
        StatusLabel.Text = "Ready.";
    }

    private void OpenProjectMenuItem_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = ProjectFile.OpenFilter, Title = "Open project" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var project = ProjectFile.Load(dialog.FileName);
            ApplyProject(project);
            _projectPath = dialog.FileName;
            UpdateTitle();
            StatusLabel.Text = "Project loaded.";

            var missing = project.MissingPaths().ToList();
            if (missing.Count > 0)
                ShowWarning("The project was loaded, but these paths no longer exist:\n\n" + string.Join("\n", missing));
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Cannot open project");
        }
    }

    private void SaveProjectMenuItem_Click(object? sender, EventArgs e)
    {
        if (_projectPath is null) SaveProjectAs();
        else SaveProject(_projectPath);
    }

    private void SaveProjectAsMenuItem_Click(object? sender, EventArgs e) => SaveProjectAs();

    private void SaveProjectAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = ProjectFile.SaveFilter,
            DefaultExt = ProjectFile.Extension,
            AddExtension = true,
            Title = "Save project",
            FileName = _projectPath is null
                ? Path.ChangeExtension(SuggestedExeName(), ProjectFile.Extension)
                : Path.GetFileName(_projectPath)
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            SaveProject(dialog.FileName);
    }

    private void SaveProject(string path)
    {
        try
        {
            CaptureProject().Save(path);
            _projectPath = path;
            UpdateTitle();
            StatusLabel.Text = "Project saved.";
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Cannot save project");
        }
    }

    private ProjectFile CaptureProject() => new()
    {
        PythonVersion = SelectedPythonVersion,
        Architecture = ArchitectureComboBox.SelectedItem as string,
        TargetAppDir = AppDirectoryTextBox.Text,
        TargetApp = EntryScriptTextBox.Text,
        ScriptArguments = ArgumentsTextBox.Text,
        IsGuiApp = GuiAppCheckBox.Checked,
        Requirements = RequirementsTextBox.Text,
        UsePassword = PasswordCheckBox.Checked,
        Password = PasswordTextBox.Text,
        AppAuthor = AuthorTextBox.Text,
        AppName = ProductNameTextBox.Text,
        AppDescription = DescriptionTextBox.Text,
        IconPath = IconPathTextBox.Text,
        OutputExePath = OutputPathTextBox.Text
    };

    private void ApplyProject(ProjectFile p)
    {
        // Python version: select it if the list is loaded, otherwise show it alone
        // and let the next drop-down reload the list and re-select it.
        if (_versionsLoaded && p.PythonVersion != null && PythonVersionComboBox.Items.Contains(p.PythonVersion))
        {
            PythonVersionComboBox.SelectedItem = p.PythonVersion;
        }
        else if (p.PythonVersion != null && BuildOptions.IsPythonVersion(p.PythonVersion))
        {
            SetPythonVersionItems([p.PythonVersion], p.PythonVersion);
            _versionsLoaded = false;
        }
        else if (!_versionsLoaded)
        {
            SetPythonVersionItems([VersionsHint]);
        }

        ArchitectureComboBox.SelectedItem = p.Architecture != null && ArchitectureComboBox.Items.Contains(p.Architecture)
            ? p.Architecture
            : ArchitectureComboBox.Items[0];

        AppDirectoryTextBox.Text = p.TargetAppDir ?? "";
        EntryScriptTextBox.Text = p.TargetApp ?? "";
        ArgumentsTextBox.Text = p.ScriptArguments ?? "";
        GuiAppCheckBox.Checked = p.IsGuiApp;
        RequirementsTextBox.Text = p.Requirements ?? "";
        PasswordCheckBox.Checked = p.UsePassword;   // before the text: unchecking clears the password box
        PasswordTextBox.Text = p.Password ?? "";
        AuthorTextBox.Text = p.AppAuthor ?? "";
        ProductNameTextBox.Text = p.AppName ?? "";
        DescriptionTextBox.Text = p.AppDescription ?? "";
        IconPathTextBox.Text = p.IconPath ?? "";
        OutputPathTextBox.Text = p.OutputExePath ?? "";

        ShowIconPreview(p.IconPath);
        UpdateCommandPreview();
    }

    private void UpdateTitle() =>
        Text = _projectPath is null
            ? $"Pyrolith {AppInfo.Version} - Python to standalone EXE packer"
            : $"Pyrolith {AppInfo.Version} - {Path.GetFileName(_projectPath)}";

    #endregion

    #region Info menu

    private void AboutMenuItem_Click(object? sender, EventArgs e) => AboutForm.Open(this, AboutForm.Page.About);

    private void ThirdPartyMenuItem_Click(object? sender, EventArgs e) => AboutForm.Open(this, AboutForm.Page.ThirdParty);

    #endregion

    #region Helpers

    private static string? NullIfEmpty(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private void ShowError(string message, string title = "Error") =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowWarning(string message, string title = "Warning") =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void ShowInfo(string message, string title = "Pyrolith") =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    private bool Confirm(string message, string title = "Pyrolith") =>
        MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    #endregion
}
