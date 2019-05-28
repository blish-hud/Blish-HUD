using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Resources {
    public static class Checkable {

        private static Texture2D _textureTitleBarLeft;
        private static Texture2D _textureTitleBarRight;
        private static Texture2D _textureTitleBarLeftActive;
        private static Texture2D _textureTitleBarRightActive;

        private static Texture2D _textureExitButton;
        private static Texture2D _textureExitButtonActive;

        static Checkable() {
            _textureTitleBarLeft        = GameService.Content.GetTexture("titlebar-inactive");
            _textureTitleBarRight       = GameService.Content.GetTexture("window-topright");
            _textureTitleBarLeftActive  = GameService.Content.GetTexture("titlebar-active");
            _textureTitleBarRightActive = GameService.Content.GetTexture("window-topright-active");

            _textureExitButton       = GameService.Content.GetTexture("button-exit");
            _textureExitButtonActive = GameService.Content.GetTexture("button-exit-active");
        }

    }
}
