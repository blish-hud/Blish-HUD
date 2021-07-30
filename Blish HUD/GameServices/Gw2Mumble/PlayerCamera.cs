using System;
using Blish_HUD.Entities;
using Gw2Sharp.Mumble;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Mumble {
    public class PlayerCamera : ICamera {

        private readonly Gw2MumbleService _service;

        private Vector3 _position    = Vector3.Zero;
        private Vector3 _forward     = Vector3.Forward;
        private float   _fieldOfView = 1;

        private Matrix _view;
        private Matrix _playerView;
        private Matrix _projection;
        private Matrix _worldViewProjection;
        
        public float NearPlaneRenderDistance { get; } = 0.01f;
        
        public float FarPlaneRenderDistance  { get; } = 1000.0f;

        #region Mumble Proxied Fields

        /// <inheritdoc cref="IGw2MumbleClient.CameraPosition"/>
        public Vector3 Position => _position;

        /// <inheritdoc cref="IGw2MumbleClient.CameraFront"/>
        public Vector3 Forward => _forward;

        /// <inheritdoc cref="IGw2MumbleClient.FieldOfView"/>
        public float FieldOfView => _fieldOfView;

        #endregion

        #region Calculated Fields

        public Matrix View                => _view;
        public Matrix PlayerView          => _playerView;
        public Matrix Projection          => _projection;
        public Matrix WorldViewProjection => _worldViewProjection;

        #endregion

        internal PlayerCamera(Gw2MumbleService service) {
            _service = service;
        }

        internal void Update(GameTime gameTime) {
            _position    = _service.RawClient.CameraPosition.ToXnaVector3();
            _forward     = _service.RawClient.CameraFront.ToXnaVector3();
            _fieldOfView = MathHelper.Clamp((float)_service.RawClient.FieldOfView, 0.01f, (float)Math.PI - 0.01f);

            // Calculated
            _view       = Matrix.CreateLookAt(_position, _position                         + _forward,                VectorUtil.UpVectorFromCameraForward(_forward));
            _playerView = Matrix.CreateLookAt(_position, _service.PlayerCharacter.Position + new Vector3(0, 0, 0.5f), VectorUtil.UpVectorFromCameraForward(_forward));
            _projection = Matrix.CreatePerspectiveFieldOfView(this.FieldOfView, GameService.Graphics.AspectRatio, this.NearPlaneRenderDistance, this.FarPlaneRenderDistance);

            _worldViewProjection = _view * _projection;
        }

    }
}
