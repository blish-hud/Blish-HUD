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

        private Info            _info;
        private PlayerCharacter _playerCharacter;
        private PlayerCamera    _playerCamera;
        private CurrentMap      _currentMap;
        private UI              _ui;

        /// <summary>
        /// Provides information about the Mumble connection and about the game instance in realtime.
        /// </summary>
        public Info Info => _info;

        /// <summary>
        /// Provides data about the active player's character in realtime.
        /// </summary>
        public PlayerCharacter PlayerCharacter => _playerCharacter;

        /// <summary>
        /// Provides data about the active player's camera in realtime.
        /// </summary>
        public PlayerCamera PlayerCamera => _playerCamera;

        /// <summary>
        /// Provides data about the in-game UI state in realtime.
        /// </summary>
        public UI UI => _ui;

        /// <summary>
        /// Provides data about the map the player is currently on in realtime.
        /// </summary>
        public CurrentMap CurrentMap => _currentMap;

        #endregion

        /// <inheritdoc cref="IGw2MumbleClient.IsAvailable"/>
        public bool IsAvailable => _rawClient.IsAvailable;

        public TimeSpan TimeSinceTick { get; private set; }

        private int _delayedTicks = 0;
        private int _prevTick = -1;

        public int Tick => _rawClient.Tick;

        protected override void Initialize() {
            _rawClient = new Gw2Client().Mumble[ApplicationSettings.Instance.MumbleMapName ?? DEFAULT_MUMBLEMAPNAME];
        }

        protected override void Load() {
            _info            = new Info(this);
            _playerCharacter = new PlayerCharacter(this);
            _playerCamera    = new PlayerCamera(this);
            _currentMap      = new CurrentMap(this);
            _ui              = new UI(this);
        }

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
                    && GameService.GameIntegration.Gw2Proc.Gw2IsRunning
                    && this.TimeSinceTick.TotalSeconds < 0.5) {

                    BlishHud.Instance.SkipDraw();
                }
            }
        }

        private void UpdateDetails(GameTime gameTime) {
            _info.Update(gameTime);
            _playerCharacter.Update(gameTime);
            _playerCamera.Update(gameTime);
            _currentMap.Update(gameTime);
            _ui.Update(gameTime);
        }

        protected override void Unload() {
            _rawClient.Dispose();
        }

    }

}
