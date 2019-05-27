using System;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Windows.Forms;
using Blish_HUD;
using Sentry;

namespace Blish_HUD {
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program {

        public const string APP_VERSION = "blish_hud@0.3.2-alpha.7.4_DEV";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if (IsMoreThanOneInstance()) {
                return;
            }
            //Console.WriteLine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create));

            // TODO: Implement for error logging in released versions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
#if SENTRY
            const string SENTRY_DSN = "https://e11516741a32440ca7a72b68d5af93df@sentry.do-ny3.svr.gw2blishhud.com/2";

            using (SentrySdk.Init(
                                  o => {
                                      o.Dsn = new Dsn(SENTRY_DSN);
                                      o.Release = APP_VERSION;
                                      o.Environment = APP_VERSION.Contains("-") ? APP_VERSION.Split('-')[1].Split('.')[0] : APP_VERSION;
                                      o.Debug = true;
                                      o.BeforeSend = sentryEvent => {
                                          /* TODO: Confirm that this filters correctly - consider filtering more details and
                                             move it all to it's own function */
                                          sentryEvent.LogEntry.Message = sentryEvent.LogEntry.Message.Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>");
                                          sentryEvent.Message = sentryEvent.Message.Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>");

                                          return sentryEvent;
                                      };
                                  })) {

                SentrySdk.ConfigureScope(
                                         scope => {
                                             // Want to try and gauge what kind of language support we'll want to provide in the future
                                             scope.SetTag("locale", CultureInfo.CurrentCulture.DisplayName);

                                             // Try to avoid accidentally pulling their user account name (since it could have their real name in it)
                                             scope.SetTag("start-dir", Directory.GetCurrentDirectory().Replace(Environment.UserName, "<SENTRY-FILTERED-OUT-USERNAME>"));
                                         });

                using (var game = new Overlay())
                    game.Run();
            }
#else
            using (var game = new Overlay())
                game.Run();
#endif

            SingleInstanceMutex.ReleaseMutex();
        }

        private static readonly Mutex SingleInstanceMutex = new Mutex(true, "{5802208e-71ca-4745-ab1b-d851bc17a460}");

        private static bool IsMoreThanOneInstance() { return !SingleInstanceMutex.WaitOne(TimeSpan.Zero, true); }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args) {
            var e = (Exception)args.ExceptionObject;

            SentryWrapper.CaptureException(e);

            InputService.mouseHook?.UnHookMouse();
            //InputService.keyboardHook?.UnHookKeyboard();

            string errorMessage = "Application error: " + e.Message + Environment.NewLine + "Trace: " + e.StackTrace + Environment.NewLine + "Runtime terminating: " + args.IsTerminating + Environment.NewLine + APP_VERSION;

            try {
                File.WriteAllText(Path.Combine(GameService.FileSrv.BasePath, "logs", "crash." + DateTime.Now.Ticks + ".log"), errorMessage);
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Blish HUD has crashed!  Additionally, there was an error saving the crash log, so here is the crash message: " + Environment.NewLine + errorMessage, "Blish HUD Crashed!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

    }
#endif
}
