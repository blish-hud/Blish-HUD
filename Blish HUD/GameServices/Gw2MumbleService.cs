using System;
using Blish_HUD.Graphics;
using Blish_HUD.Gw2Mumble;
using Gw2Sharp;
using Microsoft.Xna.Framework;
using Gw2Sharp.Mumble;
namespace Blish_HUD {

    public class Gw2MumbleService : GameService {

        private const string DEFAULT_MUMBLEMAPNAME = "MumbleLink";

        private readonly TimeSpan _syncDelay = TimeSpan.FromMilliseconds(3);

        private IGw2MumbleClient _rawClient;

        /// <inheritdoc cref="Gw2MumbleClient"/>
        public IGw2MumbleClient RawClient => _rawClient;

        #region Categorized Mumble Data

        /// <summary>
        /// Provides information about the Mumble connection and about the game instance in realtime.
        /// </summary>
        public Info Info { get; private set; }

        /// <summary>
        /// Provides data about the active player's character in realtime.
        /// </summary>
        public PlayerCharacter PlayerCharacter { get; private set; }

        /// <summary>
        /// Provides data about the active player's camera in realtime.
        /// </summary>
        public PlayerCamera PlayerCamera { get; private set; }

        /// <summary>
        /// Provides data about the in-game UI state in realtime.
        /// </summary>
        public UI UI { get; private set; }

        /// <summary>
        /// Provides data about the map the player is currently on in realtime.
        /// </summary>
        public CurrentMap CurrentMap { get; private set; }

        #endregion

        /// <inheritdoc cref="IGw2MumbleClient.IsAvailable"/>
        public bool IsAvailable => _rawClient.IsAvailable;

        public TimeSpan TimeSinceTick { get; private set; }

        private int _delayedTicks = 0;
        private int _prevTick = -1;

        public int Tick => _rawClient.Tick;

        internal Gw2MumbleService() {
            _rawClient = new Gw2Client().Mumble[ApplicationSettings.Instance.MumbleMapName ?? DEFAULT_MUMBLEMAPNAME];

            this.Info            = new Info(this);
            this.PlayerCharacter = new PlayerCharacter(this);
            this.PlayerCamera    = new PlayerCamera(this);
            this.CurrentMap      = new CurrentMap(this);
            this.UI              = new UI(this);
        }

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.TimeSinceTick += gameTime.ElapsedGameTime;
            
            _rawClient.Update();

            if (_rawClient.Tick > _prevTick) {
                _prevTick = _rawClient.Tick;

                this.TimeSinceTick = TimeSpan.Zero;

                _delayedTicks = 0;

                UpdateDetails(gameTime);
            } else {
                _delayedTicks++;

                if (GameService.Graphics.FrameLimiter == FramerateMethod.SyncWithGame
                    && GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                    && this.TimeSinceTick.TotalSeconds < 0.5) {

                    BlishHud.Instance.SkipDraw();
                }
            }
        }

        private void UpdateDetails(GameTime gameTime) {
            this.Info.Update(gameTime);
            this.PlayerCharacter.Update(gameTime);
            this.PlayerCamera.Update(gameTime);
            this.CurrentMap.Update(gameTime);
            this.UI.Update(gameTime);
        }

        protected override void Unload() {
            _rawClient.Dispose();
        }

    }

}
