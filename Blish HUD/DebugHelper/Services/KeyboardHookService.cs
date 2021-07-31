using System;
using Blish_HUD.DebugHelper.Models;
using Blish_HUD.Input;

namespace Blish_HUD.DebugHelper.Services {

    internal sealed class KeyboardHookService : IDebugService, IDisposable {

        private const int CALLBACK_TIMEOUT = 10;

        private readonly IMessageService           messageService;
        private readonly WinApiKeyboardHookManager manager;

        public KeyboardHookService(IMessageService messageService) {
            this.messageService = messageService;
            manager = new();
            manager.RegisterHandler(HookCallback);
        }

        public void Start() => manager.EnableHook();

        public void Stop() => manager.DisableHook();

        private bool HookCallback(KeyboardEventArgs args) {
            var message = new KeyboardEventMessage {
                EventType = (uint)args.EventType,
                Key = (int)args.Key
            };

            KeyboardResponseMessage? response = messageService.SendAndWait<KeyboardResponseMessage>(message, TimeSpan.FromMilliseconds(CALLBACK_TIMEOUT));

            return response?.IsHandled == true;
        }

        public void Dispose() {
            Stop();
            manager.Dispose();
        }

    }

}
