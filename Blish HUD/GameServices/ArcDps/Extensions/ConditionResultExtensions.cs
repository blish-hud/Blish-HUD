using Blish_HUD.GameServices.ArcDps.Models;
namespace Blish_HUD.ArcDps {
    public static class ConditionResultExtensions {
        public static bool IsHit(this ConditionResult result) {
            return result == ConditionResult.ExpectedToHit;
        }
    }
}
