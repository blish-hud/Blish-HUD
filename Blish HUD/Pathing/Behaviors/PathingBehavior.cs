using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {
    public abstract class PathingBehavior {

        private const string PATHINGBEHAVIOR_STORENAME = "Behaviors";

        public static List<Type> AllAvailableBehaviors { get; }

        #region Load Static

        private static readonly PersistentStore.Store _behaviorStore;

        static PathingBehavior() {
            _behaviorStore = GameService.Pathing.PathingStore.GetSubstore(PATHINGBEHAVIOR_STORENAME);

            AllAvailableBehaviors = PathingBehaviorAttribute.GetTypes(System.Reflection.Assembly.GetExecutingAssembly()).ToList();
        }

        #endregion

        protected PersistentStore.Store BehaviorStore => _behaviorStore;

        public virtual void Load() { /* NOOP */ }

        public abstract void UpdateBehavior(GameTime gameTime);

    }
}