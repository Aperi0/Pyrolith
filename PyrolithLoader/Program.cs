using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace PyrolithLoader;

/// <summary>
/// Native-AOT stub that becomes every executable produced by Pyrolith.
/// At runtime it:
///   1. finds the ZIP payload appended to its own file,
///   2. asks for a password if the payload is encrypted,
///   3. extracts it to %TEMP%\PyrolithRun_&lt;guid&gt;,
///   4. runs the command stored in run.pyex and waits for it,
///   5. deletes the temporary folder and returns Python's exit code.
/// No WinForms / no runtime dependency - the password dialog is plain Win32.
/// </summary>
internal static class Program
{
    private const string RunFileName = "run.pyex";
    private const int MaxPasswordAttempts = 3;

    [STAThread]
    private static int Main(string[] args)
    {
        string? exePath = Environment.ProcessPath;
        if (exePath is null)
        {
            Native.ErrorBox("Cannot determine the path of this executable.", "Critical Error");
            return 1;
        }

        string tempDir = Path.Combine(Path.GetTempPath(), "PyrolithRun_" + Guid.NewGuid().ToString("N"));

        try
        {
            if (!ExtractPayload(exePath, tempDir))
                return 1; // cancelled or wrong password

            return RunApplication(tempDir, exePath, args);
        }
        catch (Exception ex)
        {
            Native.ErrorBox($"Error: {ex.Message}", "Critical Error");
            return 1;
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    /// <summary>Extracts the payload; returns false when the user cancels or runs out of password attempts.</summary>
    private static bool ExtractPayload(string exePath, string destination)
    {
        bool isEncrypted;
        using (var probeStream = AppendedZip.Open(exePath))
        using (var probe = ZipArchive.OpenArchive(probeStream))
        {
            isEncrypted = probe.Entries.Any(e => e.IsEncrypted);
        }

        if (!isEncrypted)
        {
            Extract(exePath, destination, new ReaderOptions());
            return true;
        }

        // Wrong passwords are only detected while decrypting, so retry the whole extraction.
        for (int attempt = 1; attempt <= MaxPasswordAttempts; attempt++)
        {
            string? password = PasswordPrompt.Show();
            if (string.IsNullOrEmpty(password))
                return false;

            try
            {
                Extract(exePath, destination, new ReaderOptions { Password = password });
                return true;
            }
            catch (Exception ex) when (IsWrongPassword(ex))
            {
                TryDeleteDirectory(destination); // remove partially extracted files
                Native.ErrorBox(
                    attempt < MaxPasswordAttempts ? "Invalid password. Try again." : "Invalid password.",
                    "Error");
            }
        }
        return false;
    }

    /// <summary>
    /// SharpCompress reports a wrong WinZip-AES password as InvalidFormatException("bad password"),
    /// and a wrong ZipCrypto password as CryptographicException.
    /// </summary>
    private static bool IsWrongPassword(Exception ex) =>
        ex is CryptographicException
        || (ex is InvalidFormatException && ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase));

    private static void Extract(string exePath, string destination, ReaderOptions readerOptions)
    {
        using var zipStream = AppendedZip.Open(exePath);
        using var archive = ZipArchive.OpenArchive(zipStream, readerOptions);

        Directory.CreateDirectory(destination);
        var options = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };

        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            entry.WriteToDirectory(destination, options);
    }

    /// <summary>
    /// Runs "python[w].exe "main\script.py" args" from run.pyex inside the extracted folder.
    /// Arguments given to the .exe itself are appended, so the app sees them in sys.argv.
    /// </summary>
    private static int RunApplication(string tempDir, string exePath, string[] args)
    {
        string runFile = Path.Combine(tempDir, RunFileName);
        if (!File.Exists(runFile))
        {
            Native.ErrorBox($"Cannot run the application: the package is damaged (missing {RunFileName}).", "Critical Error");
            return 1;
        }

        string command = File.ReadAllText(runFile).Trim();

        // Format: "<interpreter> <arguments>" - the interpreter name never contains spaces.
        int split = command.IndexOf(' ');
        string interpreter = split > 0 ? command[..split] : command;
        string arguments = split > 0 ? command[(split + 1)..] : string.Empty;

        if (args.Length > 0)
            arguments += " " + string.Join(" ", args.Select(QuoteArgument));

        bool isConsoleApp = !interpreter.Equals("pythonw.exe", StringComparison.OrdinalIgnoreCase);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(tempDir, interpreter),
            Arguments = arguments,
            WorkingDirectory = tempDir,
            UseShellExecute = false,
            // The loader is a GUI-subsystem exe without a console, so python.exe gets a fresh console window.
            CreateNoWindow = !isConsoleApp
        };

        // Isolate the bundled interpreter from any Python installed on this machine.
        startInfo.Environment.Remove("PYTHONHOME");
        startInfo.Environment.Remove("PYTHONPATH");
        startInfo.Environment["PYTHONNOUSERSITE"] = "1";
        // sys.argv[0] points into %TEMP%; this tells the app where the real .exe is.
        startInfo.Environment["PYROLITH_EXECUTABLE"] = exePath;

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Cannot start {interpreter}.");
        process.WaitForExit();
        return process.ExitCode;
    }

    /// <summary>Quotes one argument following the CommandLineToArgvW rules.</summary>
    private static string QuoteArgument(string arg)
    {
        if (arg.Length > 0 && arg.IndexOfAny([' ', '\t', '\n', '\v', '"']) < 0)
            return arg;

        var sb = new StringBuilder("\"");
        int backslashes = 0;
        foreach (char c in arg)
        {
            if (c == '\\') { backslashes++; continue; }

            // Backslashes are literal unless they precede a quote.
            sb.Append('\\', c == '"' ? backslashes * 2 + 1 : backslashes);
            sb.Append(c);
            backslashes = 0;
        }
        sb.Append('\\', backslashes * 2).Append('"');
        return sb.ToString();
    }

    /// <summary>Python may leave child processes behind that still hold files - retry briefly.</summary>
    private static void TryDeleteDirectory(string path)
    {
        for (int i = 0; i < 5 && Directory.Exists(path); i++)
        {
            try { Directory.Delete(path, recursive: true); return; }
            catch { Thread.Sleep(200); }
        }
    }
}

/// <summary>
/// Locates a ZIP archive appended to the end of a file (here: the loader exe itself).
/// The ZIP offsets are relative to the archive start, so we compute that start from the
/// End Of Central Directory record and expose the tail of the file as a stream.
/// </summary>
internal static class AppendedZip
{
    private const int EocdSize = 22;                // EOCD record without comment
    private const int MaxEocdSearch = EocdSize + 0xFFFF; // + max comment length

    public static Stream Open(string path)
    {
        var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        try
        {
            return new OffsetStream(fs, FindZipStart(fs));
        }
        catch
        {
            fs.Dispose();
            throw;
        }
    }

    private static long FindZipStart(Stream fs)
    {
        long fileLength = fs.Length;
        int window = (int)Math.Min(fileLength, MaxEocdSearch);

        byte[] buffer = new byte[window];
        fs.Seek(fileLength - window, SeekOrigin.Begin);
        fs.ReadExactly(buffer, 0, window);

        // Scan backwards for the EOCD signature "PK\x05\x06".
        int eocd = -1;
        for (int i = window - EocdSize; i >= 0; i--)
        {
            if (buffer[i] == 0x50 && buffer[i + 1] == 0x4B && buffer[i + 2] == 0x05 && buffer[i + 3] == 0x06)
            {
                eocd = i;
                break;
            }
        }
        if (eocd < 0)
            throw new InvalidDataException("No application payload found - this executable was not built by Pyrolith or is damaged.");

        uint centralDirSize = BitConverter.ToUInt32(buffer, eocd + 12);
        uint centralDirOffset = BitConverter.ToUInt32(buffer, eocd + 16); // relative to the ZIP start

        if (centralDirSize == 0xFFFFFFFF || centralDirOffset == 0xFFFFFFFF)
            throw new NotSupportedException("Zip64 payloads are not supported (package larger than 4 GB).");

        long eocdPosition = fileLength - window + eocd;
        long centralDirStart = eocdPosition - centralDirSize;   // the central directory sits right before EOCD
        long zipStart = centralDirStart - centralDirOffset;

        return Math.Max(zipStart, 0);
    }
}

/// <summary>Read-only view of a stream that starts at <c>offset</c> (position 0 = ZIP start).</summary>
internal sealed class OffsetStream : Stream
{
    private readonly Stream _base;
    private readonly long _offset;

    public OffsetStream(Stream baseStream, long offset)
    {
        _base = baseStream;
        _offset = offset;
        _base.Position = offset;
    }

    public override bool CanRead => _base.CanRead;
    public override bool CanSeek => _base.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _base.Length - _offset;

    public override long Position
    {
        get => _base.Position - _offset;
        set => _base.Position = value + _offset;
    }

    public override int Read(byte[] buffer, int offset, int count) => _base.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
    {
        long target = origin switch
        {
            SeekOrigin.Begin => _offset + offset,
            SeekOrigin.Current => _base.Position + offset,
            SeekOrigin.End => _base.Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        _base.Position = Math.Max(target, _offset); // never expose the loader part
        return _base.Position - _offset;
    }

    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing) _base.Dispose();
        base.Dispose(disposing);
    }
}

/// <summary>
/// Minimal modal password window built with raw Win32 calls (WinForms is not AOT-friendly
/// and would add megabytes to every generated executable).
/// </summary>
internal static unsafe class PasswordPrompt
{
    private const string WindowClass = "PyrolithPasswordDialog";

    private const int IDOK = 1;
    private const int IDCANCEL = 2;
    private const int IDEDIT = 100;

    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_SYSMENU = 0x00080000;

    private const uint ES_PASSWORD = 0x0020;
    private const uint ES_AUTOHSCROLL = 0x0080;
    private const uint BS_DEFPUSHBUTTON = 0x0001;

    private const uint WS_EX_CONTROLPARENT = 0x00010000;
    private const uint WS_EX_DLGMODALFRAME = 0x00000001;

    private const uint WM_DESTROY = 0x0002;
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_COMMAND = 0x0111;
    private const uint WM_SETFONT = 0x0030;

    private const int SW_SHOW = 5;
    private const int DEFAULT_GUI_FONT = 17;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    // State shared with the static WndProc (only one dialog exists at a time).
    private static IntPtr _hEdit;
    private static string? _password;
    private static bool _ok;
    private static bool _classRegistered;

    /// <summary>Shows the dialog; returns the password or null when cancelled.</summary>
    public static string? Show()
    {
        _password = null;
        _ok = false;

        IntPtr hInst = Native.GetModuleHandleW(null);
        RegisterWindowClass(hInst);

        int width = 360, height = 190;
        int x = (Native.GetSystemMetrics(SM_CXSCREEN) - width) / 2;
        int y = (Native.GetSystemMetrics(SM_CYSCREEN) - height) / 2;

        IntPtr hMain = Native.CreateWindowExW(
            WS_EX_CONTROLPARENT | WS_EX_DLGMODALFRAME,
            WindowClass, "Password Required",
            WS_CAPTION | WS_SYSMENU,
            x, y, width, height,
            IntPtr.Zero, IntPtr.Zero, hInst, IntPtr.Zero);

        IntPtr hFont = Native.GetStockObject(DEFAULT_GUI_FONT);

        IntPtr hLabel = Native.CreateWindowExW(0, "STATIC",
            "This application is protected. Please enter the password:",
            WS_CHILD | WS_VISIBLE,
            15, 15, 320, 36, hMain, IntPtr.Zero, hInst, IntPtr.Zero);

        _hEdit = Native.CreateWindowExW(0, "EDIT", null,
            WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP | ES_PASSWORD | ES_AUTOHSCROLL,
            15, 58, 315, 24, hMain, IDEDIT, hInst, IntPtr.Zero);

        IntPtr hOk = Native.CreateWindowExW(0, "BUTTON", "Run",
            WS_CHILD | WS_VISIBLE | WS_TABSTOP | BS_DEFPUSHBUTTON,
            160, 105, 80, 28, hMain, IDOK, hInst, IntPtr.Zero);

        IntPtr hCancel = Native.CreateWindowExW(0, "BUTTON", "Cancel",
            WS_CHILD | WS_VISIBLE | WS_TABSTOP,
            250, 105, 80, 28, hMain, IDCANCEL, hInst, IntPtr.Zero);

        foreach (var control in new[] { hLabel, _hEdit, hOk, hCancel })
            Native.SendMessageW(control, WM_SETFONT, hFont, 1);

        Native.ShowWindow(hMain, SW_SHOW);
        Native.SetFocus(_hEdit);

        // Message loop; IsDialogMessageW provides Tab / Enter / Esc handling.
        int result;
        while ((result = Native.GetMessageW(out Native.MSG msg, IntPtr.Zero, 0, 0)) != 0)
        {
            if (result == -1)
                break;
            if (Native.IsDialogMessageW(hMain, ref msg) == 0)
            {
                Native.TranslateMessage(ref msg);
                Native.DispatchMessageW(ref msg);
            }
        }

        return _ok ? _password : null;
    }

    private static void RegisterWindowClass(IntPtr hInst)
    {
        if (_classRegistered) return; // the prompt can be shown several times (wrong password)

        // The class name must stay alive while the class is registered - i.e. for the process lifetime.
        IntPtr className = Marshal.StringToHGlobalUni(WindowClass);
        var wc = new Native.WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<Native.WNDCLASSEXW>(),
            lpfnWndProc = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, IntPtr, IntPtr>)&WndProc,
            hInstance = hInst,
            hCursor = Native.LoadCursorW(IntPtr.Zero, 32512), // IDC_ARROW
            hbrBackground = 16,                               // COLOR_BTNFACE + 1
            lpszClassName = className
        };
        Native.RegisterClassExW(in wc);
        _classRegistered = true;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_COMMAND:
                int id = (int)(wParam.ToInt64() & 0xFFFF); // LOWORD = control id
                if (id == IDOK)
                {
                    _password = GetText(_hEdit);
                    _ok = true;
                    Native.DestroyWindow(hWnd);
                    return IntPtr.Zero;
                }
                if (id == IDCANCEL)
                {
                    _ok = false;
                    Native.DestroyWindow(hWnd);
                    return IntPtr.Zero;
                }
                break;

            case WM_CLOSE:
                Native.DestroyWindow(hWnd);
                return IntPtr.Zero;

            case WM_DESTROY:
                Native.PostQuitMessage(0); // ends the message loop in Show()
                return IntPtr.Zero;
        }
        return Native.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    private static string GetText(IntPtr hWnd)
    {
        int length = Native.GetWindowTextLengthW(hWnd);
        if (length <= 0)
            return string.Empty;

        Span<char> buffer = length < 256 ? stackalloc char[length + 1] : new char[length + 1];
        int written;
        fixed (char* p = buffer)
        {
            written = Native.GetWindowTextW(hWnd, p, length + 1);
        }
        return new string(buffer[..written]);
    }
}

/// <summary>P/Invoke declarations (source-generated, AOT-safe).</summary>
internal static unsafe partial class Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public IntPtr lpszMenuName;
        public IntPtr lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    private const uint MB_OK_ICONERROR = 0x00000010;

    public static void ErrorBox(string text, string caption) =>
        MessageBoxW(IntPtr.Zero, text, caption, MB_OK_ICONERROR);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr GetModuleHandleW(string? lpModuleName);

    [LibraryImport("user32.dll")]
    public static partial ushort RegisterClassExW(in WNDCLASSEXW lpwcx);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr CreateWindowExW(
        uint dwExStyle, string? lpClassName, string? lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [LibraryImport("user32.dll")]
    public static partial IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    public static partial int DestroyWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("user32.dll")]
    public static partial int ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    public static partial IntPtr SetFocus(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial IntPtr SendMessageW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    public static partial int GetWindowTextLengthW(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial int GetWindowTextW(IntPtr hWnd, char* lpString, int nMaxCount);

    [LibraryImport("user32.dll")]
    public static partial int GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport("user32.dll")]
    public static partial int IsDialogMessageW(IntPtr hDlg, ref MSG lpMsg);

    [LibraryImport("user32.dll")]
    public static partial int TranslateMessage(ref MSG lpMsg);

    [LibraryImport("user32.dll")]
    public static partial IntPtr DispatchMessageW(ref MSG lpMsg);

    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetrics(int nIndex);

    [LibraryImport("user32.dll")]
    public static partial IntPtr LoadCursorW(IntPtr hInstance, IntPtr lpCursorName);

    [LibraryImport("gdi32.dll")]
    public static partial IntPtr GetStockObject(int i);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
