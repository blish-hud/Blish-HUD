namespace Blish_HUD.Input {

    internal delegate bool HandleKeyboardInputDelegate(KeyboardEventArgs keyboardEventArgs);

    internal interface IKeyboardHookManager {

        public bool EnableHook();

        public void DisableHook();

        public void RegisterHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

        public void UnregisterHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

    }

}
