using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Graphics;
using Blish_HUD.Settings;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public class GraphicsService:GameService {

        private const string GRAPHICS_SETTINGS = "GraphicsConfiguration";

        #region Load Static

        private static readonly Screen _spriteScreen;
        private static readonly World  _world;

        static GraphicsService() {
            _spriteScreen = new Screen();
            _world        = new World(GameService.Gw2Mumble.PlayerCamera);

            GameService.Gw2Mumble.FinishedLoading += delegate(object sender, EventArgs args) {
                _world.Camera = GameService.Gw2Mumble.PlayerCamera;
            };
        }

        #endregion

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hWnd);

        public float GetScaleRatio(UiSize currScale) {
            switch (currScale) {
                case UiSize.Small:
                    return 0.810f;
                case UiSize.Normal:
                    return 0.897f;
                case UiSize.Large:
                    return 1f;
                case UiSize.Larger:
                    return 1.103f;
            }

            return 1f;
        }

        public float GetDpiScaleRatio() {
            if (this.DpiScalingMethod == DpiMethod.UseGameDpi
             || this.DpiScalingMethod == DpiMethod.SyncWithGame && GameIntegration.GfxSettings.DpiScaling.GetValueOrDefault()) {
                int dpi = GetDpiForWindow(GameService.GameIntegration.Gw2Proc.Gw2WindowHandle);

                // If DPI is 0 then the window handle is likely not valid
                return dpi != 0
                           ? dpi / 96f
                           : 1f;
            }

            return 1f;
        }

        public  Matrix UIScaleTransform { get; private set; } = Matrix.Identity;

        public  float UIScaleMultiplier { get; private set; } = 1f;

        public Screen SpriteScreen => _spriteScreen;

        public World World => _world;

        public GraphicsDeviceManager GraphicsDeviceManager => BlishHud.Instance.ActiveGraphicsDeviceManager;

        public GraphicsDevice GraphicsDevice => BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice;

        public int WindowWidth => this.GraphicsDevice.Viewport.Width;
        public int WindowHeight => this.GraphicsDevice.Viewport.Height;

        public  float AspectRatio { get; private set; }

        internal SettingCollection _graphicsSettings;

        public SettingCollection GraphicsSettings => _graphicsSettings;

        private SettingEntry<FramerateMethod> _frameLimiterSetting;
        private SettingEntry<bool>            _enableVsyncSetting;
        private SettingEntry<bool>            _smoothCharacterPositionSetting;
        private SettingEntry<DpiMethod>       _dpiScalingMethodSetting;

        public FramerateMethod FrameLimiter {
            get => ApplicationSettings.Instance.TargetFramerate > 0
                       ? FramerateMethod.Custom
                       : _frameLimiterSetting.Value;
            set => _frameLimiterSetting.Value = value;
        }

        public bool EnableVsync {
            get => _enableVsyncSetting.Value;
            set => _enableVsyncSetting.Value = value;
        }

        public bool SmoothCharacterPosition {
            get => _smoothCharacterPositionSetting.Value;
            set => _smoothCharacterPositionSetting.Value = value;
        }
        
        public DpiMethod DpiScalingMethod {
            get => _dpiScalingMethodSetting.Value;
            set => _dpiScalingMethodSetting.Value = value;
        }

        public Point Resolution {
            get => new Point(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth, BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);
            set {
                try {
                    BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth  = value.X;
                    BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight = value.Y;

                    BlishHud.Instance.ActiveGraphicsDeviceManager.ApplyChanges();

                    // Exception would be from the code above, but don't update our
                    // scaling if there is an exception
                    ScreenSizeUpdated(value);
                } catch (SharpDX.SharpDXException sdxe) {
                    // If device lost, we should hopefully handle in device lost event below
                }
            }
        }

        private readonly ConcurrentQueue<Action<GraphicsDevice>> _queuedRenders = new ConcurrentQueue<Action<GraphicsDevice>>();

        /// <summary>
        /// Allows you to enqueue a call that will occur during the next time the update loop executes.
        /// </summary>
        /// <param name="call">A method accepting <see="GameTime" /> as a parameter.</param>
        public void QueueMainThreadRender(Action<GraphicsDevice> call) {
            _queuedRenders.Enqueue(call);
        }

        private void ScreenSizeUpdated(Point newSize) {
            // Update the SpriteScreen
            this.SpriteScreen.Size = new Point((int)(newSize.X / this.UIScaleMultiplier), (int)(newSize.Y / this.UIScaleMultiplier));

            // Update the aspect ratio
            this.AspectRatio = (float)Graphics.WindowWidth / (float)Graphics.WindowHeight;
        }

        protected override void Initialize() {
            // If for some reason we lose the rendering device, just restart the application
            // Might do better error handling later on
            ActiveBlishHud.GraphicsDevice.DeviceLost += delegate { GameService.Overlay.Restart(); };

            //this.UISizeMultiplier  = GetScaleRatio(UiSize.Normal);
            //this.UIScaleMultiplier = UISizeMultiplier * DpiMultiplier;
            //this.UIScaleTransform  = Matrix.CreateScale(Graphics.UIScaleMultiplier);

            _graphicsSettings = Settings.RegisterRootSettingCollection(GRAPHICS_SETTINGS);

            DefineSettings(_graphicsSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _frameLimiterSetting = settings.DefineSetting("FramerateLimiter",
                                                          FramerateMethod.SyncWithGame,
                                                          () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_DisplayName,
                                                          () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description);

            _enableVsyncSetting = settings.DefineSetting("EnableVsync",
                                                         true,
                                                         () => Strings.GameServices.GraphicsService.Setting_Vsync_DisplayName,
                                                         () => Strings.GameServices.GraphicsService.Setting_Vsync_Description);

            _smoothCharacterPositionSetting = settings.DefineSetting("EnableCharacterPositionBuffer",
                                                                     true,
                                                                     () => Strings.GameServices.GraphicsService.Setting_SmoothCharacterPosition_DisplayName,
                                                                     () => Strings.GameServices.GraphicsService.Setting_SmoothCharacterPosition_Description);

            _dpiScalingMethodSetting = settings.DefineSetting(nameof(DpiScalingMethod),
                                                        DpiMethod.SyncWithGame,
                                                        () => Strings.GameServices.GraphicsService.Setting_DPIScaling_DisplayName,
                                                        () => Strings.GameServices.GraphicsService.Setting_DPIScaling_Description);



            _frameLimiterSetting.SettingChanged += FrameLimiterSettingMethodChanged;
            _enableVsyncSetting.SettingChanged  += EnableVsyncChanged;

            EnableVsyncChanged(_enableVsyncSetting, new ValueChangedEventArgs<bool>(_enableVsyncSetting.Value, _enableVsyncSetting.Value));
            FrameLimiterSettingMethodChanged(_enableVsyncSetting, new ValueChangedEventArgs<FramerateMethod>(_frameLimiterSetting.Value, _frameLimiterSetting.Value));

            _frameLimiterSetting.SetExcluded(FramerateMethod.Custom);

            if (ApplicationSettings.Instance.TargetFramerate > 0) {
                // Disable frame limiter setting and update description - user has manually specified via launch arg
                _frameLimiterSetting.SetDisabled();
                _frameLimiterSetting.GetDescriptionFunc = () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description + Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Locked_Description;

                FrameLimiterSettingMethodChanged(_enableVsyncSetting, new ValueChangedEventArgs<FramerateMethod>(FramerateMethod.Custom, FramerateMethod.Custom));
            }
        }

        private void EnableVsyncChanged(object sender, ValueChangedEventArgs<bool> e) {
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = e.NewValue;
            GraphicsDeviceManager.ApplyChanges();
        }

        private void FrameLimiterSettingMethodChanged(object sender, ValueChangedEventArgs<FramerateMethod> e) {
            switch (e.NewValue) {
                case FramerateMethod.Custom: // Only enabled via launch options
                    BlishHud.Instance.IsFixedTimeStep   = true;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / ApplicationSettings.Instance.TargetFramerate);
                    break;
                case FramerateMethod.SyncWithGame:
                    BlishHud.Instance.IsFixedTimeStep   = false;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromMilliseconds(1);
                    break;
                case FramerateMethod.LockedTo30Fps:
                    BlishHud.Instance.IsFixedTimeStep   = true;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
                    break;
                case FramerateMethod.LockedTo60Fps:
                    BlishHud.Instance.IsFixedTimeStep   = true;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                    break;
                case FramerateMethod.LockedTo90Fps:
                    BlishHud.Instance.IsFixedTimeStep   = true;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / 90d);
                    break;
                case FramerateMethod.Unlimited:
                    BlishHud.Instance.IsFixedTimeStep   = false;
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromMilliseconds(1);
                    break;
            }
        }

        internal void Render(GameTime gameTime, SpriteBatch spriteBatch) {
            this.GraphicsDevice.Clear(Color.Transparent);

            GameService.Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game and map is closed
            if (GameService.GameIntegration.Gw2Proc.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
                this.World.Render(this.GraphicsDevice);
            GameService.Debug.StopTimeFunc("3D objects");

            // Slightly better scaling (text is a bit more legible)
            this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GameService.Debug.StartTimeFunc("UI Elements");
            if (this.SpriteScreen != null && this.SpriteScreen.Visible) {
                this.SpriteScreen.Draw(spriteBatch, this.SpriteScreen.LocalBounds, this.SpriteScreen.LocalBounds);
            }
            GameService.Debug.StopTimeFunc("UI Elements");

            GameService.Debug.StartTimeFunc("Render Queue");
            if (_queuedRenders.TryDequeue(out var renderCall)) {
                renderCall.Invoke(this.GraphicsDevice);
            }
            GameService.Debug.StopTimeFunc("Render Queue");
        }

        protected override void Load() { /* NOOP */ }

        private void Rescale() {
            this.UIScaleMultiplier = GetDpiScaleRatio() * GetScaleRatio(GameService.Gw2Mumble.UI.UISize);

            this.SpriteScreen.Size = new Point((int)(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth  / this.UIScaleMultiplier),
                                               (int)(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight / this.UIScaleMultiplier));

            this.UIScaleTransform = Matrix.CreateScale(this.UIScaleMultiplier);
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            Rescale();

            this.World.Update(gameTime);
            SharedEffect.UpdateEffects(gameTime);
            this.SpriteScreen.Update(gameTime);
        }
    }
}
