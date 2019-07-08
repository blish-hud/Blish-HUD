﻿using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class CameraService : GameService {

        public float NearPlaneRenderDistance { get; set; } = 0.01f;
        public float FarPlaneRenderDistance { get; set; } = 1000.0f;

        private Vector3 _position;
        public Vector3 Position => _position;

        private Vector3 _forward;
        public Vector3 Forward => _forward;

        private Matrix _view;
        public Matrix View => _view;

        private Matrix _playerView;
        public Matrix PlayerView => _playerView;

        private Matrix _projection;
        public Matrix Projection => _projection;

        private Matrix _worldViewProjection;
        public Matrix WorldViewProjection => _worldViewProjection;

        protected override void Update(GameTime gameTime) {
            if (GameService.Gw2Mumble.Available) {
                _position = Gw2Mumble.MumbleBacking.CameraPosition.ToXnaVector3();
                _forward  = Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3();   

                _view       = Matrix.CreateLookAt(this.Position, this.Position + _forward, VectorUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                _playerView = Matrix.CreateLookAt(this.Position, Player.Position + new Vector3(0, 0, 0.5f), VectorUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                _projection = Matrix.CreatePerspectiveFieldOfView((float)Gw2Mumble.MumbleBacking.Identity.FieldOfView, Graphics.AspectRatio, this.NearPlaneRenderDistance, this.FarPlaneRenderDistance);

                _worldViewProjection = _view * _projection;
            }
        }

        protected override void Initialize() { /* NOOP */ }
        protected override void Load() { /* NOOP */ }
        protected override void Unload() { /* NOOP */ }
    }
}
