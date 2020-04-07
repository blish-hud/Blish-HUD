using System;
using System.Windows.Forms;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {
    internal class KeyRepeatState {

        private readonly KeyboardEventArgs _repeatableArgs;

        private bool     _hasDelayed;
        private TimeSpan _lastInterval;

        public KeyRepeatState(GameTime gameTime, KeyboardEventArgs repeatableArgs) {
            _lastInterval   = gameTime.TotalGameTime;
            _repeatableArgs = repeatableArgs;
        }

        public void HandleUpdate(GameTime gameTime, EventHandler<KeyboardEventArgs> handler) {
            int minDelay = _hasDelayed
                               // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.systeminformation.keyboardspeed?view=netframework-4.8#remarks
                               ? -12 * SystemInformation.KeyboardSpeed + 400
                               // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.systeminformation.keyboarddelay?view=netframework-4.8#remarks
                               : (SystemInformation.KeyboardDelay + 1) * 250;

            if (gameTime.TotalGameTime.Subtract(_lastInterval).TotalMilliseconds  > minDelay) {
                _hasDelayed = true;
                handler(GameService.Input.Keyboard, _repeatableArgs);

                _lastInterval = gameTime.TotalGameTime;
            }
        }

    }
}
