using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Windows.Forms;
using Blish_HUD;
using Blish_HUD.Modules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Controls;
using Blish_HUD.Modules.LoadingScreenHints.Controls;

namespace Blish_HUD.Modules.LoadingScreenHints
{
    public class LoadingScreenHints : Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            Texture2D icon = null;

            return new ModuleInfo(
                "Loading Screen Hints",
                icon,
                "bh.general.loadingscreenhints",
                "Shows tips, Tyrian knowledge, narrations and character quizzes during the loading screen.",
                "Nekres.1038",
                "1.0"
            );
        }

        #region Settings

        public override void DefineSettings(Settings settings)
        {
            // Define settings
        }

        #endregion
        private LoadScreenPanel LoadScreenPanel;

        public override void OnEnabled()
        {
            base.OnEnabled();
            LoadScreenPanel = BuildLoadScreenPanel();
        }
        private LoadScreenPanel BuildLoadScreenPanel()
        {
            var tipsPanel = new LoadScreenPanel()
            {
                Parent = GameService.Graphics.SpriteScreen
            };
            return tipsPanel;
        }
        public override void OnDisabled()
        {
            sampleBuffer.Clear();
            LoadScreenPanel.Dispose();
        }
        private long lastUpdate = 0;
        private Queue<double> sampleBuffer = new Queue<double>();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Unless we're in game running around, don't show.
            if (!GameService.GameIntegration.IsInGame)
            {
                if (LoadScreenPanel.Fade != null) {
                    LoadScreenPanel.Fade.Cancel();
                    LoadScreenPanel.Fade = null;
                    LoadScreenPanel.NextHint();
                }
                LoadScreenPanel.Opacity = 1.0f;
                LoadScreenPanel.Visible = true;

                return;
            }

            if (LoadScreenPanel.Fade == null) { LoadScreenPanel.FadeOut(); }

            lastUpdate = GameService.Gw2Mumble.UiTick;
        }
    }
}
