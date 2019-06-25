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
using NLog.Targets.Wrappers;

namespace Blish_HUD {
    public class DebugService:GameService {

        private static LoggingConfiguration _logConfiguration;

        private static readonly Lazy<Logger> Logger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);

        internal static void InitDebug() {
            // Make sure crash dir is available for logs as early as possible
            string logPath = DirectoryUtil.RegisterDirectory("logs");

            // Init the Logger
            _logConfiguration = new LoggingConfiguration();

            #if DEBUG
            LogManager.ThrowExceptions = true;
#endif

            const string HEADER_LAYOUT = @"Blish HUD v${assembly-version:type=Assembly}";
            const string STANDARD_LAYOUT = @"${time:invariant=true}|${level:uppercase=true}|${logger}|${message}";
            const string ERROR_LAYOUT    = @"${message}${onexception:EXCEPTION OCCURRED\:${exception:format=type,message,method:maxInnerExceptionLevel=5:innerFormat=shortType,message,method}}";

            var logDebug = new MethodCallTarget("logdebug") {
                ClassName  = typeof(LogUtil).AssemblyQualifiedName,
                MethodName = nameof(LogUtil.TargetDebug),
                Parameters = {
                    new MethodCallParameter("${time:invariant=true}"),
                    new MethodCallParameter("${level}"),
                    new MethodCallParameter("${logger}"),
                    new MethodCallParameter("${message}")
                }
            };

            var logFile = new FileTarget("logfile") {
                Header            = HEADER_LAYOUT,
                FileNameKind      = FilePathKind.Absolute,
                ArchiveFileKind   = FilePathKind.Absolute,
                FileName          = Path.Combine(logPath, "blishhud.${cached:${date:format=yyyyMMdd-HHmm}}.log"),
                ArchiveFileName   = Path.Combine(logPath, "blishhud.{#}.log"),
                ArchiveDateFormat = "yyyyMMdd-HHmm",
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
            _logConfiguration.AddTarget(logDebug);

            _logConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, asyncLogFile);
            _logConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, logDebug);

            _logConfiguration.AddRuleForOneLevel(LogLevel.Error, asyncLogFile, ERROR_LAYOUT);
            _logConfiguration.AddRuleForOneLevel(LogLevel.Error, logDebug,     ERROR_LAYOUT);

            LogManager.Configuration = _logConfiguration;
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
                Logger.Value.Info("{funcName} ran for {$funcTime}.", func, FuncTimes[func]?.LastTime.Milliseconds().Humanize());
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
