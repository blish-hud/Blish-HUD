using Blish_HUD.GameServices.ArcDps.Models;
namespace Blish_HUD.ArcDps {
    public static class ActivationExtensions {
        public static bool StartCasting(this Activation activation) {
            return activation == Activation.Normal ||
                   activation == Activation.Quickness;
        }

        public static bool EndCasting(this Activation activation) {
            return activation == Activation.CancelFire ||
                   activation == Activation.Reset ||
                   activation == Activation.CancelCancel;
        }
    }
}
