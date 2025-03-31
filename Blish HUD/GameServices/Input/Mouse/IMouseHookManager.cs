namespace Blish_HUD.Input {
    internal delegate bool HandleMouseInputDelegate(MouseEventArgs mouseEventArgs);

    internal interface IMouseHookManager {

        bool EnableHook();

        void DisableHook();

        void RegisterHandler(HandleMouseInputDelegate handleMouseInputCallback);

        void UnregisterHandler(HandleMouseInputDelegate handleMouseInputCallback);
    }
}
