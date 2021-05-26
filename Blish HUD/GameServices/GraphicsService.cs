using System;
using System.Collections.Concurrent;
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
            _world        = new World();
        }

        #endregion

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

        public FramerateMethod FrameLimiter {
            get => _frameLimiterSetting.Value;
            set => _frameLimiterSetting.Value = value;
        }

        public bool EnableVsync {
            get => _enableVsyncSetting.Value;
            set => _enableVsyncSetting.Value = value;
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

            this.UIScaleMultiplier = GetScaleRatio(UiSize.Normal);
            this.UIScaleTransform  = Matrix.CreateScale(Graphics.UIScaleMultiplier);

            _graphicsSettings = Settings.RegisterRootSettingCollection(GRAPHICS_SETTINGS);

            DefineSettings(_graphicsSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _frameLimiterSetting = settings.DefineSetting("FramerateLimiter", FramerateMethod.SyncWithGame, Strings.GameServices.GraphicsService.Setting_FramerateLimiter_DisplayName, Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description);
            _enableVsyncSetting = settings.DefineSetting("EnableVsync", false, Strings.GameServices.GraphicsService.Setting_Vsync_DisplayName, Strings.GameServices.GraphicsService.Setting_Vsync_Description);

            _frameLimiterSetting.SettingChanged += FrameLimiterSettingMethodChanged;
            _enableVsyncSetting.SettingChanged  += EnableVsyncChanged;

            EnableVsyncChanged(_enableVsyncSetting, new ValueChangedEventArgs<bool>(_enableVsyncSetting.Value, _enableVsyncSetting.Value));
            FrameLimiterSettingMethodChanged(_enableVsyncSetting, new ValueChangedEventArgs<FramerateMethod>(_frameLimiterSetting.Value, _frameLimiterSetting.Value));

            _frameLimiterSetting.SetExcluded(FramerateMethod.Custom);

            if (ApplicationSettings.Instance.TargetFramerate > 0) {
                _frameLimiterSetting.SetDisabled(); // Disable frame limiter setting - user has manually specified via launch arg

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
                    BlishHud.Instance.TargetElapsedTime = TimeSpan.FromMilliseconds(1d / ApplicationSettings.Instance.TargetFramerate);
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
            if (GameService.GameIntegration.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
                this.World.DoDraw(this.GraphicsDevice);
            GameService.Debug.StopTimeFunc("3D objects");

            // Slightly better scaling (text is a bit more legible)
            this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GameService.Debug.StartTimeFunc("UI Elements");
            if (this.SpriteScreen != null && this.SpriteScreen.Visible) {
                this.SpriteScreen.Draw(spriteBatch, this.SpriteScreen.LocalBounds, this.SpriteScreen.LocalBounds);
            }
            GameService.Debug.StopTimeFunc("UI Elements");

            GameService.Debug.StartTimeFunc("Render Queue");
            if (this._queuedRenders.TryDequeue(out var renderCall)) {
                renderCall.Invoke(this.GraphicsDevice);
            }
            GameService.Debug.StopTimeFunc("Render Queue");
        }

        protected override void Load() {
            GameService.Gw2Mumble.UI.UISizeChanged += UIOnUISizeChanged;
        }

        private void UIOnUISizeChanged(object sender, ValueEventArgs<UiSize> e) {
            this.UIScaleMultiplier = GetScaleRatio(e.Value);
            this.SpriteScreen.Size = new Point((int)(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth / this.UIScaleMultiplier),
                                               (int)(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight / this.UIScaleMultiplier));

            this.UIScaleTransform = Matrix.CreateScale(this.UIScaleMultiplier);
        }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.World.DoUpdate(gameTime);
            Entities.Effects.EntityEffect.UpdateEffects(gameTime);
            this.SpriteScreen.Update(gameTime);
        }
    }
}
