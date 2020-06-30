using System.Collections;
using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;

namespace Blish_HUD.Input {

    internal class DebugHelperMouseHookManager : DebugHelperInputHookManager<HandleMouseInputDelegate, MouseEventMessage>, IMouseHookManager {

        public DebugHelperMouseHookManager(IMessageService debugHelperMessageService) : base(debugHelperMessageService) { }

        protected override void HookCallback(MouseEventMessage message) {
            MouseEventArgs mouseEventArgs = new MouseEventArgs(
                                                               (MouseEventType)message.EventType, message.PointX, message.PointY, message.MouseData, message.Flags,
                                                               message.Time, message.ExtraInfo
                                                              );

            bool isHandled = false;

            lock (((IList) this.Handlers).SyncRoot) {
                foreach (HandleMouseInputDelegate handler in this.Handlers) {
                    isHandled = handler(mouseEventArgs);
                    if (isHandled) break;
                }
            }

            MouseResponseMessage response = new MouseResponseMessage {
                Id        = message.Id,
                IsHandled = isHandled
            };

            this.DebugHelperMessageService.Send(response);
        }

        protected override void DummyHookCallback(MouseEventMessage message) {
            MouseResponseMessage response = new MouseResponseMessage {
                Id        = message.Id,
                IsHandled = false
            };

            this.DebugHelperMessageService.Send(response);
        }

    }

}
