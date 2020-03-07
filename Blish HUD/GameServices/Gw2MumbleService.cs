using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using GW2NET.MumbleLink;
using Gw2Sharp.Mumble;

namespace Blish_HUD {

    public class Gw2MumbleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<Gw2MumbleService>();

        internal IGw2MumbleClient SharedGw2MumbleClient => GameService.Gw2Api.SharedApiClient.Mumble;

        public event EventHandler<EventArgs> BuildIdChanged;

        private void OnBuildIdChanged(EventArgs e) {
            BuildIdChanged?.Invoke(this, e);
        }

        public bool Available => true;

        public TimeSpan TimeSinceTick { get; private set; }

        public long UiTick { get; private set; } = -1;

        private int _buildId = -1;
        public int BuildId {
            get => _buildId;
            private set {
                if (_buildId == value) return;

                _buildId = value;

                OnBuildIdChanged(EventArgs.Empty);
            }
        }

        private MumbleLinkFile gw2Link;

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {

        }

        private double lastMumbleCheck = 0;
        
        public int _delayedTicks = 0;

        private readonly Queue<int> _uiTickRates = new Queue<int>();
        public float AverageFramesPerUITick => (float)_uiTickRates.Sum(t => t) / _uiTickRates.Count;

        protected override void Update(GameTime gameTime) {
            this.TimeSinceTick += gameTime.ElapsedGameTime;

            SharedGw2MumbleClient.Update();

            if (SharedGw2MumbleClient.Tick > this.UiTick) {
                this.TimeSinceTick = TimeSpan.Zero;

                this.UiTick  = SharedGw2MumbleClient.Tick;
                this.BuildId = SharedGw2MumbleClient.BuildId;

                Graphics.UIScale = (GraphicsService.UiScale)SharedGw2MumbleClient.UiSize;

                if (_uiTickRates.Count > 10) _uiTickRates.Dequeue();

                _uiTickRates.Enqueue(_delayedTicks);
                _delayedTicks = 0;
            } else {
                _delayedTicks++;
            }
        }

        protected override void Unload() {
            
        }
    }

}
