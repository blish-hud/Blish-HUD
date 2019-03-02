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

        private Matrix _projection;
        public Matrix Projection => _projection;

        public float AspectRatio => (float)GameService.Graphics.WindowWidth / (float)GameService.Graphics.WindowHeight;

        protected override void Update(GameTime gameTime) {
            if (GameService.Gw2Mumble.Available) {
                _position = GameService.Gw2Mumble.MumbleBacking.CameraPosition.ToXnaVector3(); //Vector3.Lerp(_position, GameServices.GetService<Gw2MumbleService>().MumbleBacking.CameraPosition.ToXnaVector3(), LERPDURR);
                _forward  = GameService.Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3();    //Vector3.Lerp(_forward, GameServices.GetService<Gw2MumbleService>().MumbleBacking.CameraFront.ToXnaVector3(), LERPDURR);

                _view       = Matrix.CreateLookAt(this.Position, this.Position + _forward, Utils.DrawUtil.UpVectorFromCameraForward(GameService.Gw2Mumble.MumbleBacking.CameraFront.ToXnaVector3()));
                _projection = Matrix.CreatePerspectiveFieldOfView((float)GameService.Gw2Mumble.MumbleBacking.Identity.FieldOfView, this.AspectRatio, this.NearPlaneRenderDistance, this.FarPlaneRenderDistance);
            }
        }

        protected override void Initialize() { /* NOOP */ }
        protected override void Load() { /* NOOP */ }
        protected override void Unload() { /* NOOP */ }
    }
}
