using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Annotations;
using Microsoft.Scripting.Utils;

namespace Blish_HUD {
    public abstract class GameService :INotifyPropertyChanged {

        private static readonly GameService[] AllServices;
        public static IReadOnlyList<GameService> All => AllServices;

        // TODO: Would love to do this without needing to deal with events for this
        public event EventHandler<EventArgs> OnLoad;

        protected abstract void Initialize();
        protected abstract void Load();
        protected abstract void Unload();
        protected abstract void Update(GameTime gameTime);

        private protected Overlay Overlay;

        public void DoInitialize(Overlay game) {
            Overlay = game;

            Initialize();
        }

        public void DoLoad() {
            Load();

            // Trigger OnLoad event
            this.OnLoad?.Invoke(this, EventArgs.Empty);
        }

        public void DoUnload() => Unload();
        public void DoUpdate(GameTime gameTime) => Update(gameTime);
        
        #region Static Service References

        public static readonly DebugService Debug;
        public static readonly FileService FileSrv;
        public static readonly SettingsService Settings;
        public static readonly ContentService Content;
        public static readonly AnimationService Animation;
        public static readonly GraphicsService Graphics;
        public static readonly Gw2MumbleService Gw2Mumble;
        public static readonly PlayerService Player;
        public static readonly CameraService Camera;
        public static readonly InputService Input;
        public static readonly DirectorService Director;
        public static readonly GameIntegrationService GameIntegration;
        public static readonly HotkeysService Hotkeys;
        public static readonly PathingService Pathing;
        public static readonly ModuleService Module;
        public static readonly PersistentStoreService Store;
        public static readonly ArcDpsService ArcDps;

        #endregion

        static GameService() {
            // Init game services
            Debug = new DebugService();
            FileSrv = new FileService();
            Store = new PersistentStoreService();
            Settings = new SettingsService();
            Content = new ContentService();
            Animation = new AnimationService();
            Graphics = new GraphicsService();
            Gw2Mumble = new Gw2MumbleService();
            Player = new PlayerService();
            Camera = new CameraService();
            Input = new InputService();
            Director = new DirectorService();
            GameIntegration = new GameIntegrationService();
            Hotkeys = new HotkeysService();
            Pathing = new PathingService();
            Module = new ModuleService();
            ArcDps = new ArcDpsService();

            AllServices = new GameService[] {
                Debug,
                FileSrv,
                Settings,
                Content,
                Animation,
                Graphics,
                Gw2Mumble,
                Player,
                Camera,
                Input,
                Director,
                GameIntegration,
                Hotkeys,
                Pathing,
                Module,
                Store,
                ArcDps
            };

            GameServices.AddService<DebugService>(AllServices[0]);
            GameServices.AddService<FileService>(AllServices[1]);
            GameServices.AddService<SettingsService>(AllServices[2]);
            GameServices.AddService<ContentService>(AllServices[3]);
            GameServices.AddService<AnimationService>(AllServices[4]);
            GameServices.AddService<GraphicsService>(AllServices[5]);
            GameServices.AddService<Gw2MumbleService>(AllServices[6]);
            GameServices.AddService<PlayerService>(AllServices[7]);
            GameServices.AddService<CameraService>(AllServices[8]);
            GameServices.AddService<InputService>(AllServices[9]);
            GameServices.AddService<DirectorService>(AllServices[10]);
            GameServices.AddService<GameIntegrationService>(AllServices[11]);
            GameServices.AddService<HotkeysService>(AllServices[12]);
            GameServices.AddService<ModuleService>(AllServices[14]);
            GameServices.AddService<PersistentStoreService>(AllServices[15]);
            GameServices.AddService<ArcDpsService>(AllServices[16]);
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
