using System.Collections;
using Blish_HUD.DebugHelper.Models;
using Blish_HUD.DebugHelper.Services;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {

    internal class DebugHelperKeyboardHookManager : DebugHelperInputHookManager<HandleKeyboardInputDelegate, KeyboardEventMessage>, IKeyboardHookManager {

        public DebugHelperKeyboardHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(KeyboardEventMessage message) {
            Keys key = (Keys)message.Key;

            // Windows reports the numpad Enter key with the same virtual-key code as the
            // main Enter key. The numpad Enter key is marked as an extended key while the
            // main one is not, which lets us tell the two apart.
            if (key == Keys.Enter && (message.Flags & 0x01) != 0) { // 0x01 = LLKHF_EXTENDED
                key = KeysUtil.NumPadEnter;
            }

            KeyboardEventArgs keyboardEventArgs = new KeyboardEventArgs((KeyboardEventType)message.EventType, key);
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
