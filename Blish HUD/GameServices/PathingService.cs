﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Blish_HUD.PersistentStore;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class PathingService : GameService {

        public const string MARKER_DIRECTORY = "markers";

        private const string PATHING_STORENAME = "Pathing";

        public event EventHandler<EventArgs> NewMapLoaded;

        private readonly ConcurrentQueue<IPathable<Entity>> _queuedAddPathables = new ConcurrentQueue<IPathable<Entity>>();
        private readonly ConcurrentQueue<IPathable<Entity>> _queuedRemovePathables = new ConcurrentQueue<IPathable<Entity>>();

        public List<IPathable<Entity>> Pathables { get; set; } = new List<IPathable<Entity>>();
        
        public SynchronizedCollection<PathableResourceManager> PackManagers { get; set; } = new SynchronizedCollection<PathableResourceManager>();

        private Store _pathingStore;

        public Store PathingStore => _pathingStore ?? (_pathingStore = Store.RegisterStore(PATHING_STORENAME));

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() { /* NOOP */ }

        private void ProcessAddedPathable(IPathable<Entity> pathable) {
            if (Graphics.World.Entities.Contains(pathable.ManagedEntity)) return;

            Graphics.World.Entities.Add(pathable.ManagedEntity);
            this.Pathables.Add(pathable);
        }

        private void ProcessRemovedPathable(IPathable<Entity> pathable) {
            Graphics.World.Entities.Remove(pathable.ManagedEntity);
            this.Pathables.Remove(pathable);
        }

        public void RegisterPathable(IPathable<Entity> pathable) {
            if (pathable == null) return;

            _queuedAddPathables.Enqueue(pathable);
        }

        public void UnregisterPathable(IPathable<Entity> pathable) {
            if (pathable == null) return;

            _queuedRemovePathables.Enqueue(pathable);
        }

        public void RegisterPathableResourceManager(PathableResourceManager pathableContext) {
            if (pathableContext == null) return;

            this.PackManagers.Add(pathableContext);
        }

        public void UnregisterPathableResourceManager(PathableResourceManager pathableContext) {
            this.PackManagers.Remove(pathableContext);
        }

        protected override void Update(GameTime gameTime) {
            while (_queuedAddPathables.TryDequeue(out IPathable<Entity> queuedPathable)) {
                ProcessAddedPathable(queuedPathable);
            }

            while (_queuedRemovePathables.TryDequeue(out IPathable<Entity> queuedRemovedPathable)) {
                ProcessRemovedPathable(queuedRemovedPathable);
            }

            foreach (IPathable<Entity> pathable in this.Pathables) {
                if (!pathable.Active) continue;

                pathable.Update(gameTime);
            }
        }

        protected override void Unload() { /* NOOP */ }

    }

}
