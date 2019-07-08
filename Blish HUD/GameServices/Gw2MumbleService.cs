﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Microsoft.Xna.Framework;
using GW2NET.MumbleLink;

namespace Blish_HUD {

    public class Gw2MumbleService:GameService {

        public bool Available => gw2Link != null && this.MumbleBacking != null;

        private Avatar _mumbleBacking;
        public Avatar MumbleBacking => _mumbleBacking;

        public TimeSpan TimeSinceTick { get; private set; }

        public long UiTick { get; private set; } = -1;

        private MumbleLinkFile gw2Link;

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            TryAttachToMumble();
        }

        private void TryAttachToMumble() {
            try {
                gw2Link = MumbleLinkFile.CreateOrOpen();
            } catch (Exception ex) {
                // TODO: Make note with debug service that the mumble link could not be established
                // TODO: Consider trying again a bit later?
                Console.WriteLine("TryAttachToMumble() failed: " + ex.Message);
                this.gw2Link = null;

                //SentryWrapper.CaptureException(ex);
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
