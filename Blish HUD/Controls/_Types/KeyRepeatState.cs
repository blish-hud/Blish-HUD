using System;
using System.Windows.Forms;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {
    internal class KeyRepeatState {

        private const int KEYBOARDSPEED_MAXDELAY   = 400;
        private const int KEYBOARDSPEED_MULTIPLIER = -12;
        private const int KEYBOARDELAY_MULTIPLIER  = 250;

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
                               ? KEYBOARDSPEED_MULTIPLIER * SystemInformation.KeyboardSpeed + KEYBOARDSPEED_MAXDELAY
                               // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.systeminformation.keyboarddelay?view=netframework-4.8#remarks
                               : (SystemInformation.KeyboardDelay + 1) * KEYBOARDELAY_MULTIPLIER;

            if (gameTime.TotalGameTime.Subtract(_lastInterval).TotalMilliseconds  > minDelay) {
                _hasDelayed = true;
                handler(GameService.Input.Keyboard, _repeatableArgs);

                _lastInterval = gameTime.TotalGameTime;
            }
        }

    }
}
