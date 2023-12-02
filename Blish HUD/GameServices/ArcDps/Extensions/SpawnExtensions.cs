using static Blish_HUD.ArcDps.ArcDpsEnums;
namespace Blish_HUD.ArcDps {
    public static class SpawnExtensions {
        public static bool IsSpawn(this StateChange state) {
            return state == StateChange.None || 
                   state == StateChange.Position || 
                   state == StateChange.Velocity || 
                   state == StateChange.Rotation || 
                   state == StateChange.MaxHealthUpdate || 
                   state == StateChange.Spawn || 
                   state == StateChange.TeamChange;
        }
    }
}
