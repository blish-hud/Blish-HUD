using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World : IRenderable, IUpdatable, IWorld {

        public ICamera Camera { get; set; }

        private readonly ConcurrentQueue<(IEntity Entity, bool IsAdded)> _pendingEntityAction = new ConcurrentQueue<(IEntity Entity, bool IsAdded)>();

        private readonly SynchronizedCollection<IEntity> _entities = new SynchronizedCollection<IEntity>();

        // Reused every frame so that updating and rendering entities doesn't allocate
        private IEntity[] _entityBuffer = Array.Empty<IEntity>();
        private int       _entityCount;
        private ulong[]   _renderOrder  = Array.Empty<ulong>();

        public IEnumerable<IEntity> Entities {
            get {
                lock (_entities.SyncRoot) {
                    return _entities.ToArray();
                }
            }
        }

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

        private void RefreshEntityBuffer() {
            lock (_entities.SyncRoot) {
                int count         = _entities.Count;
                int previousCount = _entityCount;

                if (_entityBuffer.Length < count) {
                    // Leave some headroom so that entities being added over several frames don't regrow the buffer every frame
                    _entityBuffer = new IEntity[count + count / 4 + 16];
                } else if (count < previousCount) {
                    // Entities were removed so clear the now-unused slots so the buffer doesn't keep them alive
                    Array.Clear(_entityBuffer, count, previousCount - count);
                }

                _entities.CopyTo(_entityBuffer, 0);
                _entityCount = count;
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
            RefreshEntityBuffer();

            for (int i = 0; i < _entityCount; i++) {
                _entityBuffer[i].Update(gameTime);
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

            if (_renderOrder.Length < _entityCount) {
                _renderOrder = new ulong[_entityBuffer.Length];
            }

            for (int i = 0; i < _entityCount; i++) {
                _renderOrder[i] = StableSortKey.Descending(_entityBuffer[i].DrawOrder, i);
            }

            Array.Sort(_renderOrder, 0, _entityCount);

            for (int i = 0; i < _entityCount; i++) {
                _entityBuffer[StableSortKey.GetIndex(_renderOrder[i])].Render(graphicsDevice, this, this.Camera);
            }
        }

        private static class StableSortKey {

            [StructLayout(LayoutKind.Explicit)]
            private struct FloatBits {
                [FieldOffset(0)] public float F;
                [FieldOffset(0)] public uint  U;
            }

            private static uint OrderedBits(float key) {
                if (float.IsNaN(key)) return 0;
                if (key == 0f) key = 0f; // Normalize -0 to +0

                uint bits = new FloatBits { F = key }.U;
                return (bits & 0x80000000u) != 0 ? ~bits : bits | 0x80000000u;
            }

            public static ulong Descending(float key, int index) => ((ulong)~OrderedBits(key) << 32) | (uint)index;

            public static int GetIndex(ulong sortKey) => (int)(uint)sortKey;
        }
    }

}
