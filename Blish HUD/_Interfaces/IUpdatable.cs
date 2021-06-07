using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public interface IUpdatable {

        /// <summary>
        /// Indicates that this can be updated as part of the standard update loop.
        /// </summary>
        void Update(GameTime gameTime);

    }
}
