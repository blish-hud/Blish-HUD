using System;
using Blish_HUD.Debug;
using Gw2Sharp.Models;
using Gw2Sharp.Mumble;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Mumble {
    public class PlayerCharacter {

        private readonly Gw2MumbleService _service;

        #region Events

        /// <summary>
        /// Fires when the current character's name changes such as when the player switches to a different character.
        /// </summary>
        public event EventHandler<ValueEventArgs<string>> NameChanged;

        /// <summary>
        /// Fires when the current character's specialization changes.
        /// </summary>
        public event EventHandler<ValueEventArgs<int>> SpecializationChanged;

        /// <summary>
        /// Fires when the current character starts or stops being a Commander.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsCommanderChanged;

        /// <summary>
        /// Fires when the current character enters or leaves combat.
        /// </summary>
        public event EventHandler<ValueEventArgs<bool>> IsInCombatChanged;

        /// <summary>
        /// Fires when the current characters mounts or dismounts.
        /// </summary>
        public event EventHandler<ValueEventArgs<MountType>> CurrentMountChanged;

        private void OnNameChanged(ValueEventArgs<string>            e) => this.NameChanged?.Invoke(this, e);
        private void OnSpecializationChanged(ValueEventArgs<int>     e) => this.SpecializationChanged?.Invoke(this, e);
        private void OnIsCommanderChanged(ValueEventArgs<bool>       e) => this.IsCommanderChanged?.Invoke(this, e);
        private void OnIsInCombatChanged(ValueEventArgs<bool>        e) => this.IsInCombatChanged?.Invoke(this, e);
        private void OnCurrentMountChanged(ValueEventArgs<MountType> e) => this.CurrentMountChanged?.Invoke(this, e);

        private string    _prevName;
        private int       _prevSpecialization;
        private bool      _prevIsCommander;
        private bool      _prevIsInCombat;
        private MountType _prevCurrentMount;

        private void HandleEvents() {
            MumbleEventImpl.CheckAndHandleEvent(ref _prevName,           this.Name,           OnNameChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevSpecialization, this.Specialization, OnSpecializationChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsCommander,    this.IsCommander,    OnIsCommanderChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevIsInCombat,     this.IsInCombat,     OnIsInCombatChanged);
            MumbleEventImpl.CheckAndHandleEvent(ref _prevCurrentMount,   this.CurrentMount,   OnCurrentMountChanged);
        }

        #endregion

        private Logger logger = Logger.GetLogger<PlayerCharacter>();

        private Vector3 _position = Vector3.Zero;
        private Vector3 _forward  = Vector3.Forward;
        private Vector3? _lastCamDirection = null;

        /// <inheritdoc cref="IGw2MumbleClien t.AvatarPosition"/>
        public Vector3 Position => _position;

        /// <inheritdoc cref="IGw2MumbleClient.AvatarFront"/>
        public Vector3 Forward => _forward;

        /// <inheritdoc cref="IGw2MumbleClient.CharacterName"/>
        public string Name => _service.RawClient.CharacterName;

        /// <inheritdoc cref="IGw2MumbleClient.Profession"/>
        public ProfessionType Profession => _service.RawClient.Profession;

        /// <inheritdoc cref="IGw2MumbleClient.Race"/>
        public RaceType Race => _service.RawClient.Race;

        /// <inheritdoc cref="IGw2MumbleClient.Specialization"/>
        public int Specialization => _service.RawClient.Specialization;

        /// <inheritdoc cref="IGw2MumbleClient.TeamColorId"/>
        public int TeamColorId => _service.RawClient.TeamColorId;

        /// <inheritdoc cref="IGw2MumbleClient.IsCommander"/>
        public bool IsCommander => _service.RawClient.IsCommander;

        /// <inheritdoc cref="IGw2MumbleClient.IsInCombat"/>
        public bool IsInCombat => _service.RawClient.IsInCombat;

        /// <inheritdoc cref="IGw2MumbleClient.Mount"/>
        public MountType CurrentMount => _service.RawClient.Mount;

        private const int POSITIONBUFFER_MAXSIZE  = 6;

        private readonly DynamicallySmoothedValue<Vector4> _positionBuffer = new DynamicallySmoothedValue<Vector4>(POSITIONBUFFER_MAXSIZE);

        internal PlayerCharacter(Gw2MumbleService service) {
            _service = service;
        }

        internal void Update(GameTime gameTime) {
            Vector3 camPosition = _service.RawClient.CameraPosition.ToXnaVector3();
            Vector3 camDirection = _service.RawClient.CameraFront.ToXnaVector3();
            Vector3 charPosition = _service.RawClient.AvatarPosition.ToXnaVector3();

            Matrix cam = Matrix.CreateLookAt(camPosition, camPosition + camDirection, new Vector3(0.0f, 1.0f, 0.0f));
            Matrix cami = Matrix.Invert(cam);

            if (_lastCamDirection.HasValue && Vector3.DistanceSquared(_lastCamDirection.Value, camDirection) >= 0.00001f) {
                _positionBuffer.flush();
            }

            _lastCamDirection = camDirection;

            _positionBuffer.PushValue(Vector4.Transform(new Vector4(charPosition, 1.0f), cam));

            Vector4 averagedCharPosition = Vector4.Transform(_positionBuffer.Value, cami);
            averagedCharPosition = Vector4.Divide(averagedCharPosition, averagedCharPosition.W);

            _position = GameService.Graphics.SmoothCharacterPosition
                            ? new Vector3(averagedCharPosition.X, averagedCharPosition.Y, averagedCharPosition.Z)
                            : _service.RawClient.AvatarPosition.ToXnaVector3();
            
            _forward  = _service.RawClient.AvatarFront.ToXnaVector3();

            HandleEvents();
        }

    }
}
