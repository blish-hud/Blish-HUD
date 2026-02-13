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
    public enum DialogIcon {
        None,
        Exclamation,
        Question,
        Present
    }

    /// <summary>
    /// Represents a modal prompt that displays a message and allows user interaction through buttons.
    /// </summary>
    /// <remarks>
    /// The prompt is displayed in the center of the <seealso cref="SpriteBatch">screen</seealso> 
    /// and can include an optional <seealso cref="AsyncTexture2D">icon</seealso>. 
    /// It supports multiple buttons with <seealso cref="Action">callbacks</seealso>.<br/>
    /// Navigation and interaction is also possible via <see cref="Keys.Tab"/> and <seealso cref="Keys.Enter"/> respectively.
    /// <seealso cref="Keys.Escape"/> closes the prompt silently.
    /// </remarks>
    public sealed class ScreenPrompt : Container {
        private AsyncTexture2D _bgTexture;
        private Rectangle      _bgTextureBounds; // Bounds of bg texture without empty margins and borders.
        private AsyncTexture2D _icon; // Optional icon to display.

        private Rectangle _bgBounds; // Calculated bounds of container.
        private Point     _iconMargin; // Space between icon and the text, as well as between buttons.

        private const int BUTTON_HEIGHT = 30; // Fixed button height.
        private const int BUTTON_WIDTH = 117; // Minimum button width.
        private int _maxButtonWidth; // Calculated max button width based on text width.

        private readonly List<DialogButton> _buttons;
        private readonly FormattedLabel _label;

        private ScreenPrompt(
            FormattedLabelBuilder label, 
            AsyncTexture2D icon, 
            List<DialogButton> buttons) {

            if (label == null)
                throw new ArgumentNullException(nameof(label), $"[{nameof(ScreenPrompt)}] Parameter '{nameof(label)}' cannot be null.");

            // Defaulting to OK button if no buttons provided to ensure there's always a way to close the prompt.
            if (buttons == null || buttons.Count == 0)
                buttons = new List<DialogButton>() { DialogButton.OK };

            if (buttons.Count(b => b.Selected) > 1)
                throw new ArgumentException($"[{nameof(ScreenPrompt)}] Only one {nameof(DialogButton)} can be selected by default.", nameof(buttons));

            _label        = label.SetWidth(340).AutoSizeHeight().Wrap().Build();
            _label.Parent = this;

            _icon       = icon;
            _iconMargin = new Point(9, 8);

            _buttons = buttons;
            _maxButtonWidth = BUTTON_WIDTH;

            // Calculate max button width.
            foreach (DialogButton button in _buttons) {
                var bttnWidth = GameService.Content.DefaultFont14 // Default font of StandardButton.
                    .MeasureStringLogical(button.Text).X + Panel.RIGHT_PADDING * 2;
                if (bttnWidth > _maxButtonWidth) 
                    _maxButtonWidth = (int)Math.Round(bttnWidth);
            }

            this.ZIndex = Screen.TOOLTIP_BASEZINDEX - 16; // Top most layer but lower than tooltip.
            this.LoadTextures();
            GameService.Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private void LoadTextures() {
            _bgTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);
            _bgTextureBounds = new Rectangle(33, 27, 936, 936); // Ensure empty margin and border parts of bg texture are avoided.
        }

        protected override void DisposeControl() {
            _icon?.Dispose();
            GameService.Input.Keyboard.KeyPressed -= OnKeyPressed;
            base.DisposeControl();
        }

        private void OnKeyPressed(object o, KeyboardEventArgs e) {
            if (e.Key == Keys.Escape) {
                this.Dispose(); // Close the prompt silently.
                return;
            }

            if (e.Key == Keys.Enter) {
                _buttons.FirstOrDefault(b => b.Selected)?.DoClick(this);
                return;
            }

            if (e.Key == Keys.Tab) {
                // Find currently selected index.
                int currentIndex = _buttons.FindIndex(b => b.Selected);

                // Fallback if none selected yet.
                if (currentIndex < 0) currentIndex = 0;

                bool backwards = (GameService.Input.Keyboard.ActiveModifiers & ModifierKeys.Shift) != 0;

                int newIndex = backwards
                    ? (currentIndex - 1 + _buttons.Count) % _buttons.Count
                    : (currentIndex + 1) % _buttons.Count;

                // Update selection.
                for (int i = 0; i < _buttons.Count; i++) {
                    _buttons[i].Select(i == newIndex);
                }
                return;
            }
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        public static void Show(FormattedLabelBuilder label, params DialogButton[] buttons) {
            Show(label, null, buttons);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Predefined buttons that the prompt should have.</param>
        public static void Show(string text, DialogIcon icon = DialogIcon.None, params DialogButton[] buttons) {
            Show(GetDefaultLabel(text), GetDefaultIcon(icon), buttons);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        public static void Show(string text, AsyncTexture2D icon, params DialogButton[] buttons) {
            Show(GetDefaultLabel(text), icon, buttons);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="text">Text inside the popup.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        public static void Show(string text, params DialogButton[] buttons) {
            Show(GetDefaultLabel(text), null, buttons);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Predefined icon to use.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        public static void Show(FormattedLabelBuilder label, DialogIcon icon, params DialogButton[] buttons) {
            Show(label, GetDefaultIcon(icon), buttons);
        }

        /// <summary>
        /// Shows an immovable error prompt popup window in the center of the screen.
        /// </summary>
        /// <param name="label">Formatted text inside the popup. Will be build with auto wrap and sizing by the prompt.</param>
        /// <param name="icon">Custom icon to use. Will NOT be disposed with the prompt.</param>
        /// <param name="buttons">Buttons that the prompt should have.</param>
        public static void Show(FormattedLabelBuilder label, AsyncTexture2D icon = null, params DialogButton[] buttons) {
            var prompt = new ScreenPrompt(label, icon, buttons?.ToList()) {
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

        private void CalculateButtonLayout() {
            int minLeftOffset = 50;
            int buttonCount = _buttons.Count;
            int availableWidth = _bgBounds.Width - minLeftOffset;

            // Calculate button width.
            int buttonWidth = _maxButtonWidth + Panel.RIGHT_PADDING * 2;
            if (buttonCount * buttonWidth > availableWidth) {
                buttonWidth = availableWidth / buttonCount;
            }

            // Calculate total width of the button row.
            int totalWidth = buttonCount * buttonWidth
                           + (buttonCount - 1) * _iconMargin.X;

            // Anchor the whole group to the RIGHT.
            int xOffset = _bgBounds.Right - totalWidth - Panel.RIGHT_PADDING * 2;
            int yOffset = _bgBounds.Bottom - BUTTON_HEIGHT - Panel.BOTTOM_PADDING - 2;
            foreach (var button in _buttons) {
                if (button == null) continue;
                var bounds = new Rectangle(
                    new Point(xOffset, yOffset),
                    new Point(buttonWidth, BUTTON_HEIGHT)
                );
                button.Transform(this, bounds);
                xOffset += buttonWidth + _iconMargin.X;
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
        internal string Text;
        internal bool Selected;
        private Action _callback;
        private StandardButton _button;
        private DialogButton(string text) {
            if (string.IsNullOrEmpty(text)) 
                throw new ArgumentNullException(nameof(text), $"[{nameof(DialogButton)}] Parameter '{nameof(text)}' cannot be null or empty.");

            Text = text;
            _button = new StandardButton() {
                Text = text,
                Enabled = false
            };
            _button.Click += (o, e) => {
                DoClick(((StandardButton)o).Parent);
            };
        }

        internal void DoClick(Container parent) {
            GameService.Content.PlaySoundEffectByName("button-click");
            parent?.Dispose();
            _callback?.Invoke();
        }

        internal void Transform(ScreenPrompt parent, Rectangle bounds) {
            _button.Parent = parent;
            _button.Location = bounds.Location;
            _button.Size = bounds.Size;
            _button.Enabled = true;
        }

        public DialogButton Action(Action callback) {
            _callback = callback;
            return this;
        }

        public DialogButton Select(bool selected = true) {
            Selected = selected;
            _button.BackgroundColor = selected ? new Color(192, 216, 255, 217) : Color.Transparent;
            return this;
        }

        public static DialogButton OK => new DialogButton(Strings.Common.Action_OK);
        public static DialogButton Confirm => new DialogButton(Strings.Common.Action_Confirm);
        public static DialogButton Cancel => new DialogButton(Strings.Common.Action_Cancel);
        public static DialogButton Yes => new DialogButton(Strings.Common.Action_Yes);
        public static DialogButton No => new DialogButton(Strings.Common.Action_No);
        public static DialogButton Ignore => new DialogButton(Strings.Common.Action_Ignore);
        public static DialogButton Close => new DialogButton(Strings.Common.Action_Close);
        public static DialogButton Apply => new DialogButton(Strings.Common.Action_Apply);
        public static DialogButton Decline => new DialogButton(Strings.Common.Action_Decline);
        public static DialogButton Create(string text) => new DialogButton(text);
    }
}
