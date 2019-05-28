using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Modules.BeetleRacing.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.BeetleRacing {
    public class BeetleRacing : Module {

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "Racing",
                GameService.Content.GetTexture("347218"),
                "bh.general.speed",
                "Currently only provides the speedometer feature.  Additional features are planned for future releases.",
                "LandersXanders.1235",
                "1"
            );
        }

        #region Settings

        private SettingEntry<bool> settingOnlyShowAtHighSpeeds;
        private SettingEntry<bool> settingShowSpeedNumber;

        public override void DefineSettings(Settings settings) {
            // Define settings
            settingOnlyShowAtHighSpeeds = settings.DefineSetting<bool>("Only Show at High Speeds", false, false, true, "Only show the speedometer if you're going at least 1/4 the max speed.");
            settingShowSpeedNumber = settings.DefineSetting<bool>("Show Speed Value", false, false, true, "Shows the speed (in approx. inches per second) above the speedometer.");
        }

        #endregion

        private Speedometer speedometer;

        public override void OnEnabled() {
            base.OnEnabled();

            speedometer = new Speedometer {
                Parent = GameService.Graphics.SpriteScreen,
                Speed = 0
            };
        }

        public override void OnDisabled() {
            sampleBuffer.Clear();
            lastPos = Vector3.Zero;
            speedometer.Dispose();
            speedometer = null;
        }
        
        private Vector3 lastPos = Vector3.Zero;
        private long lastUpdate = 0;
        private double leftOverTime = 0;
        private Queue<double> sampleBuffer = new Queue<double>();

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            // Unless we're in game running around, don't show the speedometer
            if (!GameService.GameIntegration.IsInGame) {
                speedometer.Visible = false;
                lastPos = Vector3.Zero;
                sampleBuffer.Clear();
                return;
            }

            leftOverTime += gameTime.ElapsedGameTime.TotalSeconds;

            // TODO: Ignore same tick for speed updates
            if (lastPos != Vector3.Zero && lastUpdate != GameService.Gw2Mumble.UiTick) {
                double velocity = Vector3.Distance(GameService.Player.Position, lastPos) * 39.3700787f / leftOverTime;
                leftOverTime = 0;

                // TODO: Make the sample buffer a setting
                if (sampleBuffer.Count > 50) {
                    double sped = sampleBuffer.Average(i => i);

                    speedometer.Speed = (float) Math.Round(sped, 1);

                    speedometer.Visible        = !settingOnlyShowAtHighSpeeds.Value || speedometer.Speed / speedometer.MaxSpeed >= 0.25;
                    speedometer.ShowSpeedValue = settingShowSpeedNumber.Value;

                    sampleBuffer.Dequeue();
                }

                sampleBuffer.Enqueue(velocity);
            }

            lastPos = GameService.Player.Position;
            lastUpdate = GameService.Gw2Mumble.UiTick;
        }
    }
}
