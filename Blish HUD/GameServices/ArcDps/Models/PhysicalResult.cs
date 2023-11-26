namespace Blish_HUD.GameServices.ArcDps.Models {
    public enum PhysicalResult : byte {
        Normal = 0,
        Critical = 1,
        Glance = 2,
        Blocked = 3,
        Evaded = 4,
        Interrupted = 5,
        Absorbed = 6,
        Blinded = 7,
        KillingBlow = 8,
        Downed = 9,
        BreakbarDamage = 10,
        Activation = 11,
        Unknown = 12,
    }
}