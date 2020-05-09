using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;

namespace Blish_HUD.Input {

    internal class DebugHelperMouseHookManager : DebugHelperInputHookManager<HandleMouseInputDelegate, MouseEventMessage>, IMouseHookManager {

        public DebugHelperMouseHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(MouseEventMessage message) {
            var mouseEventArgs = new MouseEventArgs((MouseEventType)message.EventType, message.PointX, message.PointY, message.MouseData, message.Flags, message.Time, message.ExtraInfo);
            var isHandled = false;
            foreach (var handler in Handlers) {
                isHandled = handler(mouseEventArgs);
                if (isHandled)
                    break;
            }

            DebugHelperMessageService.Send(new MouseResponseMessage {
                Id = message.Id,
                IsHandled = isHandled
            });
        }

        protected override void DummyHookCallback(MouseEventMessage message) {
            DebugHelperMessageService.Send(new MouseResponseMessage {
                Id = message.Id,
                IsHandled = false
            });
        }
    }
}
