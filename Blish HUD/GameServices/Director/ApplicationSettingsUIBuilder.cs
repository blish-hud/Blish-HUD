using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameServices.Director {
    public static class ApplicationSettingsUIBuilder {

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) {
            return potentialDescendant.IsSubclassOf(potentialBase)
                || potentialDescendant == potentialBase;
        }

        public static void BuildSingleModuleSettings(Panel buildPanel, object nothing) {
            int lastBottom = 0;

            foreach (var settingEntry in GameService.Director._applicationSettings.Entries) {

                if (settingEntry.Renderer != null) {
                    Control settingCtrl = settingEntry.Renderer.Invoke(settingEntry);
                    settingCtrl.Location = new Point(0, lastBottom + 10);
                    settingCtrl.Parent   = buildPanel;

                    lastBottom = settingCtrl.Bottom;

                    continue;
                }

                foreach (var typeRenderer in GameService.Settings.SettingTypeRenderers) {

                    if (IsSameOrSubclass(typeRenderer.Key, settingEntry.SettingType)) {
                        Control settingCtrl = typeRenderer.Value.Invoke(settingEntry);
                        settingCtrl.Location = new Point(0, lastBottom + 10);
                        settingCtrl.Parent = buildPanel;

                        lastBottom = settingCtrl.Bottom;

                        break;
                    }

                }
            }
        }

    }

}
