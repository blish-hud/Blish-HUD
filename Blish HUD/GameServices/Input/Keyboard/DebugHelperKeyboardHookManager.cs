using System.Collections;
using Blish_HUD.DebugHelper.Models;
using Blish_HUD.DebugHelper.Services;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class DebugHelperKeyboardHookManager : DebugHelperInputHookManager<HandleKeyboardInputDelegate, KeyboardEventMessage>, IKeyboardHookManager {

        public DebugHelperKeyboardHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(KeyboardEventMessage message) {
            var keyboardEventArgs = new KeyboardEventArgs((KeyboardEventType)message.EventType, (Keys)message.Key);
            bool isHandled = false;

            lock (((IList)this.Handlers).SyncRoot) {
                foreach (HandleKeyboardInputDelegate handler in this.Handlers) {
                    isHandled = handler(keyboardEventArgs);
                    if (isHandled) {
                        break;
                    }
                }
            }

            var response = new KeyboardResponseMessage {
                Id = message.Id,
                IsHandled = isHandled
            };

            this.DebugHelperMessageService.Send(response);
        }

        protected override void DummyHookCallback(KeyboardEventMessage message) {
            var response = new KeyboardResponseMessage {
                Id = message.Id,
                IsHandled = false
            };

            this.DebugHelperMessageService.Send(response);
        }
    }
}
