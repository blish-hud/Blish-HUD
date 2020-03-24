using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Blish_HUD.Gw2Mumble;
using Microsoft.Xna.Framework;
using Gw2Sharp.Mumble;

namespace Blish_HUD {

    public class Gw2MumbleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<Gw2MumbleService>();

        internal IGw2MumbleClient SharedGw2MumbleClient => GameService.Gw2Api.SharedApiClient.Mumble;

        #region Categorized Mumble Data

        private Info            _info;
        private PlayerCharacter _playerCharacter;
        private PlayerCamera    _playerCamera;
        private CurrentMap             _currentMap;
        private UI              _ui;

        /// <summary>
        /// Provides information about the Mumble connection and about the game instance in realtime.
        /// </summary>
        public Info            Info            => _info;

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
        public bool IsAvailable => SharedGw2MumbleClient.IsAvailable;

        public TimeSpan TimeSinceTick { get; private set; }

        private int _prevTick = -1;

        public int Tick => SharedGw2MumbleClient.Tick;

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            _info            = new Info(this);
            _playerCharacter = new PlayerCharacter(this);
            _playerCamera    = new PlayerCamera(this);
            _currentMap      = new CurrentMap(this);
            _ui              = new UI(this);
        }

        private double _lastMumbleCheck = 0;
        
        public int _delayedTicks = 0;

        private readonly Queue<int> _uiTickRates = new Queue<int>();
        public float AverageFramesPerTick => (float)_uiTickRates.Sum(t => t) / _uiTickRates.Count;

        protected override void Update(GameTime gameTime) {
            this.TimeSinceTick += gameTime.ElapsedGameTime;

            SharedGw2MumbleClient.Update();

            if (SharedGw2MumbleClient.Tick > _prevTick) {
                _prevTick = SharedGw2MumbleClient.Tick;

                this.TimeSinceTick = TimeSpan.Zero;

                if (_uiTickRates.Count > 10) _uiTickRates.Dequeue();

                _uiTickRates.Enqueue(_delayedTicks);
                _delayedTicks = 0;

                UpdateDetails(gameTime);
            } else {
                _delayedTicks++;
            }
        }

        private void UpdateDetails(GameTime gameTime) {
            _info.Update(gameTime);
            _playerCharacter.Update(gameTime);
            _playerCamera.Update(gameTime);
            _currentMap.Update(gameTime);
            _ui.Update(gameTime);
        }

        protected override void Unload() { /* NOOP */ }

    }

}
