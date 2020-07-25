using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.GameServices;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameIntegration {
    public class TacOIntegration : ServiceModule<GameIntegrationService> {

        private const string TACO_PROCESS   = "GW2TacO";
        private const int    CHECK_INTERVAL = 3000;

        private double _timeSinceCheck;

        /// <summary>
        /// <c>True</c> if the TacO process has been found running.
        /// </summary>
        public bool TacOIsRunning { get; private set; }

        public TacOIntegration(GameIntegrationService service) : base(service) { }

        private void ListenToTacO(Process tacOProcess) {
            if (tacOProcess.HasExited) return;

            this.TacOIsRunning = true;

            tacOProcess.EnableRaisingEvents = true;

            tacOProcess.Exited += delegate { TacOIsRunning = false; };
        }

        public override void Update(GameTime gameTime) {
            if (this.TacOIsRunning) return;

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
