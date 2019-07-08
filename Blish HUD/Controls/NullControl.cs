using System.ServiceModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used as a <see cref="Control"/> proxy for caching utilities. This will not render anything.  If placed inside of a rendered container, an exception will be thrown.
    /// </summary>
    internal class NullControl : Control {

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            throw new ActionNotSupportedException($"{nameof(NullControl)} should never be painted!  Do not parent this control to a container!");
        }

    }
}
