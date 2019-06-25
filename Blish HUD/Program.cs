using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NLog;
using Sentry;

namespace Blish_HUD {
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program {

        private static readonly Lazy<NLog.Logger> Logger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);

        public static SemVer.Version OverlayVersion { get; } = new SemVer.Version(typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, true);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if (IsMoreThanOneInstance()) {
                return;
            }

            DebugService.InitDebug();

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

#if SENTRY
            const string SENTRY_DSN = "https://e11516741a32440ca7a72b68d5af93df@sentry.do-ny3.svr.gw2blishhud.com/2";

            using (SentrySdk.Init(
                                  o => {
                                      o.Dsn = new Dsn(SENTRY_DSN);
                                      o.Release = OverlayVersion.ToString();
                                      o.Environment = OverlayVersion.ToString().Contains("-") ? OverlayVersion.ToString().Split('-')[1].Split('.')[0] : OverlayVersion.ToString();
                                      o.Debug = true;
                                      o.BeforeSend = sentryEvent => {
                                          /* TODO: Confirm that this filters correctly - consider filtering more details and
                                             move it all to it's own function */
                                          if (sentryEvent.LogEntry != null && !string.IsNullOrEmpty(sentryEvent.LogEntry.Message)) {
                                              sentryEvent.LogEntry.Message = sentryEvent.LogEntry.Message.Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>");
                                          }

                                          if (!string.IsNullOrEmpty(sentryEvent.Message)) {
                                              sentryEvent.Message = sentryEvent.Message.Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>");
                                          }

                                          return sentryEvent;
                                      };
                                  })) {

                SentrySdk.ConfigureScope(scope => {
                                             // Want to try and gauge what kind of language support we'll want to provide in the future
                                             scope.SetTag("locale", CultureInfo.CurrentUICulture.DisplayName);

                                             // Try to avoid accidentally pulling their user account name (since it could have their real name in it)
                                             scope.SetTag("start-dir", Directory.GetCurrentDirectory().Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>"));
                                         });

                using (var game = new BlishHud()) game.Run();
            }
#else
            using (var game = new BlishHud()) game.Run();
#endif

            SingleInstanceMutex.ReleaseMutex();
        }

        private static readonly Mutex SingleInstanceMutex = new Mutex(true, "{5802208e-71ca-4745-ab1b-d851bc17a460}");

        private static bool IsMoreThanOneInstance() { return !SingleInstanceMutex.WaitOne(TimeSpan.Zero, true); }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args) {
            var e = (Exception)args.ExceptionObject;

            Logger.Value.Fatal(e, "Blish HUD encountered a fatal crash!");

            SentryWrapper.CaptureException(e);

            InputService.mouseHook?.UnhookMouse();
            InputService.keyboardHook?.UnhookKeyboard();

            //string errorMessage = "Application error: " + e.Message + Environment.NewLine + "Trace: " + e.StackTrace + Environment.NewLine + "Runtime terminating: " + args.IsTerminating + Environment.NewLine + APP_VERSION;

            //try {
            //    //File.WriteAllText(Path.Combine(GameService.Directory.BasePath, "logs", "crash." + DateTime.Now.Ticks + ".log"), errorMessage);
            //} catch (Exception ex) {
            //    MessageBox.Show("Blish HUD has crashed!  Additionally, there was an error saving the crash log, so here is the crash message: "
            //                  + Environment.NewLine + errorMessage
            //                  + Environment.NewLine + Environment.NewLine
            //                  + "And here is the error that prevented us from saving to the crash log: "
            //                  + Environment.NewLine + ex.Message, "Blish HUD Crashed!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //}
        }

    }
#endif
        }
