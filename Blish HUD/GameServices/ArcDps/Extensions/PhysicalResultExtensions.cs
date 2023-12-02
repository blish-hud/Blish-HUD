using static Blish_HUD.ArcDps.ArcDpsEnums;
namespace Blish_HUD.ArcDps {
    public static class PhysicalResultExtensions {
        public static bool IsHit(this PhysicalResult result) {
            return result == PhysicalResult.Normal ||
                   result == PhysicalResult.Crit ||
                   result == PhysicalResult.Glance ||
                   result == PhysicalResult.KillingBlow;
            //Downed and Interrupt omitted for now due to double procing mechanics || result == ParseEnum.Result.Downed || result == ParseEnum.Result.Interrupt; 
        }
    }
}
