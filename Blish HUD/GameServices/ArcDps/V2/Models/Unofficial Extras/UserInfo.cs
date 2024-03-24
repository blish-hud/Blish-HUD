namespace Blish_HUD.GameServices.ArcDps.Models.UnofficialExtras {
    public struct UserInfo {
        public string AccountName { get; set; }

        public ulong JoinTime { get; set; }

        public UserRole Role { get; set; }

        public byte Subgroup { get; set; }

        public bool ReadyStatus { get; set; }

        public byte _unused1 { get; set; }

        public uint _unused2 { get; set; }
    }
}
