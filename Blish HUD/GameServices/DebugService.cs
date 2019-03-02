using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD {
    public class DebugService:GameService {

        private class Alert {

            public readonly string ServiceTitle;
            public readonly string AlertMessage;
            public DateTimeOffset Expires;

            public Alert(string source, string message, DateTimeOffset expires) {
                ServiceTitle = source;
                AlertMessage = message;
                Expires = expires;
            }

        }

        private Controls.Tooltip alertTooltip;
        private Controls.Image alertIcon;

        private bool alertInvalid = false;

        private List<Alert> Alerts = new List<Alert>();
        private Dictionary<Alert, Panel> DisplayedAlerts = new Dictionary<Alert, Panel>();

        public void WriteInfo(string info, params string[] formatItems) {
            Console.Write("INFO: " + string.Format(info, formatItems));
        }

        public void WriteInfoLine(string info, params string[] formatItems) {
            WriteInfo(info + Environment.NewLine, formatItems);
        }

        public void WriteWarning(string warning, params string[] formatItems) {
            Console.Write("WARNING: " + string.Format(warning, formatItems));
        }

        public void WriteWarningLine(string warning, params string[] formatItems) {
            WriteWarning(warning + Environment.NewLine, formatItems);
        }

        public void WriteError(string error, params string[] formatItems) {
            Console.Write("ERROR: " + string.Format(error, formatItems));
        }

        public void WriteErrorLine(string error, params string[] formatItems) {
            WriteError(error + Environment.NewLine, formatItems);
        }

        public class FuncClock {

            private const int BUFFER_LENGTH = 60;

            public long LastTime { get; private set; }

            public double AverageRuntime {
                get {
                    return timeBuffer.Sum(i => i);
                }
            }

            private readonly Queue<long> timeBuffer;
            private readonly Stopwatch funcStopwatch;

            public FuncClock() {
                timeBuffer = new Queue<long>();
                funcStopwatch = new Stopwatch();
            }

            public void Start() {
                funcStopwatch.Start();
            }

            public void Stop() {
                funcStopwatch.Stop();

                if (timeBuffer.Count > BUFFER_LENGTH) timeBuffer.Dequeue();

                this.LastTime = funcStopwatch.ElapsedMilliseconds;
                timeBuffer.Enqueue(funcStopwatch.ElapsedMilliseconds);

                funcStopwatch.Reset();
            }

        }

        public Dictionary<string, FuncClock> FuncTimes;
        public void StartTimeFunc(string func) {
#if DEBUG
            if (!FuncTimes.ContainsKey(func))
                FuncTimes.Add(func, new FuncClock());

            FuncTimes[func].Start();
#endif
        }

        public void StopTimeFunc(string func) {
#if DEBUG
            if (FuncTimes.ContainsKey(func))
                FuncTimes[func].Stop();
#endif
        }

        public void DisplayAlert(string source, string message, DateTimeOffset expiration) {
            WriteWarningLine($"[{source}] {message}");

            // if the alert already exists, just extend the expiration on the existing alert
            foreach (var alert in Alerts) {
                if (alert.ServiceTitle == source && alert.AlertMessage == message) {
                    alert.Expires = expiration;
                    return;
                }
            }

            Alerts.Add(new Alert(source, message, expiration));
        }

        // TODO: Debug service needs to be fleshed out more
        protected override void Initialize() { /* NOOP */ }
        protected override void Unload() { /* NOOP */ }

        protected override void Load() {
            // Make sure crash dir is available for logs later on
            Directory.CreateDirectory(Path.Combine(GameService.FileSrv.BasePath, "logs"));

            FuncTimes = new Dictionary<string, FuncClock>();

            /*
            alertIcon = new Image(Content.GetTexture("1444522"));
            alertTooltip = new Tooltip();

            alertIcon.Location = new Point(Graphics.WindowWidth - 128 - alertIcon.Width, Graphics.WindowHeight - 64 - alertIcon.Height);
            alertIcon.Tooltip = alertTooltip;
            alertIcon.Parent = Graphics.SpriteScreen;
            alertIcon.ZIndex = Screen.TOOLTIP_BASEZINDEX - 1;

            Graphics.SpriteScreen.OnResized += delegate { alertIcon.Location = new Point(Graphics.WindowWidth - 128 - alertIcon.Width, Graphics.WindowHeight - 64 - alertIcon.Height); };

            alertIcon.OnLeftMouseButtonReleased += delegate {
                DisplayAlert("Debug Service", "This is a test error - not a real error.", DateTimeOffset.Now.AddSeconds(20));
            };
            */
        }

        protected override void Update(GameTime gameTime) {
            /*
            foreach (Alert alert in Alerts.ToList()) {
                // Check if the alert has expired
                if (alert.Expires < DateTimeOffset.Now) {
                    Alerts.Remove(alert);

                    // Remove the alert panel if it has expired
                    if (DisplayedAlerts.ContainsKey(alert)) {
                        DisplayedAlerts[alert].Dispose();
                        DisplayedAlerts.Remove(alert);

                        alertInvalid = true;
                    }

                    continue;
                }

                Panel alertPanel;
                int topPos = 0;

                // Add alert to tooltip, if it isn't in there already
                if (!DisplayedAlerts.ContainsKey(alert)) {
                    alertPanel = new Panel() { Parent = alertTooltip };

                    Label ServiceLbl = new Label() {
                        Text           = alert.ServiceTitle,
                        AutoSizeWidth  = true,
                        AutoSizeHeight = true,
                        Location       = new Point(Tooltip.PADDING),
                        Parent         = alertPanel
                    };

                    Label MessageLbl = new Label() {
                        Text           = alert.AlertMessage,
                        ShowShadow = true,
                        AutoSizeWidth  = true,
                        AutoSizeHeight = true,
                        Location       = new Point(Tooltip.PADDING, ServiceLbl.Bottom + 5),
                        Parent         = alertPanel
                    };

                    alertPanel.Size = new Point(
                                                alertPanel.Children.Max(c => c.Right),
                                                alertPanel.Children.Max(c => c.Bottom)
                                                );

                    alertInvalid = true;
                } else {
                    alertPanel = DisplayedAlerts[alert];
                }

                if (alertInvalid) {
                    // Update layout of alert panels
                    alertPanel.Location = new Point(0, topPos);
                }

                topPos += alertPanel.Height;
            }

            alertInvalid = false;
            */
        }
    }
}
