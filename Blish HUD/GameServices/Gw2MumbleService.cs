using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using GW2NET.MumbleLink;

namespace Blish_HUD {

    public class Gw2MumbleService : GameService {

        private static readonly Logger Logger = Logger.GetLogger(typeof(Gw2MumbleService));

        public event EventHandler<EventArgs> BuildIdChanged;

        private void OnBuildIdChanged(EventArgs e) {
            BuildIdChanged?.Invoke(this, e);
        }

        public bool Available => gw2Link != null && this.MumbleBacking != null;

        private Avatar _mumbleBacking;
        public Avatar MumbleBacking => _mumbleBacking;

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
            TryAttachToMumble();
        }

        private void TryAttachToMumble() {
            try {
                gw2Link = MumbleLinkFile.CreateOrOpen();
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to attach to MumbleLink API.");
                this.gw2Link = null;
            }
        }

        private double lastMumbleCheck = 0;
        
        public int _delayedTicks = 0;

        private Queue<int> _uiTickRates = new Queue<int>();
        public float AverageFramesPerUITick => (float)_uiTickRates.Sum(t => t) / _uiTickRates.Count;

        protected override void Update(GameTime gameTime) {
            this.TimeSinceTick += gameTime.ElapsedGameTime;

            if (gw2Link != null) {
                try {
                    _mumbleBacking = gw2Link.Read();

                    if (_mumbleBacking.UiTick > this.UiTick) {
                        this.TimeSinceTick = TimeSpan.Zero;
                        this.UiTick        = _mumbleBacking.UiTick;
                        this.BuildId       = _mumbleBacking.Context.BuildId;

                        GameService.Graphics.UIScale = (GraphicsService.UiScale) _mumbleBacking.Identity.UiScale;

                        if (_uiTickRates.Count > 10) _uiTickRates.Dequeue();
                        _uiTickRates.Enqueue(_delayedTicks);
                        _delayedTicks = 0;
                    } else {
                        _delayedTicks += 1;
                    }
                } catch (NullReferenceException ex) /* [BLISHHUD-X] */ {
                    Console.WriteLine("Mumble connection failed.");
                    _mumbleBacking = null;
                } catch (SerializationException ex) /* [BLISHHUD-10] */ {
                    Console.WriteLine("Failed to deserialize Mumble API structure.");
                    _mumbleBacking = null;
                }
            } else {
                lastMumbleCheck += gameTime.ElapsedGameTime.TotalSeconds;

                if (lastMumbleCheck > 10) {
                    TryAttachToMumble();

                    lastMumbleCheck = 0;
                }
            }
        }

        protected override void Unload() {
            gw2Link?.Dispose();
        }
    }

}
