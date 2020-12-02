using static Blish_HUD.ArcDps.ArcDpsEnums;
namespace Blish_HUD.ArcDps {
    public static class StateChangeAgentExtensions {
        public static bool SrcIsAgent(this StateChange state) {
            return state == StateChange.None || state == StateChange.EnterCombat
                || state == StateChange.ExitCombat || state == StateChange.ChangeUp
                || state == StateChange.ChangeDead || state == StateChange.ChangeDown
                || state == StateChange.Spawn || state == StateChange.Despawn
                || state == StateChange.HealthUpdate || state == StateChange.WeaponSwap
                || state == StateChange.MaxHealthUpdate || state == StateChange.PointOfView
                || state == StateChange.BuffInitial || state == StateChange.Position
                || state == StateChange.Velocity || state == StateChange.Rotation
                || state == StateChange.TeamChange || state == StateChange.AttackTarget
                || state == StateChange.Targetable || state == StateChange.StackActive
                || state == StateChange.StackReset || state == StateChange.BreakbarState
                || state == StateChange.BreakbarPercent;
        }

        public static bool DstIsAgent(this StateChange state) {
            return state == StateChange.None || state == StateChange.AttackTarget;
        }

        public static bool HasTime(this StateChange state) {
            return state == StateChange.None || state == StateChange.EnterCombat
                || state == StateChange.ExitCombat || state == StateChange.ChangeUp
                || state == StateChange.ChangeDead || state == StateChange.ChangeDown
                || state == StateChange.Spawn || state == StateChange.Despawn
                || state == StateChange.HealthUpdate || state == StateChange.WeaponSwap
                || state == StateChange.MaxHealthUpdate || state == StateChange.BuffInitial
                || state == StateChange.Position || state == StateChange.Velocity
                || state == StateChange.Rotation || state == StateChange.TeamChange
                || state == StateChange.AttackTarget || state == StateChange.Targetable
                || state == StateChange.StackActive || state == StateChange.StackReset
                || state == StateChange.Reward || state == StateChange.BreakbarState
                || state == StateChange.BreakbarPercent;
        }
    }
}
