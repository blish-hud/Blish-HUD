namespace Blish_HUD.GameServices.ArcDps.V2.Models {
    public enum ConditionResult : byte {
        ExpectedToHit = 0,
        InvulnerableByBuff = 1,
        InvulnerableByPlayerSkill1 = 2,
        InvulnerableByPlayerSkill2 = 3,
        InvulnerableByPlayerSkill3 = 4,
        Unknown = 5,
    }
}