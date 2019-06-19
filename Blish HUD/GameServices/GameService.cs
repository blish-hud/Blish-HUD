using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Blish_HUD.Annotations;

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

        private protected Overlay ActiveOverlay;

        public bool Loaded { get; private set; }

        public void DoInitialize(Overlay game) {
            ActiveOverlay = game;

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

        public static readonly DebugService           Debug;
        public static readonly DirectoryService       Directory;
        public static readonly SettingsService        Settings;
        public static readonly ContentService         Content;
        public static readonly AnimationService       Animation;
        public static readonly GraphicsService        Graphics;
        public static readonly Gw2ApiService          Gw2Api;
        public static readonly Gw2MumbleService       Gw2Mumble;
        public static readonly PlayerService          Player;
        public static readonly CameraService          Camera;
        public static readonly InputService           Input;
        public static readonly DirectorService        Director;
        public static readonly GameIntegrationService GameIntegration;
        public static readonly HotkeysService         Hotkeys;
        public static readonly PathingService         Pathing;
        public static readonly ModuleService          Module;
        public static readonly PersistentStoreService Store;
        public static readonly ArcDpsService          ArcDps;

        #endregion

        static GameService() {
            // Init game services
            Debug           = new DebugService();
            Directory       = new DirectoryService();
            Input           = new InputService();
            Store           = new PersistentStoreService();
            Settings        = new SettingsService();
            Content         = new ContentService();
            Animation       = new AnimationService();
            Graphics        = new GraphicsService();
            Gw2Api          = new Gw2ApiService();
            Gw2Mumble       = new Gw2MumbleService();
            Player          = new PlayerService();
            Camera          = new CameraService();
            Director        = new DirectorService();
            GameIntegration = new GameIntegrationService();
            Hotkeys         = new HotkeysService();
            Pathing         = new PathingService();
            Module          = new ModuleService();
            ArcDps          = new ArcDpsService();

            _allServices = new GameService[] {
                Debug,
                Directory,
                Store,
                Settings,
                Content,
                Animation,
                Graphics,
                Gw2Api,
                Gw2Mumble,
                Player,
                Camera,
                Input,
                Director,
                GameIntegration,
                Hotkeys,
                Pathing,
                Module
            };

        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
