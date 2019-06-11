using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Humanizer;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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

        private FrameCounter _frameCounter;
        public FrameCounter FrameCounter => _frameCounter;

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

        public void WriteError(string error, params object[] formatItems) {
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
                    float totalRuntime = 0;
                    
                    for (int i = 0; i < timeBuffer.Count - 1; i++) {
                        totalRuntime += timeBuffer[i];
                    }

                    return totalRuntime / timeBuffer.Count;
                }
            }

            private readonly List<long> timeBuffer;
            private readonly Stopwatch funcStopwatch;

            public FuncClock() {
                timeBuffer = new List<long>();
                funcStopwatch = new Stopwatch();
            }

            public void Start() {
                funcStopwatch.Start();
            }

            public void Stop() {
                funcStopwatch.Stop();

                if (timeBuffer.Count > BUFFER_LENGTH) timeBuffer.RemoveAt(0);

                this.LastTime = funcStopwatch.ElapsedMilliseconds;
                timeBuffer.Add(funcStopwatch.ElapsedMilliseconds);

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
                FuncTimes[func]?.Stop();
            #endif
        }

        public void StopTimeFuncAndOutput(string func) {
            #if DEBUG
                FuncTimes[func]?.Stop();
                Console.WriteLine($"{func} ran for {FuncTimes[func]?.LastTime.Milliseconds().Humanize()}.");
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
        protected override void Initialize() {
            _frameCounter = new FrameCounter();
        }
        protected override void Unload() { /* NOOP */ }

        protected override void Load() {
            // Make sure crash dir is available for logs later on
            System.IO.Directory.CreateDirectory(Path.Combine(GameService.Directory.BasePath, "logs"));

            FuncTimes = new Dictionary<string, FuncClock>();
        }

        protected override void Update(GameTime gameTime) {
            _frameCounter.Update(gameTime.GetElapsedSeconds());
        }
    }
}
