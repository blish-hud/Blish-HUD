﻿using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;

namespace Blish_HUD.Controls {

    public class MenuItem : Container, IMenuItem, ICheckable, IAccordion {

        private const int DEFAULT_ITEM_HEIGHT = 32;
        
        private const int ICON_PADDING = 10;
        private const int ICON_SIZE    = 32;
        
        private const int ARROW_SIZE = 16;

        #region Textures

        private readonly AsyncTexture2D _textureArrow = AsyncTexture2D.FromAssetId(156057);

        #endregion

        #region Events

        public event EventHandler<ControlActivatedEventArgs> ItemSelected;
        protected virtual void OnItemSelected(ControlActivatedEventArgs e) {
            this.ItemSelected?.Invoke(this, e);
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;
        protected virtual void OnCheckedChanged(CheckChangedEvent e) {
            this.CheckedChanged?.Invoke(this, e);
        }

        #endregion

        #region Properties
        
        protected int _menuItemHeight = DEFAULT_ITEM_HEIGHT;
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                if (!SetProperty(ref _menuItemHeight, value, true)) return;

                this.Height = _menuItemHeight;

                // Update all children to ensure they match in height
                foreach (var childMenuItem in _children.Cast<IMenuItem>().ToList()) {
                    childMenuItem.MenuItemHeight = _menuItemHeight;
                }
            }
        }

        protected bool _shouldShift = false;
        public bool ShouldShift {
            get => _shouldShift;
            set => SetProperty(ref _shouldShift, value, true);
        }

        public bool Selected => _selectedMenuItem == this;

        protected MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem {
            get => _selectedMenuItem;
        }

        protected int _menuDepth = 0;
        protected int MenuDepth {
            get => _menuDepth;
            set => SetProperty(ref _menuDepth, value);
        }

        protected string _text;
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        protected AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        protected bool _canCheck = false;
        public bool CanCheck {
            get => _canCheck;
            set => SetProperty(ref _canCheck, value);
        }

        protected bool _collapsed = true;
        [JsonIgnore]
        public bool Collapsed {
            get => _collapsed;
            set {
                if (value) {
                    Collapse();
                } else {
                    Expand();
                }
            }
        }

        protected bool _checked = false;
        public bool Checked {
            get => _checked;
            set {
                if (SetProperty(ref _checked, value)) {
                    OnCheckedChanged(new CheckChangedEvent(_checked));
                }
            }
        }

        protected Color _textColor = Color.White;
        /// <summary>
        /// The color of the <see cref="Text"/>.
        /// </summary>
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        #endregion

        #region "Internal" Properties

        protected bool _overSection = false;
        [JsonIgnore]
        private bool OverSection {
            get => _overSection;
            set {
                if (_overSection == value) return;

                _overSection = value;
                OnPropertyChanged();
            }
        }

        // Must remain internal for Glide to be able to access the property
        [JsonIgnore]
        public float ArrowRotation { get; set; } = -MathHelper.PiOver2;

        [JsonIgnore]
        private bool MouseOverIconBox { get; set; } = false;

        private int LeftSidePadding {
            get {
                int leftSideBuilder = ICON_PADDING;

                // Add space if we need to render dropdown arrow
                if (!_children.IsEmpty)
                    leftSideBuilder += ARROW_SIZE;

                return leftSideBuilder;
            }
        }

        private Rectangle FirstItemBoxRegion =>
            new Rectangle(0,
                          this.MenuItemHeight / 2 - ICON_SIZE / 2,
                          ICON_SIZE,
                          ICON_SIZE);

        #endregion

        private Glide.Tween                      _slideAnim;
        private Effects.ScrollingHighlightEffect _scrollEffect;

        public MenuItem() : this("", null) { /* NOOP */ }

        public MenuItem(string text) : this(text, null) { /* NOOP */ }

        public MenuItem(string text, AsyncTexture2D icon) {
            _text = text;
            _icon = icon;

            Initialize();
        }

        private void Initialize() {
            _scrollEffect = new Effects.ScrollingHighlightEffect(this);

            this.EffectBehind = _scrollEffect;

            this.Height = this.MenuItemHeight;

            this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, 0);
        }

        #region Menu Item Selection

        public void Select() {
            if (this.Selected) return;

            _selectedMenuItem = this;

            if (!_children.IsEmpty)
                throw new InvalidOperationException("MenuItems with sub-MenuItems can not be selected directly.");

            _scrollEffect.ForceActive = true;

            ((IMenuItem)this).Select(this);
            OnPropertyChanged(nameof(this.Selected));
        }

        void IMenuItem.Select(MenuItem menuItem) {
            ((IMenuItem)this).Select(menuItem, new List<IMenuItem>() { this });
        }

        void IMenuItem.Select(MenuItem menuItem, List<IMenuItem> itemPath) {
            itemPath.Add(this);

            OnItemSelected(new ControlActivatedEventArgs(menuItem));

            // Expand to show the selected MenuItem, if necessary
            if (!_children.IsEmpty) {
                this.Expand();
            }

            var parentMenuItem = this.Parent as IMenuItem;
            parentMenuItem?.Select(menuItem, itemPath);
        }

        public void Deselect() {
            bool isSelected = this.Selected;
            _selectedMenuItem         = null;
            _scrollEffect.ForceActive = false;

            if (isSelected) {
                OnPropertyChanged(nameof(this.Selected));
            }
        }

        #endregion
        
        public override void RecalculateLayout() {
            _scrollEffect.Size = new Vector2(_size.X, _menuItemHeight);

            UpdateContentRegion();
        }

        private void UpdateContentRegion() {
            var children = _children.ToArray();

            int bottomChild = ReflowChildLayout(children);

            this.ContentRegion = children.Any()
                                     ? new Rectangle(0, this.MenuItemHeight, _size.X, bottomChild)
                                     : new Rectangle(0, this.MenuItemHeight, _size.X, 0);

            this.Height = !_collapsed
                              ? this.ContentRegion.Bottom
                              : this.MenuItemHeight;
        }

        protected override void OnClick(MouseEventArgs e) {
            if (_canCheck && this.MouseOverIconBox) { 
                // Mouse was clicked inside of the checkbox
                Checked = !Checked;

            } else if (_overSection && !_children.IsEmpty) {
                // Mouse was clicked inside of the mainbody of the MenuItem
                GameService.Content.PlaySoundEffectByName($"menu-click-{RandomUtil.GetRandom(1,4)}");
                ToggleAccordionState();

            } else if (_overSection && _canCheck) {
                // Mouse was clicked inside of the mainbody of the MenuItem,
                // but we have no children, so we toggle checkbox
                Checked = !Checked;

            } else if (!_canCheck && _children.IsEmpty) {
                // Cannot be checked and has no children, so we probably navigate views.
                GameService.Content.PlaySoundEffectByName("menu-item-click");
            }

            if (_children.IsEmpty) {
                this.Select();
            }

            base.OnClick(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            // Helps us know when the mouse is over the MenuItem itself, or actually over its children
            OverSection = RelativeMousePosition.Y <= _menuItemHeight;

            if (OverSection) {
                _scrollEffect.Enable();
            } else {
                _scrollEffect.Disable();
            }

            // Used if this menu item has its checkbox enabled
            MouseOverIconBox = _canCheck
                            && _overSection
                            && FirstItemBoxRegion
                              .OffsetBy(LeftSidePadding, 0)
                              .Contains(RelativeMousePosition);

            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            OverSection = false;

            base.OnMouseLeft(e);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) { 
            if (!(e.ChangedChild is MenuItem newChild)) {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;
            newChild.MenuDepth = this.MenuDepth + 1;

            ReflowChildLayout(_children.ToArray());
        }

        private int ReflowChildLayout(IEnumerable<Control> allChildren) {
            int lastBottom = 0;

            foreach (var child in allChildren.Where(c => c.Visible)) {
                child.Location = new Point(0, lastBottom);
                child.Width    = this.Width;

                lastBottom = child.Bottom;
            }

            return lastBottom;
        }
        
        public bool ToggleAccordionState() {
            this.Collapsed = !_collapsed;
            return _collapsed;
        }

        public void Expand() {
            if (!_collapsed) return;

            _slideAnim?.CancelAndComplete();

            SetProperty(ref _collapsed, false);

            _slideAnim = Animation.Tweener
                                 .Tween(this,
                                        new { ArrowRotation = 0f },
                                        0.3f)
                                 .Ease(Glide.Ease.QuadOut);

            this.Height = this.ContentRegion.Bottom;
        }

        public void Collapse() {
            if (_collapsed) return;

            _slideAnim?.CancelAndComplete();

            SetProperty(ref _collapsed, true);

            _slideAnim = Animation.Tweener
                                 .Tween(this,
                                        new { ArrowRotation = -MathHelper.PiOver2 },
                                        0.3f)
                                 .Ease(Glide.Ease.QuadOut);

            this.Height = this.MenuItemHeight;
        }

        private void DrawDropdownArrow(SpriteBatch spriteBatch) {
            var arrowOrigin = new Vector2((float)ARROW_SIZE / 2, (float)ARROW_SIZE / 2);

            var arrowDest = new Rectangle(5 + ARROW_SIZE / 2,
                                          this.MenuItemHeight / 2,
                                          ARROW_SIZE,
                                          ARROW_SIZE);

            spriteBatch.DrawOnCtrl(this,
                                   _textureArrow,
                                   arrowDest,
                                   null,
                                   Color.White,
                                   this.ArrowRotation,
                                   arrowOrigin);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            int currentLeftSidePadding = this.LeftSidePadding;

            // If MenuItem has children, show dropdown arrow
            if (!_children.IsEmpty)
                DrawDropdownArrow(spriteBatch);

            TextureRegion2D firstItemSprite = null;

            if (this.CanCheck) {
                string state = this.Checked ? "-checked" : "-unchecked";

                string extension = "";
                extension = this.MouseOverIconBox ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;

                firstItemSprite = Resources.Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}");
            } else if (this.Icon != null && _children.IsEmpty) {
                // performance?
                firstItemSprite = new TextureRegion2D(this.Icon);
            }

            // Draw either the checkbox or the icon, if one or the either is available
            if (firstItemSprite != null) {
                spriteBatch.DrawOnCtrl(this,
                                       firstItemSprite,
                                       this.FirstItemBoxRegion.OffsetBy(currentLeftSidePadding, 0));
            }

            if (_canCheck) {
                currentLeftSidePadding += ICON_SIZE + ICON_PADDING;
            } else if (!_children.IsEmpty) {
                currentLeftSidePadding += ICON_PADDING;
            } else if (_icon != null) {
                currentLeftSidePadding += ICON_SIZE + ICON_PADDING;
            }

            spriteBatch.DrawStringOnCtrl(this, _text, Content.DefaultFont16, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight), _textColor, true, true);
        }

    }
}
