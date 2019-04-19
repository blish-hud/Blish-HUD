using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        private float _aspectRatio;
        public float AspectRatio => _aspectRatio;

        protected override void Update(GameTime gameTime) {
            if (GameService.Gw2Mumble.Available) {
                _aspectRatio = (float) Graphics.WindowWidth / (float) Graphics.WindowHeight;

                _position = Gw2Mumble.MumbleBacking.CameraPosition.ToXnaVector3(); //Vector3.Lerp(_position, GameServices.GetService<Gw2MumbleService>().MumbleBacking.CameraPosition.ToXnaVector3(), LERPDURR);
                _forward  = Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3();    //Vector3.Lerp(_forward, GameServices.GetService<Gw2MumbleService>().MumbleBacking.CameraFront.ToXnaVector3(), LERPDURR);

                _view       = Matrix.CreateLookAt(this.Position, this.Position + _forward, Utils.DrawUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                _playerView = Matrix.CreateLookAt(this.Position, Player.Position + new Vector3(0, 0, 0.5f), Utils.DrawUtil.UpVectorFromCameraForward(Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                _projection = Matrix.CreatePerspectiveFieldOfView((float)Gw2Mumble.MumbleBacking.Identity.FieldOfView, this.AspectRatio, this.NearPlaneRenderDistance, this.FarPlaneRenderDistance);

                _worldViewProjection = _view * _projection;
            }
        }

        protected override void Initialize() { /* NOOP */ }
        protected override void Load() { /* NOOP */ }
        protected override void Unload() { /* NOOP */ }
    }
}
