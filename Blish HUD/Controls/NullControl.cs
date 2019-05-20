using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used as a <see cref="Control"/> proxy for caching utilities. This will not render anything.
    /// </summary>
    internal class NullControl : Control {

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            throw new ActionNotSupportedException($"{nameof(NullControl)} should never be painted!");
        }

    }
}
