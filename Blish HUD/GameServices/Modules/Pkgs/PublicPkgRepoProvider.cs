namespace Blish_HUD.Modules.Pkgs {
    public class PublicPkgRepoProvider : GitHubPkgRepoProvider {

        private const string REPO_SETTINGS    = "RepoConfiguration";
        private const string REPO_URL_SETTING = "RepoUrl";

        private const string BHUDPKGS_REPOURL = "https://api.github.com/repos/blish-hud/bhud-pkgs";

        private readonly string _repoUrl;

        protected override string RepoUrl => _repoUrl;

        public PublicPkgRepoProvider() {
            // Allow manual override of built-in package repo
            _repoUrl = GameService.Settings
                                  .RegisterRootSettingCollection(REPO_SETTINGS)
                                  .DefineSetting(REPO_URL_SETTING, BHUDPKGS_REPOURL).Value;
        }

    }
}
