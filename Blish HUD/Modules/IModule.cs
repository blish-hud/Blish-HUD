using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules {

    public interface IModule {

        SettingsManager SettingsManager { get; set; }

        bool Enabled { get; set; }

        ModuleInfo GetModuleInfo();

        void DefineSettings(SettingsManager settingsManager);

        void OnLoad();

        void OnEnabled();

        void OnDisabled();

        void Update(GameTime gameTime);


    }

}