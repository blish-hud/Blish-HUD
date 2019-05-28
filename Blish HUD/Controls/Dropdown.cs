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

            protected int _highlightedItem = -1;
            private int HighlightedItem {
                get => _highlightedItem;
                set => SetProperty(ref _highlightedItem, value);
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
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(Point.Zero, _size), Color.Black);

                int index = 0;
                foreach (string item in _assocDropdown.Items) {

                    if (index == this.HighlightedItem) {
                        spriteBatch.DrawOnCtrl(
                                               this,
                                               ContentService.Textures.Pixel,
                                               new Rectangle(
                                                             2,
                                                             2                     + _assocDropdown.Height * index,
                                                             _size.X - 12          - spriteArrow.Width,
                                                             _assocDropdown.Height - 4
                                                            ),
                                               new Color(45, 37, 25, 255)
                                              );

                        spriteBatch.DrawStringOnCtrl(
                                                     this,
                                                     item,
                                                     Content.DefaultFont14,
                                                     new Rectangle(
                                                                   8,
                                                                   _assocDropdown.Height * index,
                                                                   bounds.Width - 13 - spriteArrow.Width,
                                                                   _assocDropdown.Height
                                                                  ),
                                                     ContentService.Colors.Chardonnay
                                                    );
                    } else {
                        spriteBatch.DrawStringOnCtrl(
                                                     this,
                                                     item,
                                                     Content.DefaultFont14,
                                                     new Rectangle(8,
                                                                   _assocDropdown.Height * index,
                                                                   bounds.Width - 13 - spriteArrow.Width,
                                                                   _assocDropdown.Height),
                                                     Color.FromNonPremultiplied(239, 240, 239, 255));
                    }

                    index++;
                }
            }

            protected override void DisposeControl() {
                if (_assocDropdown != null) {
                    _assocDropdown._lastPanel = null;
                    _assocDropdown = null;
                }

                Input.LeftMouseButtonPressed -= Input_MousedOffDropdownPanel;
                Input.RightMouseButtonPressed -= Input_MousedOffDropdownPanel;

                base.DisposeControl();
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

        public ObservableCollection<string> Items { get; }

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
            // Draw dropdown
            spriteBatch.DrawOnCtrl(
                                   this,
                                   Content.GetTexture("input-box"),
                                   new Rectangle(Point.Zero, _size).Subtract(new Rectangle(0, 0, 5, 0)),
                                   new Rectangle(
                                                 0, 0,
                                                 Math.Min(Content.GetTexture("textbox").Width - 5, this.Width - 5),
                                                 Content.GetTexture("textbox").Height
                                                )
                                  );

            // Draw right side of dropdown
            spriteBatch.DrawOnCtrl(
                                   this,
                                   Content.GetTexture("input-box"),
                                   new Rectangle(_size.X - 5, 0, 5, _size.Y),
                                   new Rectangle(
                                                 Content.GetTexture("textbox").Width - 5, 0,
                                                 5, Content.GetTexture("textbox").Height
                                                )
                                  );
            
            // Draw dropdown arrow
            spriteBatch.DrawOnCtrl(
                                   this,
                                   this.MouseOver ? spriteArrowActive : spriteArrow,
                                   new Rectangle(
                                                 _size.X - spriteArrow.Width - 5,
                                                 _size.Y / 2                 - spriteArrow.Height / 2,
                                                 spriteArrow.Width,
                                                 spriteArrow.Height
                                                )
                                  );

            // Draw text
            spriteBatch.DrawStringOnCtrl(
                                         this,
                                         _selectedItem,
                                         Content.DefaultFont14,
                                         new Rectangle(
                                                       5, 0,
                                                       _size.X - 10 - spriteArrow.Width,
                                                       _size.Y
                                                      ),
                                         Color.FromNonPremultiplied(239, 240, 239, 255)
                                        );
        }

    }
}
