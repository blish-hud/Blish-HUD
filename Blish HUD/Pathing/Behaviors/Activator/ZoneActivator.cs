using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors.Activator {

    public enum DistanceFrom {
        Player,
        PlayerCamera
    }

    public class ZoneActivator : Activator {

        public float ActivationDistance { get; set; } = 2f;

        public DistanceFrom DistanceFrom { get; set; } = DistanceFrom.Player;

        public Vector3 Position { get; set; } = Vector3.Zero;

        /// <inheritdoc />
        public override void Update(GameTime gameTime) {
            var farPoint = this.DistanceFrom == DistanceFrom.Player
                               ? GameService.Player.Position
                               : GameService.Camera.Position;

            if (Vector3.Distance(Position, farPoint) <= this.ActivationDistance) {
                if (!this.Active)
                    this.Activate();
            } else if (this.Active) {
                this.Deactivate();
            }

            base.Update(gameTime);
        }

    }
}
