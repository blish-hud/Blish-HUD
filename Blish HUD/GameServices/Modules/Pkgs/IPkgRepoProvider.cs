using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Pkgs {
    public interface IPkgRepoProvider {

        public Task<bool> Load(IProgress<string> progress);

        public IEnumerable<PkgManifest> GetPkgManifests();

        public IEnumerable<PkgManifest> GetPkgManifests(IEnumerable<Func<PkgManifest, bool>> filters);

        public IEnumerable<(string OptionName, Action<bool> OptionAction, bool IsToggle, bool IsChecked)> GetExtraOptions();

    }
}
