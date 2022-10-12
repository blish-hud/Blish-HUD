using System;
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

        public override void Update(GameTime gameTime) {
            if (this.TacOIsRunning || !_service.Gw2Instance.Gw2IsRunning) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceCheck > CHECK_INTERVAL) {
                Process[] tacoApp = Process.GetProcessesByName(TACO_PROCESS);

                this.TacOIsRunning = tacoApp.Length > 0;

                _timeSinceCheck = 0;
            }
        }

    }
}
