using System;
using Gw2Sharp.Mumble;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Mumble {
    public class Info {

        private readonly Gw2MumbleService _service;

        #region Events

        /// <summary>
        /// Fires when the build ID reported by the Mumble API changes.
        /// </summary>
        public event EventHandler<ValueEventArgs<int>> BuildIdChanged;

        /// <summary>
        /// Fires when the Guild Wars 2 application receives focus.
        /// </summary>
        public event EventHandler<EventArgs> GameAcquiredFocus;

        /// <summary>
        /// Fires when the Guild Wars 2 application loses focus.
        /// </summary>
        public event EventHandler<EventArgs> GameLostFocus;

        private void OnBuildIdChanged(ValueEventArgs<int> e) => BuildIdChanged?.Invoke(this, e);
        private void OnGameAcquiredFocus(EventArgs        e) => GameAcquiredFocus?.Invoke(this, e);
        private void OnGameLostFocus(EventArgs            e) => GameLostFocus?.Invoke(this, e);

        private int  _prevBuildId       = -1;
        private bool _prevIsGameFocused = false;

        private void HandleEvents() {
            if (_prevBuildId != this.BuildId) {
                _prevBuildId = this.BuildId;
                OnBuildIdChanged(new ValueEventArgs<int>(_prevBuildId));
            }

            if (_prevIsGameFocused != this.IsGameFocused) {
                _prevIsGameFocused = this.IsGameFocused;

                if (_prevIsGameFocused) {
                    OnGameAcquiredFocus(EventArgs.Empty);
                } else {
                    OnGameLostFocus(EventArgs.Empty);
                }
            }
        }

        #endregion

        /// <inheritdoc cref="IGw2MumbleClient.BuildId"/>
        public int BuildId => _service.RawClient.BuildId;

        /// <inheritdoc cref="IGw2MumbleClient.DoesGameHaveFocus"/>
        public bool IsGameFocused => _service.RawClient.DoesGameHaveFocus;

        /// <inheritdoc cref="IGw2MumbleClient.ServerAddress"/>
        public string ServerAddress => _service.RawClient.ServerAddress;

        /// <inheritdoc cref="IGw2MumbleClient.ServerPort"/>
        public ushort ServerPort => _service.RawClient.ServerPort;

        /// <inheritdoc cref="IGw2MumbleClient.ShardId"/>
        public uint ShardId => _service.RawClient.ShardId;

        /// <inheritdoc cref="IGw2MumbleClient.Version"/>
        public int Version => _service.RawClient.Version;

        /// <inheritdoc cref="IGw2MumbleClient.ProcessId"/>
        public uint ProcessId => _service.RawClient.ProcessId;

        internal Info(Gw2MumbleService service) {
            _service = service;
        }

        internal void Update(GameTime gameTime) {
            HandleEvents();
        }

    }

}
