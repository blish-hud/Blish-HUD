using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules {

    [Export(typeof(IModule))]
    public class TestMefModule : Module, IModule {

        public Settings Settings { get; set; }
        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                                  "Test MEF Module",
                                  null,
                                  "bh.test.mef",
                                  "Used for testing MEF implementation.",
                                  "LandersXanders.1235",
                                  "1"
                                 );
        }

        public override void DefineSettings(Settings settings) {
            Settings = settings;
        }

        public void OnLoad() {
            Console.WriteLine("TestMefModule loaded!");
        }

        public void OnEnabled() {
            Console.WriteLine("TestMefModule enabled!");
        }

        public void OnDisabled() {
            Console.WriteLine("TestMefModule disabled!");
        }

        public void Update(GameTime gameTime) {
            Console.WriteLine("TestMefModule updated!");
        }

    }

}
