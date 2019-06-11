using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Behaviors;
using Blish_HUD.Pathing.Entities;
using Humanizer;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Behavior {

    public enum TacOBehavior : int {
        AlwaysVisible = 0,
        ReappearOnMapChange = 1,
        ReappearOnDailyReset = 2,
        OnlyVisibleBeforeActivation = 3,
        ReappearAfterTimer = 4,
        ReappearOnMapReset = 5,
        OncePerInstance = 6,
        DailyPerChar = 7,

        OncePerInstancePerChar = 8,
        WvWObjective = 9
    }

    public class BasicTOBehavior<TPathable, TEntity> : Default<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private const string TACOBEHAVIOR_STORENAME = "TacOBehaviors";

        private TacOBehavior _behavior;

        private bool _hiddenByBehavior = false;
        private bool HiddenByBehavior {
            get => _hiddenByBehavior;
            set {
                _hiddenByBehavior = value;
                this.ManagedPathable.ManagedEntity.Visible = !_hiddenByBehavior;
            }
        }

        private int _lastMap = -1; // Used by: ReappearOnMapChange
        private DateTimeOffset _lastInteract; // Used by: ReappearOnDailyReset

        public BasicTOBehavior(TPathable managedPathable, TacOBehavior behavior) : base(managedPathable) {
            _behavior = behavior;

            PrepareBehavior();
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (this.HiddenByBehavior)
                UpdateBehavior();
        }

        private PersistentStore _tacOBehaviorStore;
        private PersistentStore TacOBehaviorStore => _tacOBehaviorStore ?? (_tacOBehaviorStore = this.BehaviorStore.GetSubstore(TACOBEHAVIOR_STORENAME).GetSubstore(this._behavior.ToString()));

        private void PrepareBehavior() {
            //this.InfoText = this._behavior.ToString();
            //this.ZoneRadius = 4.5f; //((TacOMarkerPathable) this.ManagedPathable).TriggerRange;

            //this._indicator.Click += delegate {
            //    this.HiddenByBehavior = true;

            //    if (this._behavior == TacOBehavior.ReappearAfterTimer) {
            //        _lastInteract = DateTimeOffset.UtcNow;
            //        //var b = TacOBehaviorStore.GetSubstore(nameof(TacOBehavior.ReappearAfterTimer));
            //        this.TacOBehaviorStore.GetOrSetValue(this.ManagedPathable.Guid, _lastInteract);
            //    }
            //};

            switch (_behavior) {
                case TacOBehavior.AlwaysVisible:
                    /* NOOP */
                    break;
            }
        }

        private void UpdateBehavior() {
            switch (_behavior) {
                case TacOBehavior.ReappearOnMapChange:
                    if (GameService.Player.MapId != _lastMap)
                        this.HiddenByBehavior = false;

                    break;
                case TacOBehavior.ReappearOnDailyReset:
                    if (DateTimeOffset.UtcNow.Subtract(_lastInteract).Seconds > 0)
                        this.HiddenByBehavior = false;

                    break;

                case TacOBehavior.ReappearAfterTimer:
                    if (DateTimeOffset.UtcNow.Subtract(_lastInteract).Seconds > 10) {
                        this.HiddenByBehavior = false;
                        this.TacOBehaviorStore.RemoveValueByName(this.ManagedPathable.Guid);
                    }

                    break;
            }
        }

    }
}
