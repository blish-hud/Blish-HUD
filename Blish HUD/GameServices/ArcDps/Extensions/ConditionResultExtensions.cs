using static Blish_HUD.ArcDps.ArcDpsEnums;
namespace Blish_HUD.ArcDps {
    public static class ConditionResultExtensions {
        public static bool IsHit(this ConditionResult result) {
            return result == ConditionResult.ExpectedToHit;
        }
    }
}
