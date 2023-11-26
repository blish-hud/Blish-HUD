using Blish_HUD.GameServices.ArcDps.Models;
namespace Blish_HUD.ArcDps {
    public static class PhysicalResultExtensions {
        public static bool IsHit(this PhysicalResult result) {
            return result == PhysicalResult.Normal ||
                   result == PhysicalResult.Critical ||
                   result == PhysicalResult.Glance ||
                   result == PhysicalResult.KillingBlow;
            //Downed and Interrupt omitted for now due to double procing mechanics || result == ParseEnum.Result.Downed || result == ParseEnum.Result.Interrupt; 
        }
    }
}
