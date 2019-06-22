using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Blish_HUD.Entities {

    public class EntityBillboard : Billboard {

        private Entity _attachedEntity;

        public Entity AttachedEntity { get; }

        /// <inheritdoc />
        public override Vector3 Position {
            get => AttachedEntity.Position;
        }

        public EntityBillboard(Entity attachedEntity) {
            this.AttachedEntity = attachedEntity;
        }

    }

}
