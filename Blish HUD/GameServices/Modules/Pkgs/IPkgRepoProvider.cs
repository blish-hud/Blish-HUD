using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Pkgs {
    public interface IPkgRepoProvider {

        Task<bool> Load(IProgress<string> progress);

        IEnumerable<PkgManifest> GetPkgManifests(params Func<PkgManifest, bool>[] filters);

        IEnumerable<(string OptionName, Action OptionAction)> GetExtraOptions();

    }
}
