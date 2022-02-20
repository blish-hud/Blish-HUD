using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Blish_HUD._Utils {
    public class BrowserUtil {
        /// <summary>
        /// Opens the specified URL in the default web browser.
        /// </summary>
        /// <param name="url">Website URL to open in the default browser.</param>
        /// <remarks>Local files are not allowed.</remarks>
        public static async void OpenInDefaultBrowser(string url) {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                if (uri.IsFile)
                    return;
            }

            // Trim any surrounding quotes and spaces.
            url = url.Trim().Trim('"').Trim();

            var command = GetDefaultBrowserCommand(url);

            // Default browser not found.
            if (string.IsNullOrEmpty(command)) {
                Process.Start(url); // Fallback for prefixes eg. "discord://https://"
                return;
            }

            var args = CommandLineToArgs(command);
            var exe = args[0];
            var argString = string.Join(" ", args.Skip(1).Select(s => s.Equals("%1") ? s.Replace("%1", url) : s));

            // Run the process.
            var psi = new ProcessStartInfo(exe, argString) {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? Directory.GetCurrentDirectory()
            };
            Process.Start(psi);

            var title = await TryFetchWebPageTitle(url);
            await Task.Delay(200).ContinueWith(t => {
                var proc = GetProcessWithWindowByName(Path.GetFileNameWithoutExtension(psi.FileName), title);
                ForceForegroundWindow(proc?.MainWindowHandle ?? IntPtr.Zero);
            });
        }

        /// <summary>
        /// Gets a process with a window handle that matches the given name and window title.
        /// </summary>
        private static Process GetProcessWithWindowByName(string name, string windowTitle = null) {
            var processes = Process.GetProcessesByName(name).Where(p => !p.MainWindowHandle.Equals(IntPtr.Zero)).ToList();
            if (processes.Count == 0)
                return null;
            return windowTitle != null ? processes.FirstOrDefault(p => p.MainWindowTitle.Contains(windowTitle)) ?? processes[0] : processes[0];
        }

        /// <summary>
        /// Gets the value of the title annotation from the given web page.
        /// </summary>
        private static async Task<string> TryFetchWebPageTitle(string url) {
            var request = WebRequest.Create(url);
            request.UseDefaultCredentials = true;

            return await request.GetResponseAsync().ContinueWith(async t => {
                if (t.IsFaulted) return string.Empty;
                var response = t.Result;
                if (response.Headers.AllKeys.Contains("Content-Type") && response.Headers["Content-Type"].StartsWith("text/html")) {
                    WebClient web = new WebClient {
                        UseDefaultCredentials = true
                    };
                    var page = await web.DownloadStringTaskAsync(url).ContinueWith(t => t.IsFaulted ? string.Empty : t.Result);
                    return new Regex(@"(?<=<title.*>)([\s\S]*)(?=</title>)", RegexOptions.IgnoreCase).Match(page).Value.Trim();
                }
                return string.Empty;
            }).Unwrap();
        }

        /// <summary>
        /// Forces the given window into foreground.
        /// </summary>
        private static void ForceForegroundWindow(IntPtr hWnd) {
            if (hWnd == null || hWnd.Equals(IntPtr.Zero)) return;

            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);

            uint appThread = GetCurrentThreadId();

            if (foreThread != appThread) {
                AttachThreadInput(foreThread, appThread, true); // Disguise as being part of the foreground window.

                BringWindowToTop(hWnd); // We are in a position to demand things now.

                ShowWindow(hWnd, SW_SHOW);

                AttachThreadInput(foreThread, appThread, false);
            } else {
                BringWindowToTop(hWnd);

                ShowWindow(hWnd, SW_SHOW);
            }

            SendMessage(hWnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
        }


        /// <summary>
        /// Queries the registry for the default open browser command for the specified URL in priority order.
        /// </summary>
        private static string GetDefaultBrowserCommand(string url) {
            string protocol = Uri.UriSchemeHttp;

            // Correct the protocol to that in the actual url
            if (Regex.IsMatch(url, "^[a-z]+" + Regex.Escape(Uri.SchemeDelimiter), RegexOptions.IgnoreCase)) {
                int schemeEnd = url.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
                if (schemeEnd > -1)
                    protocol = url.Substring(0, schemeEnd).ToLowerInvariant();
            }

            object userProtocol;

            // Look up user choice translation of protocol to program id
            using (RegistryKey userDefBrowserKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\" + protocol + @"\UserChoice"))
                if (userDefBrowserKey == null || (userProtocol = userDefBrowserKey.GetValue("Progid")) == null)
                    return string.Empty;

            object command;

            // Current User registry
            using (RegistryKey defBrowserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\" + userProtocol + @"\shell\open\command"))
                if (defBrowserKey != null && (command = defBrowserKey.GetValue(null)) != null)
                    return (string)command;

            // Local Machine registry
            using (RegistryKey defBrowserKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\" + userProtocol + @"\shell\open\command"))
                if (defBrowserKey != null && (command = defBrowserKey.GetValue(null)) != null)
                    return (string)command;

            // Root registry
            using (RegistryKey defBrowserKey = Registry.ClassesRoot.OpenSubKey(userProtocol + @"\shell\open\command"))
                if (defBrowserKey != null && (command = defBrowserKey.GetValue(null)) != null)
                    return (string)command;

            return string.Empty;
        }

        /// <summary>
        /// Splits the given command string into its arguments.
        /// </summary>
        private static string[] CommandLineToArgs(string commandLine) {
            var argsPtr = CommandLineToArgvW(commandLine, out var count);
            if (argsPtr == IntPtr.Zero)
                return Array.Empty<string>();
            try {
                var args = new string[count];
                for (var i = 0; i < args.Length; i++) {
                    var p = Marshal.ReadIntPtr(argsPtr, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args;
            } finally {
                Marshal.FreeHGlobal(argsPtr);
            }
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);
        /// <summary>
        /// A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu) or when the user chooses the maximize button, minimize button, restore button, or close button.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
        private static int WM_SYSCOMMAND = 0x0112;
        /// <summary>
        /// Restores the window to its normal position and size.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
        private static int SC_RESTORE = 0xF120;
        /// <summary>
        /// Maximizes the window.
        /// </summary
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
        private const int SC_MAXIMIZE = 0xF030;
        /// <summary>
        /// Minimizes the window.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
        private const int SC_MINIMIZE = 0xF020;
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow"/>
        private const uint SW_SHOW = 5;
    }
}
