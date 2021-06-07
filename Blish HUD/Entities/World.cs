using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World : IRenderable, IUpdatable, IWorld {

        public ICamera Camera { get; set; }
        
        private readonly ConcurrentQueue<(IEntity Entity, bool IsAdded)> _pendingEntityAction = new ConcurrentQueue<(IEntity Entity, bool IsAdded)>();

        private readonly SynchronizedCollection<IEntity> _entities = new SynchronizedCollection<IEntity>();

        public IEnumerable<IEntity> Entities => GetEntities(false);

        public World(ICamera camera) {
            this.Camera = camera;
        }

        public void AddEntity(IEntity entity) {
            _pendingEntityAction.Enqueue((entity, true));
        }

        public void AddEntities(IEnumerable<IEntity> entities) {
            foreach (var entity in entities) {
                AddEntity(entity);
            }
        }

        public void RemoveEntity(IEntity entity) {
            _pendingEntityAction.Enqueue((entity, false));
        }

        public void RemoveEntities(IEnumerable<IEntity> entities) {
            foreach (var entity in entities) {
                RemoveEntity(entity);
            }
        }

        private IEnumerable<IEntity> GetEntities(bool sorted) {
            lock (_entities.SyncRoot) {
                return sorted
                           ? _entities.OrderByDescending(e => e.DrawOrder).ToArray()
                           : _entities.ToArray();
            }
        }

        private void HandlePendingEntities() {
            lock (_entities.SyncRoot) {
                while (_pendingEntityAction.TryDequeue(out var pendingEntityAction)) {
                    if (pendingEntityAction.IsAdded) {
                        _entities.Add(pendingEntityAction.Entity);
                    } else {
                        _entities.Remove(pendingEntityAction.Entity);
                    }
                }
            }
        }

        private void UpdateEntities(GameTime gameTime) {
            foreach (var entity in GetEntities(false)) {
                entity.Update(gameTime);
            }
        }

        public void Update(GameTime gameTime) {
            HandlePendingEntities();
            UpdateEntities(gameTime);
        }

        public void Render(GraphicsDevice graphicsDevice) {
            if (this.Camera == null) return;

            graphicsDevice.BlendState        = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0]  = SamplerState.LinearWrap;
            graphicsDevice.RasterizerState   = RasterizerState.CullNone;

            foreach (var entity in GetEntities(true)) {
                entity.Render(graphicsDevice, this, this.Camera);
            }
        }
    }

}
