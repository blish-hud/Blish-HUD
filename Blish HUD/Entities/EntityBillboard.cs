using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Entities {

    public class EntityBillboard : Billboard {

        public Entity AttachedEntity { get; }

        /// <inheritdoc />
        public override Vector3 Position {
            get => AttachedEntity.Position + AttachedEntity.RenderOffset;
        }

        public EntityBillboard(Entity attachedEntity) {
            this.AttachedEntity = attachedEntity;
        }

    }

}
