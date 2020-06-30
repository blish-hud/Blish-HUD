using System.Collections;
using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class DebugHelperKeyboardHookManager : DebugHelperInputHookManager<HandleKeyboardInputDelegate, KeyboardEventMessage>, IKeyboardHookManager {

        private static readonly Logger Logger = Logger.GetLogger<DebugHelperKeyboardHookManager>();

        public DebugHelperKeyboardHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(KeyboardEventMessage message) {
            KeyboardEventArgs keyboardEventArgs = new KeyboardEventArgs((KeyboardEventType)message.EventType, (Keys)message.Key);
            bool              isHandled         = false;

            lock (((IList) this.Handlers).SyncRoot) {
                foreach (HandleKeyboardInputDelegate handler in this.Handlers) {
                    isHandled = handler(keyboardEventArgs);
                    if (isHandled) break;
                }
            }

            KeyboardResponseMessage response = new KeyboardResponseMessage {
                Id        = message.Id,
                IsHandled = isHandled
            };

            this.DebugHelperMessageService.Send(response);
        }

        protected override void DummyHookCallback(KeyboardEventMessage message) {
            KeyboardResponseMessage response = new KeyboardResponseMessage {
                Id        = message.Id,
                IsHandled = false
            };

            this.DebugHelperMessageService.Send(response);
        }

    }

}
