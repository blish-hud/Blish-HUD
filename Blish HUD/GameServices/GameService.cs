using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Blish_HUD {
    public abstract class GameService : INotifyPropertyChanged {

        private static readonly GameService[] _allServices;
        public static IReadOnlyList<GameService> All => _allServices;

        public event EventHandler<EventArgs> FinishedLoading;

        public virtual void OnFinishedLoading(EventArgs e) {
            this.FinishedLoading?.Invoke(this, e);
        }

        protected abstract void Initialize();
        protected abstract void Load();
        protected abstract void Unload();
        protected abstract void Update(GameTime gameTime);

        private protected BlishHud ActiveBlishHud;

        public bool Loaded { get; private set; }

        public void DoInitialize(BlishHud game) {
            ActiveBlishHud = game;

            Initialize();
        }

        public void DoLoad() {
            Load();

            this.Loaded = true;
            OnFinishedLoading(EventArgs.Empty);
        }

        public void DoUnload() {
            Unload();

            this.Loaded = false;
        }

        public void DoUpdate(GameTime gameTime) => Update(gameTime);

        #region Static Service References

        public static readonly   DebugService           Debug;
        public static readonly   SettingsService        Settings;
        public static readonly   ContentService         Content;
        public static readonly   AnimationService       Animation;
        public static readonly   GraphicsService        Graphics;
        public static readonly   OverlayService         Overlay;
        public static readonly   Gw2WebApiService       Gw2WebApi;
        public static readonly   Gw2MumbleService       Gw2Mumble;
        internal static readonly Gw2ApiService          Gw2Api;
        public static readonly   PlayerService          Player;
        public static readonly   CameraService          Camera;
        public static readonly   InputService           Input;
        public static readonly   GameIntegrationService GameIntegration;
        public static readonly   PathingService         Pathing;
        public static readonly   ModuleService          Module;
        public static readonly   PersistentStoreService Store;
        public static readonly   ArcDpsService          ArcDps;
        public static readonly   ContextsService        Contexts;

        #endregion

        static GameService() {
            // Init game services
            Debug           = new DebugService();
            Input           = new InputService();
            Store           = new PersistentStoreService();
            Settings        = new SettingsService();
            Content         = new ContentService();
            Animation       = new AnimationService();
            Graphics        = new GraphicsService();
            Overlay         = new OverlayService();
            Gw2WebApi       = new Gw2WebApiService();
            Gw2Mumble       = new Gw2MumbleService();
            Gw2Api          = new Gw2ApiService();
            Player          = new PlayerService();
            Camera          = new CameraService();
            GameIntegration = new GameIntegrationService();
            Pathing         = new PathingService();
            Module          = new ModuleService();
            ArcDps          = new ArcDpsService();
            Contexts        = new ContextsService();

            _allServices = new GameService[] {
                Debug,
                Store,
                Settings,
                Content,
                Animation,
                Graphics,
                Overlay,
                Gw2WebApi,
                Gw2Mumble,
                Gw2Api,
                Player,
                Camera,
                Input,
                GameIntegration,
                Pathing,
                Module,
                ArcDps,
                Contexts
            };

        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
