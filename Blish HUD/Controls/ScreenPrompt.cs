using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Blish_HUD.Controls {
    public sealed class ScreenPrompt : Container
    {
        [Flags]
        public enum DialogButton : ushort
        {
            None = 0,
            OK = 1 << 0,
            Confirm = 1 << 1,
            Cancel = 1 << 2,
            Yes = 1 << 3,
            No = 1 << 4,
            Ignore = 1 << 5,
            Close = 1 << 6,
            Apply = 1 << 7,
            Decline = 1 << 8
        }

        public enum DialogIcon
        {
            None,
            Exclamation,
            Question,
            Present
        }

        private Texture2D _bgTexture;
        private Texture2D _icons;
        private BitmapFont _font;

        private AsyncTexture2D _icon;
        private AsyncTexture2D _customIcon;

        private Rectangle  _bgBounds;
        private Point      _iconMargin;

        private const int BUTTON_HEIGHT = 28;
        private const int BUTTON_WIDTH = 117;

        private readonly Dictionary<DialogButton, StandardButton> _buttons;

        private readonly Action<DialogButton> _callback;

        private readonly string _text;
        private readonly DialogButton _enterButton;
        private readonly DialogButton _escapeButton;


        private ScreenPrompt(string text, DialogButton buttons, Action<DialogButton> callback = null, DialogIcon icon = DialogIcon.None, AsyncTexture2D customIcon = null, DialogButton enterButton = DialogButton.None, DialogButton escapeButton = DialogButton.None) {
            _text       = text;
            _customIcon = customIcon;
            _iconMargin = new Point(9, 8);
            _font       = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size22, ContentService.FontStyle.Regular);
            _buttons    = new Dictionary<DialogButton, StandardButton>();
            foreach (DialogButton button in Enum.GetValues(typeof(DialogButton))) {
                if (button == DialogButton.None) continue;
                if (buttons.HasFlag(button)) _buttons.Add(button, null); // Each key will be assigned a button instance in CreateButtons().
            }

            if (!IsValidDialog(out var errorMessage)) {
                throw new ArgumentException(errorMessage);
            }

            _enterButton = enterButton;
            _escapeButton = escapeButton;
            _callback = callback;

            this.ZIndex = 999;
            this.LoadTextures();
            this.LoadIcon(icon);
            GameService.Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private void LoadTextures() {
            _bgTexture = GameService.Content.GetTexture(@"controls/prompt/156003");
        }

        private void LoadIcon(DialogIcon icon) {
            if (icon == DialogIcon.None) return;
            _icons = GameService.Content.GetTexture(@"controls/prompt/154985");
            _icon = new AsyncTexture2D();
            GetIconRegion(icon, _icons);
        }

        private void GetIconRegion(DialogIcon icon, Texture2D atlas) {
            if (icon == DialogIcon.Exclamation) {
                _icon.SwapTexture(atlas.GetRegion(0, 0, 64, 64));
            } else if (icon == DialogIcon.Question) {
                _icon.SwapTexture(atlas.GetRegion(64, 0, 64, 64));
            } else if (icon == DialogIcon.Present) {
                _icon.SwapTexture(atlas.GetRegion(128, 0, 64, 64));
            }
        }

        private bool IsValidDialog(out string errorMessage) {
            errorMessage = string.Empty;
            var noButtons = _buttons.Count < 1;
            if (noButtons)
            {
                errorMessage += "Prompt dialog must have at least one button. ";
            }
            var noText = string.IsNullOrWhiteSpace(_text);
            if (noText)
            {
                errorMessage += "Prompt dialog must have text content.";
            }
            return string.IsNullOrEmpty(errorMessage);
        }

        protected override void DisposeControl() {
            _icons?.Dispose();
            _bgTexture?.Dispose();
            _icon?.Dispose();
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            base.DisposeControl();
        }

        private void ButtonPress(DialogButton button) {
            if (button == DialogButton.None) return;
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            GameService.Content.PlaySoundEffectByName("button-click");
            _callback?.Invoke(button);
            this.Dispose();
        }

        private void OnKeyPressed(object o, KeyboardEventArgs e) {
            switch (e.Key) {
                case Keys.Enter:
                    this.ButtonPress(_enterButton);
                    break;
                case Keys.Escape:
                    this.ButtonPress(_escapeButton);
                    break;
                default: return;
            }
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(string text, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton  = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(text, DialogIcon.None, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(string text, DialogIcon icon, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton  = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(text, icon, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(string text, AsyncTexture2D icon, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton  = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(text, DialogIcon.None, icon, buttons, callback, enterButton, escapeButton);
        }

        private static void Show(string text, DialogIcon icon, AsyncTexture2D customIcon, DialogButton buttons, Action<DialogButton> callback,
                                       DialogButton enterButton,
                                       DialogButton escapeButton) {
            var prompt = new ScreenPrompt(text, buttons, callback, icon, customIcon, enterButton, escapeButton)
            {
                Parent = Graphics.SpriteScreen,
                Location = Point.Zero,
                Size = Graphics.SpriteScreen.Size
            };
            prompt.Show();
        }

        private void CreateButtons() {
            int minLeftOffset = 50;
            int buttonCount = _buttons.Count;
            int availableWidth = _bgBounds.Width - minLeftOffset;

            int buttonWidth = BUTTON_WIDTH;
            if (buttonCount * buttonWidth > availableWidth) {
                buttonWidth = availableWidth / buttonCount;
            }

            int xOffset = _bgBounds.Width - minLeftOffset - buttonWidth - Panel.RIGHT_PADDING;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING;
            var buttonKeys = _buttons.Keys.ToList();
            foreach (var buttonKey in buttonKeys) {
                StandardButton button = _buttons[buttonKey];
                if (button == null) {
                    button = new StandardButton {
                        Parent = this,
                        Text = GetButtonText(buttonKey),
                        Width = buttonWidth,
                        Height = BUTTON_HEIGHT,
                        Location = new Point(_bgBounds.Left + minLeftOffset + xOffset, yOffset),
                        Enabled = true
                    };
                    button.Click += (o, e) => this.ButtonPress(buttonKey);
                }
                xOffset -= buttonWidth + _iconMargin.X;
                _buttons[buttonKey] = button;
            }
        }

        private string GetButtonText(DialogButton button) {
            return button switch {
                DialogButton.OK      => Strings.Common.Action_OK,
                DialogButton.Confirm => Strings.Common.Action_Confirm,
                DialogButton.Cancel  => Strings.Common.Action_Cancel,
                DialogButton.Yes     => Strings.Common.Action_Yes,
                DialogButton.No      => Strings.Common.Action_No,
                DialogButton.Ignore  => Strings.Common.Action_Ignore,
                DialogButton.Close   => Strings.Common.Action_Close,
                DialogButton.Apply   => Strings.Common.Action_Apply,
                DialogButton.Decline => Strings.Common.Action_Decline,
                _                     => string.Empty
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var textMarginRight = 25;
            var text = DrawUtil.WrapText(_font, _text, 412);
            var textSize = _font.MeasureString(text);
            var textWidth = (int)textSize.Width;
            var textHeight = (int)textSize.Height;

            var icon = _customIcon ?? _icon;
            var iconSize = icon == null ? 0 : 64;
            var textMargin = new Point(iconSize + _iconMargin.X * 2, 17);

            var contentWidth = textWidth + iconSize + _iconMargin.X + textMargin.X + textMarginRight;
            var contentHeight = textHeight > 64 ? textHeight : textHeight + iconSize;

            contentHeight = contentHeight < 150 ? 150 : contentHeight;

            // Darken background outside container
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.5f);

            // Container
            // Calculate background bounds
            var bgTextureSize = new Point(contentWidth, contentHeight + (BUTTON_HEIGHT + Panel.TOP_PADDING));
            var bgTexturePos = new Point((bounds.Width - bgTextureSize.X) / 2, (bounds.Height - bgTextureSize.Y) / 2);
            var bgBounds = new Rectangle(bgTexturePos, bgTextureSize);
            _bgBounds = bgBounds;

            var textBounds = new Rectangle(bgBounds.Left + textMargin.X, bgBounds.Y + textMargin.Y, bgBounds.Width - textMarginRight, contentHeight);

            // Draw Background
            spriteBatch.DrawOnCtrl(this, _bgTexture, bgBounds, new Rectangle(29, 23, 942, 942), Color.White);

            // Draw border
            spriteBatch.DrawRectangleOnCtrl(this, _bgBounds, 2, Color.Black * 0.8f);

            if (icon != null && icon.HasTexture) {
                var iconBounds = new Rectangle(bgBounds.Left + _iconMargin.X, bgBounds.Top + _iconMargin.Y, 64, 64);
                spriteBatch.DrawOnCtrl(this, icon, iconBounds);
            }

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, text, _font, textBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
            this.CreateButtons();
        }
    }
}
