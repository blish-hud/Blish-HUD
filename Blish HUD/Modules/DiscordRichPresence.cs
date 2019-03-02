using DiscordRPC;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Blish_HUD.BHGw2Api;
using Humanizer;

namespace Blish_HUD.Modules {
    public class DiscordRichPresence : Module {

        private const string DISCORD_APP_ID = "498585183792922677";

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "(General) Discord Rich Presence Module",
                "bh.general.discordrp",
                "Integrates with Discord to show what you're up to in the world of Guild Wars 2.",
                "LandersXanders.1235",
                "1"
            );
        }

        private SettingEntry<bool> settingHideDetailsInWvW;

        public override void DefineSettings(Settings settings) {
            //settingHideDetailsInWvW = settings.DefineSetting("Hide Detailed Location in WvW", true, true, true, "Prevents people on Discord from being able to see closest landmark details while you're in WvW.");
        }

        private DiscordRpcClient rpcClient;
        
        private DateTime startTime;

        private Dictionary<int, Map> MapLookup = new Dictionary<int, Map>();

        private enum MapType {
            PvP = 2,
            Instance = 4,
            PvE = 5,
            Eternal_Battlegrounds  = 9,
            WvW_Blue = 10,
            WvW_Green = 11,
            WvW_Red = 12,
            Edge_of_The_Mists = 15,
            Dry_Top = 16
        }

        private Dictionary<string, string> mapOverrides = new Dictionary<string, string>() {
            { "1206", "fractals_of_the_mists" },  // Mistlock Sanctuary
            { "350", "fractals_of_the_mists" },   // Heart of the Mists
            { "95", "eternal_battlegrounds" },    // Alpine Borderlands
            { "96", "eternal_battlegrounds" },    // Alpine Borderlands
        };

        private Dictionary<int, string> contextOverrides = new Dictionary<int, string>() {
            
        };

        private string TruncateLength(string value, int maxLength) {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private string GetDiscordSafeString(string text) {
            return Regex.Replace(text.Replace(":", "").Trim(), @"[^a-zA-Z]+", "_").ToLower();
        }

        private void UpdateDetails() {
            if (GameService.Player.Map == null) return;

            // rpcClient *shouldn't* be null at this point unless a rare race condition occurs
            // In the event that this occurs, it'll be resolved by the next loop
            rpcClient?.SetPresence(new RichPresence() {
                // Truncate length (requirements: https://discordapp.com/developers/docs/rich-presence/how-to)
                // Identified in: [BLISHHUD-11]
                Details = TruncateLength(GameService.Player.CharacterName, 128),
#if DEBUG
                State = "Working on Blish HUD",
#else
                State = TruncateLength($"in {GameService.Player.Map.Name}", 128),
#endif
                Assets = new Assets() {
                    LargeImageKey = TruncateLength(mapOverrides.ContainsKey(GameService.Player.Map.Id) ? mapOverrides[GameService.Player.Map.Id] : GetDiscordSafeString(GameService.Player.Map.Name), 32),
                    LargeImageText = TruncateLength(GameService.Player.Map.Name, 128),
                    SmallImageKey = TruncateLength(((MapType)GameService.Player.MapType).ToString().ToLower(), 32),
                    SmallImageText = TruncateLength(((MapType)GameService.Player.MapType).ToString().Replace("_", " "), 128)
                },
                Timestamps = new Timestamps() {
                    Start = startTime
                }
            });

            rpcClient?.Invoke();
        }
        
        public override void Update(GameTime gameTime) {
            //if (GameService.GameIntegration.Gw2IsRunning && GameService.Player.Available)
            //    UpdateDetails();

            //pcClient?.Invoke();
        }

        private void InitRichPresence() {
            if (!this.Enabled) return;

            try {
                startTime = GameService.GameIntegration.Gw2Process.StartTime.ToUniversalTime();
            } catch (Exception ex) {
                GameService.Debug.WriteWarningLine("Could not establish GW2 start time.  Using 'now'.");
                startTime = DateTime.Now;
            }

            rpcClient = new DiscordRpcClient(DISCORD_APP_ID);
            rpcClient.Initialize();

            UpdateDetails();
        }

        private void CleanUpRichPresence() {
            // Disposing rpcClient also clears presence
            rpcClient?.Dispose();
            rpcClient = null;
        }

        protected override void OnEnabled() {
            // Update character name
            GameService.Player.OnCharacterNameChanged += delegate { UpdateDetails(); };

            // Update map
            GameService.Player.OnMapChanged += delegate { UpdateDetails(); };

            // Initiate presence when the game is opened
            GameService.GameIntegration.OnGw2Started += delegate { InitRichPresence(); };

            // Clear presence when the game is closed
            GameService.GameIntegration.OnGw2Closed += delegate { CleanUpRichPresence(); };

            if (GameService.GameIntegration.Gw2IsRunning)
                InitRichPresence();
        }

        protected override void OnDisabled() {
            base.OnDisabled();

            CleanUpRichPresence();
        }

    }
}
