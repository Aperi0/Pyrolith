namespace Pyrolith;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Last line of defence: show unexpected errors instead of the default crash dialog.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatal(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowFatal(e.ExceptionObject as Exception);

        ApplicationConfiguration.Initialize(); // high DPI, visual styles, default font (see .csproj)
        Application.Run(new MainForm());
    }

    private static void ShowFatal(Exception? ex) =>
        MessageBox.Show(
            $"Unexpected error:\n\n{ex?.Message}\n\n{ex?.GetType().FullName}",
            "Pyrolith", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
