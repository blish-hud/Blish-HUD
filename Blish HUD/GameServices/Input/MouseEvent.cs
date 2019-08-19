using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Input {
    internal class MouseEvent {
        public MouseHook.MouseMessages  EventMessage { get; }
        public MouseHook.MSLLHOOKSTRUCT EventDetails { get; }

        public MouseEvent(MouseHook.MouseMessages message, MouseHook.MSLLHOOKSTRUCT hookdetails) {
            this.EventMessage = message;
            this.EventDetails = hookdetails;
        }
    }
}
