namespace Blish_HUD.Input {
    internal delegate bool HandleMouseInputDelegate(MouseEventArgs mouseEventArgs);

    internal interface IMouseHookManager {

        public bool EnableHook();

        public void DisableHook();

        public void RegisterHandler(HandleMouseInputDelegate handleMouseInputCallback);

        public void UnregisterHandler(HandleMouseInputDelegate handleMouseInputCallback);
    }
}
