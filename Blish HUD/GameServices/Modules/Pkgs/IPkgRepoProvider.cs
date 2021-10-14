using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Pkgs {
    public interface IPkgRepoProvider {

        Task<bool> Load(IProgress<string> progress);

        IEnumerable<PkgManifest> GetPkgManifests();

        IEnumerable<PkgManifest> GetPkgManifests(IEnumerable<Func<PkgManifest, bool>> filters);

        IEnumerable<(string OptionName, Action<bool> OptionAction, bool IsToggle, bool IsChecked)> GetExtraOptions();

    }
}
