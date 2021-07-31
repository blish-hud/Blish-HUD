using System;
using Blish_HUD.DebugHelper.Models;
using Blish_HUD.Input;

namespace Blish_HUD.DebugHelper.Services {

    internal sealed class MouseHookService : IDebugService, IDisposable {

        private const int CALLBACK_TIMEOUT = 10;

        private readonly IMessageService        messageService;
        private readonly WinApiMouseHookManager manager;

        public MouseHookService(IMessageService messageService) {
            this.messageService = messageService;
            manager = new();
            manager.RegisterHandler(HookCallback);
        }

        public void Start() => manager.EnableHook();

        public void Stop() => manager.DisableHook();

        private bool HookCallback(MouseEventArgs args) {
            var message = new MouseEventMessage {
                EventType = (int)args.EventType,
                PointX    = args.PointX,
                PointY    = args.PointY,
                MouseData = args.MouseData,
                Flags     = args.Flags,
                Time      = args.Time,
                ExtraInfo = args.Extra
            };

            MouseResponseMessage? response = messageService.SendAndWait<MouseResponseMessage>(message, TimeSpan.FromMilliseconds(CALLBACK_TIMEOUT));

            return response?.IsHandled == true;
        }

        public void Dispose() {
            Stop();
            manager.Dispose();
        }

    }

}
