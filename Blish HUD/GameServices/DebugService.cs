﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Sentry;
using Sentry.Protocol;

namespace Blish_HUD {
    public class DebugService:GameService {

        private static Logger Logger;

        private static LoggingConfiguration _logConfiguration;
        // ${message}
        private const string STANDARD_LAYOUT = @"${time:invariant=true}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=toString}${newline}}";

        internal static void InitDebug() {
            // Make sure crash dir is available for logs as early as possible
            string logPath = DirectoryUtil.RegisterDirectory("logs");

            // Init the Logger
            _logConfiguration = new LoggingConfiguration();

            string headerLayout   = $"Blish HUD v{Program.OverlayVersion}";

            var logFile = new FileTarget("logfile") {
                Header            = headerLayout,
                FileNameKind      = FilePathKind.Absolute,
                ArchiveFileKind   = FilePathKind.Absolute,
                FileName          = Path.Combine(logPath, "blishhud.${cached:${date:format=yyyyMMdd-HHmmss}}.log"),
                ArchiveFileName   = Path.Combine(logPath, "blishhud.{#}.log"),
                ArchiveDateFormat = "yyyyMMdd-HHmmss",
                ArchiveNumbering  = ArchiveNumberingMode.Date,
                MaxArchiveFiles   = 9,
                EnableFileDelete  = true,
                CreateDirs        = true,
                Encoding          = Encoding.UTF8,
                KeepFileOpen      = true,
                Layout            = STANDARD_LAYOUT
            };

            var asyncLogFile = new AsyncTargetWrapper("asynclogfile", logFile) {
                QueueLimit        = 200,
                OverflowAction    = AsyncTargetWrapperOverflowAction.Discard,
                ForceLockingQueue = false
            };

            _logConfiguration.AddTarget(asyncLogFile);

            _logConfiguration.AddRule(NLog.LogLevel.Info,  NLog.LogLevel.Fatal,  asyncLogFile);

            AddDebugTarget(_logConfiguration);
            AddSentryTarget(_logConfiguration);

            NLog.LogManager.Configuration = _logConfiguration;

            Logger = Logger.GetLogger(typeof(DebugService));
        }

        public static void TargetDebug(string time, string level, string logger, string message) {
            System.Diagnostics.Debug.WriteLine($"{time}|{level.ToUpper()}|{logger}|{message}");
        }

        [Conditional("DEBUG")]
        private static void AddDebugTarget(LoggingConfiguration logConfig) {
            NLog.LogManager.ThrowExceptions = true;

            var logDebug = new MethodCallTarget("logdebug") {
                ClassName  = typeof(DebugService).AssemblyQualifiedName,
                MethodName = nameof(DebugService.TargetDebug),
                Parameters = {
                    new MethodCallParameter("${time:invariant=true}"),
                    new MethodCallParameter("${level}"),
                    new MethodCallParameter("${logger}"),
                    new MethodCallParameter("${message}")
                }
            };

            _logConfiguration.AddTarget(logDebug);
            _logConfiguration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logDebug);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args) {
            InputService.mouseHook?.UnhookMouse();
            InputService.keyboardHook?.UnhookKeyboard();

            var e = (Exception)args.ExceptionObject;

            Logger.Fatal(e, "Blish HUD encountered a fatal crash!");

            FlushSentry();
        }

        [Conditional("SENTRY")]
        private static void FlushSentry() {
            SentrySdk.Close();
        }

        [Conditional("SENTRY")]
        private static void AddSentryTarget(LoggingConfiguration logConfig) {
            const string SENTRY_DSN = "https://e11516741a32440ca7a72b68d5af93df@sentry.do-ny3.svr.gw2blishhud.com/2";
            const string BREADCRUMB_LAYOUT = "${logger}: ${message}";

            logConfig.AddSentry(sentry => {
                sentry.Dsn              = new Dsn(SENTRY_DSN);
                sentry.Release          = $"blish_hud@{Program.OverlayVersion.Major}.{Program.OverlayVersion.Minor}.{Program.OverlayVersion.Patch}";
                sentry.Environment      = string.IsNullOrEmpty(Program.OverlayVersion.PreRelease) ? "Release" : Program.OverlayVersion.PreRelease;
                sentry.Debug            = true;
                sentry.BreadcrumbLayout = BREADCRUMB_LAYOUT;
                sentry.MaxBreadcrumbs   = 20;

                // We do this ourselves for our other logging
                // It's not working right now, though, for some reason
                //sentry.DisableAppDomainUnhandledExceptionCapture();

                sentry.BeforeBreadcrumb = delegate(Breadcrumb breadcrumb) {
                    string filteredMessage = StringUtil.ReplaceUsingStringComparison(breadcrumb.Message, Environment.UserName, "<filtered-username>", StringComparison.OrdinalIgnoreCase);

                    return new Breadcrumb(filteredMessage, breadcrumb.Type, breadcrumb.Data, breadcrumb.Category, breadcrumb.Level);
                };

                sentry.BeforeSend = delegate(SentryEvent sentryEvent) {
                    sentryEvent.SetTag("locale", CultureInfo.CurrentUICulture.DisplayName);

                    if (!string.IsNullOrEmpty(Program.OverlayVersion.Build)) {
                        sentryEvent.SetTag("Build", Program.OverlayVersion.Build);
                    }

                    try {
                        // Display installed modules
                        if (GameService.Module != null && GameService.Module.Loaded) {
                            var moduleDetails = GameService.Module.Modules.Select(m => new {
                                m.Manifest.Name,
                                m.Manifest.Namespace,
                                Version = m.Manifest.Version.ToString(),
                                m.Enabled
                            });

                            sentryEvent.SetExtra("Modules", moduleDetails.ToArray());
                        }
                    } catch (Exception unknownException) {
                        sentryEvent.SetExtra("Modules", $"Exception: {unknownException.Message}");
                    }

                    return sentryEvent;
                };
            });
        }

        public FrameCounter FrameCounter { get; private set; }

        internal class FuncClock {

            private const int BUFFER_LENGTH = 60;

            public long LastTime { get; private set; }

            public double AverageRuntime {
                get {
                    float totalRuntime = 0;
                    
                    for (int i = 0; i < _timeBuffer.Count - 1; i++) {
                        totalRuntime += _timeBuffer[i];
                    }

                    return totalRuntime / _timeBuffer.Count;
                }
            }

            private readonly List<long> _timeBuffer;
            private readonly Stopwatch _funcStopwatch;

            public FuncClock() {
                _timeBuffer = new List<long>();
                _funcStopwatch = new Stopwatch();
            }

            public void Start() {
                _funcStopwatch.Start();
            }

            public void Stop() {
                _funcStopwatch.Stop();

                if (_timeBuffer.Count > BUFFER_LENGTH) _timeBuffer.RemoveAt(0);

                this.LastTime = _funcStopwatch.ElapsedMilliseconds;
                _timeBuffer.Add(_funcStopwatch.ElapsedMilliseconds);

                _funcStopwatch.Reset();
            }

        }

        internal ConcurrentDictionary<string, FuncClock> _funcTimes;

        [Conditional("DEBUG")]
        public void StartTimeFunc(string func) {
            if (!_funcTimes.ContainsKey(func)) {
                _funcTimes.TryAdd(func, new FuncClock());
            }

            _funcTimes[func]?.Start();
        }

        [Conditional("DEBUG")]
        public void StopTimeFunc(string func) {
            _funcTimes[func]?.Stop();
        }

        [Conditional("DEBUG")]
        public void StopTimeFuncAndOutput(string func) {
            _funcTimes[func]?.Stop();
            Logger.Debug("{funcName} ran for {$funcTime}.", func, _funcTimes[func]?.LastTime.Milliseconds().Humanize());
        }

        protected override void Initialize() {
            this.FrameCounter = new FrameCounter();

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
        }

        protected override void Load() {
            _funcTimes = new ConcurrentDictionary<string, FuncClock>();
        }

        protected override void Update(GameTime gameTime) {
            this.FrameCounter.Update(gameTime.GetElapsedSeconds());
        }

        protected override void Unload() { /* NOOP */ }
    }
}
