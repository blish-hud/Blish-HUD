using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;
using Praeclarum.Bind;

namespace Blish_HUD.Controls {

    // TODO: Decide if we should split this into two classes, MenuItem and an inheriting MenuItem meant for accordion menus
    public class MenuItem : ScrollingButtonContainer, IMenuItem, ICheckable {

        public enum AccordionState {
            Expanded,
            Collapsed
        }

        private const int DEFAULT_ITEM_HEIGHT = 32;
        
        private const int ICON_PADDING = 10;
        private const int ICON_SIZE    = 32;
        
        private const int ARROW_SIZE = 16;

        public event EventHandler<EventArgs> ItemSelected;
        protected virtual void OnItemSelected(EventArgs e) {
            this.ItemSelected?.Invoke(this, e);
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;
        protected virtual void OnCheckedChanged(CheckChangedEvent e) {
            this.CheckedChanged?.Invoke(this, e);
        }

        private int _menuItemHeight = DEFAULT_ITEM_HEIGHT;
        public int MenuItemHeight {
            get => _menuItemHeight;
            set {
                if (_menuItemHeight == value) return;

                _menuItemHeight = value;
                UpdateContentRegion();

                foreach (var control in this.Children) {
                    var childMenuItem = (IMenuItem)control;

                    childMenuItem.MenuItemHeight = value;
                }

                OnPropertyChanged();
            }
        }

        private bool _selected = false;
        public bool Selected {
            get => _selected;
            set {
                if (_selected == value) return;

                _selected = value;
                OnPropertyChanged();

                OnItemSelected(EventArgs.Empty);
            }
        }

        private string _text = "";
        public string Text {
            get => _text;
            set {
                _text = value;
                OnPropertyChanged();
            }
        }

        private Texture2D _icon;
        public Texture2D Icon {
            get => _icon;
            set {
                if (_icon == value) return;

                _icon = value;
                OnPropertyChanged();
            }
        }

        private bool _canCheck = false;
        public bool CanCheck {
            get => _canCheck;
            set {
                if (_canCheck == value) return;

                _canCheck = value;
                OnPropertyChanged();
            }
        }

        private AccordionState _state = AccordionState.Collapsed;
        [JsonIgnore]
        public AccordionState State {
            get => _state;
            set {
                if (_state == value) return;

                switch (_state) {
                    case AccordionState.Expanded:
                        Collapse();
                        break;
                    case AccordionState.Collapsed:
                        Expand();
                        break;
                }

                OnPropertyChanged();
            }
        }

        private bool _checked = false;
        public bool Checked {
            get => _checked;
            set {
                if (_checked == value) return;

                _checked = value;
                OnPropertyChanged();
                OnCheckedChanged(new CheckChangedEvent(_checked));
            }
        }

        #region "Internal" Properties

        private bool _overSection = false;
        [JsonIgnore]
        private bool OverSection {
            get => _overSection;
            set {
                if (_overSection == value) return;

                _overSection = value;
                OnPropertyChanged();
            }
        }

        private float _arrowRotation = -MathHelper.PiOver2;
        // Must remain public for Glide to be able to access the property
        [JsonIgnore]
        public float ArrowRotation {
            get => _arrowRotation;
            set {
                if (_arrowRotation == value) return;

                _arrowRotation = value;
                OnPropertyChanged();
            }
        }

        private bool _mouseOverIconBox = false;
        [JsonIgnore]
        private bool MouseOverIconBox {
            get => _mouseOverIconBox;
            set {
                if (_mouseOverIconBox == value) return;

                _mouseOverIconBox = value;

                // This property is not publicly exposed, so we won't call OnPropertyChanged
                Invalidate();
            }
        }

        private int LeftSidePadding {
            get {
                int leftSideBuilder = ICON_PADDING;

                // Add space if we need to render dropdown arrow
                if (this.Children.Any())
                    leftSideBuilder += ARROW_SIZE;

                //if (this.CanCheck || this.Icon != null)

                return leftSideBuilder;
            }
        }

        private Rectangle FirstItemBoxRegion =>
            new Rectangle(
                          this.LeftSidePadding,
                          this.MenuItemHeight / 2 - ICON_SIZE / 2,
                          ICON_SIZE,
                          ICON_SIZE
                         );

        #endregion

        #region Checkbox Features

        // Basically just copying the checkbox implementation for now

        private static List<TextureRegion2D> _cbRegions;

        private static void LoadCheckboxSprites() {
            if (_cbRegions != null) return;

            _cbRegions = new List<TextureRegion2D>();

            _cbRegions.AddRange(
                                new TextureRegion2D[] {
                                    ControlAtlas.GetRegion("checkbox/cb-unchecked"),
                                    ControlAtlas.GetRegion("checkbox/cb-unchecked-active"),
                                    ControlAtlas.GetRegion("checkbox/cb-unchecked-disabled"),
                                    ControlAtlas.GetRegion("checkbox/cb-checked"),
                                    ControlAtlas.GetRegion("checkbox/cb-checked-active"),
                                    ControlAtlas.GetRegion("checkbox/cb-checked-disabled"),
                                }
                               );
        }

        #endregion

        private static Texture2D _spriteArrow;
        private Glide.Tween _slideAnim;

        public MenuItem() { Initialize();  }

        public MenuItem(string text) {
            _text = text;

            Initialize();
        }

        private void Initialize() {
            this.Height = this.MenuItemHeight;

            _spriteArrow = _spriteArrow ?? (_spriteArrow = Content.GetTexture("156057"));

            this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, this.Height - this.MenuItemHeight);

            LoadCheckboxSprites();
        }

        protected override void OnClick(MouseEventArgs e) {
            if (this.Enabled
             && this.CanCheck
             && this.MouseOverIconBox) { /* Mouse was clicked inside of the checkbox */

                this.Checked = !this.Checked;
            } else if (this.Enabled
                    && this.OverSection
                    && this.Children.Any()) { /* Mouse was clicked inside of the mainbody of the MenuItem */

                ToggleSection();
            } else if (this.Enabled
                    && this.OverSection
                    && this.CanCheck) { /* Mouse was clicked inside of the mainbody of the MenuItem,
                                           but we have no children, so we toggle checkbox */

                this.Checked = !this.Checked;
            }

            base.OnClick(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            // Used if this menu item has its checkbox enabled
            this.MouseOverIconBox = this.MouseOver && this.FirstItemBoxRegion.Contains(this.RelativeMousePosition);

            // Helps us know when the mouse is over the MenuItem itself, or actually over its children
            this.OverSection = this.RelativeMousePosition.Y <= this.MenuItemHeight;
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            base.OnMouseLeft(e);

            this.OverSection = false;
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse;

        protected override void OnChildAdded(ChildChangedEventArgs e) { 
            if (!(e.ChangedChild is IMenuItem newChild)) {
                e.Cancel = true;
                return;
            }

            newChild.MenuItemHeight = this.MenuItemHeight;

            // Ensure child items remains the same width as us
            Binding.Create(() => e.ChangedChild.Width  == this.Width);

            // We'll bind the top of the control to the bottom of the last control we added
            var lastItem = this.Children.LastOrDefault();
            if (lastItem != null)
                Binding.Create(() => e.ChangedChild.Top == lastItem.Top + lastItem.Height /* complex binding to break 2-way bind */);

            e.ChangedChild.Resized += delegate { UpdateContentRegion(); };

            UpdateContentRegion();    
        }

        private void UpdateContentRegion(IEnumerable<MenuItem> extraChildren = null) {
            var allChildItems = new List<IMenuItem>(this.Children.Select(c => (IMenuItem) c).ToList<IMenuItem>());

            // Allows OnChildAdded to include the pending child item that was added
            if (extraChildren != null)
                allChildItems.AddRange(extraChildren);

            if (allChildItems.Any()) {
                this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, allChildItems.Max(c => ((Control) c).Bottom));
            } else {
                this.ContentRegion = new Rectangle(0, this.MenuItemHeight, this.Width, 1);
            }

            this.Height = this.State == AccordionState.Expanded ? this.ContentRegion.Bottom : this.MenuItemHeight;
        }

        public void ToggleSection() {
            switch (this.State) {
                case AccordionState.Collapsed:
                    Expand();
                    break;
                case AccordionState.Expanded:
                    Collapse();
                    break;
            }
        }

        public void Expand() {
            if (this.State == AccordionState.Expanded) return;

            _slideAnim?.CancelAndComplete();

            _state = AccordionState.Expanded;

            _slideAnim = Animation.Tweener
                                 .Tween(
                                        this,
                                        new {
                                            //Height = this.ContentRegion.Bottom,
                                            ArrowRotation = 0f
                                        },
                                        0.3f
                                       )
                                 .Ease(Glide.Ease.QuadOut);

            this.Height = this.ContentRegion.Bottom;
        }

        public void Collapse() {
            if (this.State == AccordionState.Collapsed) return;

            _slideAnim?.CancelAndComplete();

            _state = AccordionState.Collapsed;

            _slideAnim = Animation.Tweener
                                 .Tween(
                                        this,
                                        new {
                                            //Height = AccordionMenu.ITEM_HEIGHT,
                                            ArrowRotation = -MathHelper.PiOver2
                                        },
                                        0.3f
                                       )
                                 .Ease(Glide.Ease.QuadOut);

            this.Height = this.MenuItemHeight;
        }

        protected override float GetVerticalDrawPercent() {
            return (float)this.MenuItemHeight / this.Height;
        }
        

        private void DrawDropdownArrow(SpriteBatch spriteBatch, Rectangle bounds) {
            var arrowOrigin = new Vector2((float)ARROW_SIZE / 2, (float)ARROW_SIZE / 2);

            var arrowDest = new Rectangle(
                                                5 + ARROW_SIZE / 2,
                                                this.MenuItemHeight / 2,
                                                ARROW_SIZE,
                                                ARROW_SIZE
                                               );

            spriteBatch.Draw(_spriteArrow,
                             arrowDest,
                             null,
                             Color.White,
                             this.ArrowRotation,
                             arrowOrigin,
                             SpriteEffects.None,
                             0);
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            int currentLeftSidePadding = this.LeftSidePadding;

            if (this.Children.Any())
                DrawDropdownArrow(spriteBatch, bounds);

            TextureRegion2D firstItemSprite = null;

            if (this.CanCheck) {
                string state = this.Checked ? "-checked" : "-unchecked";

                string extension = "";
                extension = this.MouseOverIconBox ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;

                firstItemSprite = _cbRegions.First(cb => cb.Name == $"checkbox/cb{state}{extension}");
            } else if (this.Icon != null) {
                // performance?
                firstItemSprite = new TextureRegion2D(this.Icon);
            }

            // Draw either the checkbox or the icon, if one or the either is available
            if (firstItemSprite != null) {
                spriteBatch.Draw(
                                 firstItemSprite,
                                 this.FirstItemBoxRegion,
                                 Color.White
                                );
                
                currentLeftSidePadding += ICON_SIZE + ICON_PADDING;
            }

            DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, this.Text, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight).OffsetBy(bounds.Location).OffsetBy(-1, 0), Color.Black, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
            DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, this.Text, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight).OffsetBy(bounds.Location).OffsetBy(1, 0), Color.Black, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
            DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, this.Text, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight).OffsetBy(bounds.Location).OffsetBy(0, -1), Color.Black, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
            DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, this.Text, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight).OffsetBy(bounds.Location).OffsetBy(0, 1), Color.Black, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
            DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, this.Text, new Rectangle(currentLeftSidePadding, 0, this.Width - (currentLeftSidePadding - ICON_PADDING), this.MenuItemHeight).OffsetBy(bounds.Location), Color.White, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
        }

    }
}
