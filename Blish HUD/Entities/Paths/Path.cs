using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Paths {
    public abstract class Path : Entity {

        /// <summary>
        /// Used by the Pathing service to either add or remove this path
        /// from the list of renderables whenever a map change occurs.  A
        /// MapId of -1 will prevent the pathing service from managing if
        /// this entity renders or not.
        /// </summary>
        public int MapId { get; protected set; }

        public virtual List<Vector3> PathPoints { get; protected set; }

        public Texture2D PathTexture { get; set; }

    }
}
