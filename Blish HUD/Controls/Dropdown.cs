using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {
    public class Dropdown:Control {

        private class DropdownPanel:Control {

            private Dropdown _assocDropdown;

            private int _highlightedItem = -1;
            private int HighlightedItem {
                get => _highlightedItem;
                set {
                    if (_highlightedItem == value) return;

                    _highlightedItem = value;
                    OnPropertyChanged();
                }
            }

            private DropdownPanel(Dropdown assocDropdown) {
                _assocDropdown = assocDropdown;

                this.Location = _assocDropdown.AbsoluteBounds.Location + new Point(0, assocDropdown.Height - 1);
                this.Size = new Point(_assocDropdown.Width, _assocDropdown.Height * _assocDropdown.Items.Count);
                this.Parent = Graphics.SpriteScreen;
                this.ZIndex = int.MaxValue;

                Input.LeftMouseButtonPressed += Input_MousedOffDropdownPanel;
                Input.RightMouseButtonPressed += Input_MousedOffDropdownPanel;
            }

            public static DropdownPanel ShowPanel(Dropdown assocDropdown) {
                return new DropdownPanel(assocDropdown);
            }

            private void Input_MousedOffDropdownPanel(object sender, MouseEventArgs e) {
                if (!this.MouseOver) {
                    if (_assocDropdown.MouseOver) _assocDropdown._hadPanel = true;
                    Dispose();
                }
            }

            protected override void OnMouseMoved(MouseEventArgs e) {
                base.OnMouseMoved(e);

                //var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;
                this.HighlightedItem = this.RelativeMousePosition.Y / _assocDropdown.Height;
            }

            protected override void OnClick(MouseEventArgs e) {
                base.OnClick(e);

                _assocDropdown.SelectedItem = _assocDropdown.Items[this.HighlightedItem];
                Dispose();
            }

            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
                spriteBatch.Draw(ContentService.Textures.Pixel, bounds, Color.Black);

                int index = 0;
                foreach (string item in _assocDropdown.Items) {

                    if (index == this.HighlightedItem) {
                        spriteBatch.Draw(ContentService.Textures.Pixel, new Rectangle(2, 2 + _assocDropdown.Height * index, bounds.Width - 12 - spriteArrow.Width, _assocDropdown.Height - 4), new Color(45, 37, 25, 255));

                        Utils.DrawUtil.DrawAlignedText(spriteBatch, Overlay.font_def12, item, new Rectangle(8, _assocDropdown.Height * index, bounds.Width - 13 - spriteArrow.Width, _assocDropdown.Height), ContentService.Colors.Chardonnay, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
                    } else {
                        Utils.DrawUtil.DrawAlignedText(spriteBatch, Overlay.font_def12, item, new Rectangle(8, _assocDropdown.Height * index, bounds.Width - 13 - spriteArrow.Width, _assocDropdown.Height), Color.FromNonPremultiplied(239, 240, 239, 255), Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
                    }

                    index++;
                }
            }

            protected override void Dispose(bool disposing) {
                if (_assocDropdown != null) {
                    _assocDropdown._lastPanel = null;
                    _assocDropdown = null;
                }

                Input.LeftMouseButtonPressed -= Input_MousedOffDropdownPanel;
                Input.RightMouseButtonPressed -= Input_MousedOffDropdownPanel;

                base.Dispose(disposing);
            }

        }

        private const int  DROPDOWN_HEIGHT = 25;

        public class ValueChangedEventArgs : EventArgs {
            public string PreviousValue { get; }
            public string CurrentValue { get; }

            public ValueChangedEventArgs(string previousValue, string currentValue) {
                this.PreviousValue = previousValue;
                this.CurrentValue = currentValue;
            }
        }
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected virtual void OnValueChanged(ValueChangedEventArgs e) {
            this.ValueChanged?.Invoke(this, e);
        }

        public ObservableCollection<string> Items { get; protected set; }

        private string _selectedItem;
        public string SelectedItem {
            get => _selectedItem;
            set {
                string previousValue = _selectedItem;

                _selectedItem = value;
                OnPropertyChanged();

                OnValueChanged(new ValueChangedEventArgs(previousValue, _selectedItem));
            }
        }

        private DropdownPanel _lastPanel = null;
        private bool _hadPanel = false;

        private static TextureRegion2D spriteInputBox;
        private static TextureRegion2D spriteArrow;
        private static TextureRegion2D spriteArrowActive;

        public Dropdown() {
            // Load static resources
            spriteInputBox = spriteInputBox ?? ControlAtlas.GetRegion("inputboxes/input-box");
            spriteArrow = spriteArrow ?? ControlAtlas.GetRegion("inputboxes/dd-arrow");
            spriteArrowActive = spriteArrowActive ?? ControlAtlas.GetRegion("inputboxes/dd-arrow-active");

            //
            this.Items = new ObservableCollection<string>();

            this.Items.CollectionChanged += delegate {
                ItemsUpdated();
                Invalidate();
            };

            this.Size = new Point(this.Width, DROPDOWN_HEIGHT);
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            if (_lastPanel == null && !_hadPanel)
                _lastPanel = DropdownPanel.ShowPanel(this);
            else if (_hadPanel)
                _hadPanel = false;
        }

        private void ItemsUpdated() {
            if (string.IsNullOrEmpty(this.SelectedItem)) this.SelectedItem = this.Items.FirstOrDefault();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("input-box"), bounds.Subtract(new Rectangle(0, 0, 5, 0)), new Rectangle(0, 0, Math.Min(GameServices.GetService<ContentService>().GetTexture("textbox").Width - 5, this.Width - 5), Content.GetTexture("textbox").Height), Color.White);
            spriteBatch.Draw(Content.GetTexture("input-box"), new Rectangle(bounds.Right - 5, bounds.Y, 5, bounds.Height), new Rectangle(GameServices.GetService<ContentService>().GetTexture("textbox").Width - 5, 0, 5, Content.GetTexture("textbox").Height), Color.White);
            
            spriteBatch.Draw(this.MouseOver ? spriteArrowActive : spriteArrow, new Rectangle(bounds.Right - spriteArrow.Width - 5, bounds.Height / 2 - spriteArrow.Height / 2, spriteArrow.Width, spriteArrow.Height), Color.White);

            Utils.DrawUtil.DrawAlignedText(spriteBatch, Overlay.font_def12, this.SelectedItem, new Rectangle(5,0, bounds.Width - 10 - spriteArrow.Width, bounds.Height), Color.FromNonPremultiplied(239, 240, 239, 255), Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
        }

    }
}
