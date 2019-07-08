using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules {

    public interface IModule {

        Settings Settings { get; set; }

        bool Enabled { get; set; }

        ModuleInfo GetModuleInfo();

        void DefineSettings(Settings settings);

        void OnLoad();

        void OnEnabled();

        void OnDisabled();

        void Update(GameTime gameTime);


    }

}