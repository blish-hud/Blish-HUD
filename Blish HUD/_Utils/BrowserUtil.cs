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
        /// Opens url in the default web browser.
        /// </summary>
        /// <param name="url">Website URL to open in the default browser.</param>
        /// <returns><see langword="True"/> if the default browser was opened; Otherwise <see langword="false"/>.</returns>
        /// <remarks>Local files are not allowed.</remarks>
        public static async Task<bool> OpenInDefaultBrowser(string url) {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                if (uri.IsFile)
                    return false;
            }

            // Trim any surrounding quotes and spaces.
            url = url.Trim().Trim('"').Trim();

            string protocol = Uri.UriSchemeHttp;

            // Correct the protocol to that in the actual url
            if (Regex.IsMatch(url, "^[a-z]+" + Regex.Escape(Uri.SchemeDelimiter), RegexOptions.IgnoreCase)) {
                int schemeEnd = url.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
                if (schemeEnd > -1)
                    protocol = url.Substring(0, schemeEnd).ToLowerInvariant();
            }

            object fetchedVal;
            string defBrowser = null;

            // Look up user choice translation of protocol to program id
            using (RegistryKey userDefBrowserKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\" + protocol + @"\UserChoice"))
                if (userDefBrowserKey != null && (fetchedVal = userDefBrowserKey.GetValue("Progid")) != null)
                    // Programs are looked up the same way as protocols in the later code, so we just overwrite the protocol variable.
                    protocol = fetchedVal as string;

            // Look up protocol (or programId from UserChoice) in the registry, in priority order.
            // Current User registry
            using (RegistryKey defBrowserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\" + protocol + @"\shell\open\command"))
                if (defBrowserKey != null && (fetchedVal = defBrowserKey.GetValue(null)) != null)
                    defBrowser = fetchedVal as string;

            // Local Machine registry
            if (defBrowser == null)
                using (RegistryKey defBrowserKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\" + protocol + @"\shell\open\command"))
                    if (defBrowserKey != null && (fetchedVal = defBrowserKey.GetValue(null)) != null)
                        defBrowser = fetchedVal as string;

            // Root registry
            if (defBrowser == null)
                using (RegistryKey defBrowserKey = Registry.ClassesRoot.OpenSubKey(protocol + @"\shell\open\command"))
                    if (defBrowserKey != null && (fetchedVal = defBrowserKey.GetValue(null)) != null)
                        defBrowser = fetchedVal as string;

            // Default browser not found.
            if (string.IsNullOrEmpty(defBrowser))
                return false;

            string defBrowserProcess;

            #region Process Value of Key "..\shell\open\command"
            // Preprocess registered ..\shell\open\command.
            bool hasArg = false;
            if (defBrowser.Contains("%1")) {
                // If url in the command line is surrounded by quotes, ignore those.
                if (defBrowser.Contains("\"%1\""))
                    defBrowser = defBrowser.Replace("\"%1\"", url);
                else
                    defBrowser = defBrowser.Replace("%1", url);
                hasArg = true;
            }
            int spIndex;

            // Fetch executable.
            if (defBrowser[0] == '"')
                defBrowserProcess = defBrowser.Substring(0, defBrowser.IndexOf('"', 1) + 2).Trim();
            else if ((spIndex = defBrowser.IndexOf(" ", StringComparison.Ordinal)) > -1)
                defBrowserProcess = defBrowser.Substring(0, spIndex).Trim();
            else
                defBrowserProcess = defBrowser;

            // Fetch arguments.
            string defBrowserArgs = defBrowser.Substring(defBrowserProcess.Length).TrimStart();

            if (!hasArg) {
                if (defBrowserArgs.Length > 0)
                    defBrowserArgs += " ";
                defBrowserArgs += url;
            }

            // Postprocess
            defBrowserProcess = defBrowserProcess.Trim('"');
            #endregion

            return await Task.Run(async () => {
                // Run the process.
                ProcessStartInfo psi = new ProcessStartInfo(defBrowserProcess, defBrowserArgs);
                psi.WorkingDirectory = Path.GetDirectoryName(defBrowserProcess);
                Process.Start(psi);

                // Bring browser to foreground.
                var title = await TryFetchWebPageTitle(url);
                await Task.Delay(200).ContinueWith(t => {
                    var proc = GetProcessByName(Path.GetFileNameWithoutExtension(psi.FileName), title);
                    ForceForegroundWindow(proc != null ? proc.MainWindowHandle : IntPtr.Zero);
                });

                return true;
            });
        }

        private static Process GetProcessByName(string name, string windowTitle = null) {
            var processes = Process.GetProcessesByName(name);
            if (processes.Length == 0)
                return null;
            else
                return windowTitle != null ? processes.FirstOrDefault(p => p.MainWindowTitle.ToLowerInvariant().Contains(windowTitle.ToLowerInvariant())) : processes[0];
        }

        private static async Task<string> TryFetchWebPageTitle(string url) {
            // Create a request to the url
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            // If the request wasn't an HTTP request (like a file), ignore it
            if (request == null) return string.Empty;

            // Use the user's credentials
            request.UseDefaultCredentials = true;

            // Obtain a response from the server, if there was an error, return nothing

            return await request.GetResponseAsync().ContinueWith(t => {
                if (t.IsFaulted) return string.Empty;

                var response = t.Result;

                string regex = @"(?<=<title.*>)([\s\S]*)(?=</title>)";

                // If the correct HTML header exists for HTML text, continue
                if (response.Headers.AllKeys.Contains("Content-Type") && response.Headers["Content-Type"].StartsWith("text/html")) {
                    // Download the page
                    WebClient web = new WebClient();
                    web.UseDefaultCredentials = true;
                    string page = web.DownloadString(url);
                    // Extract the title
                    Regex ex = new Regex(regex, RegexOptions.IgnoreCase);
                    return ex.Match(page).Value.Trim();
                }
                return string.Empty;
            });
        }

        private static void ForceForegroundWindow(IntPtr hWnd) {

            if (hWnd == null || hWnd.Equals(IntPtr.Zero)) return;

            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);

            uint appThread = GetCurrentThreadId();

            if (foreThread != appThread) {
                AttachThreadInput(foreThread, appThread, true); // Make sure we are taken serious.

                BringWindowToTop(hWnd); // We are in a position to demand things now.

                ShowWindow(hWnd, SW_SHOW);

                AttachThreadInput(foreThread, appThread, false); // Relax again.
            } else {
                BringWindowToTop(hWnd);

                ShowWindow(hWnd, SW_SHOW);
            }
            //SendMessage(hWnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0); // We could play around with the sizing of the window here.
        }

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(HandleRef hWnd);

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
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow"/>
        private const uint SW_SHOW = 5;
    }
}
