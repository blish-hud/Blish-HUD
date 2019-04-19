using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Color = System.Drawing.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {

    public class GameIntegrationService : GameService {

        public event EventHandler<EventArgs> OnGw2Closed;
        public event EventHandler<EventArgs> OnGw2Started;

        // How long, in seconds, between each
        // check to see if GW2 is running
        private const int GW2_EXE_CHECKRATE = 15;

        private const string GW2_REGISTRY_KEY = @"SOFTWARE\ArenaNet\Guild Wars 2";
        private const string GW2_REGISTRY_PATH_SV = "Path";

        private const string GW2_64_BIT_PROCESSNAME = "Gw2-64";

        // TODO: Confirm this is actually what the 32-bit process is called
        private const string GW2_32_BIT_PROCESSNAME = "Gw2";

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME = "ArenaNet_Dx_Window_Class";

        public NotifyIcon TrayIcon { get; private set; }
        public ContextMenuStrip TrayIconMenu { get; private set; }

        public bool IsInGame { get; private set; } = false;
        
        private bool _gw2IsRunning = false;
        public bool Gw2IsRunning {
            get => _gw2IsRunning;
            private set {
                if (_gw2IsRunning == value) return;

                _gw2IsRunning = value;

                if (_gw2IsRunning)
                    this.OnGw2Started?.Invoke(this, EventArgs.Empty);
                else
                    this.OnGw2Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private IntPtr Gw2WindowHandle { get; set; }

        private Process _gw2Process;
        public Process Gw2Process {
            get => _gw2Process;
            set {
                if (_gw2Process == value) return;

                _gw2Process = value;

                if (value == null || _gw2Process.MainWindowHandle == IntPtr.Zero) {
                    Overlay.Form.Invoke((MethodInvoker)(() => {
                                                                Overlay.Form.Hide();
                                                            }));

                    _gw2Process = null;
                } else {
                    this.Gw2WindowHandle = _gw2Process.MainWindowHandle;
                }

                this.Gw2IsRunning = _gw2Process != null && Utils.Window.GetClassNameOfWindow(this.Gw2Process.MainWindowHandle) == GW2_GAMEWINDOW_NAME;
            }
        }

        public string Gw2ExecutableLocation { get; private set; }

        protected override void Initialize() {
            // TODO: Split this out into its own function
            using (var gw2Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(GW2_REGISTRY_KEY, RegistryRights.ReadKey)) {
                if (gw2Key != null) {
                    string gw2Path = gw2Key.GetValue(GW2_REGISTRY_PATH_SV).ToString();

                    if (File.Exists(gw2Path)) {
                        this.Gw2ExecutableLocation = gw2Path;
                    }
                }
            }
        }

        private void LaunchGw2(bool autologin = false) {
            if (File.Exists(this.Gw2ExecutableLocation)) {
                var gw2Proc = new Process {
                    StartInfo = {
                        FileName = this.Gw2ExecutableLocation,
                        Arguments = autologin ? "-autologin" : ""
                    }
                };

                gw2Proc.Start();
            }
        }

        protected override void Load() {
            Overlay.Form.Shown += delegate {
                Utils.Window.SetupOverlay(Overlay.WinHandle);

                if (!this.Gw2IsRunning) {
                    Overlay.Form.Hide();
                }

                Overlay.Form.TopMost = true;
            };

            CreateTrayIcon();

            TryAttachToGw2();
        }

        #region TrayIcon Menu Items

        private ToolStripItem ts_launchGw2;
        private ToolStripItem ts_launchGw2Auto;
        private ToolStripItem ts_exit;

        #endregion

        private void CreateTrayIcon() {
            this.TrayIconMenu = new ContextMenuStrip();
            
            // Found this here: https://stackoverflow.com/a/25409865/595437
            // Gross - not sure if there is a nicer way to just pull the application icon...
            this.TrayIcon = new NotifyIcon() {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = "Blish HUD",
                Visible = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            if (!string.IsNullOrEmpty(this.Gw2ExecutableLocation)) {
                ts_launchGw2Auto = this.TrayIconMenu.Items.Add("Launch Guild Wars 2 - Autologin");
                ts_launchGw2     = this.TrayIconMenu.Items.Add("Launch Guild Wars 2");

                ts_launchGw2Auto.Click += delegate { LaunchGw2(false); };
                ts_launchGw2.Click += delegate { LaunchGw2(true); };

                this.TrayIcon.DoubleClick += delegate { LaunchGw2(true); };
            } else {
                var nogw2 = this.TrayIconMenu.Items.Add("Could not locate the gw2 executable!");
                nogw2.Enabled = false;
            }


            this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            ts_exit = this.TrayIconMenu.Items.Add("Close Blish HUD");

            ts_exit.Click += delegate { Overlay.Exit(); };
        }

        // TODO: At some point it would be nice to pull all of this into just Program.cs so that we can dispose of the
        // MonoGame instance when the game is not currently being played (and save the user a lot of memory)
        private void TryAttachToGw2() {
            this.Gw2Process = GetGw2Process();

            if (this.Gw2IsRunning) {
                try {
                    this.Gw2Process.EnableRaisingEvents =  true;
                    this.Gw2Process.Exited              += OnGw2Exit;
                } catch (Win32Exception ex) /* [BLISHHUD-W] */ {
                    // We should probably switch methods of determining when the application has closed
                    // TODO: This needs to catch exceptions nicely - it can *sometimes* glitch out when multiboxing (experienced once)
                    // System.ComponentModel.Win32Exception
                    // Access is denied

                    Console.WriteLine(ex.Message);
                } catch (InvalidOperationException ex) /* [BLISHHUD-1H] */ {
                    // Can get thrown if the game is closed if we just launched it

                    Console.WriteLine(ex.Message);
                }

                Overlay.Form.Invoke((MethodInvoker) (() => {
                                                             Overlay.Form.Show();
                                                         }));
            }
        }

        private Process GetGw2Process() {
            // Check to see if 64-bit Gw2 process is running (since it's likely the most common at this point)
            Process[] gw2Processes = Process.GetProcessesByName(GW2_64_BIT_PROCESSNAME);

            if (!gw2Processes.Any()) {
                // 64-bit process not found so see if they're using a 32-bit client instead
                gw2Processes = Process.GetProcessesByName(GW2_32_BIT_PROCESSNAME);
            }

            if (gw2Processes.Length > 0) {
                // TODO: We don't currently have multibox support, but future updates should at least handle
                // multiboxing in a better way
                return gw2Processes[0];
            }

            return null;
        }

        private void OnGw2Exit(object sender, System.EventArgs e) {
            this.Gw2Process = null;

            // TODO: Close or hide in tray (depending on user settings)
            Console.WriteLine("Guild Wars 2 application has exited!");
        }

        protected override void Unload() {
            if (this.TrayIcon != null) {
                this.TrayIcon.Visible = false;
                this.TrayIcon.Dispose();
            }
        }
        
        // Keeps track of how long it's been since we last checked for the gw2 process
        private double lastGw2Check = 0;

        protected override void Update(GameTime gameTime) {
            // Determine if we are in game or not
            this.IsInGame = !(GameService.Gw2Mumble.TimeSinceTick.TotalSeconds > 0.5); // || gameTime.IsRunningSlowly && GameService.Gw2Mumble.TimeSinceTick.TotalSeconds > 0.5);

            if (this.Gw2IsRunning) {
                Utils.Window.UpdateOverlay(Overlay.WinHandle, this.Gw2WindowHandle);
            } else {
                lastGw2Check += gameTime.ElapsedGameTime.TotalSeconds;
                
                if (lastGw2Check > GW2_EXE_CHECKRATE) {
                    TryAttachToGw2();

                    lastGw2Check = 0;
                }
            }
        }

        public void FocusGw2() {
            if (this.Gw2Process != null) {
                try {
                    var gw2WindowHandle = this.Gw2Process.MainWindowHandle;
                    Utils.Window.SetForegroundWindow(gw2WindowHandle);
                } catch (NullReferenceException ex) {
                    Console.WriteLine("gw2Process.MainWindowHandle > NullReferenceException: Ignored and skipping gw2 focus.");
                    return;
                }
            }
        }

    }

}
