using Blish_HUD.GameServices;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameIntegration {
    public sealed class TacOIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<TacOIntegration>();

        private const string TACO_WINDOW_CLASS = "CoRE2";
        private const string TACO_WINDOW_TITLE = "Guild Wars 2 Tactical Overlay";
        private const int    CHECK_INTERVAL    = 3000;

        private double _timeSinceCheck;

        /// <summary>
        /// <c>True</c> if the TacO process has been found running.
        /// </summary>
        public bool TacOIsRunning { get; private set; }

        internal TacOIntegration(GameIntegrationService service) : base(service) { }

        public override void Update(GameTime gameTime) {
            if (this.TacOIsRunning || !_service.Gw2Instance.Gw2IsRunning) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceCheck > CHECK_INTERVAL) {
                this.TacOIsRunning = WindowUtil.WindowExists(TACO_WINDOW_CLASS, TACO_WINDOW_TITLE);

                _timeSinceCheck = 0;
            }
        }

    }
}
