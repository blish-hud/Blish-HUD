using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.UI.Controls {
    internal class ModuleMenuItem : MenuItem {

        private readonly ModuleManager _module;

        public ModuleMenuItem(ModuleManager module) : base(module.Manifest.Name) {
            _module = module;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(bounds.X, bounds.Y, 5, _menuItemHeight),
                                   _module.Enabled
                                       ? Color.Green * 0.75f
                                       : Color.Gray  * 0.5f);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e) {
            base.OnRightMouseButtonPressed(e);

            IEnumerable<ContextMenuStripItem> GetModuleMenuItems() {
                var enable = new ContextMenuStripItem(Strings.GameServices.ModulesService.ModuleManagement_EnableModule) { Enabled = !_module.Enabled };
                enable.Click += (s, e) => { _module.TryEnable(); };

                var disable = new ContextMenuStripItem(Strings.GameServices.ModulesService.ModuleManagement_DisableModule) { Enabled = _module.Enabled };
                disable.Click += (s, e) => { _module.Disable(); };

                yield return enable;
                yield return disable;
            }

            var contextMenu = new ContextMenuStrip(GetModuleMenuItems);

            contextMenu.Show(e.MousePosition);
        }

    }
}
