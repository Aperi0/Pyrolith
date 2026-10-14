using System.ComponentModel;
using System.Diagnostics;

namespace Pyrolith;

/// <summary>
/// Info → About / Third-Party Libraries. Built in code (no designer file) because it is
/// mostly generated from <see cref="AppInfo.ThirdParty"/>.
/// </summary>
[DesignerCategory("")] // keep Visual Studio from opening an empty designer
internal sealed class AboutForm : Form
{
    public enum Page { About, ThirdParty }

    public static void Open(IWin32Window owner, Page page)
    {
        using var form = new AboutForm(page);
        form.ShowDialog(owner);
    }

    private AboutForm(Page page)
    {
        Text = $"About {AppInfo.Name}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 330);
        Font = SystemFonts.MessageBoxFont ?? Font;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateAboutPage());
        tabs.TabPages.Add(CreateThirdPartyPage());
        tabs.SelectedIndex = page == Page.ThirdParty ? 1 : 0;

        var close = new Button { Text = "Close", DialogResult = DialogResult.OK, Size = new Size(88, 27) };
        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(8)
        };
        bottom.Controls.Add(close);

        Controls.Add(tabs);
        Controls.Add(bottom);
        AcceptButton = CancelButton = close;
    }

    private static TabPage CreateAboutPage()
    {
        var page = new TabPage("About") { Padding = new Padding(16) };
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        var title = new Label
        {
            Text = AppInfo.Name,
            AutoSize = true,
            Font = new Font(page.Font.FontFamily, 16f, FontStyle.Bold)
        };
        var tagline = new Label { Text = AppInfo.Tagline, AutoSize = true, Margin = new Padding(3, 0, 3, 8) };
        var details = new Label
        {
            Text = $"Version:  {AppInfo.Version}\nAuthor:   {AppInfo.Author}",
            AutoSize = true,
            Margin = new Padding(3, 0, 3, 12)
        };
        var description = new Label { Text = AppInfo.Description, AutoSize = true, MaximumSize = new Size(420, 0) };
        var hint = new Label
        {
            Text = "See the Third-Party Libraries tab for the projects Pyrolith is built on.",
            AutoSize = true,
            MaximumSize = new Size(420, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(3, 12, 3, 3)
        };

        layout.Controls.AddRange([title, tagline, details, description, hint]);
        page.Controls.Add(layout);
        return page;
    }

    private static TabPage CreateThirdPartyPage()
    {
        var page = new TabPage("Third-Party Libraries") { Padding = new Padding(12) };
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        layout.Controls.Add(new Label
        {
            Text = "Pyrolith is made possible by these open-source projects:",
            AutoSize = true,
            Margin = new Padding(3, 0, 3, 8)
        });

        foreach (var component in AppInfo.ThirdParty)
        {
            var link = new LinkLabel
            {
                Text = $"{component.Name} {component.Version}  ·  {component.License}",
                AutoSize = true,
                Margin = new Padding(3, 6, 3, 0),
                Font = new Font(page.Font, FontStyle.Bold)
            };
            link.LinkClicked += (_, _) => OpenUrl(component.Url);

            var usage = new Label
            {
                Text = component.UsedFor,
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(3, 0, 3, 4)
            };

            layout.Controls.Add(link);
            layout.Controls.Add(usage);
        }

        page.Controls.Add(layout);
        return page;
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot open the browser.\n{url}\n\n({ex.Message})", "Pyrolith",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
