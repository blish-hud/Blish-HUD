namespace Blish_HUD.Modules.Pkgs {
    public class PublicPkgRepoProvider : StaticPkgRepoProvider {

        private const string REPO_SETTINGS    = "RepoConfiguration";
        private const string REPO_URL_SETTING = "PkgsUrl";

        private const string BHUDPKGS_REPOURL = "https://pkgs.blishhud.com/";

        private readonly string _repoUrl;

        public override string PkgUrl => _repoUrl;

        public PublicPkgRepoProvider() {
            // Allow manual override of built-in package repo
            _repoUrl = GameService.Settings
                                  .RegisterRootSettingCollection(REPO_SETTINGS)
                                  .DefineSetting(REPO_URL_SETTING, BHUDPKGS_REPOURL).Value;
        }

    }
}
