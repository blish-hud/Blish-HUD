using System;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Graphics {
    public interface IEffect : IDisposable {

        public EffectTechnique CurrentTechnique { get; set; }

        public EffectParameterCollection Parameters { get; }

        public EffectTechniqueCollection Techniques { get; }
    }
}
