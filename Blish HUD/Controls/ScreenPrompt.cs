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

        private static Texture2D _bgTexture;
        private static Texture2D _icons;

        private AsyncTexture2D _icon;
        private AsyncTexture2D _customIcon;

        private Rectangle  _bgBounds;
        private Point      _iconMargin;

        private const int BUTTON_HEIGHT = 28;
        private const int BUTTON_WIDTH = 117;

        private readonly Dictionary<DialogButton, StandardButton> _buttons;

        private readonly Action<DialogButton> _callback;

        private readonly FormattedLabel _label;
        private readonly DialogButton _enterButton;
        private readonly DialogButton _escapeButton;


        private ScreenPrompt(FormattedLabelBuilder label, DialogButton buttons, Action<DialogButton> callback = null, DialogIcon icon = DialogIcon.None, AsyncTexture2D customIcon = null, DialogButton enterButton = DialogButton.None, DialogButton escapeButton = DialogButton.None) {

            if (!IsValidDialog(label, buttons, out var errorMessage)) {
                throw new ArgumentException(errorMessage);
            }

            _label = label.SetWidth(350).AutoSizeHeight().Wrap().Build();
            _label.Parent = this;
            _customIcon = customIcon;
            _iconMargin = new Point(9, 8);
            _buttons    = new Dictionary<DialogButton, StandardButton>();
            foreach (DialogButton button in Enum.GetValues(typeof(DialogButton))) {
                if (button == DialogButton.None) continue;
                if ((buttons & button) == button) _buttons.Add(button, null); // Each key will be assigned a button instance in CreateButtons().
            }

            _enterButton = enterButton;
            _escapeButton = escapeButton;
            _callback = callback;

            this.ZIndex = 999;
            this.LoadTextures();
            this.LoadIcon(icon);
            GameService.Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private bool IsValidDialog(FormattedLabelBuilder label, DialogButton buttons, out string errorMessage) {
            errorMessage = string.Empty;
            var noButtons = buttons == DialogButton.None;
            if (noButtons) {
                errorMessage += "Prompt dialog must have at least one button. ";
            }
            var noText = label == null;
            if (noText) {
                errorMessage += "Prompt dialog must have text content.";
            }
            return string.IsNullOrEmpty(errorMessage);
        }

        private void LoadTextures() {
            _bgTexture ??= GameService.Content.GetTexture(@"controls/prompt/156003");
            _icons ??= GameService.Content.GetTexture(@"controls/prompt/154985");
        }

        private void LoadIcon(DialogIcon icon) {
            if (icon == DialogIcon.None) return;
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

        protected override void DisposeControl() {
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

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(label, DialogIcon.None, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, DialogIcon icon, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(label, icon, null, buttons, callback, enterButton, escapeButton);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButton">Buttons that can be pressed via the Enter key on the keyboard.</param>
        /// <param name="escapeButton">Buttons that can be pressed via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, AsyncTexture2D icon, DialogButton buttons = DialogButton.OK, Action<DialogButton> callback = null,
                                      DialogButton enterButton = DialogButton.None,
                                      DialogButton escapeButton = DialogButton.None) {
            Show(label, DialogIcon.None, icon, buttons, callback, enterButton, escapeButton);
        }

        private static void Show(string text, DialogIcon icon, AsyncTexture2D customIcon, DialogButton buttons, Action<DialogButton> callback,
                               DialogButton enterButton,
                               DialogButton escapeButton) {
            var txt2Lbl = new FormattedLabelBuilder().CreatePart(text, o => o.SetFontSize(ContentService.FontSize.Size18));
            Show(txt2Lbl, icon, customIcon, buttons, callback, enterButton, escapeButton);
        }

        private static void Show(FormattedLabelBuilder label, DialogIcon icon, AsyncTexture2D customIcon, DialogButton buttons, Action<DialogButton> callback,
                                       DialogButton enterButton,
                                       DialogButton escapeButton) {
            var prompt = new ScreenPrompt(label, buttons, callback, icon, customIcon, enterButton, escapeButton)
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

            int xOffset = _bgBounds.Width - minLeftOffset - buttonWidth - Panel.RIGHT_PADDING * 2;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING - 2;
            var buttonKeys = _buttons.Keys.Reverse().ToList();
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

            var textSize   = _label.Size;
            var textWidth  = textSize.X;
            var textHeight = textSize.Y;

            var icon = _customIcon ?? _icon;
            var iconSize = icon == null ? 0 : 64;
            var textPos = new Point(iconSize + _iconMargin.X + Panel.RIGHT_PADDING, 17);

            textHeight = textHeight > 64 ? textHeight : textHeight + iconSize;

            // Darken background outside container
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.5f);

            // Container
            // Calculate background bounds
            var bgTextureSize = new Point(textWidth + iconSize + Panel.RIGHT_PADDING * 6, textHeight + BUTTON_HEIGHT + Panel.TOP_PADDING * 3);
            var bgTexturePos = new Point((bounds.Width - bgTextureSize.X) / 2, (bounds.Height - bgTextureSize.Y) / 2);
            var bgBounds = new Rectangle(bgTexturePos, bgTextureSize);
            _bgBounds = bgBounds;

            // Draw Background
            spriteBatch.DrawOnCtrl(this, _bgTexture, bgBounds, new Rectangle(29, 23, 942, 942), Color.White * 0.9f);

            // Draw border
            spriteBatch.DrawRectangleOnCtrl(this, _bgBounds, 3, Color.Black * 0.9f);

            if (icon != null && icon.HasTexture) {
                var iconBounds = new Rectangle(bgBounds.Left + _iconMargin.X, bgBounds.Top + _iconMargin.Y, 64, 64);
                spriteBatch.DrawOnCtrl(this, icon, iconBounds);
            }

            _label.Location = new Point(bgBounds.Left + textPos.X, bgBounds.Y + textPos.Y);
            this.CreateButtons();
        }
    }
}
