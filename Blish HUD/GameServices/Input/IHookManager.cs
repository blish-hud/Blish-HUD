using System;

namespace Blish_HUD.Input {

    internal interface IHookManager : IDisposable {

        public void Load();

        public void Unload();

        public bool EnableHook();

        public void DisableHook();

        public void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback);

        public void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback);

        public void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

        public void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

    }
}
