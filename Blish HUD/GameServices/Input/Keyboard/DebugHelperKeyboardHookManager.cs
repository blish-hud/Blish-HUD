using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class DebugHelperKeyboardHookManager : DebugHelperInputHookManager<HandleKeyboardInputDelegate, KeyboardEventMessage>, IKeyboardHookManager {

        private static readonly Logger Logger = Logger.GetLogger<DebugHelperKeyboardHookManager>();

        public DebugHelperKeyboardHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(KeyboardEventMessage message) {
            var KeyboardEventArgs = new KeyboardEventArgs((KeyboardEventType)message.EventType, (Keys)message.Key);
            var isHandled = false;
            foreach (var handler in Handlers) {
                isHandled = handler(KeyboardEventArgs);
                if (isHandled)
                    break;
            }

            DebugHelperMessageService.Send(new KeyboardResponseMessage {
                Id = message.Id,
                IsHandled = isHandled
            });
        }

        protected override void DummyHookCallback(KeyboardEventMessage message) {
            DebugHelperMessageService.Send(new KeyboardResponseMessage {
                Id = message.Id,
                IsHandled = false
            });
        }
    }
}
