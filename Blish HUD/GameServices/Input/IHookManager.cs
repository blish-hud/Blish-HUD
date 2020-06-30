using System;

namespace Blish_HUD.Input {

    internal interface IHookManager : IDisposable {

        void Load();

        void Unload();

        bool EnableHook();

        void DisableHook();

        void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback);

        void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback);

        void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

        void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback);

    }

}
