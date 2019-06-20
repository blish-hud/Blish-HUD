using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Blish_HUD.Controls;
using Humanizer;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Blish_HUD {
    public class DebugService:GameService {

        private static NLog.Config.LoggingConfiguration _logConfiguration;

        private static NLog.Logger Logger;

        internal static Logger InitDebug(Type returnLoggerType) {
            // Make sure crash dir is available for logs as early as possible
            var logPath = DirectoryUtil.RegisterDirectory("logs");

            // Init the Logger
            _logConfiguration = new LoggingConfiguration();

            var logFile = new FileTarget("logfile") {
                ArchiveFileName = Path.Combine(logPath, "blishhud.{#}.log"),
                ArchiveFileKind = FilePathKind.Absolute,
                FileName = Path.Combine(logPath, "blishhud.log"),
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 9,
                EnableFileDelete = true,
                CreateDirs = true,
                Encoding = Encoding.UTF8,
                KeepFileOpen = true,
            };

            var logConsole = new ConsoleTarget("logconsole");

            _logConfiguration.AddTarget(logFile);
            _logConfiguration.AddTarget(logConsole);

            _logConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            _logConfiguration.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);

            LogManager.Configuration = _logConfiguration;

            Logger = LogManager.GetCurrentClassLogger();

            return NLog.LogManager.GetCurrentClassLogger(returnLoggerType);
        }

        public FrameCounter FrameCounter { get; private set; }

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
                Logger.Info("{funcName} ran for {$funcTime}.", func, FuncTimes[func]?.LastTime.Milliseconds().Humanize());
            #endif
        }

        protected override void Initialize() {
            this.FrameCounter = new FrameCounter();
        }
        protected override void Unload() { /* NOOP */ }

        protected override void Load() {
            FuncTimes = new Dictionary<string, FuncClock>();
        }

        protected override void Update(GameTime gameTime) {
            this.FrameCounter.Update(gameTime.GetElapsedSeconds());
        }
    }
}
