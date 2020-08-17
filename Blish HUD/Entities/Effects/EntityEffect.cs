using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Effects {
    public abstract class EntityEffect : Effect {

        private static readonly Logger Logger = Logger.GetLogger<EntityEffect>();

        private static readonly HashSet<EntityEffect> _loadedEffects = new HashSet<EntityEffect>();

        internal static void UpdateEffects(GameTime gameTime) {
            foreach (var loadedEffect in _loadedEffects.ToArray()) {
                if (loadedEffect.IsDisposed) {
                    Logger.Debug("An EntityEffect was disposed of.");
                    _loadedEffects.Remove(loadedEffect);
                    continue;
                }

                loadedEffect.View       = GameService.Gw2Mumble.PlayerCamera.View;
                loadedEffect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;

                loadedEffect.Update(gameTime);
            }
        }

        private static void RegisterEntityEffect(EntityEffect effectInstance) {
            Logger.Debug("EntityEffect {effectName} was registered.", effectInstance.GetType().FullName);
            _loadedEffects.Add(effectInstance);
        }

        private const string PARAMETER_WORLD      = "World";
        private const string PARAMETER_VIEW       = "View";
        private const string PARAMETER_PROJECTION = "Projection";

        private Matrix _world, _view, _projection;

        /// <summary>
        /// The per-entity matrix.
        /// </summary>
        public Matrix World {
            set {
                if (SetProperty(ref _world, value)) {
                    this.Parameters[PARAMETER_WORLD].SetValue(_world);
                }
            }
        }

        /// <summary>
        /// Representation of the active camera's look-at matrix.
        /// </summary>
        public Matrix View {
            set {
                if (SetProperty(ref _view, value)) {
                    this.Parameters[PARAMETER_VIEW].SetValue(_view);
                }
            }
        }

        /// <summary>
        /// The projection matrix created from the camera based on field of view and aspect ratio.
        /// </summary>
        public Matrix Projection {
            set {
                if (SetProperty(ref _projection, value)) {
                    this.Parameters[PARAMETER_PROJECTION].SetValue(_projection);
                }
            }
        }

        #region ctors

        /// <inheritdoc />
        protected EntityEffect(Effect cloneSource) : base(cloneSource) {
            RegisterEntityEffect(this);
        }

        /// <inheritdoc />
        protected EntityEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) {
            RegisterEntityEffect(this);
        }

        /// <inheritdoc />
        protected EntityEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) {
            RegisterEntityEffect(this);
        }

        #endregion

        #region `SetParameter`s

        protected bool SetParameter(string parameterName, ref Matrix property, Matrix newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Matrix[] property, Matrix[] newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Quaternion property, Quaternion newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Texture2D property, Texture2D newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector2 property, Vector2 newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector2[] property, Vector2[] newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector3 property, Vector3 newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector3[] property, Vector3[] newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector4 property, Vector4 newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref Vector4[] property, Vector4[] newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref bool property, bool newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref float property, float newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref float[] property, float[] newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        protected bool SetParameter(string parameterName, ref int property, int newValue) {
            bool assignmentResult = SetProperty(ref property, newValue);

            if (assignmentResult) {
                this.Parameters[parameterName].SetValue(newValue);
            }

            return assignmentResult;
        }

        #endregion

        protected abstract void Update(GameTime gameTime);

        protected bool SetProperty<T>(ref T property, T newValue) {
            if (Equals(property, newValue)) return false;

            property = newValue;

            return true;
        }

    }
}
