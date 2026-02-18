using Microsoft.Xna.Framework;
using System;
using Blish_HUD.Controls;
using Blish_HUD.Input;
namespace Blish_HUD.Common.Gw2.UI {
    /// <summary>
    /// Represents a button that can be added to a dialog, specifically <see cref="StandardDialog"/>.
    /// </summary>
    public class DialogButton {
        public event EventHandler<MouseEventArgs> Click;

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
            _button.Click += (o, e) => DoClick();
        }

        internal void DoClick() {
            GameService.Content.PlaySoundEffectByName("button-click");
            _callback?.Invoke();
            Click?.Invoke(this, null);
        }

        internal void Transform(Container parent, Rectangle bounds) {
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
        public static DialogButton Accept => new DialogButton(Strings.Common.Action_Accept);
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
