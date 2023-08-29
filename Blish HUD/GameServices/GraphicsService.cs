using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Graphics;
using Blish_HUD.Settings;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = Microsoft.Xna.Framework.Point;

namespace Blish_HUD {
    public class GraphicsService:GameService {

        private const string GRAPHICS_SETTINGS = "GraphicsConfiguration";

        private const int TARGET_MAX_FRAMETIME = 14;
        private const int MIN_QUEUED_RENDERS   = 1;

        private static readonly Point MinimumUnscaledGameResolution = new Point(1024, 768);

        #region Load Static

        private static readonly Screen _spriteScreen;
        private static readonly World  _world;
        private static readonly uint _legacyDpi;

        static GraphicsService() {
            _spriteScreen = new Screen();
            _world        = new World(GameService.Gw2Mumble.PlayerCamera);
            _legacyDpi    = GetDpiLegacy();

            GameService.Gw2Mumble.FinishedLoading += delegate(object sender, EventArgs args) {
                _world.Camera = GameService.Gw2Mumble.PlayerCamera;
            };
        }

        #endregion

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, MONITOR_DEFAULT dwFlags);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

        private enum MONITOR_DEFAULT : uint {
            MONITOR_DEFAULTTONULL = 0,    // Returns NULL.
            MONITOR_DEFAULTTOPRIMARY = 1, // Returns a handle to the primary display monitor.
            MONITOR_DEFAULTTONEAREST = 2, // Returns a handle to the display monitor that is nearest to the window.
        }

        private enum MONITOR_DPI_TYPE {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
        }

        private uint GetDpi() {
            Version osVersion = Environment.OSVersion.Version;

            switch (osVersion.Major, osVersion.Minor) {
                case (6, 3):  // win8.1
                    return GetDpiWin81();

                case (10, 0): // win10 & 11
                    if (osVersion.Build >= 14393) {
                        // Windows 10 1607 or newer
                        return GetDpiForWindow(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle);
                    } else {
                        return GetDpiWin81();
                    }

                // win8/2008r2/win7/older
                default:
                    return _legacyDpi;
            }
        }

        private static uint GetDpiLegacy() {
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                return (uint)g.DpiY;
            }
        }

        private uint GetDpiWin81() {
            try {
                IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
                IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULT.MONITOR_DEFAULTTONEAREST);

                int hr = GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint _, out uint dpiY);
                Marshal.ThrowExceptionForHR(hr);

                return dpiY;
            }
            catch {
                return _legacyDpi;
            }

        }

        public float GetScaleRatio(UiSize currScale) {
            if (this.UIScalingMethod != ManualUISize.SyncWithGame) {
                switch (this.UIScalingMethod) {
                    case ManualUISize.Small:
                        currScale = UiSize.Small;
                        break;
                    case ManualUISize.Normal:
                        currScale = UiSize.Normal;
                        break;
                    case ManualUISize.Large:
                        currScale = UiSize.Large;
                        break;
                    case ManualUISize.Larger:
                        currScale = UiSize.Larger;
                        break;
                }
            }

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
                    uint dpi = GetDpi();

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

        [Obsolete("To ensure exclusive use of the graphics device use GameService.Graphics.LendGraphicsDevice().", true)]
        public GraphicsDevice GraphicsDevice => BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice;

        public int WindowWidth  => BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice.Viewport.Width;
        public int WindowHeight => BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice.Viewport.Height;

        public  float AspectRatio { get; private set; }

        public SettingCollection GraphicsSettings { get; private set; }

        private SettingEntry<FramerateMethod> _frameLimiterSetting;
        private SettingEntry<bool>            _smoothCharacterPositionSetting;
        private SettingEntry<DpiMethod>       _dpiScalingMethodSetting;
        private SettingEntry<ManualUISize>    _UISizeSetting;

        public FramerateMethod FrameLimiter {
            get => ApplicationSettings.Instance.TargetFramerate > 0
                       ? FramerateMethod.Custom
                       : _frameLimiterSetting.Value;
            set => _frameLimiterSetting.Value = value;
        }

        public bool SmoothCharacterPosition {
            get => _smoothCharacterPositionSetting.Value;
            set => _smoothCharacterPositionSetting.Value = value;
        }

        public DpiMethod DpiScalingMethod {
            get => _dpiScalingMethodSetting.Value;
            set => _dpiScalingMethodSetting.Value = value;
        }
        public ManualUISize UIScalingMethod {
            get => _UISizeSetting.Value;
            set => _UISizeSetting.Value = value;
        }

        public Point Resolution {
            get => new Point(BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth, BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);
            set {
                if (!this.Resolution.Equals(value)) {
                    try {
                        using (var ctx = GameService.Graphics.LendGraphicsDeviceContext()) {
                            BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth  = value.X;
                            BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight = value.Y;
                            BlishHud.Instance.ActiveGraphicsDeviceManager.ApplyChanges();
                        }
                        
                        // Exception would be from the code above, but don't update our
                        // scaling if there is an exception
                        ScreenSizeUpdated(value);
                    } catch (SharpDXException sdxe) {
                        // If device lost, we should hopefully handle in device lost event below
                    }
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

            this.GraphicsSettings = Settings.RegisterRootSettingCollection(GRAPHICS_SETTINGS);

            DefineSettings(this.GraphicsSettings);
        }

        private void DefineSettings(SettingCollection settings) {
            _frameLimiterSetting = settings.DefineSetting("FramerateLimiter",
                                                          FramerateMethod.LockedTo60Fps,
                                                          () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_DisplayName,
                                                          () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description);

            if (_frameLimiterSetting.Value == FramerateMethod.SyncWithGame || _frameLimiterSetting.Value == FramerateMethod.Custom) {
                // SyncWithGame is no longer supported.  It causes more problems than it solves.
                // We revert to the default settings for both the framerate limiter and vsync.

                // Likewise, Custom framerates are only possible via launch option currently.
                // Old versions could enable it, so this fixes that.
                _frameLimiterSetting.Value = FramerateMethod.LockedTo60Fps;
            }

            _smoothCharacterPositionSetting = settings.DefineSetting("EnableCharacterPositionBuffer",
                                                                     true,
                                                                     () => Strings.GameServices.GraphicsService.Setting_SmoothCharacterPosition_DisplayName,
                                                                     () => Strings.GameServices.GraphicsService.Setting_SmoothCharacterPosition_Description);

            _dpiScalingMethodSetting = settings.DefineSetting(nameof(DpiScalingMethod),
                                                                     DpiMethod.SyncWithGame,
                                                                     () => Strings.GameServices.GraphicsService.Setting_DPIScaling_DisplayName,
                                                                     () => Strings.GameServices.GraphicsService.Setting_DPIScaling_Description);

            _UISizeSetting = settings.DefineSetting(nameof(UIScalingMethod),
                                                                     ManualUISize.SyncWithGame,
                                                                     () => Strings.GameServices.GraphicsService.Setting_UIScaling_DisplayName,
                                                                     () => Strings.GameServices.GraphicsService.Setting_UIScaling_Description);
            
            _frameLimiterSetting.SettingChanged += FrameLimiterSettingMethodChanged;
            FrameLimiterSettingMethodChanged(_frameLimiterSetting, new ValueChangedEventArgs<FramerateMethod>(_frameLimiterSetting.Value, _frameLimiterSetting.Value));

            _frameLimiterSetting.SetExcluded(FramerateMethod.Custom, FramerateMethod.SyncWithGame, FramerateMethod.TrueUnlimited);

            // User has specified a custom FPS target via launch arg
            if (ApplicationSettings.Instance.TargetFramerate > 0) {
                _frameLimiterSetting.SetDisabled();
                _frameLimiterSetting.GetDescriptionFunc = () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description + Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Locked_Description;

                FrameLimiterSettingMethodChanged(_frameLimiterSetting, new ValueChangedEventArgs<FramerateMethod>(FramerateMethod.Custom, FramerateMethod.Custom));
            }

            // User has unlocked the FPS via launch arg
            if (ApplicationSettings.Instance.UnlockFps) {
                // Disable frame limiter setting and update description - user has unlocked the FPS via launch arg
                _frameLimiterSetting.SetDisabled();
                _frameLimiterSetting.GetDescriptionFunc = () => Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Description + Strings.GameServices.GraphicsService.Setting_FramerateLimiter_Locked_Description;

                FrameLimiterSettingMethodChanged(_frameLimiterSetting, new ValueChangedEventArgs<FramerateMethod>(FramerateMethod.TrueUnlimited, FramerateMethod.TrueUnlimited));
            }
        }

        private void FrameLimiterSettingMethodChanged(object sender, ValueChangedEventArgs<FramerateMethod> e) {
            bool currentVsync = GraphicsDeviceManager.SynchronizeWithVerticalRetrace;

            var frameRateLookup = new Dictionary<FramerateMethod, (bool IsFixedTimeStep, TimeSpan TargetElapsedTime, bool VSync)> {
                { FramerateMethod.Custom,        (true, TimeSpan.FromSeconds(1d / ApplicationSettings.Instance.TargetFramerate), false) }, // Only enabled with launch args
                { FramerateMethod.SyncWithGame,  (false, TimeSpan.FromMilliseconds(1), false) }, // Deprecated
                { FramerateMethod.LockedTo30Fps, (true, TimeSpan.FromSeconds(1d / 30d), false) },
                { FramerateMethod.LockedTo60Fps, (true, TimeSpan.FromSeconds(1d / 60d), false) },
                { FramerateMethod.LockedTo90Fps, (true, TimeSpan.FromSeconds(1d / 90d), false) },
                { FramerateMethod.Unlimited,     (false, TimeSpan.FromMilliseconds(1), true) }, // Unlimited with vsync (safe)
                { FramerateMethod.TrueUnlimited, (false, TimeSpan.FromMilliseconds(1), false) } // Unlimited without vsync (unsafe)
            };

            if (frameRateLookup.TryGetValue(e.NewValue, out var settings)) {
                BlishHud.Instance.IsFixedTimeStep = settings.IsFixedTimeStep;
                BlishHud.Instance.TargetElapsedTime = settings.TargetElapsedTime;
                if (settings.VSync != currentVsync) {
                    GraphicsDeviceManager.SynchronizeWithVerticalRetrace = settings.VSync;
                    GraphicsDeviceManager.ApplyChanges();
                }
            } else {
                // Shouldn't be possible unless settings are manually modified
                Logger.Warn($"Attempted to set the frame rate limiter to invalid value '{e.NewValue}'.  No changes to the frame limiter were made.");
            }
        }

        private readonly object _lendLockLow    = new object();
        private readonly object _lendLockNext   = new object();
        private readonly object _lendLockDevice = new object();

        /// <summary>
        /// Provides exclusive and locked access to the <see cref="GraphicsDevice"/>. This
        /// method blocks until the device is available and will yield to higher priority
        /// lend requests. Core lend requests receive priority over these requests.  Once
        /// done with the <see cref="GraphicsDevice"/> unlock it with <see cref="ReturnGraphicsDevice"/>.
        /// </summary>
        /// <param name="highPriority">
        /// If <c>true</c> then this thread will return as soon as the <see cref="GraphicsDevice"/>
        /// becomes available - ahead of all low priority lend requests.
        /// </param>
        internal GraphicsDevice LendGraphicsDevice(bool highPriority) {
            if (!highPriority) {
                Monitor.Enter(_lendLockLow);
            }

            if (Monitor.IsEntered(_lendLockDevice)) {
                Monitor.Enter(_lendLockDevice);
            } else {
                Monitor.Enter(_lendLockNext);
                Monitor.Enter(_lendLockDevice);
                Monitor.Exit(_lendLockNext);
            }

            return BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice;
        }

        /// <summary>
        /// Provides exclusive and locked access to the <see cref="GraphicsDevice"/>. This
        /// method blocks until the device is available and will yield to higher priority
        /// lend requests. Core lend requests receive priority over these requests.  Once
        /// done with the <see cref="GraphicsDevice"/> unlock it with <see cref="ReturnGraphicsDevice"/>.
        /// </summary>
        internal GraphicsDevice LendGraphicsDevice() {
            return LendGraphicsDevice(false);
        }

        /// <summary>
        /// Provides exclusive and locked access to the <see cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice"/>. This
        /// method blocks until the device is available and will yield to higher priority
        /// lend requests. Core lend requests receive priority over these requests.
        /// The returned <see cref="GraphicsDeviceContext"/> should be disposed of either
        /// via a <see langword="using"/> statement, or by calling
        /// <see cref="GraphicsDeviceContext.Dispose"/> directly.
        /// </summary>
        public GraphicsDeviceContext LendGraphicsDeviceContext() {
            return LendGraphicsDeviceContext(false);
        }

        /// <summary>
        /// Provides exclusive and locked access to the <see cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice"/>. This
        /// method blocks until the device is available and will yield to higher priority
        /// lend requests. Core lend requests receive priority over these requests.
        /// The returned <see cref="GraphicsDeviceContext"/> should be disposed of either
        /// via a <see langword="using"/> statement, or by calling
        /// <see cref="GraphicsDeviceContext.Dispose"/> directly.
        /// </summary>
        /// <param name="highPriority">
        /// If <see langword="true"/> then this thread will return as soon as the <see cref="GraphicsDeviceContext.GraphicsDevice"/>
        /// becomes available - ahead of all low priority lend requests.
        /// </param>
        internal GraphicsDeviceContext LendGraphicsDeviceContext(bool highPriority) {
            return new GraphicsDeviceContext(this, highPriority);
        }

        /// <summary>
        /// Unlocks access to the <see cref="GraphicsDevice"/>.  You must call this after <see cref="LendGraphicsDevice"/>.
        /// </summary>
        internal void ReturnGraphicsDevice(bool highPriority) {
            Monitor.Exit(_lendLockDevice);

            if (!highPriority) {
                Monitor.Exit(_lendLockLow);
            }
        }

        private static readonly Logger Logger = Logger.GetLogger<GraphicsService>();

        private readonly Stopwatch _renderTimer = Stopwatch.StartNew();

        internal void Render(GameTime gameTime, SpriteBatch spriteBatch) {
            _renderTimer.Restart();

            using GraphicsDeviceContext ctx = this.LendGraphicsDeviceContext();
            
            if (_renderTimer.ElapsedMilliseconds > 1) {
                Logger.Debug($"Render thread stalled for {_renderTimer.ElapsedMilliseconds} ms.");
            }

            ctx.GraphicsDevice.Clear(Color.Transparent);

            // Skip rendering all elements when UI is hidden
            if (GameService.Overlay.InterfaceHidden) return;

            GameService.Debug.StartTimeFunc("3D objects");
            // Only draw 3D elements if we are in game and map is closed
            if (GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen) {
                this.World.Render(ctx.GraphicsDevice);
            }
            GameService.Debug.StopTimeFunc("3D objects");

            // Slightly better scaling (text is a bit more legible)
            ctx.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GameService.Debug.StartTimeFunc("UI Elements");

            if (this.SpriteScreen != null && this.SpriteScreen.Visible) {
                this.SpriteScreen.Draw(spriteBatch, this.SpriteScreen.LocalBounds, this.SpriteScreen.LocalBounds);
            }

            GameService.Debug.StopTimeFunc("UI Elements");

            GameService.Debug.StartTimeFunc("Render Queue");
            for (int i = MIN_QUEUED_RENDERS; i > 0 && _queuedRenders.TryDequeue(out var renderCall); i--) {
                renderCall.Invoke(ctx.GraphicsDevice);

                if (_renderTimer.ElapsedMilliseconds < TARGET_MAX_FRAMETIME) {
                    i++;
                }
            }
            GameService.Debug.StopTimeFunc("Render Queue");
        }

        protected override void Load() { /* NOOP */ }

        private void Rescale() {
            Point backbufferSize = new Point(
                BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferWidth,
                BlishHud.Instance.ActiveGraphicsDeviceManager.PreferredBackBufferHeight);

            this.UIScaleMultiplier = GetDpiScaleRatio() * GetScaleRatio(GameService.Gw2Mumble.UI.UISize) * MinimumUnscaledGameResolution.GetAspectRatioScale(backbufferSize);
            this.SpriteScreen.Size = backbufferSize.UiToScale();

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
