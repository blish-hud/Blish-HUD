using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;
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
        private Rectangle      _bgTextureBounds; // Bounds for the background texture to avoid empty margins and borders, which are not used in the prompt design.
        private AsyncTexture2D _icon;// Optional icon to display on the left side of the prompt, which can be set via predefined DialogIcon or custom AsyncTexture2D.

        private Rectangle _bgBounds; // Calculated bounds for the background, which also serves as the container for the text and buttons.
        private Point     _iconMargin; // Space between the icon and the text, as well as between buttons if multiple are present.

        private const int BUTTON_HEIGHT = 30;
        private const int BUTTON_WIDTH = 117;
        private readonly int _maxButtonWidth; // Calculated max button width based on text size, with a minimum defined by BUTTON_WIDTH.

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
            _maxButtonWidth = BUTTON_WIDTH;
            foreach (string bttnStr in buttons) {
                var bttnWidth = GameService.Content.DefaultFont14.MeasureStringLogical(bttnStr).X + Panel.RIGHT_PADDING * 2;
                if (bttnWidth > _maxButtonWidth) {
                    _maxButtonWidth = (int)Math.Round(bttnWidth);
                }
                _buttons.Add(new StandardButton() { 
                    Parent  = this, 
                    Text    = bttnStr, 
                    Enabled = false 
                });
            }

            _enterButtonIndex = enterButtonIndex;
            _escapeButtonIndex = escapeButtonIndex;
            _callback = callback;

            this.ZIndex = Screen.TOOLTIP_BASEZINDEX - 16; // Top most layer but lower than tooltip.
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
            _bgTextureBounds = new Rectangle(33, 27, 936, 936); // Define bounds because the background texture has empty margin and border parts that we avoid.
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
                .CreatePart(text, o => o.SetFontSize(ContentService.FontSize.Size16));
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

        private void CalculateButtonLayout() {
            int minLeftOffset = 50;
            int buttonCount = _buttons.Count;
            int availableWidth = _bgBounds.Width - minLeftOffset;

            int buttonWidth = _maxButtonWidth + Panel.RIGHT_PADDING * 2;
            if (buttonCount * buttonWidth > availableWidth) {
                buttonWidth = availableWidth / buttonCount;
            }

            int xOffset = _bgBounds.Width - minLeftOffset - buttonWidth - Panel.RIGHT_PADDING * 2;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING - 2;
            foreach (var button in _buttons) {
                if (button == null || button.Enabled) continue;
                button.Location = new Point(_bgBounds.Left + minLeftOffset + xOffset, yOffset);
                button.Width = buttonWidth;
                button.Height = BUTTON_HEIGHT;
                button.Click += (o, e) => this.ButtonPress(_buttons.IndexOf((StandardButton)o));
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
            bgTextureSize = new Point(bgTextureSize.X < _bgTextureBounds.Width ? bgTextureSize.X : _bgTextureBounds.Width,
                                      bgTextureSize.Y < _bgTextureBounds.Height ? bgTextureSize.Y : _bgTextureBounds.Height); // Clamp to max texture bounds.
            var bgTexturePos = new Point((bounds.Width - bgTextureSize.X) / 2, (bounds.Height - bgTextureSize.Y) / 2);
            var bgBounds = new Rectangle(bgTexturePos, bgTextureSize);
            _bgBounds = bgBounds;

            // Draw Background
            spriteBatch.DrawOnCtrl(this, _bgTexture, bgBounds, new Rectangle(_bgTextureBounds.Location, bgTextureSize), Color.White);

            // Draw border
            spriteBatch.DrawBorderOnCtrl(this, _bgBounds, 2, Color.Black);

            if (icon != null && icon.HasTexture) {
                var iconBounds = new Rectangle(bgBounds.Left + _iconMargin.X, bgBounds.Top + _iconMargin.Y, 64, 64);
                spriteBatch.DrawOnCtrl(this, icon, iconBounds);
            }

            _label.Location = new Point(bgBounds.Left + textPos.X, bgBounds.Y + textPos.Y);
            this.CalculateButtonLayout();
        }
    }

    public sealed class DialogButton {
        private Action<DialogButton> _callback;
        private Keys _key;
        private string _text;
        private DialogButton(string text, Action<DialogButton> callback, Keys key = Keys.None) { 
            _callback = callback;
            _key = key;
            _text = text;
        }

        public DialogButton Action(Action<DialogButton> callback) {
            return this;
        }

        public static DialogButton OK => new DialogButton("OK", null, Keys.Enter);
        public static DialogButton Cancel => new DialogButton("Cancel", null, Keys.Escape);
    }
}
