using System;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Input;
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
                this.HighlightedItem = this.RelativeMousePosition.Y / _assocDropdown.Height;

                base.OnMouseMoved(e);
            }

            protected override void OnClick(MouseEventArgs e) {
                _assocDropdown.SelectedItem = _assocDropdown.Items[this.HighlightedItem];

                base.OnClick(e);

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
                                                             _size.X - 12          - _textureArrow.Width,
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
                                                                   bounds.Width - 13 - _textureArrow.Width,
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
                                                                   bounds.Width - 13 - _textureArrow.Width,
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

        public static readonly DesignStandard Standard = new DesignStandard(/*          Size */ new Point(250, 27),
                                                                            /*   PanelOffset */ new Point(5,   2),
                                                                            /* ControlOffset */ Control.ControlStandard.ControlOffset);

        #region Load Static

        private static readonly Texture2D _textureInputBox;

        private static readonly TextureRegion2D _textureArrow;
        private static readonly TextureRegion2D _textureArrowActive;

        static Dropdown() {
            _textureInputBox = Content.GetTexture("input-box");

            _textureArrow       = Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow");
            _textureArrowActive = Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow-active");
        }

        #endregion

        #region Events

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected virtual void OnValueChanged(ValueChangedEventArgs e) {
            this.ValueChanged?.Invoke(this, e);
        }

        #endregion

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
        private bool          _hadPanel  = false;

        public Dropdown() {
            this.Items = new ObservableCollection<string>();

            this.Items.CollectionChanged += delegate {
                ItemsUpdated();
                Invalidate();
            };

            this.Size = Standard.Size;
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
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
            spriteBatch.DrawOnCtrl(this,
                                   _textureInputBox,
                                   new Rectangle(Point.Zero, _size).Subtract(new Rectangle(0, 0, 5, 0)),
                                   new Rectangle(
                                                 0, 0,
                                                 Math.Min(_textureInputBox.Width - 5, this.Width - 5),
                                                 _textureInputBox.Height
                                                )
                                  );

            // Draw right side of dropdown
            spriteBatch.DrawOnCtrl(
                                   this,
                                   _textureInputBox,
                                   new Rectangle(_size.X - 5, 0, 5, _size.Y),
                                   new Rectangle(
                                                 _textureInputBox.Width - 5, 0,
                                                 5, _textureInputBox.Height
                                                )
                                  );
            
            // Draw dropdown arrow
            spriteBatch.DrawOnCtrl(
                                   this,
                                   this.MouseOver ? _textureArrowActive : _textureArrow,
                                   new Rectangle(
                                                 _size.X - _textureArrow.Width - 5,
                                                 _size.Y / 2                 - _textureArrow.Height / 2,
                                                 _textureArrow.Width,
                                                 _textureArrow.Height
                                                )
                                  );

            // Draw text
            spriteBatch.DrawStringOnCtrl(
                                         this,
                                         _selectedItem,
                                         Content.DefaultFont14,
                                         new Rectangle(
                                                       5, 0,
                                                       _size.X - 10 - _textureArrow.Width,
                                                       _size.Y
                                                      ),
                                         Color.FromNonPremultiplied(239, 240, 239, 255)
                                        );
        }

    }
}
