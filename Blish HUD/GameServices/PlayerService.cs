using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class PlayerService:GameService {


        public Vector3 Position { get; protected set; } = Vector3.Zero;
        public Vector3 Forward { get; protected set; } = Vector3.Zero;

        // Well this is kind of dumb - player service and gw2mumble need a stronger distinction
        public bool Available => GameService.Gw2Mumble.Available;

        // Context Events
        public event EventHandler<EventArgs> OnMapChanged;
        public event EventHandler<EventArgs> OnMapIdChanged;
        public event EventHandler<EventArgs> OnMapTypeChanged;
        public event EventHandler<EventArgs> OnShardIdChanged;
        public event EventHandler<EventArgs> OnInstanceChanged;

        // Identity Events
        public event EventHandler<EventArgs> OnCharacterNameChanged;
        public event EventHandler<EventArgs> OnCharacterProfessionChanged;
        public event EventHandler<EventArgs> OnRaceChanged;
        public event EventHandler<EventArgs> OnUiScaleChanged;

        #region Context Properties

        private Map _map;
        public Map Map => _map;

        private int _mapId = -1;
        public int MapId {
            get => _mapId;
            private set {
                if (_mapId == value) return;

                _mapId = value;

                Task<Map> mapNameTask = BHGw2Api.Map.GetFromId(_mapId);
                mapNameTask.ContinueWith(
                                         mapTsk => {
                                             if (!mapTsk.IsFaulted) {
                                                 this._map = mapTsk.Result;

                                                 OnPropertyChanged(nameof(this.Map));
                                                 this.OnMapChanged?.Invoke(this, EventArgs.Empty);
                                             }
                                         });

                this.OnMapIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _mapType;
        public int MapType {
            get => _mapType;
            set {
                if (_mapType == value) return;

                _mapType = value;

                this.OnMapTypeChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _shardId;
        public int ShardId {
            get => _shardId;
            set {
                if (_shardId == value) return;

                _shardId = value;

                this.OnShardIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _instance;
        public int Instance {
            get => _instance;
            set {
                if (_instance == value) return;

                _instance = value;

                this.OnInstanceChanged?.Invoke(this, EventArgs.Empty);
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

                this.OnCharacterNameChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _characterProfession;
        public int CharacterProfession {
            get => _characterProfession;
            set {
                if (_characterProfession == value) return;

                _characterProfession = value;

                this.OnCharacterProfessionChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _race;
        public int Race {
            get => _race;
            set {
                if (_race == value) return;

                _race = value;

                this.OnRaceChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _uiScale;
        public int UiScale {
            get => _uiScale;
            set {
                if (_uiScale == value) return;

                _uiScale = value;

                this.OnUiScaleChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        #endregion
        
        protected override void Initialize() {
        }

        private const float LERPDURR = 0.2f;
        protected override void Update(GameTime gameTime) {
            // This data is LERP'd to help smooth out movements
            // Mumble API updates at most 50 times per second
            if (GameService.Gw2Mumble.MumbleBacking != null) {
                //this.Position = Vector3.Lerp(this.Position, GameService.Gw2Mumble.MumbleBacking.AvatarPosition.ToXnaVector3(), LERPDURR);
                //this.Forward  = Vector3.Lerp(this.Forward, GameService.Gw2Mumble.MumbleBacking.AvatarFront.ToXnaVector3(),    LERPDURR);
                this.Position = GameService.Gw2Mumble.MumbleBacking.AvatarPosition.ToXnaVector3();
                this.Forward = GameService.Gw2Mumble.MumbleBacking.AvatarFront.ToXnaVector3();

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
