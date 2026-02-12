using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Blish_HUD.Common.Gw2.UI {
    public sealed class ScreenPrompt : Container {
        [Flags]
        public enum DialogButton : ushort {
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

        public enum DialogIcon {
            None,
            Exclamation,
            Question,
            Present
        }

        private AsyncTexture2D _bgTexture;
        private AsyncTexture2D _icon;

        private Rectangle  _bgBounds;
        private Point      _iconMargin;

        private const int BUTTON_HEIGHT = 28;
        private const int BUTTON_WIDTH = 117;

        private readonly List<StandardButton> _buttons;

        private readonly Action<int> _callback;

        private readonly FormattedLabel _label;
        private readonly int _enterButtonIndex;
        private readonly int _escapeButtonIndex;

        private ScreenPrompt(
            FormattedLabelBuilder label, 
            AsyncTexture2D icon, 
            IReadOnlyList<string> buttons, 
            Action<int> callback, 
            int enterButtonIndex, 
            int escapeButtonIndex) {

            if (!IsValidDialog(label, buttons, out var errorMessage)) {
                icon?.Dispose();
                throw new ArgumentException(errorMessage);
            }

            _label        = label.SetWidth(340).AutoSizeHeight().Wrap().Build();
            _label.Parent = this;

            _icon       = icon;
            _iconMargin = new Point(9, 8);

            _buttons = new List<StandardButton>();
            foreach (string bttnStr in buttons) {
                _buttons.Add(new StandardButton() { 
                    Parent  = this, 
                    Text    = bttnStr, 
                    Enabled = false 
                });
            }

            _enterButtonIndex = enterButtonIndex;
            _escapeButtonIndex = escapeButtonIndex;
            _callback = callback;

            this.ZIndex = 999;
            this.LoadTextures();
            GameService.Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private bool IsValidDialog(FormattedLabelBuilder label, IReadOnlyList<string> buttons, out string errorMessage) {
            errorMessage = string.Empty;
            var noButtons = buttons == null || buttons.Count == 0;
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
            _bgTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);
        }

        protected override void DisposeControl() {
            _icon?.Dispose();
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            base.DisposeControl();
        }

        private void ButtonPress(int buttonIndex) {
            if (buttonIndex < 0 || buttonIndex >= _buttons.Count) return;
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            GameService.Content.PlaySoundEffectByName("button-click");
            _callback?.Invoke(buttonIndex);
            this.Dispose();
        }

        private void OnKeyPressed(object o, KeyboardEventArgs e) {
            switch (e.Key) {
                case Keys.Enter:
                    this.ButtonPress(_enterButtonIndex);
                    break;
                case Keys.Escape:
                    this.ButtonPress(_escapeButtonIndex);
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
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(string text, DialogButton buttons = DialogButton.OK, Action<int> callback = null,
                                      int enterButtonIndex  = 0,
                                      int escapeButtonIndex = 1) {
            Show(text, DialogIcon.None, buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, DialogButton buttons, Action<int> callback = null,
                                      int enterButtonIndex = 0,
                                      int escapeButtonIndex = 1) {
            Show(label, DialogIcon.None, buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Predefined buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(
            string text,
            DialogIcon icon = DialogIcon.None,
            DialogButton buttons = DialogButton.OK,
            Action<int> callback = null,
            int enterButtonIndex = 0,
            int escapeButtonIndex = 1) {
            Show(GetDefaultLabel(text), icon, buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(
            string text,
            AsyncTexture2D icon,
            DialogButton buttons = DialogButton.OK,
            Action<int> callback = null,
            int enterButtonIndex = 0,
            int escapeButtonIndex = 1) {
            Show(GetDefaultLabel(text), icon, buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(
            FormattedLabelBuilder label,
            DialogIcon icon,
            DialogButton buttons = DialogButton.OK,
            Action<int> callback = null,
            int enterButtonIndex = 0,
            int escapeButtonIndex = 1) {
            Show(label, GetDefaultIcon(icon), buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, DialogIcon icon, IEnumerable<string> buttons = null, Action<int> callback = null,
                                      int enterButtonIndex = 0,
                                      int escapeButtonIndex = 1) {
            Show(label, GetDefaultIcon(icon), buttons, callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, AsyncTexture2D icon, DialogButton buttons = DialogButton.OK, Action<int> callback = null,
                                      int enterButtonIndex = 0,
                                      int escapeButtonIndex = 1) {
            Show(label, icon, GetDefaultButtons(buttons), callback, enterButtonIndex, escapeButtonIndex);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        /// <param name="callback">Function that is called when a button is pressed.</param>
        /// <param name="enterButtonIndex">Index of a button that can be triggered via the Enter key on the keyboard.</param>
        /// <param name="escapeButtonIndex">Index of a button that can be triggered via the Escape key on the keyboard.</param>
        public static void Show(FormattedLabelBuilder label, AsyncTexture2D icon = null, IEnumerable<string> buttons = null, Action<int> callback = null,
                                       int enterButtonIndex = 0,
                                       int escapeButtonIndex = 1) {
            var prompt = new ScreenPrompt(label, icon, buttons.ToList(), callback, enterButtonIndex, escapeButtonIndex)
            {
                Parent = Graphics.SpriteScreen,
                Location = Point.Zero,
                Size = Graphics.SpriteScreen.Size
            };
            prompt.Show();
        }

        private static FormattedLabelBuilder GetDefaultLabel(string text) {
            return new FormattedLabelBuilder()
                .CreatePart(text, o => o.SetFontSize(ContentService.FontSize.Size18));
        }

        private static AsyncTexture2D GetDefaultIcon(DialogIcon icon) {
            var iconTex = new AsyncTexture2D();
            var iconAtlas = GameService.Content.DatAssetCache.GetTextureFromAssetId(154985);
            iconAtlas.TextureSwapped += (o, e) => {
                if (icon == DialogIcon.Exclamation) {
                    iconTex.SwapTexture(iconAtlas.Texture.GetRegion(0, 0, 64, 64));
                } else if (icon == DialogIcon.Question) {
                    iconTex.SwapTexture(iconAtlas.Texture.GetRegion(64, 0, 64, 64));
                } else if (icon == DialogIcon.Present) {
                    iconTex.SwapTexture(iconAtlas.Texture.GetRegion(128, 0, 64, 64));
                }
            };
            return iconTex;
        }

        private static IReadOnlyList<string> GetDefaultButtons(DialogButton buttons) {
            return Enum.GetValues(typeof(DialogButton))
                .Cast<DialogButton>()
                .Reverse()
                .Where(b => b != DialogButton.None && (buttons & b) == b)
                .Select(GetDefaultButtonText)
                .ToList();
        }

        private static string GetDefaultButtonText(DialogButton button) {
            return button switch {
                DialogButton.OK => Strings.Common.Action_OK,
                DialogButton.Confirm => Strings.Common.Action_Confirm,
                DialogButton.Cancel => Strings.Common.Action_Cancel,
                DialogButton.Yes => Strings.Common.Action_Yes,
                DialogButton.No => Strings.Common.Action_No,
                DialogButton.Ignore => Strings.Common.Action_Ignore,
                DialogButton.Close => Strings.Common.Action_Close,
                DialogButton.Apply => Strings.Common.Action_Apply,
                DialogButton.Decline => Strings.Common.Action_Decline,
                _ => string.Empty
            };
        }

        private void CalcButtonLayout() {
            int minLeftOffset = 50;
            int buttonCount = _buttons.Count;
            int availableWidth = _bgBounds.Width - minLeftOffset;

            int buttonWidth = BUTTON_WIDTH;
            if (buttonCount * buttonWidth > availableWidth) {
                buttonWidth = availableWidth / buttonCount;
            }

            int xOffset = _bgBounds.Width - minLeftOffset - buttonWidth - Panel.RIGHT_PADDING * 2;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING - 2;
            for (int i = 0; i < buttonCount; i++) { 
                var button = _buttons[i];
                if (button == null || button.Enabled) continue;
                button.Location = new Point(_bgBounds.Left + minLeftOffset + xOffset, yOffset);
                button.Width = buttonWidth;
                button.Height = BUTTON_HEIGHT;
                button.Click += (o, e) => this.ButtonPress(i);
                button.Enabled = true;
                xOffset -= buttonWidth + _iconMargin.X;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var textSize   = _label.Size;
            var textWidth  = textSize.X;
            var textHeight = textSize.Y;

            var icon = _icon;
            var iconSize = icon == null ? 0 : 64;
            var textPos = new Point(iconSize + _iconMargin.X + Panel.RIGHT_PADDING * 2, 17);

            textHeight = textHeight > 64 ? textHeight : textHeight + iconSize;

            // Darken background outside container
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.15f);

            // Container
            // Calculate background bounds
            var bgTextureSize = new Point(textWidth + iconSize + Panel.RIGHT_PADDING * 10, textHeight + BUTTON_HEIGHT + Panel.TOP_PADDING * 4);
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
            this.CalcButtonLayout();
        }
    }
}
