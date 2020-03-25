using System;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Gw2Sharp.Mumble;

namespace Blish_HUD.Gw2Mumble {
    public class CurrentMap {

        private readonly Gw2MumbleService _service;

        #region Events

        /// <summary>
        /// Fires when the in-game map changes.
        /// </summary>
        public event EventHandler<ValueEventArgs<int>> MapChanged;

        private void OnMapChanged(ValueEventArgs<int> e) => MapChanged?.Invoke(this, e);

        private int _prevId = -1;

        private void HandleEvents() {
            if (_prevId != this.Id) {
                _prevId = this.Id;
                OnMapChanged(new ValueEventArgs<int>(_prevId));
            }
        }

        #endregion

        /// <inheritdoc cref="IGw2MumbleClient.MapId"/>
        public int Id => _service.RawClient.MapId;

        /// <inheritdoc cref="IGw2MumbleClient.MapType"/>
        public MapType Type => _service.RawClient.MapType;

        /// <inheritdoc cref="IGw2MumbleClient.IsCompetitiveMode"/>
        public bool IsCompetitiveMode => _service.RawClient.IsCompetitiveMode;

        internal CurrentMap(Gw2MumbleService service) {
            _service = service;
        }

        internal void Update(GameTime gameTime) {
            HandleEvents();
        }

    }
}
