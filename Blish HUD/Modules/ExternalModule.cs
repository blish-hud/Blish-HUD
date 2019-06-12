using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules {

    public abstract class ExternalModule : IDisposable {

        #region Module Events

        public event EventHandler<EventArgs>                        ModuleLoaded;
        public event EventHandler<UnobservedTaskExceptionEventArgs> ModuleException;

        protected virtual void OnModuleLoaded(EventArgs e) {
            _loaded = true;

            ModuleLoaded?.Invoke(this, e);
        }

        protected virtual void OnModuleException(UnobservedTaskExceptionEventArgs e) {
            ModuleException?.Invoke(this, e);
        }

        #endregion

        private readonly ModuleParameters _moduleParameters;

        private bool _loaded = false;
        public bool Loaded => _loaded;

        #region Manifest & Parameter Aliases

        // Manifest

        public string Name => _moduleParameters.Manifest.Name;

        public string Namespace => _moduleParameters.Manifest.Namespace;

        public SemVer.Version Version => _moduleParameters.Manifest.Version;

        // Service Managers

        protected SettingsManager SettingsManager => _moduleParameters.SettingsManager;

        protected ContentsManager ContentsManager => _moduleParameters.ContentsManager;

        protected DirectoriesManager DirectoriesManager => _moduleParameters.DirectoriesManager;

        protected Gw2ApiManager Gw2ApiManager => _moduleParameters.GW2ApiManager;

        #endregion

        private Task _loadTask;

        [ImportingConstructor]
        public ExternalModule([Import("ModuleParameters")] ModuleParameters moduleParameters) {
            _moduleParameters = moduleParameters;
        }

        #region Module Method Interface

        public void DoInitialize() {
            DefineSettings(this.SettingsManager);

            Initialize();
        }

        public void DoLoad() {
            _loadTask = LoadAsync();
        }

        private void CheckForLoaded() {
            switch (_loadTask.Status) {
                case TaskStatus.Faulted:
                    OnModuleException(new UnobservedTaskExceptionEventArgs(_loadTask.Exception));
                    OnModuleLoaded(EventArgs.Empty);
                    break;

                case TaskStatus.RanToCompletion:
                    OnModuleLoaded(EventArgs.Empty);
                    break;

                default:
                    GameService.Debug.WriteErrorLine($"Unexpected module load result status '{_loadTask.Status.ToString()}'.");
                    break;
            }
        }

        public void DoUpdate(GameTime gameTime) {
            if (_loaded)
                Update(gameTime);
            else
                CheckForLoaded();
        }

        private void DoUnload() {
            Unload();
        }

        #endregion

        #region Virtual Methods

        protected virtual void Initialize() { /* NOOP */ }

        protected virtual void DefineSettings(SettingsManager settingsManager) { /* NOOP */ }

        protected virtual async Task LoadAsync() { /* NOOP */ }

        protected virtual void Update(GameTime gameTime) { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

        #endregion

        #region IDispose

        protected virtual void Dispose(bool disposing) {
            DoUnload();
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~ExternalModule() {
            Dispose(false);
        }

        #endregion

    }

}
