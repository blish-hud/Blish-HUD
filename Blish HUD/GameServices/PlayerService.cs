using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD {
    public class PlayerService:GameService {

        public Vector3 Position { get; protected set; } = Vector3.Zero;
        public Vector3 Forward { get; protected set; } = Vector3.Zero;

        public bool Available => GameService.Gw2Mumble.Available;

        // Context Events
        public event EventHandler<EventArgs> MapChanged;
        public event EventHandler<EventArgs> MapIdChanged;
        public event EventHandler<EventArgs> MapTypeChanged;
        public event EventHandler<EventArgs> ShardIdChanged;
        public event EventHandler<EventArgs> InstanceChanged;

        // Identity Events
        public event EventHandler<EventArgs> CharacterNameChanged;
        public event EventHandler<EventArgs> CharacterProfessionChanged;
        public event EventHandler<EventArgs> RaceChanged;
        public event EventHandler<EventArgs> UiScaleChanged;

        #region Context Properties

        private Map _map;
        public Map Map => _map;

        private int _mapId = -1;
        public int MapId {
            get => _mapId;
            private set {
                if (_mapId == value) return;

                _mapId = value;

                this.MapIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();

                Task<Map> mapNameTask = GameService.Gw2Api.SharedClient.V2.Maps.GetAsync(_mapId);
                mapNameTask.ContinueWith(mapTsk => {
                                             if (!mapTsk.IsFaulted) {
                                                 _map = mapTsk.Result;

                                                 OnPropertyChanged(nameof(this.Map));
                                                 this.MapChanged?.Invoke(this, EventArgs.Empty);
                                             }
                                         });
            }
        }

        private int _mapType;
        public int MapType {
            get => _mapType;
            set {
                if (_mapType == value) return;

                _mapType = value;

                this.MapTypeChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _shardId;
        public int ShardId {
            get => _shardId;
            set {
                if (_shardId == value) return;

                _shardId = value;

                this.ShardIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _instance;
        public int Instance {
            get => _instance;
            set {
                if (_instance == value) return;

                _instance = value;

                this.InstanceChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Identity Properties

        private string _characterName;
        public string CharacterName {
            get => _characterName;
            set {
                if (_characterName == value) return;

                _characterName = value;

                this.CharacterNameChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _characterProfession;
        public int CharacterProfession {
            get => _characterProfession;
            set {
                if (_characterProfession == value) return;

                _characterProfession = value;

                this.CharacterProfessionChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _race;
        public int Race {
            get => _race;
            set {
                if (_race == value) return;

                _race = value;

                this.RaceChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _uiScale;
        public int UiScale {
            get => _uiScale;
            set {
                if (_uiScale == value) return;

                _uiScale = value;

                this.UiScaleChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        #endregion
        
        protected override void Initialize() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            if (GameService.Gw2Mumble.MumbleBacking != null) {
                this.Position = GameService.Gw2Mumble.MumbleBacking.AvatarPosition.ToXnaVector3();
                this.Forward  = GameService.Gw2Mumble.MumbleBacking.AvatarFront.ToXnaVector3();

                this.MapId    = GameService.Gw2Mumble.MumbleBacking.Context.MapId;
                this.MapType  = GameService.Gw2Mumble.MumbleBacking.Context.MapType;
                this.ShardId  = GameService.Gw2Mumble.MumbleBacking.Context.ShardId;
                this.Instance = GameService.Gw2Mumble.MumbleBacking.Context.Instance;

                this.CharacterName       = GameService.Gw2Mumble.MumbleBacking.Identity.Name;
                this.CharacterProfession = (int)GameService.Gw2Mumble.MumbleBacking.Identity.Profession;
                this.Race                = (int)GameService.Gw2Mumble.MumbleBacking.Identity.Race;

                this.UiScale = GameService.Gw2Mumble.MumbleBacking.Identity.UiScale;
            }
        }

        protected override void Load() {
            
        }

        protected override void Unload() {

        }

    }
}
