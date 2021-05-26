namespace Blish_HUD.Graphics {
    public enum FramerateMethod : int {
        Custom        = -1,
        SyncWithGame  = 0,
        LockedTo30Fps = 1,
        LockedTo60Fps = 2,
        Unlimited     = 3,
    }
}
