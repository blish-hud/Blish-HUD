using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Blish_HUD.Debug;
using Blish_HUD.Settings;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Blish_HUD {

    public class DebugService : GameService {

        private const string DEBUG_SETTINGS = "DebugConfiguration";

        internal SettingCollection _debugSettings;
        public SettingCollection DebugSettings => _debugSettings;
        public SettingEntry<bool> EnableDebugLogging { get; private set; }
        public SettingEntry<bool> EnableFPSDisplay { get; private set; }
        public SettingEntry<bool> EnableAdditionalDebugDisplay { get; private set; }

        #region Logging

        private static Logger Logger;

        private static LoggingConfiguration _logConfiguration;

        private const string STRUCLOG_TIME      = "${time:invariant=true}";
        private const string STRUCLOG_LEVEL     = "${level:uppercase=true:padding=-5}";
        private const string STRUCLOG_LOGGER    = "${logger}";
        private const string STRUCLOG_MESSAGE   = "${message}";
        private const string STRUCLOG_EXCEPTION = "${onexception:${newline}${exception:format=toString}${newline}}";

        private const long MAX_LOG_SIZE     = 1048576; // 1 MB
        private const int  MAX_LOG_SESSIONS = 6;

        internal static void InitDebug() {
            // Make sure crash dir is available for logs as early as possible
            string logPath = DirectoryUtil.RegisterDirectory("logs");

            // Init the Logger
            _logConfiguration = new LoggingConfiguration();

            string headerLayout = $"Blish HUD v{Program.OverlayVersion}";

            var logFile = new FileTarget("logfile") {
                Layout            = $"{STRUCLOG_TIME} | {STRUCLOG_LEVEL} | {STRUCLOG_LOGGER} | {STRUCLOG_MESSAGE}{STRUCLOG_EXCEPTION}",
                Header            = headerLayout,
                FileNameKind      = FilePathKind.Absolute,
                ArchiveFileKind   = FilePathKind.Absolute,
                FileName          = Path.Combine(logPath, "blishhud.${cached:${date:format=yyyyMMdd-HHmmss}}.log"),
                MaxArchiveFiles   = MAX_LOG_SESSIONS,
                ArchiveAboveSize  = MAX_LOG_SIZE,
                EnableFileDelete  = true,
                CreateDirs        = true,
                Encoding          = Encoding.UTF8,
                KeepFileOpen      = true
            };

            var asyncLogFile = new AsyncTargetWrapper("asynclogfile", logFile) {
                QueueLimit        = 200,
                OverflowAction    = AsyncTargetWrapperOverflowAction.Discard,
                ForceLockingQueue = false
            };

            _logConfiguration.AddTarget(asyncLogFile);

            _logConfiguration.AddRule(ApplicationSettings.Instance.DebugEnabled
                                          ? LogLevel.Debug
                                          : File.Exists(DirectoryUtil.BasePath + "\\EnableDebugLogging")
                                          ? LogLevel.Debug : LogLevel.Info,
                                      LogLevel.Fatal, asyncLogFile);

            if (ApplicationSettings.Instance.DebugEnabled) {
                AddDebugTarget(_logConfiguration);
            }

            LogManager.Configuration = _logConfiguration;

            Logger = Logger.GetLogger<DebugService>();
        }

        private static readonly object _debugLock = new object();

        public static void TargetDebug(string time, string level, string logger, string message) {
            if (!Debugger.IsAttached) return;

            const int INTERNAL_DEBUG_WRITESIZE = 4091;

            lock (_debugLock) {
                string outEntry = $"{time} | {level} | {logger} | {message}\r\n";

                // Messages that are too large can cause issues for various debuggers
                if (outEntry.Length >= INTERNAL_DEBUG_WRITESIZE) {
                    int offset;

                    for (offset = 0; offset < outEntry.Length - INTERNAL_DEBUG_WRITESIZE; offset += INTERNAL_DEBUG_WRITESIZE) {
                        Debugger.Log(0, null, outEntry.Substring(offset, INTERNAL_DEBUG_WRITESIZE));
                    }

                    Debugger.Log(0, null, outEntry.Substring(offset));
                } else {
                    Debugger.Log(0, null, outEntry);
                }
            }
        }

        private static void AddDebugTarget(LoggingConfiguration logConfig) {
            LogManager.ThrowExceptions = true;

            var logDebug = new MethodCallTarget("logdebug") {
                ClassName  = typeof(DebugService).AssemblyQualifiedName,
                MethodName = nameof(TargetDebug),
                Parameters = {
                    new MethodCallParameter(STRUCLOG_TIME),
                    new MethodCallParameter(STRUCLOG_LEVEL),
                    new MethodCallParameter(STRUCLOG_LOGGER),
                    new MethodCallParameter(STRUCLOG_MESSAGE)
                }
            };

            _logConfiguration.AddTarget(logDebug);
            _logConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, logDebug);
        }

        public static void UpdateLogLevel(LogLevel newLogLevel) {
            foreach(var rule in LogManager.Configuration.LoggingRules) {
                foreach(var target in rule.Targets) {
                    rule.SetLoggingLevels(newLogLevel, LogLevel.Fatal);
                }
            }

            LogManager.ReconfigExistingLoggers();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args) {
            if (args.ExceptionObject is Exception e) {
                Fatal(e);
            }
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs args) {
            Fatal(args.Exception);
        }

        private static void Fatal(Exception e) {
            Input.DisableHooks();
            
            Logger.Fatal(e, "Blish HUD encountered a fatal crash!");
        }

        #endregion

        #region FPS

        private const int FRAME_DURATION_SAMPLES = 100;

        public DynamicallySmoothedValue<float> FrameCounter { get; private set; }

        #endregion

        #region Measuring

        private const int DEFAULT_DEBUGCOUNTER_SAMPLES = 60;

        private ConcurrentDictionary<string, DebugCounter> _funcTimes;

        /// <summary>
        /// </summary>
        /// <param name="func"></param>
        [Conditional("DEBUG")]
        public void StartTimeFunc(string func) {
            StartTimeFunc(func, DEFAULT_DEBUGCOUNTER_SAMPLES);
        }

        [Conditional("DEBUG")]
        public void StartTimeFunc(string func, int length) {
            if (!_funcTimes.ContainsKey(func)) {
                _funcTimes.TryAdd(func, new DebugCounter(length));
            } else {
                _funcTimes[func].StartInterval();
            }
        }

        [Conditional("DEBUG")]
        public void StopTimeFunc(string func) {
            _funcTimes[func].EndInterval();
        }

        [Conditional("DEBUG")]
        public void StopTimeFuncAndOutput(string func) {
            _funcTimes[func].EndInterval();
            Logger.Debug("{funcName} ran for {$funcTime}.", func, _funcTimes[func]?.GetTotal().Seconds().Humanize());
        }

        #endregion

        #region Debug Overlay

        public OverlayStrings OverlayTexts { get; private set; }

        public void DrawDebugOverlay(SpriteBatch spriteBatch, GameTime gameTime) {
            int debugLeft = Graphics.WindowWidth - 600;

            if (EnableFPSDisplay.Value || ApplicationSettings.Instance.DebugEnabled) {
                spriteBatch.DrawString(Content.DefaultFont14, $"FPS: {Math.Round(Debug.FrameCounter.Value, 0)}", new Vector2(debugLeft, 25), Color.Red);
            }

            if (EnableAdditionalDebugDisplay.Value || ApplicationSettings.Instance.DebugEnabled) {
                int i = 0;

                foreach (KeyValuePair<string, DebugCounter> timedFuncPair in _funcTimes.Where(ft => ft.Value.GetAverage() > 1).OrderByDescending(ft => ft.Value.GetAverage())) {
                    spriteBatch.DrawString(Content.DefaultFont14, $"{timedFuncPair.Key} {Math.Round(timedFuncPair.Value.GetAverage())} ms", new Vector2(debugLeft, 50 + i++ * 25), Color.Orange);
                }

                foreach (Func<GameTime, string> func in this.OverlayTexts.Values) {
                    spriteBatch.DrawString(Content.DefaultFont14, func(gameTime), new Vector2(debugLeft, 50 + i++ * 25), Color.Yellow);
                }
            }
        }

        #endregion

        #region Service Implementation

        protected override void Initialize() {
            _debugSettings = Settings.RegisterRootSettingCollection(DEBUG_SETTINGS);

            DefineSettings(_debugSettings);

            this.EnableDebugLogging.Value = File.Exists(DirectoryUtil.BasePath + "\\EnableDebugLogging");

            this.FrameCounter = new DynamicallySmoothedValue<float>(FRAME_DURATION_SAMPLES);

            if (!Debugger.IsAttached) {
                Application.ThreadException                += ApplicationThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }
        }

        protected override void Load() {
            _funcTimes = new ConcurrentDictionary<string, DebugCounter>();

            this.OverlayTexts = new OverlayStrings();
            this.OverlayTexts.TryAdd("entityCount", _ => $"3D Entities Displayed: {Graphics.World.Entities.Count()}");
            this.OverlayTexts.TryAdd("renderLate",  gameTime => "Render Late: "     + (gameTime.IsRunningSlowly ? "Yes" : "No"));
            this.OverlayTexts.TryAdd("arcDps",      _ => "ArcDPS Bridge: "          + (ArcDps.RenderPresent ? "Yes" : "No"));
            this.OverlayTexts.TryAdd("volume",      _ => "Average In-Game Volume: " + GameIntegration.Audio.Volume);
        }

        protected override void Update(GameTime gameTime) {
            /* NOOP */
        }

        internal void TickFrameCounter(float elapsedTime) {
            this.FrameCounter.PushValue(1f / elapsedTime);
        }

        protected override void Unload() {
            /* NOOP */
        }

        private void DefineSettings(SettingCollection settings) {
            EnableDebugLogging =           settings.DefineSetting("EnableDebugLogging", 
                                                                  File.Exists(DirectoryUtil.BasePath + "\\EnableDebugLogging"),
                                                                  () => Strings.GameServices.DebugService.Setting_DebugLogging_DisplayName,
                                                                  () => Strings.GameServices.DebugService.Setting_DebugLogging_Description);

            EnableFPSDisplay =             settings.DefineSetting("EnableFPSDisplay",
                                                                  false,
                                                                  () => Strings.GameServices.DebugService.Setting_FPSDisplay_DisplayName,
                                                                  () => Strings.GameServices.DebugService.Setting_FPSDisplay_Description);

            EnableAdditionalDebugDisplay = settings.DefineSetting("EnableAdditionalDebugDisplay",
                                                                  false,
                                                                  () => Strings.GameServices.DebugService.Setting_AdditionalDebugDisplay_DisplayName,
                                                                  () => Strings.GameServices.DebugService.Setting_AdditionalDebugDisplay_Description);


            EnableDebugLogging.SettingChanged += EnableDebugLoggingOnSettingChanged;


            if (ApplicationSettings.Instance.DebugEnabled) {
                // Disable all debug setting and update description - user has manually specified --debug as launch arg
                EnableDebugLogging.SetDisabled();
                EnableDebugLogging.GetDescriptionFunc =           () => Strings.GameServices.DebugService.Setting_DebugLogging_Description +           "\n" + Strings.GameServices.DebugService.Setting_Debug_Locked_Description;

                EnableFPSDisplay.SetDisabled();
                EnableFPSDisplay.GetDescriptionFunc =             () => Strings.GameServices.DebugService.Setting_FPSDisplay_Description +             "\n" + Strings.GameServices.DebugService.Setting_Debug_Locked_Description;

                EnableAdditionalDebugDisplay.SetDisabled();
                EnableAdditionalDebugDisplay.GetDescriptionFunc = () => Strings.GameServices.DebugService.Setting_AdditionalDebugDisplay_DisplayName + "\n" + Strings.GameServices.DebugService.Setting_Debug_Locked_Description;
            }
        }

        private void EnableDebugLoggingOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            if (e.NewValue) {
                Logger.Info("User activated debug logging");
                UpdateLogLevel(LogLevel.Debug);
                File.Create(DirectoryUtil.BasePath + "\\EnableDebugLogging").Dispose();
            } else {
                Logger.Info("User deactivated debug logging");
                UpdateLogLevel(LogLevel.Info);
                File.Delete(DirectoryUtil.BasePath + "\\EnableDebugLogging");
            }
        }

        #endregion

    }

}