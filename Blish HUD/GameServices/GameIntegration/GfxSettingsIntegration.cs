using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Debug;
using Blish_HUD.GameIntegration.GfxSettings;
using Blish_HUD.GameServices;

namespace Blish_HUD.GameIntegration {
    public sealed class GfxSettingsIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<GfxSettingsIntegration>();

        private const string GFXSETTINGS_PATH = "Guild Wars 2";
        private const string GFXSETTINGS_NAME = "GFXSettings.Gw2-64.exe.xml";

        private const string GFXS_TRUE = "true";

        private const int FILELOCKED_ATTEMPTS = 3;

        public event EventHandler<EventArgs> GfxSettingsReloaded;

        /// <summary>
        /// Indicates that we've successfully read and parsed the contents of the GFXSettings.Gw2-64.exe.xml file.
        /// </summary>
        public bool IsAvailable { get; private set; }

        public FrameLimitSetting? FrameLimit => GetStringEnumSetting(FrameLimitSetting.FromString);

        public ShadowsSetting? Shadows => GetStringEnumSetting(ShadowsSetting.FromString);

        public ReflectionsSetting? Reflections => GetStringEnumSetting(ReflectionsSetting.FromString);

        public CharModelLimitSetting? CharModelLimit => GetStringEnumSetting(CharModelLimitSetting.FromString);

        public ScreenModeSetting? ScreenMode => GetStringEnumSetting(ScreenModeSetting.FromString);

        public AntiAliasingSetting? AntiAliasing => GetStringEnumSetting(AntiAliasingSetting.FromString);

        public TextureDetailSetting? TextureDetail => GetStringEnumSetting(TextureDetailSetting.FromString);

        public AnimationSetting? Animation => GetStringEnumSetting(AnimationSetting.FromString);

        public CharModelQualitySetting? CharModelQuality => GetStringEnumSetting(CharModelQualitySetting.FromString);

        public EnvironmentSetting? Environment => GetStringEnumSetting(EnvironmentSetting.FromString);

        public LodDistanceSetting? LodDistance => GetStringEnumSetting(LodDistanceSetting.FromString);

        public PostProcSetting? PostProc => GetStringEnumSetting(PostProcSetting.FromString);

        public SamplingSetting? Sampling => GetStringEnumSetting(SamplingSetting.FromString);

        public ShadersSetting? Shaders => GetStringEnumSetting(ShadersSetting.FromString);

        public float? Gamma => GetFloatSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "Effect LOD" which is described in game as:
        /// "Limit detail of particle effects."
        /// </summary>
        public bool? EffectLod => GetBoolSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "High-Res Character Textures" which is described in game as:
        /// "Enables high-resolution textures for NPCs and other player character modules.  Requires a map change to take effect."
        /// </summary>
        public bool? HighResCharacter => GetBoolSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "Best Texture Filtering" which is described in game as:
        /// "Override the default texture filtering to use the best."
        /// </summary>
        public bool? BestTextureFiltering => GetBoolSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "Depth Blur" which is described in game as:
        /// "Enables depth blurring effects for a stylized appearance on distance objects."
        /// </summary>
        public bool? DepthBlur => GetBoolSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "Vertical Sync" which is described in game as:
        /// "Forces the framerate to syncrhronize with the monitor's refresh rate.  Helps elimiate tearing artifacts but can result in artificually low framerates."
        /// </summary>
        public bool? VerticalSync => GetBoolSetting();

        /// <summary>
        /// Indicates the value of the in-game setting "DPI Scaling" which is described in game as:
        /// "Enables additional scaling of the UI according to your system settings."
        /// </summary>
        public bool? DpiScaling => GetBoolSetting();

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private FileSystemWatcher _fileSystemWatcher;

        private bool _loadLock;

        internal GfxSettingsIntegration(GameIntegrationService service) : base(service) { /* NOOP */ }

        public override void Load() {
            _service.Gw2Instance.Gw2Started += Gw2Proc_Gw2Started;

            EnableWatchDir();

            Task.Run(LoadGfxSettings);
        }

        private bool? GetBoolSetting([CallerMemberName] string settingName = null) {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return _settings.TryGetValue(settingName, out string result)
                       ? result == GFXS_TRUE
                       : default(bool?);
        }

        private float? GetFloatSetting([CallerMemberName] string settingName = null) {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return _settings.TryGetValue(settingName, out string result)
                       ? InvariantUtil.TryParseFloat(result, out float floatResult)
                             ? floatResult
                             : default(float?)
                       : default;
        }    

        private T? GetStringEnumSetting<T>(Func<string, T?> getSettingFunc, [CallerMemberName] string settingName = null) where T : struct {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return _settings.TryGetValue(settingName, out string result)
                       ? getSettingFunc(result)
                       : null;
        }

        private void EnableWatchDir() {
            string gw2AppDataPath = Path.Combine(_service.Gw2Instance.AppDataPath, GFXSETTINGS_PATH);

            if (!Directory.Exists(gw2AppDataPath)) {
                Logger.Warn("Guild Wars 2 AppData path '{appDataPath}' does not appear to exist so GfxSettings will not be loaded.", gw2AppDataPath);
                return;
            }

            _fileSystemWatcher = new FileSystemWatcher {
                Path                  = gw2AppDataPath,
                NotifyFilter          = NotifyFilters.LastWrite,
                Filter                = GFXSETTINGS_NAME,
                EnableRaisingEvents   = true,
                IncludeSubdirectories = false
            };

            _fileSystemWatcher.Changed += GfxSettingsFileChanged;
        }

        private bool _changedDebounce = false;

        private async void GfxSettingsFileChanged(object sender, FileSystemEventArgs e) {
            // This typically fires twice
            if (_changedDebounce) {
                return;
            }

            _changedDebounce = true;

            // GW2 is usually still locked when we detect the file change, so we give it a chance to let go
            await Task.Delay(100);
            await LoadGfxSettings();

            _changedDebounce = false;
        }

        private bool TryGetGfxSettingsFileStream(out FileStream gfxSettingsFileStream) {
            string path = Path.Combine(_service.Gw2Instance.AppDataPath, GFXSETTINGS_PATH, GFXSETTINGS_NAME);

            gfxSettingsFileStream = null;

            if (!File.Exists(path)) {
                Logger.Debug($"Failed to load GfxSettings from path '{path}'.");
                return false;
            }

            gfxSettingsFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, true);

            return gfxSettingsFileStream.CanRead;
        }

        private async Task LoadGfxSettings() {
            if (_loadLock) {
                return;
            }

            _loadLock = true;

            await LoadGfxSettings(FILELOCKED_ATTEMPTS);

            _loadLock = false;
        }

        private async Task LoadGfxSettings(int remainingAttempts) {
            try {
                if (TryGetGfxSettingsFileStream(out var gfxSettingsFileStream)) {
                    using (var gfxSettingsXmlReader = XmlReader.Create(gfxSettingsFileStream, new XmlReaderSettings { Async = true })) {
                        await gfxSettingsXmlReader.MoveToContentAsync();

                        while (gfxSettingsXmlReader.ReadToFollowing("OPTION")) {
                            gfxSettingsXmlReader.MoveToAttribute("Name");
                            string settingName = await gfxSettingsXmlReader.GetValueAsync();
                            gfxSettingsXmlReader.MoveToAttribute("Value");
                            string settingValue = await gfxSettingsXmlReader.GetValueAsync();

                            _settings[settingName] = settingValue;

                            Logger.Trace($"Loaded {settingName} = {settingValue} from GSA.");

                            this.IsAvailable = true;

                            GfxSettingsReloaded?.Invoke(this, EventArgs.Empty);
                        }
                    }

                    gfxSettingsFileStream.Dispose();

                    Logger.Debug("Finished parsing GSA file.");

                    // Easiest place to check where we should now know if the user is in fullscreen or not
                    if (this.IsAvailable) {
                        ContingencyChecks.CheckForFullscreenDx9Conflict();
                    }
                }
            } catch (IOException ex) {
                if (remainingAttempts > 0) {
                    // GW2 is likely still locking the file and should be done very soon.
                    Logger.Debug("Failed to read GSA file.  Trying again...");
                    await Task.Delay(100);
                    await LoadGfxSettings(remainingAttempts - 1);
                } else {
                    Logger.Warn(ex, $"Failed to parse GfxSettings after {FILELOCKED_ATTEMPTS} attempts.");
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to parse GfxSettings.");
            }
        }

        private async void Gw2Proc_Gw2Started(object sender, EventArgs e) {
            await LoadGfxSettings();
        }

        public override void Unload() {
            _service.Gw2Instance.Gw2Started -= Gw2Proc_Gw2Started;

            if (_fileSystemWatcher != null) {
                _fileSystemWatcher.Changed -= GfxSettingsFileChanged;
                _fileSystemWatcher.Dispose();
            }
        }
    }
}
