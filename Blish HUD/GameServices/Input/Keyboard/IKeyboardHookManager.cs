namespace Blish_HUD.Input {

    delegate bool HandleKeyboardInputDelegate(KeyboardEventArgs keyboardEventArgs);

    internal interface IKeyboardHookManager {

        bool EnableHook();

        void DisableHook();

        void RegisterHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

        void UnregisterHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);
    }
}
