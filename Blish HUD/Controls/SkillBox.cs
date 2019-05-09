using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    public enum SkillBoxDirection {
        Up,
        Right,
        Down,
        Left
    }

    public enum SkillBoxSize {
        /// <summary>
        /// The size of F1-F5 Abilities / Mount
        /// </summary>
        Small,
        /// <summary>
        /// The size of standard 1-0 abilities
        /// </summary>
        Normal
    }

    public class SkillBox:Control {

        private const int BOX_DIMENSIONSATSCALE_NORMAL = 60;
        private const int ARROW_DIMENSIONATSCALE_NORMAL = 14;

        private const int BOX_DIMENSIONSATSCALE_SMALL = 45;
        private const int ARROW_DIMENSIONATSCALE_SMALL = 10;

        protected static TextureAtlas skillBoxAtlas;

        private bool _dropped = false;
        public bool Dropped { get { return _dropped; } set { if (_dropped != value) { _dropped = value; Invalidate(); } } }


        private static void LoadStaticResources() {
            //if (skillBoxAtlas == null) {
            //    skillBoxAtlas = GameServices.GetService<ContentService>().GetTextureAtlas2(@"atlas\skillbox");
            //}
        }

        private Texture2D _icon;
        public Texture2D Icon { get { return _icon; } set { if (_icon != value) { _icon = value; Invalidate(); } } }

        private bool _hasDropdown = false;
        public bool HasDropdown { get { return _hasDropdown; } set { _hasDropdown = value; Invalidate(); } }

        private SkillBoxDirection _Direction = SkillBoxDirection.Up;
        public SkillBoxDirection Direction {
            get {
                return _Direction;
            }
            set {
                _Direction = value;
                Invalidate();
            }
        }

        private SkillBoxSize _boxScale = SkillBoxSize.Normal;
        public SkillBoxSize BoxScale {
            get {
                return _boxScale;
            }
            set {
                _boxScale = value;
                Invalidate();
            }
        }

        public List<SkillBox> Items = new List<SkillBox>();

        private EaseAnimation animPulseLoad;
        private EaseAnimation animFlipIcon;

        public void FlipIcon(Texture2D newIcon) {
            if (animFlipIcon != null || animPulseLoad.Active) return;

            bool stageOneComplete = false;

            animFlipIcon = GameServices.GetService<AnimationService>().Tween(0, this.BoxScale == SkillBoxSize.Normal ? BOX_DIMENSIONSATSCALE_NORMAL : BOX_DIMENSIONSATSCALE_SMALL, 150, AnimationService.EasingMethod.Linear);

            animFlipIcon.AnimationCompleted += delegate {
                if (!stageOneComplete) {
                    stageOneComplete = true;
                    this.Icon = newIcon;
                    animFlipIcon.Reverse();
                } else {
                    GameServices.GetService<AnimationService>().RemoveAnim(animFlipIcon);
                    animFlipIcon = null;
                    this.BackgroundColor = Color.Black;
                    Invalidate();
                }
            };

            this.BackgroundColor = Color.Black;

            animFlipIcon.Start();
        }

        public void SetLoading() {
            if (animFlipIcon != null || animPulseLoad.Active) return;

            this.BackgroundColor = Color.White;
            animPulseLoad.Start(true);
        }

        public void StopLoading() {
            if (!animPulseLoad.Active) return;

            animPulseLoad.Stop();
            this.BackgroundColor = Color.Transparent;
            Invalidate();
        }

        public SkillBox() : base() {
            LoadStaticResources();

            this.ZIndex = Screen.MENUUI_BASEINDEX;

            animPulseLoad = Animation.Tween(1, 7, 600, AnimationService.EasingMethod.Linear);

            Invalidate();
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            this.Dropped = !this.Dropped;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        /*
        public override void Invalidate() {
            if (this.LayoutIsInvalid) return;

            base.Invalidate();
            
            var drawSize = Point.Zero;

            drawSize.X = this.BoxScale == SkillBoxSize.Normal ? BOX_DIMENSIONSATSCALE_NORMAL : BOX_DIMENSIONSATSCALE_SMALL;
            drawSize.Y = drawSize.X;
            
            if (this.HasDropdown) {
                if (this.Direction == SkillBoxDirection.Up || this.Direction == SkillBoxDirection.Down) {
                    drawSize.Y += this.BoxScale == SkillBoxSize.Normal ? ARROW_DIMENSIONATSCALE_NORMAL : ARROW_DIMENSIONATSCALE_SMALL;
                } else {
                    drawSize.X += this.BoxScale == SkillBoxSize.Normal ? ARROW_DIMENSIONATSCALE_NORMAL : ARROW_DIMENSIONATSCALE_SMALL;
                }
            }
            
            this.Size = drawSize;
            
            if (this.Dropped) {
                int ItemTop = this.AbsoluteBounds.Bottom;
                Items.ForEach(item => {
                    item.Visible = true;
                    item.Top = ItemTop;
                    item.Left = this.AbsoluteBounds.Left;
                    ItemTop = item.AbsoluteBounds.Bottom + 2;
                });
            } else {
                Items.ForEach(item => {
                    item.Visible = false;
                    item.Items.ForEach(subitem => subitem.Visible = false);
                });
            }
        }
        */

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (animPulseLoad.Active || animFlipIcon != null)
                Invalidate();
        }
        
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            int VertOffset = 0;
            int HorzOffset = 0;

            if (this.HasDropdown) {
                if (this.Direction == SkillBoxDirection.Up) {
                    VertOffset = this.BoxScale == SkillBoxSize.Normal ? ARROW_DIMENSIONATSCALE_NORMAL : ARROW_DIMENSIONATSCALE_SMALL;
                } else if (this.Direction == SkillBoxDirection.Left) {
                    HorzOffset = this.BoxScale == SkillBoxSize.Normal ? ARROW_DIMENSIONATSCALE_NORMAL : ARROW_DIMENSIONATSCALE_SMALL;
                }
            }
            
            var primaryTileBounds = new Rectangle(HorzOffset, VertOffset, BOX_DIMENSIONSATSCALE_NORMAL, BOX_DIMENSIONSATSCALE_NORMAL).OffsetBy(bounds.Location);
            
            if (!animPulseLoad.Active)
                spriteBatch.Draw(ControlAtlas.GetRegion("skillbox/sb-blank"), primaryTileBounds.OffsetBy(bounds.Location), Color.White);

            if (this.Icon != null)
                if (animFlipIcon == null)
                    spriteBatch.Draw(this.Icon, primaryTileBounds.OffsetBy(bounds.Location), new Rectangle(16, 16, 96, 96), Color.White);
                else
                    spriteBatch.Draw(this.Icon, primaryTileBounds.Add(0, animFlipIcon.CurrentValueInt / 2, 0, Math.Max(-animFlipIcon.CurrentValueInt, -primaryTileBounds.Height + 1)).OffsetBy(bounds.Location), new Rectangle(16, 16, 96, 96), Color.White);
            else
                spriteBatch.Draw(ControlAtlas.GetRegion("skillbox/sb-blank"), primaryTileBounds.OffsetBy(bounds.Location), Color.White);

            if (animPulseLoad.Active)
                spriteBatch.Draw(ControlAtlas.GetRegion($"skillbox/sb-anim1-f{animPulseLoad.CurrentValueInt}"), primaryTileBounds.OffsetBy(bounds.Location), Color.White);

            spriteBatch.Draw(ControlAtlas.GetRegion("skillbox/sb-outline"), primaryTileBounds.OffsetBy(bounds.Location), Color.White);

            if (this.MouseOver) {
                spriteBatch.Draw(ControlAtlas.GetRegion("skillbox/sb-hover"), primaryTileBounds.OffsetBy(bounds.Location), Color.White);
            }

            //if (this.Dropped && Items.Count == 0)
            //    spriteBatch.Draw(controlAtlas.GetRegion("skillbox/sb-close"), primaryTileBounds, Color.White * (this.MouseOver ? 0.8f : 1f));
        }


    }
}
