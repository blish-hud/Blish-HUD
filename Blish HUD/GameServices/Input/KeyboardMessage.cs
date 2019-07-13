using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public struct KeyboardMessage {

        public int               uMsg;
        public KeyboardEventType EventType;
        public Keys              Key;

        public KeyboardMessage(int _uMsg, IntPtr _wParam, int _lParam) {
            uMsg      = _uMsg;
            EventType = (KeyboardEventType)_wParam;
            Key       = (Keys)_lParam;
        }

    }
}
