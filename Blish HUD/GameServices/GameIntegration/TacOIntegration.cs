using System.ComponentModel;
using System.Diagnostics;
using Blish_HUD.GameServices;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameIntegration {
    public class TacOIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<TacOIntegration>();

        private const string TACO_PROCESS   = "GW2TacO";
        private const int    CHECK_INTERVAL = 3000;

        private double _timeSinceCheck;

        /// <summary>
        /// <c>True</c> if the TacO process has been found running.
        /// </summary>
        public bool TacOIsRunning { get; private set; }

        public TacOIntegration(GameIntegrationService service) : base(service) { }

        private void ListenToTacO(Process tacOProcess) {
            try {
                if (tacOProcess.HasExited) return;

                tacOProcess.EnableRaisingEvents = true;

                tacOProcess.Exited += delegate { TacOIsRunning = false; };
            } catch (Win32Exception ex) {
                // Typically means that TacO was ran as an admin and we weren't
                Logger.Warn(ex, "Encountered an error interacting with the TacO process - it may have been run as admin.");
            }

            this.TacOIsRunning = true;
        }

        public override void Update(GameTime gameTime) {
            if (this.TacOIsRunning || !_service.Gw2Proc.Gw2IsRunning) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceCheck > CHECK_INTERVAL) {
                Process[] tacoApp = Process.GetProcessesByName(TACO_PROCESS);

                if (tacoApp.Length > 0) {
                    ListenToTacO(tacoApp[0]);
                }

                _timeSinceCheck = 0;
            }
        }

    }
}
