namespace Pyrolith
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            PythonVersionLabel = new Label();
            PythonVersionComboBox = new ComboBox();
            ArchitectureLabel = new Label();
            ArchitectureComboBox = new ComboBox();
            AppDirectoryLabel = new Label();
            AppDirectoryTextBox = new TextBox();
            BrowseAppDirectoryButton = new Button();
            EntryScriptLabel = new Label();
            EntryScriptTextBox = new TextBox();
            BrowseEntryScriptButton = new Button();
            ArgumentsLabel = new Label();
            ArgumentsTextBox = new TextBox();
            CommandPreviewTextBox = new TextBox();
            BrowseRequirementsButton = new Button();
            ClearRequirementsButton = new Button();
            RequirementsTextBox = new TextBox();
            RequirementsLabel = new Label();
            BuildButton = new Button();
            StatusLabel = new Label();
            BuildProgressBar = new ProgressBar();
            GuiAppCheckBox = new CheckBox();
            PasswordCheckBox = new CheckBox();
            PasswordTextBox = new TextBox();
            ApplicationGroupBox = new GroupBox();
            PythonGroupBox = new GroupBox();
            OptionsGroupBox = new GroupBox();
            MetadataGroupBox = new GroupBox();
            BrowseIconButton = new Button();
            ClearIconButton = new Button();
            IconPathTextBox = new TextBox();
            IconLabel = new Label();
            AuthorTextBox = new TextBox();
            AuthorLabel = new Label();
            ProductNameLabel = new Label();
            ProductNameTextBox = new TextBox();
            DescriptionTextBox = new TextBox();
            DescriptionLabel = new Label();
            IconPreviewPictureBox = new PictureBox();
            BuildGroupBox = new GroupBox();
            OutputLabel = new Label();
            OutputPathTextBox = new TextBox();
            BrowseOutputButton = new Button();
            MenuBar = new MenuStrip();
            ProjectMenuItem = new ToolStripMenuItem();
            NewProjectMenuItem = new ToolStripMenuItem();
            OpenProjectMenuItem = new ToolStripMenuItem();
            SaveProjectMenuItem = new ToolStripMenuItem();
            SaveProjectAsMenuItem = new ToolStripMenuItem();
            InfoMenuItem = new ToolStripMenuItem();
            AboutMenuItem = new ToolStripMenuItem();
            ThirdPartyMenuItem = new ToolStripMenuItem();
            HintToolTip = new ToolTip(components);
            ApplicationGroupBox.SuspendLayout();
            PythonGroupBox.SuspendLayout();
            OptionsGroupBox.SuspendLayout();
            MetadataGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)IconPreviewPictureBox).BeginInit();
            BuildGroupBox.SuspendLayout();
            MenuBar.SuspendLayout();
            SuspendLayout();
            // 
            // PythonVersionLabel
            // 
            PythonVersionLabel.AutoSize = true;
            PythonVersionLabel.Location = new Point(6, 25);
            PythonVersionLabel.Name = "PythonVersionLabel";
            PythonVersionLabel.Size = new Size(48, 15);
            PythonVersionLabel.TabIndex = 0;
            PythonVersionLabel.Text = "Version:";
            // 
            // PythonVersionComboBox
            // 
            PythonVersionComboBox.FormattingEnabled = true;
            PythonVersionComboBox.Location = new Point(128, 22);
            PythonVersionComboBox.Name = "PythonVersionComboBox";
            PythonVersionComboBox.Size = new Size(142, 23);
            PythonVersionComboBox.TabIndex = 1;
            PythonVersionComboBox.DropDown += PythonVersionComboBox_DropDown;
            // 
            // ArchitectureLabel
            // 
            ArchitectureLabel.AutoSize = true;
            ArchitectureLabel.Location = new Point(6, 54);
            ArchitectureLabel.Name = "ArchitectureLabel";
            ArchitectureLabel.Size = new Size(75, 15);
            ArchitectureLabel.TabIndex = 2;
            ArchitectureLabel.Text = "Architecture:";
            // 
            // ArchitectureComboBox
            // 
            ArchitectureComboBox.FormattingEnabled = true;
            ArchitectureComboBox.Location = new Point(128, 51);
            ArchitectureComboBox.Name = "ArchitectureComboBox";
            ArchitectureComboBox.Size = new Size(142, 23);
            ArchitectureComboBox.TabIndex = 3;
            // 
            // AppDirectoryLabel
            // 
            AppDirectoryLabel.AutoSize = true;
            AppDirectoryLabel.Location = new Point(8, 25);
            AppDirectoryLabel.Name = "AppDirectoryLabel";
            AppDirectoryLabel.Size = new Size(43, 15);
            AppDirectoryLabel.TabIndex = 0;
            AppDirectoryLabel.Text = "Folder:";
            // 
            // AppDirectoryTextBox
            // 
            AppDirectoryTextBox.BorderStyle = BorderStyle.FixedSingle;
            AppDirectoryTextBox.Location = new Point(107, 23);
            AppDirectoryTextBox.Name = "AppDirectoryTextBox";
            AppDirectoryTextBox.ReadOnly = true;
            AppDirectoryTextBox.Size = new Size(119, 23);
            AppDirectoryTextBox.TabIndex = 1;
            AppDirectoryTextBox.TabStop = false;
            // 
            // BrowseAppDirectoryButton
            // 
            BrowseAppDirectoryButton.Location = new Point(232, 23);
            BrowseAppDirectoryButton.Name = "BrowseAppDirectoryButton";
            BrowseAppDirectoryButton.Size = new Size(50, 23);
            BrowseAppDirectoryButton.TabIndex = 2;
            BrowseAppDirectoryButton.Text = "...";
            BrowseAppDirectoryButton.UseVisualStyleBackColor = true;
            BrowseAppDirectoryButton.Click += BrowseAppDirectoryButton_Click;
            // 
            // EntryScriptLabel
            // 
            EntryScriptLabel.AutoSize = true;
            EntryScriptLabel.Location = new Point(8, 54);
            EntryScriptLabel.Name = "EntryScriptLabel";
            EntryScriptLabel.Size = new Size(69, 15);
            EntryScriptLabel.TabIndex = 3;
            EntryScriptLabel.Text = "Entry script:";
            // 
            // EntryScriptTextBox
            // 
            EntryScriptTextBox.BorderStyle = BorderStyle.FixedSingle;
            EntryScriptTextBox.Location = new Point(107, 51);
            EntryScriptTextBox.Name = "EntryScriptTextBox";
            EntryScriptTextBox.ReadOnly = true;
            EntryScriptTextBox.Size = new Size(119, 23);
            EntryScriptTextBox.TabIndex = 4;
            EntryScriptTextBox.TabStop = false;
            // 
            // BrowseEntryScriptButton
            // 
            BrowseEntryScriptButton.Location = new Point(232, 51);
            BrowseEntryScriptButton.Name = "BrowseEntryScriptButton";
            BrowseEntryScriptButton.Size = new Size(50, 23);
            BrowseEntryScriptButton.TabIndex = 5;
            BrowseEntryScriptButton.Text = "...";
            BrowseEntryScriptButton.UseVisualStyleBackColor = true;
            BrowseEntryScriptButton.Click += BrowseEntryScriptButton_Click;
            // 
            // ArgumentsLabel
            // 
            ArgumentsLabel.AutoSize = true;
            ArgumentsLabel.Location = new Point(8, 111);
            ArgumentsLabel.Name = "ArgumentsLabel";
            ArgumentsLabel.Size = new Size(69, 15);
            ArgumentsLabel.TabIndex = 10;
            ArgumentsLabel.Text = "Arguments:";
            // 
            // ArgumentsTextBox
            // 
            ArgumentsTextBox.BorderStyle = BorderStyle.FixedSingle;
            ArgumentsTextBox.Location = new Point(107, 109);
            ArgumentsTextBox.Name = "ArgumentsTextBox";
            ArgumentsTextBox.PlaceholderText = "(optional)";
            ArgumentsTextBox.Size = new Size(175, 23);
            ArgumentsTextBox.TabIndex = 11;
            // 
            // CommandPreviewTextBox
            // 
            CommandPreviewTextBox.BorderStyle = BorderStyle.FixedSingle;
            CommandPreviewTextBox.Location = new Point(8, 138);
            CommandPreviewTextBox.Name = "CommandPreviewTextBox";
            CommandPreviewTextBox.ReadOnly = true;
            CommandPreviewTextBox.Size = new Size(274, 23);
            CommandPreviewTextBox.TabIndex = 12;
            CommandPreviewTextBox.TabStop = false;
            // 
            // BrowseRequirementsButton
            // 
            BrowseRequirementsButton.Location = new Point(232, 80);
            BrowseRequirementsButton.Name = "BrowseRequirementsButton";
            BrowseRequirementsButton.Size = new Size(24, 23);
            BrowseRequirementsButton.TabIndex = 8;
            BrowseRequirementsButton.Text = "...";
            BrowseRequirementsButton.UseVisualStyleBackColor = true;
            BrowseRequirementsButton.Click += BrowseRequirementsButton_Click;
            // 
            // ClearRequirementsButton
            // 
            ClearRequirementsButton.Location = new Point(258, 80);
            ClearRequirementsButton.Name = "ClearRequirementsButton";
            ClearRequirementsButton.Size = new Size(24, 23);
            ClearRequirementsButton.TabIndex = 9;
            ClearRequirementsButton.Text = "×";
            ClearRequirementsButton.UseVisualStyleBackColor = true;
            ClearRequirementsButton.Click += ClearRequirementsButton_Click;
            // 
            // RequirementsTextBox
            // 
            RequirementsTextBox.BorderStyle = BorderStyle.FixedSingle;
            RequirementsTextBox.Location = new Point(107, 80);
            RequirementsTextBox.Name = "RequirementsTextBox";
            RequirementsTextBox.PlaceholderText = "(optional)";
            RequirementsTextBox.ReadOnly = true;
            RequirementsTextBox.Size = new Size(119, 23);
            RequirementsTextBox.TabIndex = 7;
            RequirementsTextBox.TabStop = false;
            // 
            // RequirementsLabel
            // 
            RequirementsLabel.AutoSize = true;
            RequirementsLabel.Location = new Point(8, 82);
            RequirementsLabel.Name = "RequirementsLabel";
            RequirementsLabel.Size = new Size(83, 15);
            RequirementsLabel.TabIndex = 6;
            RequirementsLabel.Text = "Requirements:";
            // 
            // BuildButton
            // 
            BuildButton.Location = new Point(6, 51);
            BuildButton.Name = "BuildButton";
            BuildButton.Size = new Size(264, 23);
            BuildButton.TabIndex = 3;
            BuildButton.Text = "Build EXE";
            BuildButton.UseVisualStyleBackColor = true;
            BuildButton.Click += BuildButton_Click;
            // 
            // StatusLabel
            // 
            StatusLabel.AutoSize = true;
            StatusLabel.Location = new Point(6, 77);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(42, 15);
            StatusLabel.TabIndex = 5;
            StatusLabel.Text = "Ready.";
            // 
            // BuildProgressBar
            // 
            BuildProgressBar.Location = new Point(276, 51);
            BuildProgressBar.Name = "BuildProgressBar";
            BuildProgressBar.Size = new Size(291, 23);
            BuildProgressBar.TabIndex = 4;
            // 
            // GuiAppCheckBox
            // 
            GuiAppCheckBox.AutoSize = true;
            GuiAppCheckBox.Location = new Point(6, 22);
            GuiAppCheckBox.Name = "GuiAppCheckBox";
            GuiAppCheckBox.Size = new Size(182, 19);
            GuiAppCheckBox.TabIndex = 0;
            GuiAppCheckBox.Text = "GUI app (no console window)";
            GuiAppCheckBox.UseVisualStyleBackColor = true;
            // 
            // PasswordCheckBox
            // 
            PasswordCheckBox.AutoSize = true;
            PasswordCheckBox.Location = new Point(6, 48);
            PasswordCheckBox.Name = "PasswordCheckBox";
            PasswordCheckBox.Size = new Size(143, 19);
            PasswordCheckBox.TabIndex = 1;
            PasswordCheckBox.Text = "Protect with password";
            PasswordCheckBox.UseVisualStyleBackColor = true;
            PasswordCheckBox.CheckedChanged += PasswordCheckBox_CheckedChanged;
            // 
            // PasswordTextBox
            // 
            PasswordTextBox.BorderStyle = BorderStyle.FixedSingle;
            PasswordTextBox.Enabled = false;
            PasswordTextBox.Location = new Point(157, 47);
            PasswordTextBox.Name = "PasswordTextBox";
            PasswordTextBox.Size = new Size(113, 23);
            PasswordTextBox.TabIndex = 2;
            PasswordTextBox.UseSystemPasswordChar = true;
            // 
            // ApplicationGroupBox
            // 
            ApplicationGroupBox.Controls.Add(AppDirectoryLabel);
            ApplicationGroupBox.Controls.Add(AppDirectoryTextBox);
            ApplicationGroupBox.Controls.Add(BrowseAppDirectoryButton);
            ApplicationGroupBox.Controls.Add(EntryScriptLabel);
            ApplicationGroupBox.Controls.Add(EntryScriptTextBox);
            ApplicationGroupBox.Controls.Add(BrowseEntryScriptButton);
            ApplicationGroupBox.Controls.Add(RequirementsLabel);
            ApplicationGroupBox.Controls.Add(CommandPreviewTextBox);
            ApplicationGroupBox.Controls.Add(RequirementsTextBox);
            ApplicationGroupBox.Controls.Add(BrowseRequirementsButton);
            ApplicationGroupBox.Controls.Add(ClearRequirementsButton);
            ApplicationGroupBox.Controls.Add(ArgumentsTextBox);
            ApplicationGroupBox.Controls.Add(ArgumentsLabel);
            ApplicationGroupBox.Location = new Point(297, 26);
            ApplicationGroupBox.Name = "ApplicationGroupBox";
            ApplicationGroupBox.Size = new Size(290, 172);
            ApplicationGroupBox.TabIndex = 1;
            ApplicationGroupBox.TabStop = false;
            ApplicationGroupBox.Text = "Python Application";
            // 
            // PythonGroupBox
            // 
            PythonGroupBox.Controls.Add(PythonVersionLabel);
            PythonGroupBox.Controls.Add(PythonVersionComboBox);
            PythonGroupBox.Controls.Add(ArchitectureLabel);
            PythonGroupBox.Controls.Add(ArchitectureComboBox);
            PythonGroupBox.Location = new Point(12, 27);
            PythonGroupBox.Name = "PythonGroupBox";
            PythonGroupBox.Size = new Size(279, 85);
            PythonGroupBox.TabIndex = 0;
            PythonGroupBox.TabStop = false;
            PythonGroupBox.Text = "Python Runtime";
            // 
            // OptionsGroupBox
            // 
            OptionsGroupBox.Controls.Add(GuiAppCheckBox);
            OptionsGroupBox.Controls.Add(PasswordCheckBox);
            OptionsGroupBox.Controls.Add(PasswordTextBox);
            OptionsGroupBox.Location = new Point(12, 117);
            OptionsGroupBox.Name = "OptionsGroupBox";
            OptionsGroupBox.Size = new Size(279, 81);
            OptionsGroupBox.TabIndex = 2;
            OptionsGroupBox.TabStop = false;
            OptionsGroupBox.Text = "Options";
            // 
            // MetadataGroupBox
            // 
            MetadataGroupBox.Controls.Add(BrowseIconButton);
            MetadataGroupBox.Controls.Add(ClearIconButton);
            MetadataGroupBox.Controls.Add(IconPathTextBox);
            MetadataGroupBox.Controls.Add(IconLabel);
            MetadataGroupBox.Controls.Add(AuthorTextBox);
            MetadataGroupBox.Controls.Add(AuthorLabel);
            MetadataGroupBox.Controls.Add(ProductNameLabel);
            MetadataGroupBox.Controls.Add(ProductNameTextBox);
            MetadataGroupBox.Controls.Add(DescriptionTextBox);
            MetadataGroupBox.Controls.Add(DescriptionLabel);
            MetadataGroupBox.Location = new Point(12, 204);
            MetadataGroupBox.Name = "MetadataGroupBox";
            MetadataGroupBox.Size = new Size(428, 141);
            MetadataGroupBox.TabIndex = 3;
            MetadataGroupBox.TabStop = false;
            MetadataGroupBox.Text = "Executable Details";
            // 
            // BrowseIconButton
            // 
            BrowseIconButton.Location = new Point(372, 108);
            BrowseIconButton.Name = "BrowseIconButton";
            BrowseIconButton.Size = new Size(24, 23);
            BrowseIconButton.TabIndex = 8;
            BrowseIconButton.Text = "...";
            BrowseIconButton.UseVisualStyleBackColor = true;
            BrowseIconButton.Click += BrowseIconButton_Click;
            // 
            // ClearIconButton
            // 
            ClearIconButton.Location = new Point(398, 108);
            ClearIconButton.Name = "ClearIconButton";
            ClearIconButton.Size = new Size(24, 23);
            ClearIconButton.TabIndex = 9;
            ClearIconButton.Text = "×";
            ClearIconButton.UseVisualStyleBackColor = true;
            ClearIconButton.Click += ClearIconButton_Click;
            // 
            // IconPathTextBox
            // 
            IconPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            IconPathTextBox.Location = new Point(105, 108);
            IconPathTextBox.Name = "IconPathTextBox";
            IconPathTextBox.PlaceholderText = "(optional)";
            IconPathTextBox.ReadOnly = true;
            IconPathTextBox.Size = new Size(261, 23);
            IconPathTextBox.TabIndex = 7;
            IconPathTextBox.TabStop = false;
            // 
            // IconLabel
            // 
            IconLabel.AutoSize = true;
            IconLabel.Location = new Point(6, 111);
            IconLabel.Name = "IconLabel";
            IconLabel.Size = new Size(63, 15);
            IconLabel.TabIndex = 6;
            IconLabel.Text = "Icon (.ico):";
            // 
            // AuthorTextBox
            // 
            AuthorTextBox.BorderStyle = BorderStyle.FixedSingle;
            AuthorTextBox.Location = new Point(105, 79);
            AuthorTextBox.Name = "AuthorTextBox";
            AuthorTextBox.PlaceholderText = "Unknown";
            AuthorTextBox.Size = new Size(317, 23);
            AuthorTextBox.TabIndex = 5;
            // 
            // AuthorLabel
            // 
            AuthorLabel.AutoSize = true;
            AuthorLabel.Location = new Point(6, 82);
            AuthorLabel.Name = "AuthorLabel";
            AuthorLabel.Size = new Size(47, 15);
            AuthorLabel.TabIndex = 4;
            AuthorLabel.Text = "Author:";
            // 
            // ProductNameLabel
            // 
            ProductNameLabel.AutoSize = true;
            ProductNameLabel.Location = new Point(6, 24);
            ProductNameLabel.Name = "ProductNameLabel";
            ProductNameLabel.Size = new Size(85, 15);
            ProductNameLabel.TabIndex = 0;
            ProductNameLabel.Text = "Product name:";
            // 
            // ProductNameTextBox
            // 
            ProductNameTextBox.BorderStyle = BorderStyle.FixedSingle;
            ProductNameTextBox.Location = new Point(105, 22);
            ProductNameTextBox.Name = "ProductNameTextBox";
            ProductNameTextBox.PlaceholderText = "Python Application";
            ProductNameTextBox.Size = new Size(317, 23);
            ProductNameTextBox.TabIndex = 1;
            // 
            // DescriptionTextBox
            // 
            DescriptionTextBox.BorderStyle = BorderStyle.FixedSingle;
            DescriptionTextBox.Location = new Point(105, 50);
            DescriptionTextBox.Name = "DescriptionTextBox";
            DescriptionTextBox.PlaceholderText = "Python Script";
            DescriptionTextBox.Size = new Size(317, 23);
            DescriptionTextBox.TabIndex = 3;
            // 
            // DescriptionLabel
            // 
            DescriptionLabel.AutoSize = true;
            DescriptionLabel.Location = new Point(6, 53);
            DescriptionLabel.Name = "DescriptionLabel";
            DescriptionLabel.Size = new Size(70, 15);
            DescriptionLabel.TabIndex = 2;
            DescriptionLabel.Text = "Description:";
            // 
            // IconPreviewPictureBox
            // 
            IconPreviewPictureBox.BackgroundImageLayout = ImageLayout.Zoom;
            IconPreviewPictureBox.BorderStyle = BorderStyle.FixedSingle;
            IconPreviewPictureBox.Location = new Point(446, 204);
            IconPreviewPictureBox.Name = "IconPreviewPictureBox";
            IconPreviewPictureBox.Size = new Size(141, 141);
            IconPreviewPictureBox.TabIndex = 5;
            IconPreviewPictureBox.TabStop = false;
            // 
            // BuildGroupBox
            // 
            BuildGroupBox.Controls.Add(OutputLabel);
            BuildGroupBox.Controls.Add(OutputPathTextBox);
            BuildGroupBox.Controls.Add(BrowseOutputButton);
            BuildGroupBox.Controls.Add(BuildButton);
            BuildGroupBox.Controls.Add(BuildProgressBar);
            BuildGroupBox.Controls.Add(StatusLabel);
            BuildGroupBox.Location = new Point(12, 351);
            BuildGroupBox.Name = "BuildGroupBox";
            BuildGroupBox.Size = new Size(575, 100);
            BuildGroupBox.TabIndex = 4;
            BuildGroupBox.TabStop = false;
            BuildGroupBox.Text = "Build";
            // 
            // OutputLabel
            // 
            OutputLabel.AutoSize = true;
            OutputLabel.Location = new Point(6, 26);
            OutputLabel.Name = "OutputLabel";
            OutputLabel.Size = new Size(71, 15);
            OutputLabel.TabIndex = 0;
            OutputLabel.Text = "Output .exe:";
            // 
            // OutputPathTextBox
            // 
            OutputPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            OutputPathTextBox.Location = new Point(105, 22);
            OutputPathTextBox.Name = "OutputPathTextBox";
            OutputPathTextBox.ReadOnly = true;
            OutputPathTextBox.Size = new Size(406, 23);
            OutputPathTextBox.TabIndex = 1;
            OutputPathTextBox.TabStop = false;
            // 
            // BrowseOutputButton
            // 
            BrowseOutputButton.Location = new Point(517, 22);
            BrowseOutputButton.Name = "BrowseOutputButton";
            BrowseOutputButton.Size = new Size(50, 23);
            BrowseOutputButton.TabIndex = 2;
            BrowseOutputButton.Text = "...";
            BrowseOutputButton.UseVisualStyleBackColor = true;
            BrowseOutputButton.Click += BrowseOutputButton_Click;
            // 
            // MenuBar
            // 
            MenuBar.Items.AddRange(new ToolStripItem[] { ProjectMenuItem, InfoMenuItem });
            MenuBar.Location = new Point(0, 0);
            MenuBar.Name = "MenuBar";
            MenuBar.Size = new Size(593, 24);
            MenuBar.TabIndex = 6;
            MenuBar.Text = "MenuBar";
            // 
            // ProjectMenuItem
            // 
            ProjectMenuItem.DropDownItems.AddRange(new ToolStripItem[] { NewProjectMenuItem, OpenProjectMenuItem, SaveProjectMenuItem, SaveProjectAsMenuItem });
            ProjectMenuItem.Name = "ProjectMenuItem";
            ProjectMenuItem.Size = new Size(56, 20);
            ProjectMenuItem.Text = "&Project";
            // 
            // NewProjectMenuItem
            // 
            NewProjectMenuItem.Name = "NewProjectMenuItem";
            NewProjectMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            NewProjectMenuItem.Size = new Size(195, 22);
            NewProjectMenuItem.Text = "&New";
            NewProjectMenuItem.Click += NewProjectMenuItem_Click;
            // 
            // OpenProjectMenuItem
            // 
            OpenProjectMenuItem.Name = "OpenProjectMenuItem";
            OpenProjectMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            OpenProjectMenuItem.Size = new Size(195, 22);
            OpenProjectMenuItem.Text = "&Open...";
            OpenProjectMenuItem.Click += OpenProjectMenuItem_Click;
            // 
            // SaveProjectMenuItem
            // 
            SaveProjectMenuItem.Name = "SaveProjectMenuItem";
            SaveProjectMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            SaveProjectMenuItem.Size = new Size(195, 22);
            SaveProjectMenuItem.Text = "&Save";
            SaveProjectMenuItem.Click += SaveProjectMenuItem_Click;
            // 
            // SaveProjectAsMenuItem
            // 
            SaveProjectAsMenuItem.Name = "SaveProjectAsMenuItem";
            SaveProjectAsMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            SaveProjectAsMenuItem.Size = new Size(195, 22);
            SaveProjectAsMenuItem.Text = "Save &As...";
            SaveProjectAsMenuItem.Click += SaveProjectAsMenuItem_Click;
            // 
            // InfoMenuItem
            // 
            InfoMenuItem.DropDownItems.AddRange(new ToolStripItem[] { AboutMenuItem, ThirdPartyMenuItem });
            InfoMenuItem.Name = "InfoMenuItem";
            InfoMenuItem.Size = new Size(40, 20);
            InfoMenuItem.Text = "&Info";
            // 
            // AboutMenuItem
            // 
            AboutMenuItem.Name = "AboutMenuItem";
            AboutMenuItem.Size = new Size(190, 22);
            AboutMenuItem.Text = "&About Pyrolith...";
            AboutMenuItem.Click += AboutMenuItem_Click;
            // 
            // ThirdPartyMenuItem
            // 
            ThirdPartyMenuItem.Name = "ThirdPartyMenuItem";
            ThirdPartyMenuItem.Size = new Size(190, 22);
            ThirdPartyMenuItem.Text = "&Third-Party Libraries...";
            ThirdPartyMenuItem.Click += ThirdPartyMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(593, 463);
            Controls.Add(BuildGroupBox);
            Controls.Add(IconPreviewPictureBox);
            Controls.Add(MetadataGroupBox);
            Controls.Add(OptionsGroupBox);
            Controls.Add(PythonGroupBox);
            Controls.Add(ApplicationGroupBox);
            Controls.Add(MenuBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = MenuBar;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pyrolith";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ApplicationGroupBox.ResumeLayout(false);
            ApplicationGroupBox.PerformLayout();
            PythonGroupBox.ResumeLayout(false);
            PythonGroupBox.PerformLayout();
            OptionsGroupBox.ResumeLayout(false);
            OptionsGroupBox.PerformLayout();
            MetadataGroupBox.ResumeLayout(false);
            MetadataGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)IconPreviewPictureBox).EndInit();
            BuildGroupBox.ResumeLayout(false);
            BuildGroupBox.PerformLayout();
            MenuBar.ResumeLayout(false);
            MenuBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip MenuBar;
        private ToolStripMenuItem ProjectMenuItem;
        private ToolStripMenuItem NewProjectMenuItem;
        private ToolStripMenuItem OpenProjectMenuItem;
        private ToolStripMenuItem SaveProjectMenuItem;
        private ToolStripMenuItem SaveProjectAsMenuItem;
        private ToolStripMenuItem InfoMenuItem;
        private ToolStripMenuItem AboutMenuItem;
        private ToolStripMenuItem ThirdPartyMenuItem;
        private ToolTip HintToolTip;

        private GroupBox PythonGroupBox;
        private Label PythonVersionLabel;
        private ComboBox PythonVersionComboBox;
        private Label ArchitectureLabel;
        private ComboBox ArchitectureComboBox;

        private GroupBox ApplicationGroupBox;
        private Label AppDirectoryLabel;
        private TextBox AppDirectoryTextBox;
        private Button BrowseAppDirectoryButton;
        private Label EntryScriptLabel;
        private TextBox EntryScriptTextBox;
        private Button BrowseEntryScriptButton;
        private Label RequirementsLabel;
        private TextBox RequirementsTextBox;
        private Button BrowseRequirementsButton;
        private Button ClearRequirementsButton;
        private Label ArgumentsLabel;
        private TextBox ArgumentsTextBox;
        private TextBox CommandPreviewTextBox;

        private GroupBox OptionsGroupBox;
        private CheckBox GuiAppCheckBox;
        private CheckBox PasswordCheckBox;
        private TextBox PasswordTextBox;

        private GroupBox MetadataGroupBox;
        private Label ProductNameLabel;
        private TextBox ProductNameTextBox;
        private Label DescriptionLabel;
        private TextBox DescriptionTextBox;
        private Label AuthorLabel;
        private TextBox AuthorTextBox;
        private Label IconLabel;
        private TextBox IconPathTextBox;
        private Button BrowseIconButton;
        private Button ClearIconButton;
        private PictureBox IconPreviewPictureBox;

        private GroupBox BuildGroupBox;
        private Label OutputLabel;
        private TextBox OutputPathTextBox;
        private Button BrowseOutputButton;
        private Button BuildButton;
        private ProgressBar BuildProgressBar;
        private Label StatusLabel;
    }
}
