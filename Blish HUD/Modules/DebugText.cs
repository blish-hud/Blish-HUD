using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules {
    public class DebugText : Module {

        private Controls.Label lblInfo;

        private Gw2MumbleService Gw2Mumble;
        private PlayerService Player;

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "Debug Module",
                GameService.Content.GetTexture("155018"),
                "bh.general.debug",
                "Allows you to show basic 'debug' details such as your xyz coordinates, server IP address, shard ID, and a few other minor details.",
                "LandersXanders.1235",
                "1"
            );
        }

        private delegate string DebugValueRendererDelegate(GameTime gameTime);

        private Dictionary<SettingEntry<bool>, DebugValueRendererDelegate> DebugValueRenderers =
            new Dictionary<SettingEntry<bool>, DebugValueRendererDelegate>();

        #region Settings

        private SettingEntry<bool> settingShowMapId;
        private SettingEntry<bool> settingShowServerIp;
        private SettingEntry<bool> settingShowCurrentShard;
        private SettingEntry<bool> settingShowPlayerPosition;
        private SettingEntry<bool> settingShowOverlayFPS;

        public override void DefineSettings(Settings settings) {
            // Define settings
            settingShowMapId = settings.DefineSetting<bool>("Show Map ID", true, true, true, "Show ID of current map (future release will show name instead of ID).");
            settingShowServerIp = settings.DefineSetting<bool>("Show Server IP", true, true, true, "Show the IP address of the server you're currently connected to.");
            settingShowCurrentShard = settings.DefineSetting<bool>("Show Current Shard", true, true, true, "Show the current shard ID.");
            settingShowPlayerPosition = settings.DefineSetting<bool>("Show Player Position", true, true, true, "Show the player's current XYZ position.");
            //settingShowOverlayFPS = settings.DefineSetting<bool>("Show Overlay FPS", true, true, true, "Show FPS of Blish HUD.");
        }

        #endregion

        private void LoadRenderers() {
            // Used to avoid a race condition (or logic mistake) that's causing [BLISHHUD-1A]
            DebugValueRenderers.Clear();

            DebugValueRenderers.Add(settingShowMapId,        time => $"Map: {GameService.Player.MapId}");
            DebugValueRenderers.Add(settingShowServerIp,     time => $"Server {Gw2Mumble.MumbleBacking.Context.ServerAddress}");
            DebugValueRenderers.Add(settingShowCurrentShard, time => $"Shard: {GameService.Player.ShardId}");

            // TODO: Provide setting that allows user to decide how precise the position provided should be
            DebugValueRenderers.Add(settingShowPlayerPosition, time => $"Position: {Player.Position.ToRoundedString()}");

            //DebugValueRenderers.Add(settingShowOverlayFPS, time => $"BHUD FPS: {0}");
        }

        public override void OnLoad() {
            base.OnLoad();

            Gw2Mumble = GameService.Gw2Mumble;
            Player = GameService.Player;

            lblInfo = new Controls.Label {
                Font                = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Center,
                VerticalAlignment   = Utils.DrawUtil.VerticalAlignment.Middle,
                ShowShadow          = true,
                ShadowColor         = Color.Black,
                TextColor           = Color.White,
                StrokeText        = true
            };

            //lblInfo.Location = new Point(0, 2);

            UpdateSizeAndLocation(null, null);
            GameService.Graphics.SpriteScreen.Resized += UpdateSizeAndLocation;
        }

        private void UpdateSizeAndLocation(object sender, EventArgs e) {
            lblInfo.Size = new Point(GameService.Graphics.SpriteScreen.Width, 14);
            lblInfo.Location = new Point(0, GameService.Graphics.SpriteScreen.Height - 15);
        }

        public override void OnEnabled() {
            base.OnEnabled();

            lblInfo.Parent = GameService.Graphics.SpriteScreen;
        }

        public override void OnDisabled() {
            base.OnDisabled();

            lblInfo.Parent = null;
        }

        private LinkedList<string> displayedDebugValues = new LinkedList<string>();
        public override void Update(GameTime gameTime) {
            displayedDebugValues.Clear();

            if (GameService.GameIntegration.IsInGame) {
                if (DebugValueRenderers.Any() && Gw2Mumble.MumbleBacking != null) {
                    foreach (KeyValuePair<SettingEntry<bool>, DebugValueRendererDelegate> debugRenderer in DebugValueRenderers) {
                        if (!debugRenderer.Key.Value) continue;

                        displayedDebugValues.AddLast(debugRenderer.Value.Invoke(gameTime));
                    }

                    lblInfo.Text    = string.Join("; ", displayedDebugValues);
                    lblInfo.Visible = true;
                } else {
                    LoadRenderers();
                }
            } else {
                lblInfo.Visible = false;
            }
        }

    }
}
